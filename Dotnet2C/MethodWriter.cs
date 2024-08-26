using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.CodeDom.Compiler;
using System.Net.Sockets;

namespace Dotnet2C
{
    public class MethodWriter
    {
        private ILTranspiller _transpiller = new();

        public void WriteMethods(TypeDef type, IndentedTextWriter headerWriter, IndentedTextWriter sourceWriter)
        {
            foreach (var method in type.Methods)
                WriteMethod(method, headerWriter, sourceWriter);
        }

        public void WriteVirtualMethods(TypeDef type, IndentedTextWriter writer)
        {
            foreach (var method in type.Methods)
            {
                if(method.IsVirtual)
                    WriteVirtualMethod(method, writer);
            }
        }

        private void WriteMethod(MethodDef method, IndentedTextWriter headerWriter, IndentedTextWriter sourceWriter)
        {
            if (!method.IsPrivate)
            {
                WriteTop(headerWriter, method);
                headerWriter.WriteLine(';');
            }

            CilBody body = method.Body;
            if(!body.HasInstructions)
                return;

            WriteTop(sourceWriter, method);
            sourceWriter.WriteLine(" {");
            sourceWriter.Indent++;

            var context = new TranspileContext(method);
            foreach (Local varible in body.Variables)
            {
                context.Varibles.Add(new LocalVarible(varible.Name, varible.Type));
            }

            foreach (var instruction in body.Instructions)
            {
                if (instruction.IsBr() || instruction.IsConditionalBranch())
                {
                    var gotoInstruction = (Instruction)instruction.Operand;
                    context.GotoInstructions.Add(gotoInstruction);
                }
            }

            for (int i = 0; i < body.Instructions.Count; i++)
            {
                Instruction? instruction = body.Instructions[i];

                // Skip empty return in the end of the void method.
                //if(i < body.Instructions.Count-1 || method.HasReturnType)
                _transpiller.TranspileInstruction(instruction, context, sourceWriter);
            }

            sourceWriter.Indent--;
            sourceWriter.WriteLine('}');
            sourceWriter.WriteLine();
        }

        private void WriteTop(IndentedTextWriter writer, MethodDef method)
        {
            var methodName = Naming.MangleMemberName(method);
            var returnType = Naming.MangleTypeReferenceName(method.ReturnType);

            writer.Write($"{returnType} {methodName}(");

            for (int i = 0; i < method.Parameters.Count; i++)
            {
                Parameter? param = method.Parameters[i];

                var paramTypeName = Naming.MangleTypeReferenceName(param.Type);

                string paramName = param.Name;
                if (param.IsHiddenThisParameter)
                    paramName = "_this";

                writer.Write($"{paramTypeName} {paramName}");
                if (i < method.Parameters.Count - 1)
                    writer.Write(", ");
            }
            writer.Write(')');
        }

        private void WriteVirtualMethod(MethodDef method, IndentedTextWriter writer)
        {
            var methodName = Naming.MangleName(method.Name);
            var returnType = Naming.MangleTypeReferenceName(method.ReturnType);

            writer.Write($"{returnType} (*{methodName})(");

            for (int i = 0; i < method.Parameters.Count; i++)
            {
                Parameter? param = method.Parameters[i];
                var paramTypeName = Naming.MangleTypeReferenceName(param.Type);

                writer.Write($"{paramTypeName}");
                if (i < method.Parameters.Count - 1)
                    writer.Write(", ");
            }
            writer.WriteLine(");");
        }
    }
}
