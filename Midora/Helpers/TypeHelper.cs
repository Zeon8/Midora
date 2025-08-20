using Mono.Cecil;
using System.Reflection;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;

namespace Midora.Helpers;

public static class TypeHelper
{
    public static int GetId(TypeReference type)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(type.FullName);
        byte[] hash = MD5.HashData(bytes);
        return BitConverter.ToInt32(hash, 0);
    }
}
