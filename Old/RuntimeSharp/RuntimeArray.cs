namespace Runtime;

public unsafe struct RuntimeArray
{
    public RuntimeObject Object;
    public int Length;
    private byte _data;

    public byte* Data
    {
        get
        {
            fixed (byte* ptr = &_data)
                return ptr;
        }
    }

    public static RuntimeArray* New(TypeInfo* typeInfo, int length) {
        nuint size = (nuint)sizeof(RuntimeObject) + typeInfo->ElementSize * (nuint)length;
        var array = (RuntimeArray*)GC.Allocate(size);
        array->Length = length;
        return array;
    }

    public void* GetElementReference(int index)
    {
        return Data + Object.TypeInfo->ElementSize * (nuint)index;
    }
}
