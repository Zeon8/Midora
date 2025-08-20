#pragma once
#include <stdlib.h>
#include <stdint.h>
#include <stdbool.h>
#include <setjmp.h>

#ifdef _WIN32
#include <uchar.h>
#else
typedef uint16_t char16_t;
#endif

#ifdef _WIN32
#define midora_localloc _malloca
#else
#define midora_localloc alloca
#endif

typedef struct TypeInfo TypeInfo;
typedef struct
{
	TypeInfo* type;
	size_t offset;
} InterfaceOffset;

struct TypeInfo
{
	int32_t id;
	size_t instance_size;
	TypeInfo *base_type;
	TypeInfo *element_type;
	
	bool is_interface;
	size_t interfaces_count;
	TypeInfo **interfaces;
	InterfaceOffset *interface_offsets;

	size_t reference_offsets_count;
	size_t *reference_offsets;

	void *vptr;
};

typedef enum {
	OBJECT_MARKED = 1,
	OBJECT_SUPRESSED_FINALIZER = 2,
	OBJECT_KEEP_ALIVE = 4
} ObjectFlags;

typedef struct
{
	ObjectFlags flags;
	size_t size;
	TypeInfo *type;
} RuntimeObject;

typedef struct
{
	RuntimeObject base;
	int32_t length;
	char16_t data[];
} RuntimeString;

RuntimeObject *midora_new(TypeInfo *type_info);

static inline void *midora_get_vtable(RuntimeObject *obj)
{
	return obj->type->vptr;
}

static inline TypeInfo *midora_get_type(RuntimeObject *obj)
{
	return obj->type;
}

RuntimeObject *midora_is_instance(RuntimeObject *obj, TypeInfo *type);
void *midora_resolve_interface_vtable(RuntimeObject *obj, TypeInfo *interface_type);


RuntimeObject *midora_array_new(TypeInfo* type, int32_t length);
int32_t midora_array_get_length(RuntimeObject *array);
void *midora_array_get_element_ref(RuntimeObject *array, int32_t index);

RuntimeObject *midora_string_new(int32_t length, char16_t* string);

RuntimeObject *midora_box(void *data, TypeInfo *type_info);
void *midora_box_get_data(RuntimeObject *box_object);

typedef struct GCFrame GCFrame;
struct GCFrame
{
	GCFrame *prev;
	size_t count;
	RuntimeObject ***roots;
};

void midora_init();
void midora_gc_add_root(RuntimeObject **obj);
void midora_gc_frame_push(GCFrame *frame);
void midora_gc_frame_set(GCFrame *frame);
void midora_gc_frame_pop();
void midora_gc_supress_finalizer(RuntimeObject *obj);
size_t midora_gc_get_allocated_memory();

void midora_gc_collect();

typedef struct ExceptionFrame ExceptionFrame;
struct ExceptionFrame
{
	jmp_buf buffer;
	ExceptionFrame *prev;
};

void midora_throw(RuntimeObject *exception);
void midora_rethrow();
void midora_exception_frame_push(ExceptionFrame *frame);
void midora_exception_frame_pop();
RuntimeObject *midora_get_exception();

// typedef struct System_RuntimeTypeHandle System_RuntimeTypeHandle;
// typedef struct System_RuntimeFieldHandle System_RuntimeFieldHandle;
// typedef struct System_RuntimeMethodHandle System_RuntimeMethodHandle;

// System_RuntimeTypeHandle midora_get_type_handle(TypeInfo *typeInfo);

// typedef struct
// {
// 	size_t size;
// 	int8_t bytes[];
// } FieldInitialValue;