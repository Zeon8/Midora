namespace Midora.Syntax;

public record InterfaceDeclaration(VTableDeclaration VTable, TypeInfoDefinition TypeInfo)
{
    public void Write(Writers writers)
    {
        VTable.Write(writers);
        TypeInfo.Write(writers);
    }
}
