using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.CodeDom.Compiler;

namespace Dotnet2C
{
    public class ILTranspiller
    {
        public delegate void TranspileDelegate(Instruction instruction, TranspileContext context, IndentedTextWriter writer);

        private CorLibTypes _corLibTypes;

        public Dictionary<OpCode, TranspileDelegate> Transpillers { get; } = new()
        {
            {OpCodes.Nop, (_, _, _) => { } },
            {OpCodes.Ret, TranspileRet},
            {OpCodes.Pop, TranspilePop},
            {OpCodes.Ldstr, TranspileLdstr},

            // Integers
            {OpCodes.Ldc_I4, (instruction, context, _) => TranspileLdc(context, (int)instruction.Operand, types => types.Int32)},
            {OpCodes.Ldc_I4_S, (instruction, context, _) => TranspileLdc(context, (sbyte)instruction.Operand, types => types.Int32)},
            {OpCodes.Ldc_I8, (instruction, context, _) => TranspileLdc(context, (long)instruction.Operand, types => types.Int64)},
            {OpCodes.Ldc_R4, (instruction, context, _) => TranspileLdc(context, (float)instruction.Operand, types => types.Single)},
            {OpCodes.Ldc_R8, (instruction, context, _) => TranspileLdc(context, (double)instruction.Operand, types => types.Double)},

            {OpCodes.Ldc_I4_0, (_, context, _) => TranspileLdc(context, 0, types => types.Int32)},
            {OpCodes.Ldc_I4_1, (_, context, _) => TranspileLdc(context, 1, types => types.Int32)},
            {OpCodes.Ldc_I4_2, (_, context, _) => TranspileLdc(context, 2, types => types.Int32)},
            {OpCodes.Ldc_I4_3, (_, context, _) => TranspileLdc(context, 3, types => types.Int32)},
            {OpCodes.Ldc_I4_4, (_, context, _) => TranspileLdc(context, 4, types => types.Int32)},
            {OpCodes.Ldc_I4_5, (_, context, _) => TranspileLdc(context, 5, types => types.Int32)},
            {OpCodes.Ldc_I4_6, (_, context, _) => TranspileLdc(context, 6, types => types.Int32)},
            {OpCodes.Ldc_I4_7, (_, context, _) => TranspileLdc(context, 7, types => types.Int32)},
            {OpCodes.Ldc_I4_8, (_, context, _) => TranspileLdc(context, 8, types => types.Int32)},
            {OpCodes.Ldc_I4_M1, (_, context, _) => TranspileLdc(context, -1, types => types.Int32)},

            {OpCodes.Ldind_I, TranspileLdind},
            {OpCodes.Ldind_I1, TranspileLdind},
            {OpCodes.Ldind_I2, TranspileLdind},
            {OpCodes.Ldind_I4, TranspileLdind},
            {OpCodes.Ldind_R4, TranspileLdind},
            {OpCodes.Ldind_R8, TranspileLdind},
            {OpCodes.Ldind_Ref, TranspileLdind},
            {OpCodes.Ldind_U1, TranspileLdind},
            {OpCodes.Ldind_U2, TranspileLdind},
            {OpCodes.Ldind_U4, TranspileLdind},

            {OpCodes.Stind_I, TranspileStind},
            {OpCodes.Stind_I1, TranspileStind},
            {OpCodes.Stind_I2, TranspileStind},
            {OpCodes.Stind_I4, TranspileStind},
            {OpCodes.Stind_R4, TranspileStind},
            {OpCodes.Stind_R8, TranspileStind},
            {OpCodes.Stind_Ref, TranspileStind},

            //Locals 
            {OpCodes.Ldarg, (instruction,context,_) => TranspileLdarg(context, (Parameter)instruction.Operand)},
            {OpCodes.Ldarg_S, (instruction,context,_) => TranspileLdarg(context, (Parameter)instruction.Operand)},
            {OpCodes.Ldarg_0, (_,context,_) => TranspileLdarg(context, 0)},
            {OpCodes.Ldarg_1, (_,context,_) => TranspileLdarg(context, 1)},
            {OpCodes.Ldarg_2, (_,context,_) => TranspileLdarg(context, 2)},
            {OpCodes.Ldarg_3, (_,context,_) => TranspileLdarg(context, 3)},

            {OpCodes.Ldarga,  (instruction, context, _) => TranspileLdarga(context, (Parameter)instruction.Operand)},
            {OpCodes.Ldarga_S,  (instruction, context, _) => TranspileLdarga(context, (Parameter)instruction.Operand)},

            {OpCodes.Starg, TranspileStarg},
            {OpCodes.Starg_S, TranspileStarg},

            {OpCodes.Stloc, (instruction, context, writer) => TranspileStloc(context, writer, (ushort)instruction.Operand)},
            {OpCodes.Stloc_S, (instruction, context, writer) => TranspileStloc(context, writer, (byte)instruction.Operand)},
            {OpCodes.Stloc_0, (instruction, context, writer) => TranspileStloc(context, writer, 0)},
            {OpCodes.Stloc_1, (instruction, context, writer) => TranspileStloc(context, writer, 1)},
            {OpCodes.Stloc_2, (instruction, context, writer) => TranspileStloc(context, writer, 2)},
            {OpCodes.Stloc_3, (instruction, context, writer) => TranspileStloc(context, writer, 3)},

            {OpCodes.Ldloc, (instruction, context, writer) => TranspileLdloc(context, (ushort)instruction.Operand) },
            {OpCodes.Ldloc_S, (instruction, context, writer) => TranspileLdloc(context, (byte)instruction.Operand) },
            {OpCodes.Ldloc_0, (instruction, context, writer) => TranspileLdloc(context, 0) },
            {OpCodes.Ldloc_1, (instruction, context, writer) => TranspileLdloc(context, 1) },
            {OpCodes.Ldloc_2, (instruction, context, writer) => TranspileLdloc(context, 2) },
            {OpCodes.Ldloc_3, (instruction, context, writer) => TranspileLdloc(context, 3) },

            {OpCodes.Ldloca, TranspileLdloca},
            {OpCodes.Ldloca_S, TranspileLdloca},

            // Fields
            {OpCodes.Ldfld, TranspileLdfld},
            {OpCodes.Ldsfld, TranspileLdsfld},
            {OpCodes.Stfld, TranspileStfld},
            {OpCodes.Stsfld, TranspileStsfld},

            // Calls
            {OpCodes.Call, TranspileCall},
            {OpCodes.Callvirt,  TranspileCallvirt},
            {OpCodes.Constrained, TranspileConstrained},

            {OpCodes.Brfalse, TranspileBranch },
            {OpCodes.Brfalse_S, TranspileBranch }
        };

