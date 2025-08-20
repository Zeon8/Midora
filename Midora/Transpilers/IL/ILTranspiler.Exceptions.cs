using Midora.Syntax.Expressions;
using Midora.Syntax.Instructions;
using Mono.Cecil.Cil;

namespace Midora.Transpilers;
public partial class ILTranspiler
{
    private EndFilterInstruction EndFinallyInstruction(Instruction current)
    {
        var value = _context.Stack.Pop();
        string variableName = GetFilterVariableName()!;
        return new EndFilterInstruction(variableName, value.Expression);

        string? GetFilterVariableName()
        {
            foreach (var handler in _context.ExceptionHandlers)
            {
                if (handler.HandlerStart.Previous == current)
                    return _context.GetOrCreateFilterVariable(handler);
            }
            return null;
        }
    }

    private IEnumerable<IInstruction> TranspileExceptionHandlers(List<ExceptionHandler> tryStartHandlers, List<ExceptionHandler> tryEndHandlers, Instruction instruction)
    {
        foreach (var handler in _context.ExceptionHandlers)
        {
            bool tryStartExists = tryStartHandlers.Any(h => h.TryStart == handler.TryStart && h.TryEnd == handler.TryEnd);
            if (instruction == handler.TryStart && !tryStartExists)
            {
                string variableName = _context.NewExceptionFrameVariable();
                yield return new TryBlockStartInstruction(variableName);
                tryStartHandlers.Add(handler);
            }

            bool tryEndExists = tryEndHandlers.Any(h => h.TryStart == handler.TryStart && h.TryEnd == handler.TryEnd);
            if (instruction == handler.TryEnd && !tryEndExists)
            {
                yield return new BlockEndInstruction();
                tryEndHandlers.Add(handler);
            }

            if (handler.FilterStart == instruction)
                _context.Stack.Push(new StackItem(handler.CatchType, new GetExceptionExpression()));

            if (handler.HandlerType == ExceptionHandlerType.Catch && instruction == handler.HandlerStart)
            {
                _context.Stack.Push(new StackItem(handler.CatchType, new GetExceptionExpression()));
                var exceptionName = Naming.Mangle(handler.CatchType);
                yield return new CatchBlockStart(exceptionName);
            }

            if (handler.HandlerType != ExceptionHandlerType.Finally && instruction == handler.HandlerEnd)
                yield return new BlockEndInstruction();

            if (handler.HandlerType == ExceptionHandlerType.Filter && instruction == handler.HandlerStart)
            {
                _context.Stack.Push(new StackItem(handler.CatchType, new GetExceptionExpression()));
                yield return new FilterStart(_context.GetOrCreateFilterVariable(handler));
            }
        }
    }
}
