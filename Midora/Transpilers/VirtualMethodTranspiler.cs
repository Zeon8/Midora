using Midora;
using Midora.Helpers;
using Midora.Metadata;
using Midora.Syntax;
using Mono.Cecil;

namespace Midora.Transpilers;

public class VirtualMethodTranspiler
{
    private readonly TypeMetadata _typeMetadata;

    public VirtualMethodTranspiler(TypeMetadata typeMetadata)
    {
        _typeMetadata = typeMetadata;
    }

    public IEnumerable<InterfaceOffset> GetInterfaceOffsets()
    {
        foreach(var interfaceImplementation in _typeMetadata.Definition.Interfaces)
        {
            var definition = interfaceImplementation.InterfaceType.Resolve();
            if (!definition.HasMethods)
                continue;

            string mangledName = Naming.Mangle(definition);
            yield return new InterfaceOffset(mangledName, _typeMetadata.VtableName, '_'+mangledName);
        }
    }

    public IEnumerable<string> GetVtableEntries()
    {
        foreach (MethodReference method in GetVirtualMethods())
        {
            string methodName = Naming.Mangle(method);
            if (_typeMetadata.Definition.IsValueType
                && _typeMetadata.Definition == method.DeclaringType)
                yield return Naming.GetBoxedMethod(methodName);
            else
                yield return methodName;
        }
    }

    public IEnumerable<VTableFunction> GetVTableFunctionDeclarations()
    {
        var interfaceMethods = _typeMetadata.GetBaseTypes()
            .Append(_typeMetadata.Definition)
            .SelectMany(t => t.Interfaces)
            .Select(t => t.InterfaceType.Resolve())
            .SelectMany(t => t.Methods)
            .Select(m => Naming.Mangle(m, includeTypeName: false))
            .ToHashSet();

        foreach ((string mangledName, MethodDefinition method) in OrderMethods(_typeMetadata.Definition))
        {
            if (!method.IsVirtual || !method.IsNewSlot || interfaceMethods.Contains(mangledName))
                continue;

            string returnTypeName = Naming.MangleReference(method.ReturnType);

            yield return new VTableFunction()
            {
                ReturnType = returnTypeName,
                Name = mangledName,
                ParameterTypes = method.Parameters.Select(p => Naming.MangleReference(p.ParameterType)),
            };
        }
    }

    private IEnumerable<MethodReference> GetVirtualMethods()
    {
        var methods = new Dictionary<string, MethodReference>();
        TypeDefinition[] list = _typeMetadata.GetBaseTypes()
            .Reverse()
            .Append(_typeMetadata.Definition)
            .ToArray();

        foreach (TypeDefinition? baseType in list)
        {
            foreach ((string mangledName, MethodDefinition method) in OrderMethods(baseType))
            {
                if (!method.IsVirtual)
                    continue;

                if (!methods.ContainsKey(mangledName))
                    methods[mangledName] = method;
            }
        }

        return methods.Values;
    }

    public MethodReference FindFinalizer() => FindFinalizer(_typeMetadata.Definition);

    private static MethodDefinition FindFinalizer(TypeDefinition type)
    {
        var finalizer = type.Methods.FirstOrDefault(m => m.IsVirtual && m.Name == "Finalize");

        if (finalizer is not null)
            return finalizer;

        return FindFinalizer(type.BaseType.Resolve());
    }

    private static IEnumerable<(string mangledName, MethodDefinition)> OrderMethods(TypeDefinition type)
    {
        return type.Methods
            .Select(m => (MangledName: Naming.Mangle(m, withTypeName: false), m))
            .OrderBy(i => i.MangledName);
    }
}
