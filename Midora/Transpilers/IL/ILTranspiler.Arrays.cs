using Midora.Syntax.Expressions;
using Midora.Syntax.Instructions;
using Mono.Cecil;

namespace Midora.Transpilers;

public partial class ILTranspiler
{
    private NewArrayInstruction TranspileNewarr(TypeReference type)
    {
        var item = _context.Stack.Pop();
        var tempVariable = _context.CreateTemporaryVariable();
        _context.Stack.Push(new StackItem(new ArrayType(type), new VariableExpression(tempVariable)));
        
        return new NewArrayInstruction
        {
            ElementTypeName = Naming.Mangle(type),
            LengthExpression = item.Expression,
            TempVaribleName = tempVariable,
        };
    }

    private NoInstruction TranspileLdelem(TypeReference type)
    {
        var expression = GetElementExpression(type);
        _context.Stack.Push(new StackItem(type, expression));
        return s_noInstruction;
    }

    private NoInstruction TranspileLdelema(TypeReference type)
    {
        var index = _context.Stack.Pop();
        var arr = _context.Stack.Pop();
        var item = new StackItem(type, new ElementAddressExpression
        {
            Array = arr.Expression,
            Index = index.Expression,
            Type = Naming.MangleReference(type),
        });
        _context.Stack.Push(item);
        return s_noInstruction;
    }

    private AssignInstruction TranspileStelem(TypeReference type)
    {
        var value = _context.Stack.Pop();
        var expression = GetElementExpression(type);
        return new AssignInstruction(expression, value.Expression);
    }

    private ElementExpression GetElementExpression(TypeReference type)
    {
        var index = _context.Stack.Pop();
        var arr = _context.Stack.Pop();

        return new ElementExpression
        {
            Array = arr.Expression,
            Index = index.Expression,
            Type = Naming.MangleReference(type),
        };
    }

    private NoInstruction TranspileLen()
    {
        var obj = _context.Stack.Pop();
        var item = new StackItem(TypeSystem.Int32, new LengthExpression(obj.Expression));
        _context.Stack.Push(item);
        return s_noInstruction;
    }
}
