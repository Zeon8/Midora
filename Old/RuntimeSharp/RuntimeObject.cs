namespace Runtime;

public unsafe struct RuntimeObject
{
    public nuint AllocatedSize;
    public bool Marked;

    public TypeInfo* TypeInfo;

    public static RuntimeObject* New(TypeInfo* typeInfo)
    {
        RuntimeObject* runtimeObject = GC.Allocate(typeInfo->InstanceSize);
        runtimeObject->TypeInfo = typeInfo;
        return runtimeObject;
    }
}
