using Midora.Helpers;
using Midora.Metadata;
using Midora.Syntax.Expressions;
using Midora.Syntax.Instructions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Diagnostics;

namespace Midora.Transpilers;

public partial class ILTranspiler
{
    private TypeSystem TypeSystem => _context.Method.Module.TypeSystem;

    private readonly TranspileContext _context;
    private readonly StringLiteralTranspiler _stringLiteralTranspiler;

    private static readonly NoInstruction s_noInstruction = new();

    public ILTranspiler(TranspileContext transpileContext, 
        StringLiteralTranspiler stringLiteralTranspiler)
    {
        _context = transpileContext;
        _stringLiteralTranspiler = stringLiteralTranspiler;
    }

    public IEnumerable<IInstruction> TranspileInstructions(IEnumerable<Instruction> instructions)
    {
        List<ExceptionHandler> tryStartHandlers = new();
        List<ExceptionHandler> tryEndHandlers = new();

        foreach (var instruction in instructions)
        {
            foreach (var handlerInstruction in TranspileExceptionHandlers(tryStartHandlers, tryEndHandlers, instruction))
                yield return handlerInstruction;

            IInstruction trasnpiledInstruction = TranspileInstruction(instruction);
            if (trasnpiledInstruction is not NoInstruction)
                yield return trasnpiledInstruction;
        }
    }

