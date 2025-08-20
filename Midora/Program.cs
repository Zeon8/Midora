using Midora;
using Mono.Cecil;
using System.CodeDom.Compiler;

ModuleDefinition module = ModuleDefinition.ReadModule(args[0]);
var outputName = Path.Combine(args[1], Path.GetFileNameWithoutExtension(args[0]));

using var headerStreamWriter = new StreamWriter(outputName + ".h");
using var sourceStreamWriter = new StreamWriter(outputName + ".c");
var headerWriter = new IndentedTextWriter(headerStreamWriter);
var sourceWriter = new IndentedTextWriter(sourceStreamWriter);

headerWriter.WriteLine("#pragma once");
headerWriter.WriteLine(@"#include ""midora.h""");
foreach (var assembly in module.AssemblyReferences)
    headerWriter.WriteLine(@$"#include ""{assembly.Name}.h""");
headerWriter.WriteLine();


sourceWriter.WriteLine(@$"#include ""{module.Assembly.Name.Name}.h""");
sourceWriter.WriteLine();

var writers = new Writers();

var transpiler = new Transpiler(module);
transpiler.Transpile();
transpiler.Write(writers);

writers.Write(headerWriter, sourceWriter);
