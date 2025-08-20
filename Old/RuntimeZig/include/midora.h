#pragma once
#include <stdlib.h>
#include <stdint.h>
#include <stdbool.h>
#include <setjmp.h>

#ifdef _WIN32
#define midora_localloc _malloca
#else
#define midora_localloc alloca
#endif

typedef struct TypeInfo TypeInfo;

typedef struct
{
	TypeInfo *type;
	size_t offset;
} InterfaceOffset;

struct TypeInfo
{
	size_t instance_size;
	bool is_value_type;
	TypeInfo *base_type;

	bool is_array;
	TypeInfo *element_type;

	size_t interfaces_count;
	TypeInfo **interfaces;
	InterfaceOffset *interface_offsets;

	size_t reference_offsets_count;
	size_t *reference_offsets;

	void *finalizer;
	void *vptr;
};

typedef struct
{
	TypeInfo *type;
} RuntimeObject;

RuntimeObject *midora_new(TypeInfo *type_info);
RuntimeObject *midora_is_instance(RuntimeObject *obj, TypeInfo *type);
TypeInfo *midora_get_type_info(RuntimeObject *obj);
void *midora_resolve_vtable(RuntimeObject *obj, TypeInfo *interface_type);

static inline void *midora_get_vtable(RuntimeObject *obj)
{
	return midora_get_type_info(obj)->vptr;
}

RuntimeObject *midora_string_new(const char *string);

RuntimeObject *midora_array_new(TypeInfo *type, int32_t length);
int32_t midora_array_get_length(RuntimeObject *array);
void *midora_array_get_element_ref(RuntimeObject *array, int32_t index);

RuntimeObject *midora_box(void *value, TypeInfo *typeInfo);
void *midora_box_get_value(RuntimeObject *boxObject);

typedef struct GCFrame GCFrame;
struct GCFrame
{
	GCFrame *prev;
	size_t count;
	RuntimeObject ***roots;
};

void midora_gc_add_root(RuntimeObject **obj);
void midora_gc_frame_push(GCFrame *frame);
void midora_gc_frame_set(GCFrame *frame);
void midora_gc_frame_pop();
void midora_gc_supress_finalizer(RuntimeObject *obj);

void midora_gc_collect();
void midora_gc_trigger();

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