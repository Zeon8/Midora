using dnlib.DotNet;
using System.CodeDom.Compiler;
using System.Text;

namespace Dotnet2C
{
    public class TypeWriter
    {
        private readonly MethodWriter _methodWriter = new();
        private readonly FieldWriter _fieldWriter = new();

        public void WriteType(TypeDef type, IndentedTextWriter headerWriter, IndentedTextWriter sourceWriter)
        {
            var mangledName = Naming.MangleName(type.FullName);

            bool isStatic = type.IsAbstract && type.IsSealed;
            if (!isStatic && !type.IsEnum)
            {
                if (!type.IsValueType)
                {
                    headerWriter.WriteLine("typedef struct {");
                    headerWriter.Indent++;
                    _methodWriter.WriteVirtualMethods(type, headerWriter);
                    headerWriter.Indent--;
                    headerWriter.WriteLine($"}} {mangledName}_vtable;");
                    headerWriter.WriteLine();
                }
          
                headerWriter.WriteLine("typedef struct {");
                headerWriter.Indent++;
                if(!type.IsValueType)
                    headerWriter.WriteLine($"{mangledName}_vtable* __vptr;");
                _fieldWriter.WriteInstanceFields(type, headerWriter);
                headerWriter.Indent--;
                headerWriter.WriteLine($"}} {mangledName};");
            }

            headerWriter.WriteLine();
            _fieldWriter.WriteStaticFields(type, headerWriter);
            headerWriter.WriteLine();

            _methodWriter.WriteMethods(type, headerWriter, sourceWriter);
            headerWriter.WriteLine();
        }

    }
}
