using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Dotnet2C
{
    public class Naming
    {
        public const string ThisArgument = "_this";

        public const string VTablePointerName = "__vptr";

        public static string GetLocalName(uint index) => $"local_{index}";

        public static string GetLabelName(Instruction instruction)
        {
            return $"IL_{instruction.Offset}";
        }

        public static string MangleName(string typeName)
        {
            return typeName.Replace('.', '_');
        }

        public static string MangleTypeReferenceName(IType type)
        {
            var mangledName = MangleName(type.FullName);
            if (type is TypeSig signature && signature.IsByRef)
            {
                // Remove & from name
                mangledName = mangledName[..(mangledName.Length - 1)];

                if (!type.ScopeType.IsValueType)
                    mangledName += "**";
                else
                    mangledName += "*";
            }
            else if (!type.IsValueType)
                mangledName += '*';
            return mangledName;
        }

        public static string MangleMemberName(IMemberRef member)
        {
            string typeName = MangleName(member.DeclaringType.FullName);
            string memberName = MangleName(member.Name);
            return $"{typeName}__{memberName}";
        }

        public static string RemoveBackfieldSigns(UTF8String fieldName)
        {
            if (fieldName.Contains("k__BackingField"))
                return fieldName.Replace('<', '_').Replace('>', '_');
            return fieldName;
        }

        public static string RemoveBackfieldSigns(string fieldName)
        {
            if (fieldName.Contains("k__BackingField"))
                return fieldName.Replace('<', '_').Replace('>', '_');
            return fieldName;
        }
    }
}
