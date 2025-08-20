using System.CodeDom.Compiler;

namespace Midora.Syntax;

public record FieldOffset(string DelaringType, string Field)
{
    public string Emit() => $"offsetof({DelaringType}, {Field})";
}

public class TypeInfoDefinition
{
    public required int Id { get; init; }

    public bool IsInterface { get; init; }

    public string? ElementTypeName { get; init; }

    public required string TypeName { get; init; }

    public string? BaseTypeName { get; init; }

    public IReadOnlyCollection<string> Interfaces { get; init; } = [];
    public IReadOnlyCollection<InterfaceOffset> InterfaceOffsets { get; init; } = [];

    public IReadOnlyCollection<IEnumerable<FieldOffset>> ReferenceTypeFieldOffsets { get; init; } = [];

    public IReadOnlyCollection<string> VTableEntries { get; init; } = [];

    public void Write(Writers writers)
    {
        string typeInfoName = Naming.GetTypeInfoName(TypeName);

        if (VTableEntries.Count > 0)
        {
            writers.Header.WriteLine($"extern const void* {typeInfoName}_vptr[];");
            writers.Source.WriteLine($"const void* {typeInfoName}_vptr[] = {{");
            writers.Source.Indent++;
            foreach (var entry in VTableEntries)
                writers.Source.WriteLine($"&{entry},");
            writers.Source.Indent--;
            writers.Source.WriteLine("};");
        }

        writers.Header.WriteLine($"extern const {Naming.TypeInfoStructName} {typeInfoName};");
        writers.Source.WriteLine($"const {Naming.TypeInfoStructName} {typeInfoName} = {{");
        writers.Source.Indent++;

        writers.Source.WriteLine($".id = {Id},");

        if (IsInterface)
            writers.Source.WriteLine($".is_interface = true,");
        else
            writers.Source.WriteLine($".instance_size = sizeof({TypeName}),");

        if (BaseTypeName is not null)
            writers.Source.WriteLine($".base_type = &{Naming.GetTypeInfoName(BaseTypeName)},");

        if (ElementTypeName is not null)
            writers.Source.WriteLine($".element_type = &{Naming.GetTypeInfoName(ElementTypeName!)},");

        if (InterfaceOffsets.Count > 0)
        {
            writers.Source.WriteLine($".interfaces_count = {InterfaceOffsets.Count},");
            writers.Source.WriteLine($".interface_offsets = (InterfaceOffset[]){{");
            writers.Source.Indent++;
            foreach (InterfaceOffset offset in InterfaceOffsets)
                writers.Source.WriteLine(offset.Emit()+',');
            writers.Source.Indent--;
            writers.Source.WriteLine("},");
        }

        if(Interfaces.Count > 0)
        {
            writers.Source.WriteLine(".interfaces = (TypeInfo*[]){");
            writers.Source.Indent++;
            foreach (string interfaceName in Interfaces)
                writers.Source.WriteLine($"&{Naming.GetTypeInfoName(interfaceName)},");
            writers.Source.Indent--;
            writers.Source.WriteLine("},");
        }

        if(ReferenceTypeFieldOffsets.Count > 0)
        {
            writers.Source.WriteLine($".reference_offsets_count = {ReferenceTypeFieldOffsets.Count},");
            writers.Source.WriteLine($".reference_offsets = (size_t[]){{");
            writers.Source.Indent++;
            foreach (IEnumerable<FieldOffset> fieldOffsets in ReferenceTypeFieldOffsets)
            {
                string offsets = string.Join('+', fieldOffsets.Select(o => o.Emit()));
                writers.Source.WriteLine(offsets + ",");
            }
            writers.Source.Indent--;
            writers.Source.WriteLine("},");
        }

        if (VTableEntries.Count > 0)
            writers.Source.WriteLine($".vptr = {typeInfoName}_vptr");

        writers.Source.Indent--;
        writers.Source.WriteLine("};");
    }
}