    private IInstruction TranspileInstruction(Instruction instruction)
    {
        IInstruction transpiledInstruction = instruction.OpCode.Code switch
        {
            Code.Nop => s_noInstruction,
            Code.Readonly => s_noInstruction,
            Code.Initobj => TranspileInitObj((TypeReference)instruction.Operand),
            Code.Constrained => s_noInstruction,

            Code.Ret => TranspileRet(instruction),
            Code.Dup => TranspileDup(),
            Code.Pop => TranspilePop(),

            Code.Ldarg => TranspileLdarg((ParameterReference)instruction.Operand),
            Code.Ldarg_S => TranspileLdarg((ParameterReference)instruction.Operand),
            Code.Ldarg_0 => TranspileLdarg(0),
            Code.Ldarg_1 => TranspileLdarg(1),
            Code.Ldarg_2 => TranspileLdarg(2),
            Code.Ldarg_3 => TranspileLdarg(3),

            Code.Ldarga => TranspileLdarga((ParameterReference)instruction.Operand),
            Code.Ldarga_S => TranspileLdarga((ParameterReference)instruction.Operand),
            Code.Starg => TranspileStarg((ParameterReference)instruction.Operand),
            Code.Starg_S => TranspileStarg((ParameterReference)instruction.Operand),

            Code.Ldc_I4 => TranspileLdc((int)instruction.Operand, TypeSystem.Int32),
            Code.Ldc_I4_S => TranspileLdc((sbyte)instruction.Operand, TypeSystem.Int32),
            Code.Ldc_I8 => TranspileLdc((long)instruction.Operand, TypeSystem.Int64),
            Code.Ldc_R4 => TranspileLdc((float)instruction.Operand, TypeSystem.Single),
            Code.Ldc_R8 => TranspileLdc((double)instruction.Operand, TypeSystem.Double),

            Code.Ldc_I4_0 => TranspileLdc(0, TypeSystem.Int32),
            Code.Ldc_I4_1 => TranspileLdc(1, TypeSystem.Int32),
            Code.Ldc_I4_2 => TranspileLdc(2, TypeSystem.Int32),
            Code.Ldc_I4_3 => TranspileLdc(3, TypeSystem.Int32),
            Code.Ldc_I4_4 => TranspileLdc(4, TypeSystem.Int32),
            Code.Ldc_I4_5 => TranspileLdc(5, TypeSystem.Int32),
            Code.Ldc_I4_6 => TranspileLdc(6, TypeSystem.Int32),
            Code.Ldc_I4_7 => TranspileLdc(7, TypeSystem.Int32),
            Code.Ldc_I4_8 => TranspileLdc(8, TypeSystem.Int32),
            Code.Ldc_I4_M1 => TranspileLdc(-1, TypeSystem.Int32),

            Code.Ldnull => TranspileLdnull(),

            Code.Ldind_I => TranspileLdind(),
            Code.Ldind_I1 => TranspileLdind(),
            Code.Ldind_I2 => TranspileLdind(),
            Code.Ldind_I4 => TranspileLdind(),
            Code.Ldind_I8 => TranspileLdind(),
            Code.Ldind_R4 => TranspileLdind(),
            Code.Ldind_R8 => TranspileLdind(),
            Code.Ldind_U1 => TranspileLdind(),
            Code.Ldind_U2 => TranspileLdind(),
            Code.Ldind_U4 => TranspileLdind(),
            Code.Ldind_Ref => TranspileLdind(),

            Code.Stind_I => TranspileStind(),
            Code.Stind_I1 => TranspileStind(),
            Code.Stind_I2 => TranspileStind(),
            Code.Stind_I4 => TranspileStind(),
            Code.Stind_I8 => TranspileStind(),
            Code.Stind_R4 => TranspileStind(),
            Code.Stind_R8 => TranspileStind(),
            Code.Stind_Ref => TranspileStind(),

            Code.Ldloc => TranspileLdloc((VariableReference)instruction.Operand),
            Code.Ldloc_S => TranspileLdloc((VariableReference)instruction.Operand),
            Code.Ldloc_0 => TranspileLdloc(0),
            Code.Ldloc_1 => TranspileLdloc(1),
            Code.Ldloc_2 => TranspileLdloc(2),
            Code.Ldloc_3 => TranspileLdloc(3),
            Code.Ldloca => TranspileLdloca((VariableReference)instruction.Operand),
            Code.Ldloca_S => TranspileLdloca((VariableReference)instruction.Operand),

            Code.Stloc => TranspileStloc((VariableReference)instruction.Operand),
            Code.Stloc_S => TranspileStloc((VariableReference)instruction.Operand),
            Code.Stloc_0 => TranspileStloc(0),
            Code.Stloc_1 => TranspileStloc(1),
            Code.Stloc_2 => TranspileStloc(2),
            Code.Stloc_3 => TranspileStloc(3),

            Code.Ldstr => TranspileLdstr((string)instruction.Operand),
            Code.Ldobj => TranspileLdind(),
            Code.Stobj => TranspileStind(),

            Code.Newobj => TranspileNewObj((MethodReference)instruction.Operand),

            Code.Call => TranspileCall((MethodReference)instruction.Operand),
            Code.Callvirt => TranspileCallvirt((MethodReference)instruction.Operand),
            Code.Calli => throw new NotImplementedException(),

            Code.Box => TranspileBox((TypeReference)instruction.Operand),
            Code.Unbox => TranspileUnbox((TypeReference)instruction.Operand),
            Code.Unbox_Any => TranspileUnboxAny((TypeReference)instruction.Operand),

            Code.Ldfld => TranspileLdfld((FieldReference)instruction.Operand),
            Code.Ldsfld => TranspileLdsfld((FieldReference)instruction.Operand),
            Code.Ldflda => TranspileLdflda((FieldReference)instruction.Operand),
            Code.Ldsflda => TranspileLdsflda((FieldReference)instruction.Operand),
            Code.Stfld => TranspileStfld((FieldReference)instruction.Operand),
            Code.Stsfld => TranspileStsfld((FieldReference)instruction.Operand),

            Code.Newarr => TranspileNewarr((TypeReference)instruction.Operand),

            Code.Ldelem_I => TranspileLdelem(TypeSystem.IntPtr),
            Code.Ldelem_I1 => TranspileLdelem(TypeSystem.SByte),
            Code.Ldelem_I2 => TranspileLdelem(TypeSystem.Int16),
            Code.Ldelem_I4 => TranspileLdelem(TypeSystem.Int32),
            Code.Ldelem_I8 => TranspileLdelem(TypeSystem.Int64),
            Code.Ldelem_R4 => TranspileLdelem(TypeSystem.Single),
            Code.Ldelem_R8 => TranspileLdelem(TypeSystem.Double),
            Code.Ldelem_U1 => TranspileLdelem(TypeSystem.Byte),
            Code.Ldelem_U2 => TranspileLdelem(TypeSystem.UInt16),
            Code.Ldelem_U4 => TranspileLdelem(TypeSystem.UInt32),
            Code.Ldelem_Ref => TranspileLdelem(TypeSystem.Object),
            Code.Ldelem_Any => TranspileLdelem((TypeReference)instruction.Operand),
            Code.Ldelema => TranspileLdelema((TypeReference)instruction.Operand),

            Code.Stelem_I => TranspileStelem(TypeSystem.IntPtr),
            Code.Stelem_I2 => TranspileStelem(TypeSystem.Int16),
            Code.Stelem_I1 => TranspileStelem(TypeSystem.SByte),
            Code.Stelem_I4 => TranspileStelem(TypeSystem.Int32),
            Code.Stelem_I8 => TranspileStelem(TypeSystem.Int64),
            Code.Stelem_R4 => TranspileStelem(TypeSystem.Single),
            Code.Stelem_R8 => TranspileStelem(TypeSystem.Double),
            Code.Stelem_Ref => TranspileStelem(TypeSystem.Object),
            Code.Stelem_Any => TranspileStelem((TypeReference)instruction.Operand),

            Code.Ldlen => TranspileLen(),

            Code.Endfinally => new EndFinallyInstruction(),
            Code.Rethrow => new RethrowInstruction(),
            Code.Throw => new ThrowInstruction(_context.Stack.Pop().Expression),
            Code.Endfilter => EndFinallyInstruction(instruction),

            Code.Break => new BreakInstruction(),
            Code.Jmp => throw new NotImplementedException(),
            Code.Br => TranspileBr((Instruction)instruction.Operand),
            Code.Br_S => TranspileBr((Instruction)instruction.Operand),
            Code.Brtrue => TranspileBr((Instruction)instruction.Operand, true),
            Code.Brtrue_S => TranspileBr((Instruction)instruction.Operand, true),
            Code.Brfalse => TranspileBr((Instruction)instruction.Operand, false),
            Code.Brfalse_S => TranspileBr((Instruction)instruction.Operand, false),
            Code.Beq => TranspileBranch((Instruction)instruction.Operand, Operator.EqualsOp),
            Code.Beq_S => TranspileBranch((Instruction)instruction.Operand, Operator.EqualsOp),
            Code.Bge => TranspileBranch((Instruction)instruction.Operand, Operator.GreaterEquals),
            Code.Bge_S => TranspileBranch((Instruction)instruction.Operand, Operator.GreaterEquals),
            Code.Bge_Un => TranspileBranch((Instruction)instruction.Operand, Operator.GreaterEquals),
            Code.Bge_Un_S => TranspileBranch((Instruction)instruction.Operand, Operator.GreaterEquals),
            Code.Bgt => TranspileBranch((Instruction)instruction.Operand, Operator.GreaterThen),
            Code.Bgt_S => TranspileBranch((Instruction)instruction.Operand, Operator.GreaterThen),
            Code.Bgt_Un => TranspileBranch((Instruction)instruction.Operand, Operator.GreaterThen),
            Code.Bgt_Un_S => TranspileBranch((Instruction)instruction.Operand, Operator.GreaterThen),
            Code.Ble => TranspileBranch((Instruction)instruction.Operand, Operator.LessEquals),
            Code.Ble_S => TranspileBranch((Instruction)instruction.Operand, Operator.LessEquals),
            Code.Ble_Un => TranspileBranch((Instruction)instruction.Operand, Operator.LessEquals),
            Code.Ble_Un_S => TranspileBranch((Instruction)instruction.Operand, Operator.LessEquals),
            Code.Blt => TranspileBranch((Instruction)instruction.Operand, Operator.LessThen),
            Code.Blt_S => TranspileBranch((Instruction)instruction.Operand, Operator.LessThen),
            Code.Blt_Un => TranspileBranch((Instruction)instruction.Operand, Operator.LessThen),
            Code.Blt_Un_S => TranspileBranch((Instruction)instruction.Operand, Operator.LessThen),
            Code.Bne_Un => TranspileBranch((Instruction)instruction.Operand, Operator.NotEquals),
            Code.Bne_Un_S => TranspileBranch((Instruction)instruction.Operand, Operator.NotEquals),
            Code.Switch => TranspileSwitch((Instruction[])instruction.Operand),
            Code.Leave => TranspileLeave(instruction, (Instruction)instruction.Operand),
            Code.Leave_S => TranspileLeave(instruction, (Instruction)instruction.Operand),

            Code.Ceq => TranspileBinaryExpression(Operator.EqualsOp),
            Code.Cgt => TranspileBinaryExpression(Operator.GreaterThen),
            Code.Cgt_Un => TranspileBinaryExpression(Operator.GreaterThen),
            Code.Clt => TranspileBinaryExpression(Operator.LessThen),
            Code.Clt_Un => TranspileBinaryExpression(Operator.LessThen),

            Code.Add => TranspileBinaryExpression(Operator.Add),
            Code.Sub => TranspileBinaryExpression(Operator.Substitute),
            Code.Mul => TranspileBinaryExpression(Operator.Multiply),
            Code.Div => TranspileBinaryExpression(Operator.Divide),
            Code.Rem => TranspileBinaryExpression(Operator.Remainder),

            Code.Div_Un => TranspileBinaryExpression(Operator.Divide),
            Code.Rem_Un => TranspileBinaryExpression(Operator.Remainder),

            Code.Add_Ovf => TranspileBinaryExpression(Operator.Add),
            Code.Sub_Ovf => TranspileBinaryExpression(Operator.Substitute),
            Code.Mul_Ovf => TranspileBinaryExpression(Operator.Multiply),

            Code.Add_Ovf_Un => TranspileBinaryExpression(Operator.Add),
            Code.Mul_Ovf_Un => TranspileBinaryExpression(Operator.Multiply),
            Code.Sub_Ovf_Un => TranspileBinaryExpression(Operator.Substitute),

            Code.And => TranspileBinaryExpression(Operator.And),
            Code.Or => TranspileBinaryExpression(Operator.Or),
            Code.Xor => TranspileBinaryExpression(Operator.Xor),
            Code.Shl => TranspileBinaryExpression(Operator.ShiftLeft),
            Code.Shr => TranspileBinaryExpression(Operator.ShiftRight),
            Code.Shr_Un => TranspileBinaryExpression(Operator.ShiftRight),
            Code.Neg => TranspileUnaryExpression(Operator.Not),
            Code.Not => TranspileUnaryExpression(Operator.Neg),

            Code.Conv_I => TranspileConvert(TypeSystem.IntPtr),
            Code.Conv_I1 => TranspileConvert(TypeSystem.SByte),
            Code.Conv_I2 => TranspileConvert(TypeSystem.Int16),
            Code.Conv_I4 => TranspileConvert(TypeSystem.Int32),
            Code.Conv_I8 => TranspileConvert(TypeSystem.Int64),
            Code.Conv_R4 => TranspileConvert(TypeSystem.Single),
            Code.Conv_R8 => TranspileConvert(TypeSystem.Double),
            Code.Conv_U => TranspileConvert(TypeSystem.UIntPtr),
            Code.Conv_U1 => TranspileConvert(TypeSystem.Byte),
            Code.Conv_U2 => TranspileConvert(TypeSystem.UInt16),
            Code.Conv_U4 => TranspileConvert(TypeSystem.UInt32),
            Code.Conv_U8 => TranspileConvert(TypeSystem.UInt64),

            Code.Conv_Ovf_I => TranspileConvert(TypeSystem.IntPtr),
            Code.Conv_Ovf_I1 => TranspileConvert(TypeSystem.SByte),
            Code.Conv_Ovf_I2 => TranspileConvert(TypeSystem.Int16),
            Code.Conv_Ovf_I4 => TranspileConvert(TypeSystem.Int32),
            Code.Conv_Ovf_I8 => TranspileConvert(TypeSystem.Int64),
            Code.Conv_Ovf_U => TranspileConvert(TypeSystem.UIntPtr),
            Code.Conv_Ovf_U1 => TranspileConvert(TypeSystem.Byte),
            Code.Conv_Ovf_U2 => TranspileConvert(TypeSystem.UInt16),
            Code.Conv_Ovf_U4 => TranspileConvert(TypeSystem.UInt32),
            Code.Conv_Ovf_U8 => TranspileConvert(TypeSystem.UInt64),

            Code.Conv_Ovf_I_Un => TranspileConvert(TypeSystem.IntPtr),
            Code.Conv_Ovf_I1_Un => TranspileConvert(TypeSystem.SByte),
            Code.Conv_Ovf_I2_Un => TranspileConvert(TypeSystem.Int16),
            Code.Conv_Ovf_I4_Un => TranspileConvert(TypeSystem.Int32),
            Code.Conv_Ovf_I8_Un => TranspileConvert(TypeSystem.Int64),
            Code.Conv_Ovf_U_Un => TranspileConvert(TypeSystem.UIntPtr),
            Code.Conv_Ovf_U1_Un => TranspileConvert(TypeSystem.Byte),
            Code.Conv_Ovf_U2_Un => TranspileConvert(TypeSystem.UInt16),
            Code.Conv_Ovf_U4_Un => TranspileConvert(TypeSystem.UInt32),
            Code.Conv_Ovf_U8_Un => TranspileConvert(TypeSystem.UInt64),
            Code.Conv_R_Un => TranspileConvert(TypeSystem.Single),

            Code.Sizeof => TranspileSizeof((TypeReference)instruction.Operand),
            Code.Castclass => s_noInstruction,
            Code.Isinst => TranspileIsinst((TypeReference)instruction.Operand),

            Code.Ldftn => TranspileLdftn((MethodReference)instruction.Operand),
            Code.Ldvirtftn => TranspileLdvirtftn((MethodReference)instruction.Operand),
            Code.Localloc => TranspileLocalloc(),
            Code.Ldtoken => TranspileLdToken(instruction.Operand),
            Code.Volatile => s_noInstruction,

            Code.Arglist => throw new NotImplementedException(),
            Code.Cpobj => throw new NotImplementedException(),
            Code.Ckfinite => throw new NotImplementedException(),
            Code.Mkrefany => throw new NotImplementedException(),
            Code.Refanyval => throw new NotImplementedException(),
            Code.Unaligned => throw new NotImplementedException(),
            Code.Tail => throw new NotImplementedException(),
            Code.Cpblk => throw new NotImplementedException(),
            Code.Initblk => throw new NotImplementedException(),
            Code.No => throw new NotImplementedException(),
            Code.Refanytype => throw new NotImplementedException(),

            _ => throw new NotSupportedException($"Instruction {instruction.OpCode} is not supported.")
        };

        if (_context.GotoInstructions.Contains(instruction))
        {
            string label = Naming.GetLabel(instruction);
            return new LabelInstruction(label, transpiledInstruction);
        }
        return transpiledInstruction;
    }