        private static void TranspileBranch(Instruction instruction, TranspileContext context, IndentedTextWriter writer)
        {
            var value = context.Stack.Pop();
            var gotoInstruction = (Instruction)instruction.Operand;
            string condition = instruction.IsBrtrue() ? "1" : "0";
            writer.WriteLine($"if({value.Expression} == {condition})");
            writer.Indent++;
            writer.WriteLine($"goto {Naming.GetLabelName(gotoInstruction)};");
            writer.Indent--;
        }

        private static void TranspileLdloca(Instruction instruction, TranspileContext context, IndentedTextWriter writer)
        {
            var local = (Local)instruction.Operand;
            context.Stack.Push(new StackItem($"{local.Name}", local.Type));
        }

        private static void TranspileStarg(Instruction instruction, TranspileContext context, IndentedTextWriter writer)
        {
            var argument = (Parameter)instruction.Operand;
            var value = context.Stack.Pop();
            writer.WriteLine($"{argument.Name} = {value.Expression};");
        }

        private static void TranspileLdarg(TranspileContext context, Parameter argument)
        {
            string argumentName = argument.IsHiddenThisParameter? Naming.ThisArgument : argument.Name;
            context.Stack.Push(new StackItem(argumentName, argument.Type));
        }

        private static void TranspileLdarga(TranspileContext context, Parameter argument)
        {
            var sign = new ByRefSig(argument.Type);
            context.Stack.Push(new StackItem($"(&{argument.Name})", sign));
        }

