# Midora
Midora - lightweight .NET runtime designed for resource-constrained devices. 

## Personal note
I tried to create small subset of .NET runtime to develop games for old game consoles in C#, but it didn't work.
Runtime contains IL to C transpiler, so it could compile C# code to any platform has C compiler. Also important point was compilation times, since C++ compiles really slow.
But generics are really complicated and i coudn't find way to figure out how to implement them in my small runtime. So I gave up...

I experiemented with languages to use for runtime native part, so i created runtime in [C](Runtime/), [Zig](Old/RuntimeZig), [C#](Old/RuntimeSharp).
It appears that LLVM compiled code incompatible with PSP-SDK linker, so I ported native library back to C.

## Supported runtime features
- [x] Value types
- [x] Reference types
- [x] Boxing
- [ ] Arrays
	- [x] Single-dimensional arrays
	- [ ] Multidimensional arrays
- [x] Unsafe
- [x] P/Invoke
- [x] Exceptions
- [x] Garbage collector
- [ ] All IL opcodes (Most implemented)
- [ ] Delegates
- [ ] Generics
- [ ] Reflection
- [ ] Corelib