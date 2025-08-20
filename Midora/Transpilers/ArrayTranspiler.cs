using Midora.Syntax;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Midora.Transpilers;

public class ArrayTranspiler
{
    public IEnumerable<ArrayTypeInfo> Transpile(ModuleDefinition module)
    {
        HashSet<TypeReference> elementTypes = new();
        foreach (var type in module.Types)
        {
            foreach (var method in type.Methods)
            {
                if (!method.HasBody)
                    continue;

                foreach (Instruction instruction in method.Body.Instructions)
                {
                    if (instruction.OpCode.Code == Code.Newarr)
                    {
                        var arrayType = (TypeReference)instruction.Operand;
                        if(!elementTypes.Contains(arrayType))
                            elementTypes.Add(arrayType);
                    }
                }
            }
        }

        foreach (var type in elementTypes)
        {
            var elementTypeName = Naming.Mangle(type.GetElementType());
            yield return new ArrayTypeInfo(elementTypeName);
        }
    }
}
