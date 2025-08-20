//! By convention, root.zig is the root source file when making a library. If
//! you are making an executable, the convention is to delete this file and
//! start with main.zig instead.
const std = @import("std");
const testing = std.testing;
const c = @cImport({
    @cInclude("../include/midora.h");
    @cInclude("setjmp.h");
});

const gc_collect_threshold: usize = 100000;

const GCObject = struct {
    size: usize,
    marked: bool = false,
    supress_finalizer: bool = false,
};

pub const GCFrame = struct {
    count: usize,
    prev: ?*GCFrame,
    _roots: usize,

    pub fn roots(self: *GCFrame) []*?*Object {
        const arr: [*]*?*Object = @ptrCast(&self._roots);
        return arr[0..self.count];
    }
};

const GC = struct {
    objects: std.AutoHashMap(*Object, GCObject),
    roots: std.ArrayList(*?*Object),
    stack_frame: ?*GCFrame = null,
    allocated: usize = 0,

    pub fn init() GC {
        return .{
            .roots = std.ArrayList(*?*Object).init(allocator),
            .objects = std.AutoHashMap(*Object, GCObject).init(allocator),
        };
    }

    pub fn collect(self: *GC) void {
        self.mark();
        self.sweep();
    }

    pub fn collectIfFull(self: *GC) void {
        if (gc.allocated > gc_collect_threshold)
            collect(self);
    }

    pub fn trackObject(self: *GC, obj: *Object, size: usize) !void {
        try self.objects.put(obj, GCObject{
            .size = size,
        });
        gc.allocated += size;
    }

    pub fn supressFinalizer(self: *GC, obj: *Object) void {
        self.objects.getPtr(obj).?.supress_finalizer = true;
    }

    fn markObject(self: *GC, obj: *Object) void {
        self.objects.getPtr(obj).?.marked = true;
        self.markReferences(obj);

        if (obj.type.is_array and !obj.type.element_type.*.is_value_type) {
            const array: *Array = @ptrCast(obj);
            for (array.getSlice(*Object)) |element| {
                self.markObject(element);
            }
        }
    }

    fn markReferences(self: *GC, obj: *Object) void {
        var type_info: ?*TypeInfo = obj.type;
        while (type_info != null) {
            const slice = type_info.?.reference_offsets[0..type_info.?.reference_offsets_count];
            for (slice) |offset| {
                const ref: *?*Object = @ptrFromInt(@intFromPtr(obj) + offset);
                if (ref.*) |ref_obj|
                    self.markObject(ref_obj);
            }
            type_info = type_info.?.base_type;
        }
    }

    fn markFrame(self: *GC, frame: *GCFrame) void {
        for (0..frame.count) |i| {
            const obj: ?*Object = frame.roots()[i].*;
            if (obj == null)
                continue;

            self.markObject(obj.?);
        }
    }

    fn mark(self: *GC) void {
        if (self.stack_frame) |stack_frame|
            self.markFrame(stack_frame);

        for (self.roots.items) |root_ptr| {
            if (root_ptr.*) |root|
                self.markObject(root);
        }
    }

    fn sweep(self: *GC) void {
        var iterator = self.objects.iterator();
        while (iterator.next()) |entry| {
            var gc_object: *GCObject = entry.value_ptr;
            if (entry.value_ptr.marked) {
                gc_object.marked = false;
            } else {
                const obj: *Object = entry.key_ptr.*;
                if (!gc_object.supress_finalizer) {
                    if (obj.type.*.finalizer) |finalizerPtr| {
                        const finalizer: *const fn () void = @ptrCast(@alignCast(finalizerPtr));
                        finalizer();
                    }
                }

                self.allocated -= gc_object.size;
                allocator.destroy(obj);
                _ = self.objects.remove(obj);
            }
        }
    }
};

const allocator: std.mem.Allocator = std.heap.c_allocator;
pub var gc: GC = GC.init();

pub const Error = error{ResolveError};
pub const TypeInfo = c.TypeInfo;

fn isAssignableTo(type_info: *TypeInfo, assign_type: *TypeInfo) bool {
    if (type_info == assign_type)
        return true;
    if (type_info.base_type == assign_type)
        return true;
    if (type_info.base_type == null)
        return false;

    return isAssignableTo(type_info.base_type, assign_type);
}

