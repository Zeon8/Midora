using Midora;
using Midora.Metadata;
using Midora.Syntax;
using Midora.Transpilers;
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace Midora;

public class Transpiler
{
    private readonly ModuleDefinition _module;
    private readonly StringLiteralTranspiler _stringLiteralTranspiler = new();
    private readonly ArrayTranspiler _arrayTranspiler = new();

    private readonly List<TranspiledType> _types = new();
    private readonly List<InterfaceDeclaration> _interfaces = new();
    private readonly List<StaticFieldDeclaration> _staticFields = new();
    private readonly List<StringFieldDeclaration> _stringFields = new();

    private IEnumerable<ArrayTypeInfo>? _arrayTypeInfos = [];
    private AssemblyInitalizer? _assemblyInitalizer;

    public Transpiler(ModuleDefinition moduleDefinition)
    {
        _module = moduleDefinition;
    }

    public void Transpile()
    {
        List<TypeDefinition> transpiledTypes = new();
        foreach (TypeDefinition type in _module.Types)
        {
            if (type.Name == "<Module>" || type.HasGenericParameters)
                continue;

            Transpile(type);
            transpiledTypes.Add(type);
        }

        _arrayTypeInfos = _arrayTranspiler.Transpile(_module).ToArray();
        _stringFields.AddRange(_stringLiteralTranspiler.DeclareStringFields());

        _assemblyInitalizer = new AssemblyInitalizer
        {
            AssemblyName = _module.Assembly.Name.Name,
            StaticConstructorCalls = transpiledTypes
                .Where(t => t.GetStaticConstructor() != null)
                .Select(t => new StaticConstructorCall(Naming.Mangle(t)))
                .ToArray(),
            GCRootFields = transpiledTypes
                .SelectMany(t => t.Fields)
                .Where(f => f.IsStatic)
                .Select(f => new GCRootField(Naming.Mangle(f)))
        };
    }

    private void Transpile(TypeReference type)
    {
        var definition = type.Resolve();
        var typeMetadata = new TypeMetadata(definition);
        var fieldTranspiler = new FieldTranspiler(typeMetadata);
        var typeTranspiler = new TypeTranspiler(typeMetadata, fieldTranspiler, _stringLiteralTranspiler);

        if (!typeMetadata.IsStatic)
        {
            if (definition.IsValueType)
                _types.Add(typeTranspiler.DeclareStruct());
            else if (definition.IsInterface)
                _interfaces.Add(typeTranspiler.DeclareInterface());
            else
                _types.Add(typeTranspiler.DeclareClass());
        }
        else
            _staticFields.AddRange(fieldTranspiler.DeclareStaticFields());

        foreach (var nestedType in definition.NestedTypes)
            Transpile(nestedType);
    }

    public void Write(Writers writers)
    {
        foreach (StringFieldDeclaration field in _stringFields)
            field.Write(writers.Source);

        foreach(ArrayTypeInfo arrayTypeInfo in _arrayTypeInfos)
            arrayTypeInfo.Write(writers.Source);

        foreach (InterfaceDeclaration @interface in _interfaces)
            @interface.Write(writers);

        foreach (TranspiledType typeDeclaration in _types)
            typeDeclaration.Write(writers);

        foreach (StaticFieldDeclaration staticField in _staticFields)
            staticField.Write(writers);

        _assemblyInitalizer!.Write(writers);
    }
}
