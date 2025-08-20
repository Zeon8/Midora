using System.Drawing;
using System.Runtime.InteropServices;

namespace Runtime;

public unsafe struct Box
{
    public RuntimeObject Object;

    private byte _value;
    public byte* Value
    {
        get
        {
            fixed (byte* value = &_value)
                return value;
        }
    }


    public Box* New(byte* value, TypeInfo* typeInfo)
    {
        nuint size = (nuint)sizeof(RuntimeObject) + typeInfo->InstanceSize;
        var box = (Box*)GC.Allocate(size);
        NativeMemory.Copy(value, Value, typeInfo->InstanceSize);
        return box;
    }
}
