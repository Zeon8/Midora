using System.CodeDom.Compiler;

namespace Midora.Syntax;

public record Root(string Variable, IEnumerable<FieldOffset> Offsets);

public record StackFrame(IReadOnlyCollection<Root> Roots)
{
    public void Write(IndentedTextWriter writer)
    {
        if (Roots.Count == 0)
            return;

        writer.WriteLine($"GCFrame {Naming.GCFrameVariableName} = {{");
        writer.Indent++;
        writer.WriteLine($".count = {Roots.Count},");
        writer.WriteLine($".roots = (RuntimeObject**[]){{");
        writer.Indent++;
        foreach (var root in Roots)
        {
            if (root.Offsets.Any())
            {
                writer.Write($"(RuntimeObject**)(((char*)&{root.Variable})+");
                var offsets = string.Join('+', root.Offsets.Select(o => o.Emit()));
                writer.WriteLine($"{offsets}),");
            }
            else
                writer.WriteLine($"&{root.Variable},");
        }
        writer.Indent--;
        writer.WriteLine("},");
        writer.Indent--;
        writer.WriteLine("};");

        writer.WriteLine($"midora_gc_frame_push(&{Naming.GCFrameVariableName});");
    }

    public void WritePop(IndentedTextWriter writer)
    {
        if(Roots.Count != 0)
            writer.WriteLine($"midora_gc_frame_pop();");
    }
}
