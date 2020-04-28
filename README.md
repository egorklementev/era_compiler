# ERA COMPILER

This is system-level programming language compiler for processors with ERA architecture.

### Basic architecture
The most abstract architecture of the compiler:  
  
File with code -> Lexical Analyzer ->  
-> Tokens -> Syntax Analyzer ->  
-> Abstract Syntax Tree -> Semantic Analyzer ->  
-> Annotated AST -> Generator ->  
-> File with compiled code  

### Compiler usage
Compiler works as console appilcation.  
The main command is `ERACompiler`.  
There are several arguments available:
* `-h`  :  see help
* `-s {filename}`  :  specify source files to be compiled
* `-o {filename}`  :  specify output files  
* `--lexis`  :  perform tokenization only  
* `--syntax`  :  perform tokenization and AST assembling
* `--semantic`  :  perform tokenization, AST assembling, and AAST assembling
* `--flog`  :  log errors and exceptions to the file  
  
Examples of commands:  
`ERACompiler -h`  -  get help message  
`ERACompiler -s test.txt folder1/file1.java`  -  will compile source files to `compiled_test.txt` and `folder1/compiled_file1.java`  
`ERACompiler --lexis -o out.a`  -  will compile default `code.era` file to the `out.a` using only tokenization
