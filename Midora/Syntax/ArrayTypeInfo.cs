using System.CodeDom.Compiler;

namespace Midora.Syntax;

public class ArrayTypeInfo(string ElementType)
{
    public void Write(IndentedTextWriter writer)
    {
        string arrayTypeInfo = Naming.GetTypeInfoName("System_Array");

        string typeInfoName = Naming.GetTypeInfoName($"System_Array_{ElementType}");
        writer.WriteLine($"static const TypeInfo {typeInfoName} = {{");
        writer.Indent++;
        writer.WriteLine($".base_type = &{arrayTypeInfo},");
        writer.WriteLine($".element_type = &{Naming.GetTypeInfoName(ElementType)},");
        writer.WriteLine($".vptr = {arrayTypeInfo}_vptr");
        writer.Indent--;
        writer.WriteLine("};");
    }
}