        private static void TranspileConstrained(Instruction instruction, TranspileContext context, IndentedTextWriter writer)
        {
            // TODO: Add boxing
        }

        private static void TranspileLdind(Instruction instruction, TranspileContext context, IndentedTextWriter writer)
        {
            var item = context.Stack.Pop();
            context.Stack.Push(new StackItem('*' + item.Expression, item.Type));
        }

        private static void TranspileStind(Instruction instruction, TranspileContext context, IndentedTextWriter writer)
        {
            var value = context.Stack.Pop();
            var adress = context.Stack.Pop();
            writer.WriteLine($"*{adress.Expression} = {value.Expression};");
        }

        private static void TranspileCallvirt(Instruction instruction, TranspileContext context, IndentedTextWriter writer)
        {
            if (instruction.Operand is IMethod method)
            {
                List<string> argList = GetArguments(context, method);

                var item = context.Stack.Pop();
                argList.Add(item.Expression);
                string args = string.Join(", ", argList);

                string expression;
                if (item.Type!.IsValueType)
                {
                    var methodName = Naming.MangleMemberName(method);
                    expression = $"{methodName}({args});";
                }
                else
                    expression = $"{item.Expression}->{Naming.VTablePointerName}->{method.Name}({args})";

                WriteOrPushMethodCall(context, writer, method, expression);
            }
        }

        private static void WriteOrPushMethodCall(TranspileContext context, IndentedTextWriter writer, IMethod method, string expression)
        {
            if (method.MethodSig.RetType.RemovePinnedAndModifiers().ElementType == ElementType.Void)
                writer.WriteLine(expression + ';');
            else
                context.Stack.Push(new StackItem(expression, method.MethodSig.RetType));
        }

        private static List<string> GetArguments(TranspileContext context, IMethod method)
        {
            var args = new List<string>();
            foreach (var @param in method.GetParams())
            {
                StackItem value = context.Stack.Pop();
                args.Add(value.Expression);
            }
            return args;
        }

        private static void TranspileStfld(Instruction instruction, TranspileContext context, IndentedTextWriter writer)
        {
            if (instruction.Operand is IField field)
            {
                var value = context.Stack.Pop();
                var obj = context.Stack.Pop();

                string fieldExpression = GetFieldExpression(field, obj);
                writer.WriteLine($"{fieldExpression} = {value.Expression};");
            }
        }

        private static void TranspileStsfld(Instruction instruction, TranspileContext context, IndentedTextWriter writer)
        {
            if (instruction.Operand is IField field)
            {
                var value = context.Stack.Pop();

                string fieldName = Naming.MangleMemberName(field);
                fieldName = Naming.RemoveBackfieldSigns(fieldName);
                writer.WriteLine($"{fieldName} = {value.Expression};");
            }
        }

        private static void TranspileLdfld(Instruction instruction, TranspileContext context, IndentedTextWriter writer)
        {
            if(instruction.Operand is FieldDef field)
            {
                var obj = context.Stack.Pop();
                string fieldExpression = GetFieldExpression(field, obj);
                context.Stack.Push(new StackItem(fieldExpression, field.FieldType));
            }
        }

        private static string GetFieldExpression(IField field, StackItem obj)
        {
            string fieldExpression = Naming.RemoveBackfieldSigns(field.Name);
            if (obj.Type.IsValueType)
                fieldExpression = $"{obj.Expression}.{fieldExpression}";
            else
                fieldExpression = $"{obj.Expression}->{fieldExpression}";
            return fieldExpression;
        }

