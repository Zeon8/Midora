namespace Midora.Syntax;

public abstract class TranspiledType
{
    public required string Name { get; init; }

    public required IEnumerable<FieldDeclaration> Fields { get; init; }

    public IEnumerable<StaticFieldDeclaration> StaticFields { get; init; } = [];

    public IEnumerable<TranspiledMethod> Methods { get; init; } = [];

    public TypeInfoDefinition? TypeInfo { get; init; }

    public virtual void Write(Writers writers)
    {
        writers.Prototype.WriteLine($"typedef struct {Name} {Name};");

        writers.Header.WriteLine($"struct {Name} {{");
        writers.Header.Indent++;

        WriteFields(writers);

        writers.Header.Indent--;
        writers.Header.WriteLine("};");

        foreach (StaticFieldDeclaration fieldDeclaration in StaticFields)
            fieldDeclaration.Write(writers);

        foreach (var method in Methods)
            method.Write(writers);

        if (TypeInfo is null)
            return;

        TypeInfo.Write(writers);
    }

    protected virtual void WriteFields(Writers writers)
    {
        foreach (FieldDeclaration fieldDeclaration in Fields)
            writers.Header.WriteLine(fieldDeclaration.Emit());
    }
}
