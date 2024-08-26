using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Reflection;

namespace Dotnet2C
{
    public class TranspileContext
    {
        public Stack<StackItem> Stack { get; } = new();

        public List<LocalVarible> Varibles { get; } = new(); 
        public List<Instruction> GotoInstructions { get; } = new(); 

        public MethodDef Method { get; }

        public TranspileContext(MethodDef method)
        {
            Method = method;
        }
    }
}
