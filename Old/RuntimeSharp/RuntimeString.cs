using System.Runtime.InteropServices;

namespace Runtime;

public unsafe struct RuntimeString
{
    public RuntimeObject Object;
    public int Length;
    private char _data;

    public char* Data
    {
        get
        {
            fixed (char* ptr = &_data)
                return ptr;
        }
    }

    public static RuntimeString* New(char* value, int length)
    {
        nuint size = (nuint)sizeof(RuntimeString) + sizeof(char) * (nuint)length;
        var stringObject = (RuntimeString*)GC.Allocate(size);
        stringObject->Length = length;
        NativeMemory.Copy(value, stringObject->Data, (nuint)length);
        return stringObject;
    }
}