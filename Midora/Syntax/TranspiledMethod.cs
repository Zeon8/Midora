namespace Midora.Syntax;

public record MethodParameter(string Name, string Type)
{
    public override string ToString() => $"{Type} {Name}";
}

public abstract class TranspiledMethod
{
    public required string Name { get; init; }

    public required string ReturnType { get; init; }

    public string? DeclarationType { get; init; }

    public required IEnumerable<MethodParameter> Parameters { get; init; }

    public abstract void Write(Writers writers);
}
