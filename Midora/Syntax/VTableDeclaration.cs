namespace Midora.Syntax;

public class VTableDeclaration
{
    public required string Name { get; init; }

    public string? BaseVTable { get; init; }

    public IEnumerable<string> Interfaces { get; init; } = [];

    public required IEnumerable<VTableFunction> Functions { get; init; }

    public void Write(Writers writers)
    {
        writers.Prototype.WriteLine($"typedef struct {Name} {Name};");

        writers.Header.WriteLine($"struct {Name} {{");
        writers.Header.Indent++;

        if (BaseVTable is not null)
            writers.Header.WriteLine($"{BaseVTable} {Naming.BaseFieldName};");

        foreach (var @interface in Interfaces)
            writers.Header.WriteLine($"{Naming.GetVTableName(@interface)} _{@interface};");

        foreach (var function in Functions)
            function.Write(writers.Header);

        writers.Header.Indent--;
        writers.Header.WriteLine("};");
    }
}
