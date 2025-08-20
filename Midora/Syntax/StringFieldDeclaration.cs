using System.CodeDom.Compiler;

namespace Midora.Syntax;

public record StringFieldDeclaration(string Name, string Data)
{
    public void Write(IndentedTextWriter writer)
    {
        writer.WriteLine($"static const RuntimeString {Name} = {{");
        writer.Indent++;
        writer.WriteLine(".base = {");
        writer.Indent++;
        writer.WriteLine(".flags = OBJECT_KEEP_ALIVE,");
        writer.WriteLine(".type = &System_String_type");
        writer.Indent--;
        writer.WriteLine("},");
        writer.WriteLine($".length = {Data.Length},");
        writer.WriteLine(@$".data = u""{Data}"",");
        writer.Indent--;
        writer.WriteLine("};");
    }
}