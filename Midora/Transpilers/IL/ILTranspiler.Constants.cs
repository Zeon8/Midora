using Midora.Syntax.Expressions;
using Midora.Syntax.Instructions;
using Mono.Cecil;

namespace Midora.Transpilers;

public partial class ILTranspiler
{
    private NoInstruction TranspileLdc<T>(T number, TypeReference type)
            where T : struct
    {
        _context.Stack.Push(new StackItem(type, new ConstantValueExpression(number.ToString()!)));
        return s_noInstruction;
    }

    private NoInstruction TranspileLdstr(string operand)
    {
        var name = _stringLiteralTranspiler.GetVariableName(operand);
        var expression = new StringConstantExpression(name);
        _context.Stack.Push(new StackItem(TypeSystem.String, expression));
        return s_noInstruction;
    }

    private NoInstruction TranspileLdnull()
    {
        var item = new StackItem(TypeSystem.Object, new NullExpression());
        _context.Stack.Push(item);

        return s_noInstruction;
    }
}
