using System.CodeDom.Compiler;

namespace Midora;

public class Writers
{
    public IndentedTextWriter Prototype { get; } = new(new StringWriter());

    public IndentedTextWriter Header { get; } = new(new StringWriter());

    public IndentedTextWriter Source { get; } = new(new StringWriter());

    public void Write(TextWriter headerWriter, TextWriter sourceWriter)
    {
        headerWriter.WriteLine(Prototype.InnerWriter.ToString());
        headerWriter.WriteLine(Header.InnerWriter.ToString());
        sourceWriter.WriteLine(Source.InnerWriter.ToString());
    }
}
