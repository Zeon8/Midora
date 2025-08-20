using Midora.Metadata;
using Midora.Syntax;
using Mono.Cecil;

namespace Midora.Transpilers;

public class FieldTranspiler
{
    private readonly TypeMetadata _type;

    public FieldTranspiler(TypeMetadata type)
    {
        _type = type;
    }

    public IEnumerable<FieldDeclaration> DeclareFields()
    {
        foreach (var field in _type.Definition.Fields)
        {
            if (field.IsStatic)
                continue;

            string fieldName = Naming.Mangle(field, includeTypeName: false);
            string fieldTypeName = Naming.MangleReference(field.FieldType);

            yield return new FieldDeclaration(fieldTypeName, fieldName);
        }
    }

    public IEnumerable<StaticFieldDeclaration> DeclareStaticFields()
    {
        foreach (var field in _type.Definition.Fields)
        {
            if (!field.IsStatic)
                continue;

            string fieldName = Naming.Mangle(field);
            string fieldTypeName = Naming.MangleReference(field.FieldType);

            yield return new StaticFieldDeclaration(fieldTypeName, fieldName, InitValue: field.InitialValue);
        }
    }

    public IEnumerable<IEnumerable<FieldOffset>> ResolveReferenceTypeFieldOffsets(TypeReference type)
    {
        var nodes = new List<Node>();
        GetOffsets(type, parent: null);
        foreach (var node in nodes)
            yield return node.GetOffsets();

        void GetOffsets(TypeReference reference, Node? parent)
        {
            var definition = reference.Resolve();
            string declaringTypeName = Naming.Mangle(reference);

            foreach (var field in definition.Fields)
            {
                if (field.IsStatic)
                    continue;

                string fieldName = Naming.Mangle(field, includeTypeName: false);

                var fieldTypeDefinition = field.FieldType.Resolve();
                if (fieldTypeDefinition.IsPrimitive)
                    continue;

                var node = new Node(new FieldOffset(declaringTypeName, fieldName), parent);

                if (fieldTypeDefinition.IsValueType)
                    GetOffsets(field.FieldType, node);
                else
                    nodes.Add(node);
            }
        }
    }

    private record Node(FieldOffset Offset, Node? Parent)
    {
        public IEnumerable<FieldOffset> GetOffsets()
        {
            if (Parent is not null)
            {
                foreach (var offset in Parent.GetOffsets())
                    yield return offset;
            }
            yield return Offset;
        }
    }
}
