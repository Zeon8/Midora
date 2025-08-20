using Midora;
using System.CodeDom.Compiler;

namespace Midora.Syntax;

public class VTableFunction
{
    public required string Name { get; init; }

    public required string ReturnType { get; init; }

    public IEnumerable<string> ParameterTypes { get; init; } = [];

    public void Write(IndentedTextWriter writer)
    {
        writer.Write($"{ReturnType} (*{Name})({Naming.RuntimeObjectName}*");
        if (ParameterTypes.Any())
        {
            writer.Write(',');
            writer.Write(string.Join(',', ParameterTypes));
        }
        writer.WriteLine(");");
    }
}
