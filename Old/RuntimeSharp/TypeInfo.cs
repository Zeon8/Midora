using System.Reflection;

namespace Runtime;
public readonly unsafe struct TypeInfo
{
    public readonly nuint InstanceSize;
    public readonly TypeInfo* BaseType;
    public readonly TypeInfo* ElementType;

    public readonly bool IsValueType;
    public readonly bool IsArray;

    public readonly int InterfacesCount;
    public readonly TypeInfo** Interfaces;
    public readonly InterfaceOffset* InterfaceOffsets;

    public readonly int ReferenceOffsetsCounts;
    public readonly UIntPtr* ReferenceOffsets;

    public readonly delegate*<void> Finalizer;
    public readonly void* VirtualTable;

    public nuint ElementSize
    {
        get 
        {
            if (ElementType->IsValueType)
                return ElementType->InstanceSize;

            return (nuint)sizeof(RuntimeObject*);
        }
    }

    public void* ResolveInterface(TypeInfo* InterfaceTypeInfo)
    {
        for (int i = 0; i < InterfacesCount; i++)
        {
            InterfaceOffset interfaceOffset = InterfaceOffsets[i];
            if (interfaceOffset.TypeInfo == InterfaceTypeInfo)
                return (byte*)VirtualTable + interfaceOffset.Offset;
        }

        if (BaseType != null)
            return BaseType->ResolveInterface(InterfaceTypeInfo);

         // @panic("Interface vtable not found.");
        return null;
    }

    public static bool IsAssignableTo(TypeInfo* source, TypeInfo* assignTypeInfo) 
    {
        if (source == assignTypeInfo)
            return true;

        if (source->BaseType == source)
            return true;

        for (int i = 0; i < source->InterfacesCount; i++)
        {
            if(source->Interfaces[i] == assignTypeInfo)
                return true;
        }

        if (source->BaseType != null)
            return IsAssignableTo(source->BaseType, assignTypeInfo);

        return false;
    }
}
