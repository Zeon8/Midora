using Mono.Cecil;
using System.CodeDom.Compiler;

namespace Midora;
public class VirtualMethodWriter
{
    private readonly TypeMetadata _typeMetadata;
    private readonly Writers _writers;

    public VirtualMethodWriter(TypeMetadata typeMetadata, Writers writers)
    {
        _typeMetadata = typeMetadata;
        _writers = writers;
    }

    public void DefineVirtualMethods()
    {
        foreach ((string mangledName, MethodReference method) in GetVirtualMethods().Reverse())
        {
            DefineVirtualMethod(method, mangledName);
        }
    }

    public void WriteTypeHeaderInstance(string mangledName)
    {
        DefineVTableInstance(mangledName);

        var baseTypes = _typeMetadata.GetBaseTypes().Reverse();
        Dictionary<string, MethodReference> virtualMethods = GetVirtualMethods();
        List<TypeMetadata> interfaces = new();
        foreach (var type in baseTypes)
        {
            foreach (var @interface in type.Interfaces)
            {
                var typeMetadata = new TypeMetadata(@interface.InterfaceType.Resolve());
                interfaces.Add(typeMetadata);
                GenerateInterfaceVtableInstance(mangledName, virtualMethods, typeMetadata);
            }
        }

        _writers.Source.WriteLine($"static TypeInfoVariable {Naming.TypeInfoVariableName} = {{");
        _writers.Source.Indent++;
        _writers.Source.WriteLine($".{Naming.VtableFieldName} = &{Naming.VtableVariableName},");

        if (interfaces.Count > 0)
        {
            _writers.Source.WriteLine($".interfaces_count = {interfaces.Count},");
            _writers.Source.WriteLine($".interface_vtables = {{");
            _writers.Source.Indent++;
            foreach (var @interface in interfaces)
            {
                _writers.Source.WriteLine($"{{{@interface.GetId()}, &{@interface.VtableInstance}}}");
            }
            _writers.Source.Indent--;
            _writers.Source.WriteLine("}");
        }
        _writers.Source.Indent--;
        _writers.Source.WriteLine("};");


    }

    private void GenerateInterfaceVtableInstance(string mangledName, Dictionary<string, MethodReference> virtualMethods, TypeMetadata type)
    {
        _writers.Source.WriteLine($"static void* {type.VtableInstance} = {{");
        _writers.Source.Indent++;
        foreach (var method in type.Definition.Methods)
        {
            foreach ((string virtualMethodName, MethodReference virtualMethod) in virtualMethods)
            {
                if (virtualMethod.Name == method.Name && virtualMethod.Parameters.SequenceEqual(method.Parameters))
                {
                    _writers.Source.WriteLine($"&{Naming.MangleMethod(virtualMethod)}");
                }
            }
        }
        _writers.Source.Indent--;
        _writers.Source.WriteLine("};");
    }

    private void DefineVTableInstance(string mangledName)
    {
        var writer = _writers.Source;
        writer.WriteLine($"static void* {Naming.VtableVariableName} = {{");
        writer.Indent++;
        Dictionary<string, MethodReference> methods = GetVirtualMethods();
        foreach ((_, MethodReference method) in methods.Reverse())
        {
            string methodName = Naming.MangleMethod(method);
            if (_typeMetadata.Definition.IsValueType && _typeMetadata.Definition == method.DeclaringType)
                writer.Write($"&{Naming.GetBoxedMethod(methodName)},");
            else
                writer.Write($"&{methodName},");

            writer.WriteLine();
        }
        writer.Indent--;
        writer.WriteLine("};");
    }

    private void DefineVirtualMethod(MethodReference method, string mangledName)
    {
        //string declaringTypeName = Naming.Mangle(method.DeclaringType.FullName);
        var declaringType = new TypeMetadata(method.DeclaringType.Resolve());
        string returnType = Naming.MangleTypeReference(method.ReturnType);

        if (method.DeclaringType.IsValueType)
            _writers.Header.Write($"{returnType} (*{mangledName})({declaringType.BoxName}*");
        else
            _writers.Header.Write($"{returnType} (*{mangledName})({declaringType.MangledName}*");

        if (method.Parameters.Count > 0)
        {
            _writers.Header.Write(',');
            _writers.Header.Write(string.Join(',', method.Parameters.Select(p => Naming.MangleTypeReference(p.ParameterType))));
        }
        _writers.Header.WriteLine(");");
    }

    private Dictionary<string, MethodReference> GetVirtualMethods()
    {
        var methods = new Dictionary<string, MethodReference>();
        List<TypeDefinition> list = _typeMetadata.GetBaseTypes().ToList();

        foreach (TypeDefinition? baseType in list)
        {
            foreach (var method in baseType.Methods)
            {
                if (!method.IsVirtual)
                    continue;

                string name = Naming.MangleMethod(method, includeFullName: false);
                if (!methods.ContainsKey(name))
                    methods[name] = method;
            }
        }
        return methods;
    }


}
