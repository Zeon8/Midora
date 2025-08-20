using Midora.Metadata;
using Midora.Syntax.Expressions;
using Midora.Syntax.Instructions;
using Mono.Cecil;

namespace Midora.Transpilers;

public partial class ILTranspiler
{
    private IEnumerable<IExpression> GetArguments(MethodReference method)
    {
        for (int i = 0; i < method.Parameters.Count; i++)
        {
            StackItem item = _context.Stack.Pop();
            yield return item.Expression;
        }
    }

    private IInstruction WriteOrPushMethodCall(MethodReference method, IExpression expression)
    {
        if (method.ReturnType == TypeSystem.Void)
            return new WriteExpressionInstruction(expression);

        StackItem returnType = new(method.ReturnType, expression);
        _context.Stack.Push(returnType);
        return s_noInstruction;
    }


    private IInstruction TranspileCall(MethodReference method)
    {
        var args = GetArguments(method).ToList();
        if (method.HasThis)
        {
            StackItem item = _context.Stack.Pop();
            args.Add(item.Expression);
        }
        args.Reverse();

        var callExpression = new CallStaticMethodExpression
        {
            Name = Naming.Mangle(method),
            Arguments = new MethodArgumentsExpression(args),
        };

        return WriteOrPushMethodCall(method, callExpression);
    }

    private IInstruction TranspileCallvirt(MethodReference method)
    {
        List<IExpression> args = GetArguments(method).ToList();
        args.Reverse();

        StackItem obj = _context.Stack.Pop();
        var argumentsExpression = new MethodArgumentsExpression(obj.Expression, args);

        MethodDefinition methodDefinition = method.Resolve();
        TypeDefinition typeDefinition = obj.Type.Resolve();

        IExpression expression;
        if (typeDefinition.IsValueType)
        {
            expression = CallConstrained(typeDefinition, method, argumentsExpression);
        }
        else if (!methodDefinition.IsVirtual)
        {
            expression = new CallStaticMethodExpression
            {
                Name = Naming.Mangle(method),
                Arguments = argumentsExpression,
            };
        }
        else
        {
            var type = new TypeMetadata(method.DeclaringType);
            string methodName = Naming.Mangle(method, withTypeName: false);
            if (type.Definition.IsInterface)
            {
                expression = new CallMethodInstruction(new GetInterfaceMethod
                {
                    MethodName = methodName,
                    ObjectExpression = obj.Expression,
                    VTableName = type.VtableName,
                    InterfaceType = type.MangledName,
                }, argumentsExpression);
            }
            else
            {
                expression = new CallMethodInstruction(new GetVirtualMethod
                {
                    MethodName = methodName,
                    ObjectExpression = obj.Expression,
                    VTableName = type.VtableName,
                }, argumentsExpression);
            }
        }

        return WriteOrPushMethodCall(method, expression);
    }

    private NewObjectInstruction TranspileNewObj(MethodReference constructor)
    {
        TypeReference declaringType = constructor.DeclaringType;
        string tempVaribleName = _context.CreateTemporaryVariable();

        List<IExpression> args = GetArguments(constructor).ToList();
        args.Add(new VariableExpression(tempVaribleName));
        args.Reverse();

        _context.Stack.Push(new StackItem(constructor.DeclaringType, new VariableExpression(tempVaribleName)));

        return new NewObjectInstruction
        {
            TypeName = Naming.Mangle(declaringType),
            TempVaribleName = tempVaribleName,
            ConstructorCall = new CallStaticMethodExpression
            {
                Name = Naming.Mangle(constructor),
                Arguments = new MethodArgumentsExpression(args),
            }
        };
    }

    private CallStaticMethodExpression CallConstrained(TypeDefinition type, MethodReference method, MethodArgumentsExpression arguments)
    {
        MethodReference callMethod = type.Methods.FirstOrDefault(m => m.Name == method.Name)
            ?? type.BaseType.Resolve().Methods.First(m => m.Name == method.Name);

        var thisExpression = new CastExpression(arguments.ThisExpression!, Naming.Mangle(callMethod.DeclaringType) + '*');
        
        return new CallStaticMethodExpression
        {
            Name = Naming.Mangle(callMethod),
            Arguments = new MethodArgumentsExpression(thisExpression, arguments.Arguments),
        };
    }
}
