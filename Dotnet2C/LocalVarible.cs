using dnlib.DotNet;

namespace Dotnet2C
{
    public struct LocalVarible
    {
        public LocalVarible(string name, IType type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; set; }

        public IType Type { get; }

        public bool Defined { get; set; }
    }
}
