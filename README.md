Nios II Simulator
======

Simulator for the Nios II processor architecture written in C# for .NET 4.5.

<h3>Features:</h3>
* WPF GUI application.
* Assembler.
* Interpreter.
* JIT compiler (experimental).

<h2>Indepth</h2>

<h3>Supported instructions</h3>
The basic arithmetic, logic, branch, compare, call and memory instructions.
<br>
All instruction formats are supported, so implementing the full range of instructions should be trivial.

<h3>Interpreter</h3>
Supported features:
* Read/Write from registers.
* Read/Write from memory.
* Functions.
* "System calls" (calling .NET methods).

Not supported features:
* Program structured in memory according to real programs.
* Exceptions.
* IO.

<h3>Assembler</h3>
Supported features:
* All supported instructions.
* Assembler macros (%lo, %hi, %hiadj).
* Labels.
* Data variables (.word, .byte).

Not supported features:
* Assembler directives: .align.

<h3>JIT Compiler</h3>
The JIT compiler compiles the Nios II instructions to [CIL](http://en.wikipedia.org/wiki/Common_Intermediate_Language) instructions.
<br>
At the moment, it exists two JIT compilers, the "Full" and the "Partial". The JIT compilers
isn't fully integrated to the simulator.
<br>
<h4>Full JIT</h4>
Compiles on a program basis. This compiler will lose some semantics.
<h4>Partial JIT</h4>
Compiles on a function bases running along side the interpreter. This should allow
more correct semantics but still good performance for often called functions.
At the moment only supports calling JITted functions, not calling other JITted functions or
interpreted functions.

<h3>Roadmap</h3>
* Unit tests for full instruction set.
* One-to-one mapping between supported instructions and supported JIT instructions.
* Rewritten assembler.
* User defined macro instructions.
* Constants (.eq).
* Unsigned instructions and better handling of signed and unsigned in the code base.
* Custom assembler (console) program.
* Custom VM (console) program.