    private InitializeStructInstruction TranspileInitObj(TypeReference typeReference)
    {
        var address = _context.Stack.Pop();
        var name = Naming.Mangle(typeReference);
        return new InitializeStructInstruction(name, address.Expression);
    }

    private NoInstruction TranspileLdToken(object operand)
    {
        if(operand is TypeReference type)
        {
            var handleType = _context.Method.Module.ImportReference(typeof(RuntimeTypeHandle));
            var expression = new GetRuntimeTypeToken(Naming.Mangle(type));
            _context.Stack.Push(new StackItem(handleType, expression));
            return s_noInstruction;
        }
        if(operand is FieldReference field)
        {
            //var fieldName = Naming.GetInitValueFieldName(Naming.Mangle(field));
            //var handleType = _context.Method.Module.ImportReference(typeof(RuntimeTypeHandle));
            //var expression = new TakeAddressExpression(new StaticFieldExpression(fieldName));
            //_context.Stack.Push(new StackItem(handleType, expression));
            //return s_noInstruction;
            throw new NotImplementedException();
        }
        throw new NotImplementedException();
    }

    private IInstruction TranspileLocalloc()
    {
        var item = _context.Stack.Pop();
        var expression = new LocallocExpression(item.Expression);
        _context.Stack.Push(new StackItem(TypeSystem.UIntPtr, expression));
        return s_noInstruction;
    }