        private static void TranspileLdsfld(Instruction instruction, TranspileContext context, IndentedTextWriter writer)
        {
            if (instruction.Operand is IField field)
            {
                string fieldName = Naming.MangleMemberName(field);
                fieldName = Naming.RemoveBackfieldSigns(fieldName);
                context.Stack.Push(new StackItem(fieldName, field.FieldSig.Type));
            }
        }

        private static void TranspileLdloc(TranspileContext context, ushort localIndex)
        {
            var local = context.Varibles[localIndex];
            string localName = local.Name ?? Naming.GetLocalName(localIndex);
            context.Stack.Push(new StackItem($"{localName}", local.Type));
        }

        private static void TranspileStloc(TranspileContext context, IndentedTextWriter writer, ushort localIndex)
        {
            StackItem item = context.Stack.Pop();
            LocalVarible local = context.Varibles[localIndex];
            string localName = local.Name ?? Naming.GetLocalName(localIndex);
            string localType = Naming.MangleTypeReferenceName(local.Type);

            if (local.Defined)
                writer.WriteLine($"{localName} = {item.Expression};");
            else
                writer.WriteLine($"{localType} {localName} = {item.Expression};");
        }

        private static void TranspilePop(Instruction instruction, TranspileContext context, IndentedTextWriter writer)
        {
            var value = context.Stack.Pop();
            writer.WriteLine(value.Expression);
        }

        private static void TranspileLdarg(TranspileContext context, ushort argumentIndex)
        {
            Parameter argument = context.Method.Parameters[argumentIndex];
            string argumentName = argument.Name;
            if (string.IsNullOrEmpty(argumentName))
            {
                if (argumentIndex != 0 || !context.Method.HasThis)
                    argumentName = $"local_{argumentIndex}";
                else
                    argumentName = Naming.ThisArgument;
            }
            context.Stack.Push(new StackItem(argumentName, argument.Type));
        }

        private static void TranspileLdstr(Instruction instruction, TranspileContext context, IndentedTextWriter writer)
        {
            if(instruction.Operand is string str)
            {
                // TODO: Replace with System.String object
                context.Stack.Push(new StackItem('"'+str+'"'));
            }
        }

        private static void TranspileCall(Instruction instruction, TranspileContext context, IndentedTextWriter writer)
        {
            if(instruction.Operand is IMethod method)
            {
                var args = GetArguments(context, method);
                if (method.MethodSig.HasThis)
                {
                    StackItem value = context.Stack.Pop();
                    args.Add(value.Expression);
                }
                args.Reverse();

                string methodName = Naming.MangleMemberName(method);
                string expression = $"{methodName}({string.Join(", ", args)})";

                WriteOrPushMethodCall(context, writer, method, expression);
            }
        }

        private static void TranspileLdc<T>(TranspileContext context, T number, Func<ICorLibTypes, CorLibTypeSig> getType)
            where T : notnull
        {
            CorLibTypes types = (CorLibTypes)context.Method.Module.CorLibTypes;
            context.Stack.Push(new StackItem(number.ToString()!, getType(types)));
        }

        private static void TranspileRet(Instruction instruction, TranspileContext context, IndentedTextWriter writer)
        {
            if (context.Method.HasReturnType)
            {
                var returnType = context.Stack.Pop();
                writer.WriteLine($"return {returnType.Expression};");
            }
            else
                writer.WriteLine("return;");
        }

        public void TranspileInstruction(Instruction instruction, TranspileContext context, IndentedTextWriter writer)
        {
            if (Transpillers.TryGetValue(instruction.OpCode, out TranspileDelegate? transpile))
            {
                if (context.GotoInstructions.Contains(instruction))
                    writer.WriteLineNoTabs(Naming.GetLabelName(instruction)+":");

                transpile(instruction, context, writer);
            }
            else
                throw new NotSupportedException($"Instruction {instruction.OpCode} is not supported.");
        }
    }
}
