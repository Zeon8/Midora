#include <string.h>
#include "midora.h"
#include <stdio.h>
#include <assert.h>
#include <setjmp.h>

typedef struct
{
	intptr_t stub1;
	intptr_t stub2;
	intptr_t stub3;
	void (*Finalize)(RuntimeObject *);
} RuntimeObjectVtable;

struct GC
{
	size_t objects_count;
	RuntimeObject **objects;
	size_t roots_count;
	RuntimeObject ***roots;
	GCFrame *stack_frame;
	size_t allocated;
} GC;

typedef struct
{
	RuntimeObject base;
	int32_t length;
	char data[];
} RuntimeArray;

typedef struct
{
	RuntimeObject base;
	char data[];
} Box;

extern const TypeInfo System_Array_type;
extern const TypeInfo System_ValueType_type;

static inline bool midora_is_value_type(TypeInfo *type)
{
	return type->base_type == &System_ValueType_type;
}

static inline bool midora_is_array(TypeInfo *type)
{
	return type == &System_Array_type;
}

void midora_init()
{
	GC.objects = malloc(sizeof(RuntimeObject *) * 1000);
	GC.roots = malloc(sizeof(RuntimeObject *) * 1000);
}

void midora_gc_frame_push(GCFrame *frame)
{
	frame->prev = GC.stack_frame;
	GC.stack_frame = frame;
}

void midora_gc_frame_set(GCFrame *frame)
{
	GC.stack_frame = frame;
}

void midora_gc_frame_pop()
{
	GC.stack_frame = GC.stack_frame->prev;
}

void midora_gc_add_root(RuntimeObject **obj)
{
	GC.roots[GC.roots_count] = obj;
	GC.roots_count++;
}

void midora_gc_mark_object(RuntimeObject *obj);

void midora_gc_mark_references(RuntimeObject *obj)
{
	TypeInfo *type_info = obj->type;
	while (type_info != NULL)
	{
		for (int i = 0; i < type_info->reference_offsets_count; i++)
		{
			size_t offset = type_info->reference_offsets[i];
			if (midora_is_value_type(obj->type))
				offset += sizeof(Box);

			RuntimeObject *reference = *(RuntimeObject **)((char *)(obj) + offset);
			if (reference == NULL)
				continue;

			midora_gc_mark_object(reference);
		}

		type_info = type_info->base_type;
	}
}

void midora_gc_mark_object(RuntimeObject *obj)
{
	if ((obj->flags & OBJECT_KEEP_ALIVE) == OBJECT_KEEP_ALIVE)
		return;

	obj->flags |= OBJECT_MARKED;
	midora_gc_mark_references(obj);

	if (midora_is_array(obj->type))
	{
		RuntimeArray *array = (RuntimeArray *)obj;
		TypeInfo *element_type = obj->type->element_type;

		if (!midora_is_value_type(element_type))
		{
			for (int i = 0; i < array->length; i++)
			{
				char *element = array->data + i * element_type->instance_size;
				for (int j = 0; j < element_type->reference_offsets_count; j++)
				{
					size_t offset = element_type->reference_offsets[j];
					RuntimeObject *reference = *(RuntimeObject **)(element + offset);
					if (reference != NULL)
						midora_gc_mark_object(reference);
				}
			}
		}
		else
		{
			RuntimeObject **elements = (RuntimeObject **)array->data;
			for (int i = 0; i < array->length; i++)
				midora_gc_mark_object(elements[i]);
		}
	}
}

void midora_gc_mark_frame(GCFrame *frame)
{
	for (size_t i = 0; i < frame->count; i++)
	{
		RuntimeObject *obj = *frame->roots[i];
		if (obj == NULL)
			continue;

		midora_gc_mark_object(obj);
	}
}

void midora_gc_mark()
{
	if (GC.stack_frame != NULL)
		midora_gc_mark_frame(GC.stack_frame);

	for (size_t i = 0; i < GC.roots_count; i++)
	{
		RuntimeObject *root = *GC.roots[i];
		if (root == NULL)
			continue;

		midora_gc_mark_object(root);
	}
}

void midora_gc_sweep()
{
	size_t new_count = 0;
	for (size_t i = 0; i < GC.objects_count; i++)
	{
		RuntimeObject *obj = GC.objects[i];

		if ((obj->flags & OBJECT_MARKED) == OBJECT_MARKED)
		{
			obj->flags ^= OBJECT_MARKED;
			GC.objects[new_count] = obj;
			new_count++;
		}
		else
		{
			if ((obj->flags & OBJECT_SUPRESSED_FINALIZER) == OBJECT_SUPRESSED_FINALIZER)
				((RuntimeObjectVtable *)obj->type->vptr)->Finalize(obj);

			GC.allocated -= obj->size;
			free(obj);
		}
	}
	GC.objects_count = new_count;
}

