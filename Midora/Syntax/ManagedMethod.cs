using System.CodeDom.Compiler;

namespace Midora.Syntax;

public class ManagedMethod : TranspiledMethod
{
    public MethodBodyDeclaration? Body { get; init; }

    public bool Inline { get; init; }

    public override void Write(Writers writers)
    {
        WriteTop(writers.Header);
        writers.Header.WriteLine(';');

        if (Body is null)
            return;

        if (Inline)
            writers.Source.Write("static inline ");

        WriteTop(writers.Source);

        writers.Source.WriteLine();
        writers.Source.WriteLine('{');
        writers.Source.Indent++;

        Body.Write(writers.Source);

        writers.Source.Indent--;
        writers.Source.WriteLine('}');

    }

    private void WriteTop(IndentedTextWriter writer)
    {
        writer.Write($"{ReturnType} {Name}(");
        if (DeclarationType is not null)
        {
            writer.Write($"{DeclarationType}* {Naming.ThisArgument}");
            if (Parameters.Any())
                writer.Write(',');
        }
        writer.Write(string.Join(',', Parameters));
        writer.Write(")");
    }
}
