namespace Midora.Syntax.Expressions;

public record Operator
{
    public string Value { get; }

    private Operator(string value) => Value = value;

    public static Operator Add { get; } = new("+");
    public static Operator Substitute { get; } = new("-");
    public static Operator Divide { get; } = new("/");
    public static Operator Multiply { get; } = new("*");
    public static Operator Remainder { get; } = new("%");
    public static Operator EqualsOp { get; } = new("==");
    public static Operator NotEquals { get; } = new("!=");
    public static Operator GreaterThen { get; } = new(">");
    public static Operator GreaterEquals { get; } = new(">=");
    public static Operator LessThen { get; } = new("<");
    public static Operator LessEquals { get; } = new("<=");
    public static Operator And { get; } = new("&");
    public static Operator Or { get; } = new("|");
    public static Operator Not { get; } = new("~");
    public static Operator Neg { get; } = new("-");
    public static Operator ShiftLeft { get; } = new("<<");
    public static Operator ShiftRight { get; } = new(">>");
    public static Operator Xor { get; } = new("^");
}