void midora_gc_supress_finalizer(RuntimeObject *obj)
{
	obj->flags |= OBJECT_SUPRESSED_FINALIZER;
}

size_t midora_gc_get_allocated_memory()
{
	return GC.allocated;
}

void midora_gc_collect()
{
	midora_gc_mark();
	midora_gc_sweep();
}

RuntimeObject *midora_gc_alloc(size_t size)
{
	if (GC.allocated > 1000)
		midora_gc_collect();

	RuntimeObject *obj = malloc(size);
	memset(obj, 0, size);
	obj->size = size;

	GC.objects[GC.objects_count] = obj;
	GC.objects_count++;
	GC.allocated += size;

	return obj;
}

RuntimeObject *midora_new(TypeInfo *type_info)
{
	RuntimeObject *obj = midora_gc_alloc(type_info->instance_size);
	obj->type = type_info;
	return obj;
}

static inline size_t midora_get_element_size(TypeInfo *type){
	return midora_is_value_type(type->element_type) 
		? type->element_type->instance_size 
		: sizeof(RuntimeObject *);
}

RuntimeObject *midora_array_new(TypeInfo *type, int32_t length)
{
	size_t element_size = midora_get_element_size(type);
	RuntimeArray *array = (RuntimeArray *)midora_gc_alloc(sizeof(RuntimeArray) + element_size * length);
	array->base.type = type;
	array->length = length;
	return (RuntimeObject *)array;
}

int32_t midora_array_get_length(RuntimeObject *array)
{
	return ((RuntimeArray *)array)->length;
}

void *midora_array_get_element_ref(RuntimeObject *array, int32_t index)
{
	size_t element_size = midora_get_element_size(array->type);
	return ((RuntimeArray *)array)->data + element_size * index;
}

extern TypeInfo System_RuntimeString_type;

RuntimeObject *midora_string_new(int32_t length, char16_t *string)
{
	size_t string_size = length * sizeof(char16_t);
	TypeInfo *type = &System_RuntimeString_type;
	RuntimeString *object = (RuntimeString *)midora_gc_alloc(sizeof(RuntimeString) + string_size);
	object->base.type = type;
	object->length = length;
	memcpy(object->data, string, string_size);
	return (RuntimeObject *)object;
}

RuntimeObject *midora_box(void *value, TypeInfo *type_info)
{
	Box *box = (Box *)midora_gc_alloc(sizeof(Box) + type_info->instance_size);
	box->base.type = type_info;
	memcpy(box->data, value, type_info->instance_size);
	return (RuntimeObject *)box;
}

void *midora_box_get_data(RuntimeObject *object)
{
	return ((Box *)object)->data;
}

bool midora_type_is_instance(TypeInfo *type, TypeInfo *search_type)
{
	if (type->id == search_type->id)
		return true;

	for (size_t i = 0; i < type->interfaces_count; i++)
	{
		TypeInfo *interface_type = type->interfaces[i];
		
		if(midora_type_is_instance(interface_type, search_type))
			return true;
	}

	if(type->base_type != NULL)
		return midora_type_is_instance(type->base_type, search_type);

	return false;
}

void *midora_resolve_interface_vtable(RuntimeObject *obj, TypeInfo *interface_type)
{
	TypeInfo *type_info = obj->type;
	while (type_info != NULL)
	{
		for (size_t i = 0; i < obj->type->interfaces_count; i++)
		{
			InterfaceOffset offset = obj->type->interface_offsets[i];
			if (midora_type_is_instance(offset.type, interface_type))
				return (char*)obj->type->vptr + offset.offset;
		}

		type_info = type_info->base_type;
	}

	return NULL;
}

RuntimeObject *midora_is_instance(RuntimeObject *obj, TypeInfo *type)
{
	if(midora_type_is_instance(obj->type, type))
		return obj;

	return NULL;
}

static RuntimeObject *thrown_exception;
static ExceptionFrame *exception_frame;

void midora_throw(RuntimeObject *exception)
{
	thrown_exception = exception;
	longjmp(exception_frame->buffer, true);
}

void midora_rethrow()
{
	midora_exception_frame_pop();
	longjmp(exception_frame->buffer, 1);
}

void midora_exception_frame_push(ExceptionFrame *frame)
{
	frame->prev = exception_frame;
	exception_frame = frame;
}

void midora_exception_frame_pop()
{
	exception_frame = exception_frame->prev;
}

RuntimeObject *midora_get_exception()
{
	return thrown_exception;
}

// struct System_RuntimeTypeHandle
// {
// 	intptr_t value;
// };

// System_RuntimeTypeHandle midora_get_type_handle(TypeInfo *typeInfo)
// {
// 	return (System_RuntimeTypeHandle){(intptr_t)typeInfo};
// }