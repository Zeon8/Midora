namespace Midora.Syntax;

public record StaticFieldDeclaration(string Type, string Name, IReadOnlyCollection<byte> InitValue)
{
    public void Write(Writers writers)
    {
        string fieldDeclaration = $"{Type} {Name}";
        writers.Header.WriteLine($"extern {fieldDeclaration};");
        writers.Source.WriteLine($"{fieldDeclaration} = {{0}};");

        if (InitValue is not null)
        {
            var bytes = string.Join(',', InitValue.Select(v => $"0x{v:X}"));
            writers.Source.WriteLine(@$"FieldInitialValue {Naming.GetInitValueFieldName(Name)} = {{{InitValue.Count}, {{{bytes}}} }};");
        }
    }
}
