using Mono.Cecil;

namespace Midora;

public class LocalVariable
{
    public string Name { get; }

    public TypeReference Type { get; }
    
    public LocalVariable(string name, TypeReference type)
    {
        Name = name;
        Type = type;
    }
}
