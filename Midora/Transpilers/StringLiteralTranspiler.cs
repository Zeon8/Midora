using Midora.Syntax;

namespace Midora.Transpilers;

public class StringLiteralTranspiler
{
    private readonly List<string> _literals = new();

    private string GetName(int index) => $"__string_{index}";

    public string GetVariableName(string literal)
    {
        var index = _literals.IndexOf(literal);
        if (index != -1)
            return GetName(index);

        _literals.Add(literal);
        return GetName(_literals.Count - 1);
    }

    public IEnumerable<StringFieldDeclaration> DeclareStringFields()
    {
        for (int i = 0; i < _literals.Count; i++)
        {
            var literal = _literals[i];
            yield return new StringFieldDeclaration(GetName(i), literal);
        }
    }
}