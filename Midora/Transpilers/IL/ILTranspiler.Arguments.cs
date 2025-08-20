using Midora.Syntax.Expressions;
using Midora.Syntax.Instructions;
using Mono.Cecil;
using System.Diagnostics;

namespace Midora.Transpilers;

public partial class ILTranspiler
{
    private NoInstruction TranspileLdarg(ParameterReference argument)
    {
        var expression = new ArgumentExpression(argument.Name);
        _context.Stack.Push(new StackItem(argument.ParameterType, expression));
        return s_noInstruction;
    }

    private NoInstruction TranspileLdarga(ParameterReference argument)
    {
        var parameterType = argument.ParameterType;
        var sign = new ByReferenceType(parameterType);
        var expression = new TakeAddressExpression(new ArgumentExpression(argument.Name));
        _context.Stack.Push(new StackItem(sign, expression));
        return s_noInstruction;
    }

    private NoInstruction TranspileLdarg(ushort argumentIndex)
    {
        if (_context.Method.HasThis && argumentIndex == 0)
        {
            TypeReference declaringType = _context.Method.DeclaringType;
            if (declaringType.Resolve().IsValueType)
                declaringType = new ByReferenceType(declaringType);
            _context.Stack.Push(new StackItem(declaringType, new ThisExpression()));
        }
        else
        {
            if (_context.Method.HasThis && !_context.Method.ExplicitThis)
                argumentIndex--;

            ParameterReference parameter = _context.Method.Parameters[argumentIndex];
            string argumentName = parameter.Name;
            Debug.Assert(argumentName is not null);

            _context.Stack.Push(new StackItem(parameter.ParameterType, new ArgumentExpression(argumentName)));
        }
        return s_noInstruction;
    }

    private AssignInstruction TranspileStarg(ParameterReference parameter)
    {
        var argumentExpression = new ArgumentExpression(parameter.Name);
        var item = _context.Stack.Pop();
        return new AssignInstruction(argumentExpression, item.Expression);
    }
}
