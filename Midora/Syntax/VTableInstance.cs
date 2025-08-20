using Midora;
using System.CodeDom.Compiler;

namespace Midora.Syntax;

public record VTableInstance(string TypeName, IEnumerable<string> Functions)
{
    public void Write(IndentedTextWriter writer)
    {
        writer.WriteLine($"static const void* {Naming.GetVTableInstance(TypeName)}[] = {{");
        writer.Indent++;

        foreach (var function in Functions)
            writer.WriteLine($"&{function},");

        writer.Indent--;
        writer.WriteLine("};");
    }
}
