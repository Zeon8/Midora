using Mono.Cecil;

namespace Midora;
public class GenericContext
{
    private readonly IReadOnlyDictionary<GenericParameter, TypeReference> _arguments;

    public GenericContext(IReadOnlyDictionary<GenericParameter, TypeReference> arguments)
    {
        _arguments = arguments;
    }

    public TypeReference GetArgument(GenericParameter parameter) => _arguments[parameter];
}
