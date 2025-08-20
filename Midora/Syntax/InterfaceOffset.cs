namespace Midora.Syntax;

public record InterfaceOffset(string InterfaceTypeName, string Vtable, string Slot)
{
    public string Emit() => $"{{{Naming.GetTypeInfoName(InterfaceTypeName)}.id, offsetof({Vtable}, {Slot})}}";
}
