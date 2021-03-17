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
* `-d {paths}`  :  specify folders with the code to be compiled (cannot be used with `-o` parameter)  
* `-p`  :  force to create folders if they do not exist  
* `--err`  :  display more detailed error messages  
* `--lex`  :  perform tokenization only  
* `--syn`  :  perform tokenization and AST assembling  
* `--sem`  :  perform tokenization, AST assembling, and AAST assembling  
* `--semext`  :  same as `--sem` with more detailed output  
* `--asm`  :  full compilation with the assembly code output  
  
Examples of commands:  
`ERACompiler -h`  -  get help message  
`ERACompiler -s test.txt folder1/file1.java`  -  will compile source files to `compiled_test.txt` and `folder1/compiled_file1.java`  
`ERACompiler --lex -o out.a`  -  will compile default `code.era` file to the `out.a` using only tokenization