    private IInstruction TranspileLdvirtftn(MethodReference method)
    {
        var item = _context.Stack.Pop();
        var type = method.DeclaringType.Resolve();
        IExpression expression;
        if (type.IsInterface)
        {
            expression = new GetInterfaceMethod
            {
                InterfaceType = Naming.Mangle(type),
                MethodName = Naming.Mangle(method),
                ObjectExpression = item.Expression,
                VTableName = Naming.Mangle(method.DeclaringType),
            };
        }
        else
        {
            expression = new GetVirtualMethod
            {
                MethodName = Naming.Mangle(method),
                ObjectExpression = item.Expression,
                VTableName = Naming.Mangle(method.DeclaringType),
            };
        }
        _context.Stack.Push(new StackItem(TypeSystem.UIntPtr, new TakeAddressExpression(expression)));
        return s_noInstruction;
    }

    private IInstruction TranspileLdftn(MethodReference method)
    {
        var item = new GetMethodPointerInstruction(Naming.Mangle(method));
        _context.Stack.Push(new StackItem(TypeSystem.UIntPtr, item));
        return s_noInstruction;
    }

    private NoInstruction TranspileIsinst(TypeReference type)
    {
        var obj = _context.Stack.Pop();
        var id = TypeHelper.GetId(type);
        var expression = new IsInstanceExpresion(obj.Expression, id);
        _context.Stack.Push(new StackItem(type, expression));
        return s_noInstruction;
    }

