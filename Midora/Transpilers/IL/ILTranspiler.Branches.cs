using Midora.Syntax.Expressions;
using Midora.Syntax.Instructions;
using Mono.Cecil.Cil;

namespace Midora.Transpilers;

public partial class ILTranspiler
{
    private static GotoInstruction TranspileBr(Instruction operand)
    {
        return new GotoInstruction(Naming.GetLabel(operand));
    }

    private GotoIfInstruction TranspileBr(Instruction operand, bool isTrue)
    {
        var item = _context.Stack.Pop();
        var zero = new ConstantValueExpression("0");
        var @operator = isTrue ? Operator.NotEquals : Operator.EqualsOp;
        var condition = new BinaryExpression(item.Expression, zero, @operator);
        return new GotoIfInstruction(condition, Naming.GetLabel(operand));
    }

    private GotoIfInstruction TranspileBranch(Instruction operand, Operator @operator)
    {
        BinaryExpression condition = GetBinaryExpression(@operator);
        return new GotoIfInstruction(condition, Naming.GetLabel(operand));
    }

    private SwitchInstruction TranspileSwitch(Instruction[] instructions)
    {
        var item = _context.Stack.Pop();
        string[] labels = instructions.Select(Naming.GetLabel).ToArray();
        return new SwitchInstruction(item.Expression, labels);
    }

    private IInstruction TranspileLeave(Instruction current, Instruction operand)
    {
        foreach (var handler in _context.ExceptionHandlers)
        {
            if (handler.HandlerType == ExceptionHandlerType.Finally
                && current == handler.TryEnd.Previous)
            {
                return new LeaveInstruction(Naming.GetLabel(handler.HandlerStart), true);
            }
        }
        return new LeaveInstruction(Naming.GetLabel(operand), true);
    }
}
