using Midora.Syntax.Expressions;
using Midora.Syntax.Instructions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Midora.Transpilers;
public partial class ILTranspiler
{
    private NoInstruction TranspileBinaryExpression(Operator @operator)
    {
        BinaryExpression expression = GetBinaryExpression(@operator);
        _context.Stack.Push(new StackItem(TypeSystem.Int32, expression));
        return s_noInstruction;
    }

    private BinaryExpression GetBinaryExpression(Operator @operator)
    {
        var rvalue = _context.Stack.Pop();
        var lvalue = _context.Stack.Pop();
        return new BinaryExpression(lvalue.Expression, rvalue.Expression, @operator);
    }

    private NoInstruction TranspileUnaryExpression(Operator @operator)
    {
        var value = _context.Stack.Pop();
        var expression = new UnaryExpression(value.Expression, @operator);
        _context.Stack.Push(new StackItem(TypeSystem.Int32, expression));
        return s_noInstruction;
    }
}
