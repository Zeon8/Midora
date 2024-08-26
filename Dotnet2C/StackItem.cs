using dnlib.DotNet;

namespace Dotnet2C
{
    public struct StackItem
    {
        public IType? Type { get; }

        public string Expression { get; }

        public StackItem(string expression, IType? type = null)
        {
            Expression = expression;
            Type = type;
        }
    }
}
