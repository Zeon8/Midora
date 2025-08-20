namespace Midora.Syntax;

public class ClassDefinition : TranspiledType
{
    public required string BaseClassName { get; init; }

    public required VTableDeclaration VTable { get; init; }

    public override void Write(Writers writers)
    {
        VTable.Write(writers);

        base.Write(writers);
    }

    protected override void WriteFields(Writers writers)
    {
        writers.Header.WriteLine($"{BaseClassName} {Naming.BaseFieldName};");
        base.WriteFields(writers);
    }
}
