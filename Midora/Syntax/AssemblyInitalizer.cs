using Midora.Transpilers;
using System.CodeDom.Compiler;

namespace Midora.Syntax;

public record GCRootField(string Field)
{
    public void Write(IndentedTextWriter writer)
    {
        writer.WriteLine($"midora_gc_add_root(&{Field});");
    }
}

public record StaticConstructorCall(string Type)
{
    public void Write(IndentedTextWriter writer)
        => writer.WriteLine($"{Type}___cctor();");
}

public class AssemblyInitalizer
{
    public required string AssemblyName { get; init; }

    public required IEnumerable<StaticConstructorCall> StaticConstructorCalls { get; init; }

    public required IEnumerable<GCRootField> GCRootFields { get; init; }

    public void Write(Writers writers)
    {
        var functionHeader = $"void {AssemblyName}__initialize()";
        writers.Header.WriteLine(functionHeader + ';');
        writers.Source.WriteLine(functionHeader + '{');
        writers.Source.Indent++;

        foreach (var call in StaticConstructorCalls)
            call.Write(writers.Source);

        foreach (GCRootField field in GCRootFields)
            field.Write(writers.Source);

        writers.Source.Indent--;
        writers.Source.WriteLine('}');
    }
}
