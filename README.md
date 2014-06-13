Nios II Simulator
======

Simulator for the Nios II processor architecture written in C# for .NET 4.5.

<h3>Features:</h3>
* WPF GUI application.
* Assembler.
* Interpreter.
* JIT compiler (experimental).

<h3>Supported instructions</h3>
The basic arithmetic, logic, branch, call and memory instructions.
<br>
All instruction formats are supported, so implementing the full range of instructions should be trivial.

<h3>JIT Compiler</h3>
The JIT compiler compiles the Nios instructions to [CIL](http://en.wikipedia.org/wiki/Common_Intermediate_Language) instructions.
<br>
At the moment, it exists to JIT compiler, the "Full" and the "Partial".
<br>
The full compiles on a program basis and partial on function basis which means
it can run at the same time the interpreter is running.
<br>
The partial supports atm only being called, not calling other JITted functions or
interpreted functions.