    private AssignInstruction TranspileStind()
    {
        var value = _context.Stack.Pop();
        var adress = _context.Stack.Pop();
        return new AssignInstruction(new DereferenceExpression(adress.Expression), value.Expression);
    }

    private NoInstruction TranspileLdind()
    {
        var item = _context.Stack.Pop();
        _context.Stack.Push(new StackItem(item.Type, new DereferenceExpression(item.Expression)));

        return s_noInstruction;
    }

    private IInstruction TranspileDup()
    {
        var item = _context.Stack.Pop();
        var variableName = _context.CreateTemporaryVariable();

        var newStackItem = new StackItem(item.Type, new VariableExpression(variableName));
        _context.Stack.Push(newStackItem);
        _context.Stack.Push(newStackItem);

        return new DefineVariableInstruction
        {
            Name = variableName,
            Type = Naming.MangleReference(item.Type),
            DefaultValue = item.Expression
        };
    }

    private IInstruction TranspilePop()
    {
        StackItem value = _context.Stack.Pop();
        if (value.Expression is CallMethodInstruction
            || value.Expression is CallStaticMethodExpression)
            return new WriteExpressionInstruction(value.Expression);

        return s_noInstruction;
    }


    private IInstruction TranspileRet(Instruction instruction)
    {
        if (_context.Method.ReturnType.GetElementType() != TypeSystem.Void)
        {
            var returnValue = _context.Stack.Pop();
            return new ReturnInstruction(returnValue.Expression);
        }
        else if (instruction.Next is not null || _context.GotoInstructions.Contains(instruction))
            return new ReturnInstruction();

        return s_noInstruction;
    }

    private NoInstruction TranspileSizeof(TypeReference type)
    {
        var expression = new SizeofExpression(Naming.Mangle(type));
        _context.Stack.Push(new StackItem(TypeSystem.Int32, expression));
        return s_noInstruction;
    }

    private NoInstruction TranspileConvert(TypeReference type)
    {
        var value = _context.Stack.Pop();
        var expression = new CastExpression(value.Expression, Naming.Mangle(type));
        _context.Stack.Push(new StackItem(type, expression));
        return s_noInstruction;
    }
}
