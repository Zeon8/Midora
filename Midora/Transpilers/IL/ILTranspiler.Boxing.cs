using Midora.Syntax.Expressions;
using Midora.Syntax.Instructions;
using Mono.Cecil;

namespace Midora.Transpilers;

public partial class ILTranspiler
{
    private NoInstruction TranspileBox(TypeReference type)
    {
        StackItem value = _context.Stack.Pop();
        var expression = new BoxExpression
        {
            TypeName = Naming.Mangle(type),
            ValueExpression = value.Expression,
        };
        _context.Stack.Push(new StackItem(TypeSystem.Object, expression));

        return s_noInstruction;
    }

    private NoInstruction TranspileUnboxAny(TypeReference type)
    {
        _context.Stack.Push(new StackItem(type, Unbox(type)));
        return s_noInstruction;
    }

    private NoInstruction TranspileUnbox(TypeReference type)
    {
        DereferenceExpression expression = new(Unbox(type));
        _context.Stack.Push(new StackItem(type, expression));
        return s_noInstruction;
    }

    private UnboxExpression Unbox(TypeReference type)
    {
        StackItem value = _context.Stack.Pop();

        return new UnboxExpression
        {
            TypeName = Naming.Mangle(type),
            BoxExpression = value.Expression,
        };
    }
}
