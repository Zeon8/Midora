using Midora.Syntax.Expressions;
using Midora.Syntax.Instructions;
using Mono.Cecil;

namespace Midora.Transpilers;
public partial class ILTranspiler
{
    private AssignInstruction TranspileStfld(FieldReference field)
    {
        var value = _context.Stack.Pop();
        return new AssignInstruction(GetFieldExpression(field), value.Expression);
    }

    private NoInstruction TranspileLdfld(FieldReference field)
    {
        _context.Stack.Push(new StackItem(field.FieldType, GetFieldExpression(field)));
        return s_noInstruction;
    }

    private AssignInstruction TranspileStsfld(FieldReference field)
    {
        var item = _context.Stack.Pop();
        return new AssignInstruction(GetStaticFieldExpression(field), item.Expression);
    }

    private NoInstruction TranspileLdsfld(FieldReference field)
    {
        var item = new StackItem(field.FieldType, GetStaticFieldExpression(field));
        _context.Stack.Push(item);
        return s_noInstruction;
    }

    private NoInstruction TranspileLdflda(FieldReference field)
    {
        var expresion = new DereferenceExpression(GetFieldExpression(field));
        _context.Stack.Push(new StackItem(field.FieldType, expresion));
        return s_noInstruction;
    }

    private NoInstruction TranspileLdsflda(FieldReference field)
    {
        var expresion = new DereferenceExpression(GetStaticFieldExpression(field));
        _context.Stack.Push(new StackItem(field.FieldType, expresion));
        return s_noInstruction;
    }

    private FieldExpression GetFieldExpression(FieldReference field)
    {
        var obj = _context.Stack.Pop();
        return new FieldExpression
        {
            DeclrationType = Naming.Mangle(field.DeclaringType),
            FieldName = Naming.Mangle(field, includeTypeName: false),
            ThisExpression = obj.Expression,
            IsValueType = field.DeclaringType.IsValueType,
            IsByReference = obj.Type.IsByReference,
        };
    }

    private static StaticFieldExpression GetStaticFieldExpression(FieldReference field)
    {
        string declaringTypeName = Naming.Mangle(field.DeclaringType);
        string fieldName = Naming.Mangle(field, includeTypeName: false);
        return new StaticFieldExpression(declaringTypeName, fieldName);
    }
}
