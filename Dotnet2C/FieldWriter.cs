using dnlib.DotNet;
using System.CodeDom.Compiler;

namespace Dotnet2C
{
    public class FieldWriter
    {
        public void WriteInstanceFields(TypeDef type, IndentedTextWriter writer)
        {
            foreach (FieldDef field in type.Fields)
            {
                if (!field.IsStatic)
                {
                    string returnType = Naming.MangleTypeReferenceName(field.FieldType);
                    UTF8String fieldName = Naming.RemoveBackfieldSigns(field.Name);
                    writer.WriteLine($"{returnType} {fieldName};");
                }
            }
        }

        public void WriteStaticFields(TypeDef type, IndentedTextWriter writer)
        {
            foreach (FieldDef field in type.Fields)
            {
                if (field.IsStatic)
                {
                    string returnType = Naming.MangleTypeReferenceName(field.FieldType);
                    string fieldName = Naming.MangleMemberName(field);
                    fieldName = Naming.RemoveBackfieldSigns(fieldName);
                    writer.WriteLine($"{returnType} {fieldName};");
                }
            }
        }
    }
}
