const runtime = @import("runtime.zig");
const std = @import("std");
const c = @cImport({
    @cInclude("midora.h");
});

export fn midora_gc_frame_push(frame: *runtime.GCFrame) void {
    frame.prev = runtime.gc.stack_frame;
    runtime.gc.stack_frame = frame;
}

export fn midora_gc_frame_set(frame: *runtime.GCFrame) void {
    runtime.gc.stack_frame = frame;
}

export fn midora_gc_frame_pop() void {
    runtime.gc.stack_frame = runtime.gc.stack_frame.?.prev;
}

export fn midora_gc_add_root(obj: *?*runtime.Object) void {
    runtime.gc.roots.append(obj) catch unreachable;
}

export fn midora_gc_collect() void {
    runtime.gc.collect();
}

export fn midora_gc_supress_finalizer(obj: *runtime.Object) void {
    runtime.gc.supressFinalizer(obj);
}

export fn midora_new(type_info: *runtime.TypeInfo) *runtime.Object {
    return runtime.Object.new(type_info.instance_size, type_info) catch unreachable;
}

export fn midora_array_new(array_type: *runtime.TypeInfo, length: i32) *runtime.Object {
    const array = runtime.Array.new(array_type, length) catch unreachable;
    return @ptrCast(array);
}

export fn midora_array_get_element_ref(obj: *runtime.Object, index: i32) *anyopaque {
    const array: *runtime.Array = @ptrCast(obj);
    return array.getElementRef(index);
}

export fn midora_string_new(string: [*:0]u8) *runtime.Object {
    const stringObj = runtime.String.new(string) catch unreachable;
    return @ptrCast(stringObj);
}

export fn midora_box(value: [*]u8, type_info: *runtime.TypeInfo) *runtime.Object {
    const box = runtime.Box.new(value, type_info) catch unreachable;
    return @ptrCast(box);
}

export fn midora_resolve_vtable(obj: *runtime.Object, type_info: *runtime.TypeInfo) *anyopaque {
    return obj.resolveInterfaceVtable(type_info);
}

export fn midora_get_type_info(obj: *runtime.Object) *runtime.TypeInfo {
    return obj.type;
}

export fn midora_is_instance(obj: *runtime.Object, type_info: *runtime.TypeInfo) ?*runtime.Object {
    return if (obj.isInstanceOf(type_info)) obj else null;
}

export fn midora_get_array_length(obj: *runtime.Object) i32 {
    const array: *runtime.Array = @ptrCast(obj);
    return array.length;
}

export fn midora_box_get_value(obj: *runtime.Object) *anyopaque {
    const box: *runtime.Box = @ptrCast(obj);
    return box.getValuePointer();
}

export fn midora_throw(exception: *runtime.Object) void {
    return runtime.throwException(exception);
}

export fn midora_rethrow() void {
    runtime.rethrowException();
}

export fn midora_exception_frame_push(frame: *runtime.ExceptionFrame) void {
    runtime.pushExceptionFrame(frame);
}

export fn midora_exception_frame_pop() void {
    runtime.popExceptionFrame();
}

export fn midora_get_exception() ?*runtime.Object {
    return runtime.getException();
}
