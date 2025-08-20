using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Text;

namespace Midora;

public class Naming
{
    public const string ThisArgument = "this";

    public const string TypeInfoFieldName = "type";

    public const string BaseFieldName = "__base";

    public const string TypeInfoVariableName = "type_info";

    public const string VtableFieldName = "vptr";

    public const string VtableVariableName = "vtable";

    public const string BoxedValueFieldName = "value";

    public const string RuntimeObjectName = "RuntimeObject";

    public const string ArrayTypeName = "System_Array";

    public const string TypeInfoStructName = "TypeInfo";

    public const string GCFrameVariableName = "__gc_frame";

    public const string ExceptionHandledVariable = "__exception_handled";

    public static string GetDefaultLocal(int index) => "__local_" + index;

    public static string GetLabel(Instruction instruction) => "IL_" + instruction.Offset;

    public static string GetBoxedType(string name) => name + "_boxed";

    public static string GetBoxedMethod(string name) => name + "_boxed";

    public static string GetBoxFunction(string name) => name + "__box";

    public static string GetVTableName(string typeName) => typeName + "_vtable";

    public static string GetVTableInstance(string typeName) => typeName + "_vtable_instance";

    public static string GetTypeInfoName(string typeName) => typeName + "_type";

    public static string GetInitValueFieldName(string fieldName) => $"{fieldName}_init_value";

    public static string Mangle(TypeReference type, GenericContext? genericContext = null)
    {
        if (type is GenericParameter genericParameter)
        {
            if (genericContext is null)
                throw new InvalidOperationException("GenericContext is not provided for generic parameter.");

            return Mangle(genericContext!.GetArgument(genericParameter));
        }

        if (type.IsByReference)
            return "ref_" + Mangle(type.GetElementType(), genericContext);

        if (type.IsArray)
            return $"{ArrayTypeName}_{Mangle(type.GetElementType(), genericContext)}";

        if (type is GenericInstanceType instanceType)
        {
            StringBuilder builder = new();
            builder.Append(Mangle(instanceType.ElementType.FullName));
            foreach (TypeReference parameter in instanceType.GenericArguments)
                builder.Append($"_{Mangle(parameter, genericContext)}");

            return builder.ToString();
        }

        return Mangle(type.FullName);
    }

    public static string Mangle(MemberReference member, bool includeTypeName = true, GenericContext? genericContext = null)
    {
        string memberName = Mangle(member.Name);
        if (!includeTypeName)
            return memberName;

        string typeName = Mangle(member.DeclaringType.FullName);
        return $"{typeName}__{memberName}";
    }

    public static string Mangle(MethodReference method, bool withTypeName = true)
    {
        string name = Mangle((MemberReference)method, withTypeName);
        var methodNameBuilder = new StringBuilder(name);

        foreach (var parameter in method.Parameters)
        {
            methodNameBuilder.Append('_');
            methodNameBuilder.Append(Mangle(parameter.ParameterType));
        }

        return methodNameBuilder.ToString();
    }

    public static string MangleReference(TypeReference reference)
    {
        if (reference is ArrayType || reference.IsGenericParameter)
            return RuntimeObjectName + '*';

        TypeDefinition definition = reference.Resolve();
        if (definition.IsValueType)
        {
            var mangledName = Mangle(reference);

            if (reference.IsByReference || reference.IsPinned)
                return mangledName + '*';

            if (reference.IsPointer)
                return "System_UIntPtr";

            return mangledName;
        }
        else
        {
            if (reference.IsByReference)
                return RuntimeObjectName + "**";

            return RuntimeObjectName + '*';
        }
    }

    private static string Mangle(string typeName)
    {
        return typeName
            .Replace('.', '_')
            .Replace('/', '_')
            .Replace('|', '_')
            .Replace('<', '_')
            .Replace('>', '_')
            .Replace('=', '_')
            .Replace('`', '_');
    }
}
