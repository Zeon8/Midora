using Midora;
using Midora.Helpers;
using Mono.Cecil;

namespace Midora.Metadata;

public class TypeMetadata
{
    public TypeDefinition Definition { get; }

    public string MangledName { get; }

    public bool HasVtable => !Definition.IsValueType && !Definition.IsEnum;
    public bool IsStatic => Definition.IsAbstract && Definition.IsSealed;
    public string VtableName => Naming.GetVTableName(ReferenceTypeName);

    private string ReferenceTypeName => Definition.IsValueType ? BoxName : MangledName;
    private string BoxName => Naming.GetBoxedType(MangledName);

    public TypeMetadata(TypeDefinition definition)
    {
        Definition = definition;
        MangledName = Naming.Mangle(definition);
    }

    public TypeMetadata(TypeReference reference) : this(reference.Resolve()) { }

    public IEnumerable<TypeDefinition> GetBaseTypes()
    {
        TypeDefinition? current = Definition.BaseType?.Resolve();
        while (current is not null)
        {
            yield return current;
            current = current.BaseType?.Resolve();
        }
    }
}
