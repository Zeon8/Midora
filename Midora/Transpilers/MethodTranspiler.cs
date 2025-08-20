using Midora.Metadata;
using Midora.Syntax;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Midora.Transpilers;

public class MethodTranspiler
{
    private readonly TypeMetadata _typeMetadata;
    private readonly FieldTranspiler _fieldTranspiler;
    private readonly StringLiteralTranspiler _stringLiteralTranspiler;

    public MethodTranspiler(TypeMetadata typeMetadata, FieldTranspiler fieldTranspiler, StringLiteralTranspiler stringLiteralTranspiler)
    {
        _typeMetadata = typeMetadata;
        _fieldTranspiler = fieldTranspiler;
        _stringLiteralTranspiler = stringLiteralTranspiler;
    }

    public IEnumerable<TranspiledMethod> DeclareMethods()
    {
        foreach (var method in _typeMetadata.Definition.Methods)
        {
            if (method.HasGenericParameters)
                continue;

            if (method.IsPInvokeImpl)
                yield return DeclareUnmanagedMethod(method);

            yield return DeclareManagedMethod(method);
        }
    }


    private ManagedMethod DeclareManagedMethod(MethodReference method, Dictionary<GenericParameter, TypeReference>? genericArguments = null)
    {
        var definition = method.Resolve();

        MethodBodyDeclaration? bodyDeclaration = null;
        if (definition.HasBody)
        {
            MethodBody body = definition.Body;

            List<Instruction> gotoInstructions = new();
            foreach (var instruction in body.Instructions)
            {
                var flowConrol = instruction.OpCode.FlowControl;
                if (flowConrol == FlowControl.Branch || flowConrol == FlowControl.Cond_Branch)
                    gotoInstructions.Add((Instruction)instruction.Operand);
                if(instruction.OpCode.Code == Code.Switch)
                    gotoInstructions.AddRange((Instruction[])instruction.Operand);
            }
            foreach(var handler in body.ExceptionHandlers)
            {
                if (handler.HandlerType == ExceptionHandlerType.Finally)
                    gotoInstructions.Add(handler.HandlerStart);
            }

            var variables = new List<LocalVariable>();
            foreach(var variable in body.Variables)
            {
                string name = Naming.GetDefaultLocal(variable.Index);
                variables.Add(new LocalVariable(name, variable.VariableType));
            }

            var context = new TranspileContext()
            {
                Method = method,
                Variables = variables,
                GotoInstructions = gotoInstructions,
                ExceptionHandlers = body.ExceptionHandlers,
            };

            var transpiler = new ILTranspiler(context, _stringLiteralTranspiler);

            bodyDeclaration = new MethodBodyDeclaration
            {
                Variables = GetVariables(definition.Body, context),
                StackFrame = GetStackFrame(definition.Body),
                Instructions = transpiler.TranspileInstructions(body.Instructions).ToArray(),
            };
        }

        return new ManagedMethod
        {
            Name = Naming.Mangle(method, true),
            DeclarationType = GetDeclaringTypeName(definition),
            Parameters = GetParameters(method),
            ReturnType = Naming.MangleReference(method.ReturnType),
            Inline = definition.AggressiveInlining,
            Body = bodyDeclaration,
        };
    }

    private IEnumerable<VariableDeclaration> GetVariables(MethodBody methodBody, TranspileContext context)
    {
        foreach (var variable in methodBody.Variables)
        {
            bool isValueType = variable.VariableType.Resolve().IsValueType;
            string type = Naming.MangleReference(variable.VariableType);
            string name = Naming.GetDefaultLocal(variable.Index);
            yield return new VariableDeclaration(type, name, isValueType);
        }

        if (methodBody.HasExceptionHandlers)
        {
            yield return new VariableDeclaration(
                Type: "bool",
                Name: Naming.ExceptionHandledVariable,
                IsValueType: true);
        }

        foreach(var exceptionHandler in methodBody.ExceptionHandlers)
        {
            if (exceptionHandler.HandlerType == ExceptionHandlerType.Filter)
            {
                string name = context.GetOrCreateFilterVariable(exceptionHandler);
                yield return new VariableDeclaration(Type: "bool", name, IsValueType: true);
            }
        }
    }

    private StackFrame GetStackFrame(MethodBody body)
    {
        var roots = new List<Root>();
        foreach (var variable in body.Variables)
        {
            var typeDefinition = variable.VariableType.Resolve();

            if (typeDefinition.IsByReference
                || variable.VariableType.IsPointer 
                || (typeDefinition.IsPrimitive && !variable.VariableType.IsArray))
            {
                continue;
            }

            string name = Naming.GetDefaultLocal(variable.Index);

            if (!typeDefinition.IsValueType || variable.VariableType.IsArray)
                roots.Add(new Root(name, []));
            else
            {
                IEnumerable<IEnumerable<FieldOffset>> offsetsList = _fieldTranspiler.ResolveReferenceTypeFieldOffsets(typeDefinition);
                foreach (var offsets in offsetsList)
                {
                    roots.Add(new Root(name, offsets));
                }
            }
        }
        return new StackFrame(roots);
    }
    private UnmanagedMethod DeclareUnmanagedMethod(MethodDefinition method)
    {
        string entryPoint = method.PInvokeInfo.EntryPoint ?? method.Name;

        return new UnmanagedMethod
        {
            Name = Naming.Mangle(method, true),
            DeclarationType = GetDeclaringTypeName(method),
            Parameters = GetParameters(method),
            ReturnType = Naming.MangleReference(method.ReturnType),
            EntryPoint = entryPoint,
        };
    }

    private string? GetDeclaringTypeName(MethodDefinition method)
    {
        if (method.IsStatic)
            return null;
        if (method.DeclaringType.IsValueType)
            return _typeMetadata.MangledName;
        return Naming.RuntimeObjectName;
    }

    private IEnumerable<MethodParameter> GetParameters(MethodReference method)
    {
        return method.Parameters
            .Select(p => new MethodParameter(p.Name, Naming.MangleReference(p.ParameterType)))
            .ToArray();
    }
}