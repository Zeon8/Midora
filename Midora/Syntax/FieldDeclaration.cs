namespace Midora.Syntax;

public record FieldDeclaration(string Type, string Name)
{
    public string Emit() => $"{Type} {Name};";
}