fn resolveInterface(type_info: *TypeInfo, interface_type: *TypeInfo) *anyopaque {
    for (0..type_info.interfaces_count) |i| {
        const interface_offset: c.InterfaceOffset = type_info.interface_offsets[i];
        if (interface_offset.type == interface_type) {
            const ptr: usize = @intFromPtr(type_info.vptr);
            return @ptrFromInt(ptr + interface_offset.offset);
        }
    }

    if (type_info.base_type) |base_type|
        return resolveInterface(base_type, interface_type);

    @panic("Interface vtable not found.");
}

pub const Object = struct {
    type: *TypeInfo,

    pub fn new(size: usize, type_info: *TypeInfo) !*Object {
        gc.collectIfFull();

        const slice = try allocator.alloc(u8, size);
        @memset(slice, 0);

        const obj: *Object = @ptrFromInt(@intFromPtr(slice.ptr));
        obj.type = type_info;
        try gc.trackObject(obj, size);

        return obj;
    }

    pub fn resolveInterfaceVtable(self: *Object, interface_type: *TypeInfo) *anyopaque {
        return resolveInterface(self.type, interface_type);
    }

    pub fn isInstanceOf(self: *Object, type_info: *TypeInfo) bool {
        return isAssignableTo(self.type, type_info);
    }
};

pub const Array = struct {
    base: Object,
    length: i32,
    _value: void,

    pub fn new(type_info: *TypeInfo, length: i32) !*Array {
        const element_size: usize = getElementSize(type_info);
        const size = @sizeOf(Array) + element_size * @as(usize, @intCast(length));
        const obj = try Object.new(size, type_info);
        var array: *Array = @ptrCast(obj);
        array.length = length;
        return array;
    }

    pub fn getElementRef(self: *Array, index: i32) *anyopaque {
        const ptr: usize = @intFromPtr(&self._value);
        const elementSize: usize = getElementSize(self.base.type);
        return @ptrFromInt(ptr + elementSize * @as(usize, @intCast(index)));
    }

    pub fn getSlice(self: *Array, comptime T: type) []T {
        const ptr: [*]T = @ptrCast(@alignCast(&self._value));
        const length: usize = @intCast(self.length);
        return ptr[0..length];
    }

    fn getElementSize(type_info: *TypeInfo) usize {
        if (type_info.element_type.*.is_value_type) {
            return type_info.element_type.*.instance_size;
        } else {
            return @sizeOf(*Object);
        }
    }
};

extern const System_RuntimeString_type: TypeInfo;

pub const String = struct {
    base: Object,
    length: usize,
    _data: void,

    pub fn new(literal: [*:0]u8) !*String {
        const length: usize = std.mem.len(literal);
        const size = @sizeOf(String) + length * @sizeOf(u8);
        const obj = try Object.new(size, @constCast(&System_RuntimeString_type));
        var string: *String = @ptrCast(obj);
        string.length = length;

        const slice = literal[0..length];
        @memcpy(getString(string), slice);
        return string;
    }

    fn getString(self: *String) [*]u8 {
        return @ptrCast(&self._data);
    }
};

pub const Box = struct {
    base: Object,
    _value: void,

    pub fn new(value: [*]u8, value_type_info: *TypeInfo) !*Box {
        const size = @sizeOf(Object) + value_type_info.instance_size;
        const obj = try Object.new(size, value_type_info);
        const box: *Box = @ptrCast(obj);

        const data_ptr: [*]u8 = @ptrCast(&box._value);
        @memcpy(data_ptr, value[0..value_type_info.instance_size]);
        return box;
    }

    pub fn getValuePointer(self: *Box) *anyopaque {
        return @ptrCast(&self._value);
    }
};

pub const ExceptionFrame = struct {
    buffer: c.jmp_buf,
    prev: ?*ExceptionFrame,
};

var thrown_exception: ?*Object = null;
var exception_frame: ?*ExceptionFrame = null;

pub fn pushExceptionFrame(frame: *ExceptionFrame) void {
    frame.prev = exception_frame;
    exception_frame = frame;
}

pub fn popExceptionFrame() void {
    exception_frame = exception_frame.?.prev;
}

pub fn throwException(exception: *Object) void {
    thrown_exception = exception;
    c.longjmp(&exception_frame.?.buffer, 1);
}

pub fn rethrowException() void {
    popExceptionFrame();
    c.longjmp(&exception_frame.?.buffer, 1);
}

pub fn getException() ?*Object {
    return thrown_exception;
}
