using Midora.Helpers;
using Midora.Metadata;
using Midora.Syntax;
using Mono.Cecil;

namespace Midora.Transpilers;

public class TypeTranspiler
{
    private readonly TypeMetadata _typeMetadata;
    private readonly VirtualMethodTranspiler _virtualMethodTranspiler;
    private readonly FieldTranspiler _fieldTranspiler;
    private readonly MethodTranspiler _methodTranspiler;

    public TypeTranspiler(TypeMetadata typeMetadata, FieldTranspiler fieldTranspiler, StringLiteralTranspiler stringLiteralTranspiler)
    {
        _typeMetadata = typeMetadata;
        _fieldTranspiler = fieldTranspiler;
        _virtualMethodTranspiler = new(typeMetadata);
        _methodTranspiler = new(typeMetadata, fieldTranspiler, stringLiteralTranspiler);
    }

    public ClassDefinition DeclareClass()
    {
        TypeReference? baseType = _typeMetadata.Definition.BaseType;

        return new()
        {
            VTable = DeclareVTable(),
            BaseClassName = Naming.Mangle(baseType),
            Name = _typeMetadata.MangledName,
            Fields = _fieldTranspiler.DeclareFields().ToArray(),
            StaticFields = _fieldTranspiler.DeclareStaticFields().ToArray(),
            TypeInfo = DeclareTypeInfo(),
            Methods = _methodTranspiler.DeclareMethods().ToArray(),
        };
    }

    public StructDeclaration DeclareStruct() => new()
    {
        Name = _typeMetadata.MangledName,
        Fields = _fieldTranspiler.DeclareFields(),
        StaticFields = _fieldTranspiler.DeclareStaticFields(),
        Methods = _methodTranspiler.DeclareMethods(),
        TypeInfo = DeclareTypeInfo(),
    };

    public InterfaceDeclaration DeclareInterface() => new(DeclareVTable(), DeclareTypeInfo());

    private VTableDeclaration DeclareVTable()
    {
        string? baseClassName = null;
        if (_typeMetadata.Definition.BaseType is TypeReference baseType)
            baseClassName = Naming.GetVTableName(Naming.Mangle(baseType));

        IEnumerable<string> interfaces = _typeMetadata.Definition.Interfaces
                .Select(i => Naming.Mangle(i.InterfaceType));

        return new VTableDeclaration()
        {
            Name = _typeMetadata.VtableName,
            BaseVTable = baseClassName,
            Interfaces = interfaces,
            Functions = _virtualMethodTranspiler.GetVTableFunctionDeclarations()
        };
    }

    private TypeInfoDefinition DeclareTypeInfo()
    {
        TypeReference baseType = _typeMetadata.Definition.BaseType;
        return new TypeInfoDefinition
        {
            Id = TypeHelper.GetId(_typeMetadata.Definition),
            TypeName = _typeMetadata.MangledName,
            IsInterface = _typeMetadata.Definition.IsInterface,
            ElementTypeName = null,
            Interfaces = _typeMetadata.Definition.Interfaces
                .Select(i => Naming.Mangle(i.InterfaceType))
                .ToArray(),
            BaseTypeName = baseType is not null ? Naming.Mangle(baseType) : null,
            InterfaceOffsets = _virtualMethodTranspiler
                    .GetInterfaceOffsets()
                    .ToArray(),
            ReferenceTypeFieldOffsets = _typeMetadata.Definition.IsValueType ? _fieldTranspiler
                    .ResolveReferenceTypeFieldOffsets(_typeMetadata.Definition)
                    .ToArray() : [],
            VTableEntries = !_typeMetadata.Definition.IsInterface && !_typeMetadata.Definition.IsAbstract
                ? _virtualMethodTranspiler.GetVtableEntries().ToArray() : [],
        };
    }
}
