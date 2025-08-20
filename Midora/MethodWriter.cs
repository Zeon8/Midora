using Mono.Cecil;
using Mono.Cecil.Cil;
using System.CodeDom.Compiler;

namespace Midora;

public class MethodWriter
{
    private readonly TypeMetadata _typeMetadata;
    private readonly VirtualMethodWriter _virtualMethodWriter;
    private readonly Writers _writers;
    private readonly ILTranspiller _transpiller = new();

    public MethodWriter(TypeMetadata typeMetadata, Writers writers, VirtualMethodWriter virtualMethodWriter)
    {
        _writers = writers;
        _typeMetadata = typeMetadata;
        _virtualMethodWriter = virtualMethodWriter;
    }

    public void WriteMethods()
    {
        foreach (var method in _typeMetadata.Definition.Methods)
        {
            WriteMethod(method);

            bool isVirtualStructMethod = _typeMetadata.Definition.IsValueType && method.IsVirtual;
            if (isVirtualStructMethod)
                WriteBoxedMethod(method);
        }
    }

    public void WriteAllocator()
    {
        string functionName = $"{_typeMetadata.MangledName}* {_typeMetadata.AllocationFunctionName}";
        _writers.Header.WriteLine($"{functionName}();");
        
        _writers.Source.WriteLine($"{functionName}(){{");
        _writers.Source.Indent++;
        _virtualMethodWriter.WriteTypeHeaderInstance(_typeMetadata.MangledName);
        _writers.Source.WriteLine($"{_typeMetadata.MangledName}* obj = ({_typeMetadata.MangledName}*)DOTNET2C_ALLOC(sizeof({_typeMetadata.MangledName}));");
        _writers.Source.WriteLine($"obj->{Naming.TypeInfoFieldName} = &{Naming.TypeInfoVariableName};");
        _writers.Source.WriteLine("return obj;");
        _writers.Source.Indent--;
        _writers.Source.WriteLine('}');
    }

    public void DefineBoxFunction(TypeDefinition type, string mangledName, string mangledBoxName)
    {
        string boxFunctionName = Naming.GetBoxFunctionName(mangledName);
        string definition = $"{mangledBoxName}* {boxFunctionName}({mangledName}* value)";

        _writers.Header.WriteLine(definition + ';');
        _writers.Source.WriteLine(definition + " {");
        _writers.Source.Indent++;
        _virtualMethodWriter.WriteTypeHeaderInstance(mangledBoxName);
        _writers.Source.WriteLine($"{mangledBoxName}* box = ({mangledBoxName}*)DOTNET2C_ALLOC({mangledBoxName});");
        _writers.Source.WriteLine($"box->{Naming.TypeInfoFieldName} = &{Naming.TypeInfoVariableName};");
        _writers.Source.WriteLine($"box->{Naming.BoxedValueName} = *value;");
        _writers.Source.WriteLine($"return box;");
        _writers.Source.Indent--;
        _writers.Source.WriteLine("}");
        _writers.Source.WriteLine();
    }

    private void WriteMethod(MethodDefinition method)
    {
        WriteTop(_writers.Header, method);
        _writers.Header.WriteLine(';');

        if (!method.HasBody)
            return;

        MethodBody body = method.Body;

        WriteTop(_writers.Source, method);
        _writers.Source.WriteLine(" {");
        _writers.Source.Indent++;

        var context = new TranspileContext(method);
        foreach (VariableDefinition varible in body.Variables)
        {
            string name = Naming.GetDefaultLocal(varible.Index);
            context.Variables.Add(new LocalVariable(name, varible.VariableType));
        }

        foreach (var instruction in body.Instructions)
        {
            if (instruction.OpCode.OperandType == OperandType.InlineBrTarget
                || instruction.OpCode.OperandType == OperandType.InlineSwitch)
            {
                var gotoInstruction = (Instruction)instruction.Operand;
                context.GotoInstructions.Add(gotoInstruction);
            }
        }

        for (int i = 0; i < body.Instructions.Count; i++)
        {
            Instruction? instruction = body.Instructions[i];
            _transpiller.TranspileInstruction(instruction, context, _writers.Source);
        }

        _writers.Source.Indent--;
        _writers.Source.WriteLine('}');
        _writers.Source.WriteLine();
    }

    private void WriteTop(IndentedTextWriter writer, MethodDefinition method)
    {
        string methodName = Naming.MangleMethod(method);
        string returnType = Naming.MangleTypeReference(method.ReturnType);

        writer.Write($"{returnType} {methodName}(");
        if (method.HasThis)
        {
            var declaringTypeName = Naming.Mangle(method.DeclaringType.FullName);
            writer.Write($"{declaringTypeName}* _this");
            if (method.Parameters.Count > 0)
                writer.Write(',');
        }
        WriteParameters(writer, method);
        writer.Write(')');
    }

    private static void WriteParameters(IndentedTextWriter writer, MethodDefinition method)
    {
        var parameters = string.Join(',', method.Parameters.Select(parameter =>
        {
            string typeName = Naming.MangleTypeReference(parameter.ParameterType);
            return $"{typeName} {parameter.Name}";
        }));

        writer.Write(parameters);
    }

    private void WriteBoxedMethodTop(MethodDefinition method,
        string typeName, string methodName,
        string returnType, IndentedTextWriter writer)
    {
        writer.Write($"{returnType} {methodName}(");
        writer.Write($"{typeName}* _this");
        if (method.Parameters.Count > 0)
            writer.Write(',');
        WriteParameters(writer, method);
        writer.Write(')');
    }

    private void WriteBoxedMethod(MethodDefinition method)
    {
        string methodName = Naming.MangleMethod(method);
        string typeName = Naming.Mangle(method.DeclaringType.FullName);
        string returnType = Naming.MangleTypeReference(method.ReturnType);
        string boxName = Naming.GetBox(typeName);
        string boxedMethodName = Naming.GetBoxedMethod(methodName);

        WriteBoxedMethodTop(method, boxName, boxedMethodName, returnType, _writers.Header);
        _writers.Header.WriteLine(';');

        WriteBoxedMethodTop(method, boxName, boxedMethodName, returnType, _writers.Source);
        _writers.Source.WriteLine(" {");
        _writers.Source.Indent++;
        if (method.ReturnType.FullName != "System.Void")
            _writers.Source.Write("return ");
        _writers.Source.Write($"{methodName}(&_this->{Naming.BoxedValueName}");
        if (method.Parameters.Count > 0)
        {
            _writers.Source.Write(',');
            for (int i = 0; i < method.Parameters.Count; i++)
            {
                _writers.Source.Write(method.Parameters[i].Name);
                if (i != method.Parameters.Count - 1)
                    _writers.Source.Write(',');
            }
        }
        _writers.Source.WriteLine(");");
        _writers.Source.Indent--;
        _writers.Source.WriteLine("}");
        _writers.Source.WriteLine();
    }
}
