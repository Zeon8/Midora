using System.CodeDom.Compiler;

namespace Midora.Syntax;
public class VariableDeclaration(string Type, string Name, bool IsValueType)
{
    public void Write(IndentedTextWriter writer)
    {
        if(IsValueType)
            writer.WriteLine($"{Type} {Name} = {{0}};");
        else
            writer.WriteLine($"{Type} {Name} = NULL;");
    }
}
