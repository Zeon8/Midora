using System.CodeDom.Compiler;

namespace Midora.Syntax;

public class UnmanagedMethod : TranspiledMethod
{
    public required string EntryPoint { get; init; }

    public override void Write(Writers writers)
    {
        WriteTop(writers.Source, EntryPoint);
        writers.Source.WriteLine(';');

        WriteTop(writers.Header, Name);
        writers.Header.WriteLine(';');

        WriteTop(writers.Source, Name);

        writers.Source.WriteLine();
        writers.Source.WriteLine('{');
        writers.Source.Indent++;

        var args = string.Join(',', Parameters.Select(p => p.Name));
        writers.Source.WriteLine($"{EntryPoint}({args});");

        writers.Source.Indent--;
        writers.Source.WriteLine('}');

    }

    private void WriteTop(IndentedTextWriter writer, string name)
    {
        string args = string.Join(',', Parameters);
        writer.Write($"{ReturnType} {name}({args})");
    }
}