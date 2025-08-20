using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Midora;

public class TranspileContext
{
    public required MethodReference Method { get; init; }

    public required IReadOnlyList<LocalVariable> Variables { get; init; }

    public required IEnumerable<Instruction> GotoInstructions { get; init; }

    public required IEnumerable<ExceptionHandler> ExceptionHandlers { get; init; }

    public Stack<StackItem> Stack { get; } = new();
    
    private readonly Dictionary<TypeReference, string> _tempVariables = new();
    private readonly Dictionary<ExceptionHandler, string> _filterVariables = new();

    private int _temporaryVaribles;
    private int _exceptionFrames;

    public string CreateTemporaryVariable() => $"__tmp_{_temporaryVaribles++}";

    public string NewExceptionFrameVariable() => $"__exception_frame_{_exceptionFrames++}";

    public string GetOrCreateFilterVariable(ExceptionHandler handler)
    {
        if(_filterVariables.TryGetValue(handler, out string? variable))
            return variable;

        variable = $"__filter_{_filterVariables.Count}";
        _filterVariables.Add(handler, variable);
        return variable;
    }
}
