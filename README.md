# Midora
Midora - lightweight .NET runtime designed for resource-constrained devices. 

## Personal note
I tried to create a small subset of the .NET runtime to develop games for old consoles in C#, but it didn't work.
The runtime included an IL-to-C transpiler, so it could compile C# code for any platform with a C compiler. Another reason I chose C was compilation speed, since C++ code compiles really slowly.
However, generics turned out to be really complicated, and I couldn’t figure out how to implement them in my small runtime. So, I gave up.

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
