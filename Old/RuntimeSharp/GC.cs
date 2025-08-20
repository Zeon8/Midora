using System.Runtime.InteropServices;

namespace Runtime;

public unsafe struct GC
{
    private const int HeapSize = 5 * 1024;

    private static int s_objectsCount;
    private static int s_rootsCount;
    private static GCFrame* s_gcFrame;
    private static nuint s_allocatedSize;

    private static readonly RuntimeObject** s_objects = (RuntimeObject**)NativeMemory.Alloc((nuint)(sizeof(IntPtr) * 100));
    private static readonly RuntimeObject*** s_roots = (RuntimeObject***)NativeMemory.Alloc((nuint)(sizeof(IntPtr) * 100));
    private static readonly byte* s_markBitmap = (byte*)NativeMemory.Alloc(100);

    public static RuntimeObject* Allocate(nuint size)
    {
        if(s_allocatedSize >= HeapSize)
        {
            Collect();
        }

        var ptr = NativeMemory.Alloc(size);
        NativeMemory.Fill(ptr, size, 0);

        var obj = (RuntimeObject*)ptr;
        obj->AllocatedSize = size;

        s_objects[s_objectsCount] = obj;
        s_objectsCount++;
        s_allocatedSize += size;

        return obj;
    }

    public static void Collect()
    {
        Mark();
        Sweep();
    }

    public static void AddRoot(RuntimeObject** root)
    {
        s_roots[s_rootsCount] = root;
        s_rootsCount++;
    }

    private static void MarkObject(RuntimeObject* obj) {
        obj->Marked = true;
        MarkReferences(obj);

        if (obj->TypeInfo->IsArray) 
        {
            if(!obj->TypeInfo->ElementType->IsValueType)
            {
                var array = (RuntimeArray*)obj;
                var elements = (RuntimeObject**)array->Data;
                for (int i = 0; i < array->Length; i++)
                    MarkObject(elements[i]);
            }
            // TODO value types
        }
    }

    private static void MarkReferences(RuntimeObject* obj) 
    {
        var typeInfo = obj->TypeInfo;
        while (typeInfo != null)
        {
            for(int i = 0; i < typeInfo->ReferenceOffsetsCounts; i++)
            {
                nuint offset = typeInfo->ReferenceOffsets[i];
                RuntimeObject* reference = *(RuntimeObject**)((char*)(obj) + offset);
                if (reference == null)
                    continue;

                MarkObject(reference);
            }
        }
    }

    private static void MarkFrame(GCFrame* frame)
    {
        for (int i = 0; i < frame->Count; i++)
        {
            RuntimeObject* obj = *frame->Roots[i];
            if (obj == null)
                continue;

            MarkObject(obj);
        }
    }

    private static void Mark()
    {
        MarkFrame(s_gcFrame);

        for(int i = 0; i < s_rootsCount; i++)
        {
            RuntimeObject* root = *s_roots[i];
            if (root == null)
                continue;

            MarkObject(root);
        }
    }

    private static void Sweep()
    {
        int markedCount = 0;
        for (int i = 0; i < s_objectsCount; i++)
        {
            RuntimeObject* obj = s_objects[i];
            if (obj->Marked)
            {
                obj->Marked = false;
                s_objects[markedCount] = obj;
                markedCount++;
            }
            else
            {
                if (obj->TypeInfo->Finalizer != null)
                    obj->TypeInfo->Finalizer();

                s_allocatedSize -= obj->AllocatedSize;
                NativeMemory.Free(obj);
            }
        }
        s_objectsCount = markedCount;
    }

}
