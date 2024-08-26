using dnlib.DotNet;
using Dotnet2C;
using System.CodeDom.Compiler;
using System.Reflection;

ModuleContext modCtx = ModuleDef.CreateModuleContext();
ModuleDefMD module = ModuleDefMD.Load(args[0], modCtx);

var outputName = Path.Combine(Path.GetDirectoryName(args[0])!, Path.GetFileNameWithoutExtension(args[0]));

using var headerStreamWriter = new StreamWriter(outputName+".h");
using var sourceStreamWriter = new StreamWriter(outputName+".c");

var headerWriter = new IndentedTextWriter(headerStreamWriter);
var sourceWriter = new IndentedTextWriter(sourceStreamWriter);

foreach (var reference in module.GetAssemblyRefs())
    headerWriter.WriteLine($"#include <{reference.Name}.h>");
headerWriter.WriteLine();

sourceWriter.WriteLine($"#include \"{module.Assembly.Name}.h\"");
sourceWriter.WriteLine();

var typeWriter = new TypeWriter();
foreach (var type in module.Types)
{
    if (type.IsGlobalModuleType)
        continue;

    typeWriter.WriteType(type, headerWriter, sourceWriter);
}