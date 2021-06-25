# ERA COMPILER

![example workflow](https://github.com/egorklementev/era_compiler/actions/workflows/deploy.yaml/badge.svg)

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
* `--prefix`  :  custom filename prefix for output files  
* `--ignconf`  :  ignore configuration file and rely only upon command-line flags  
  
Examples of commands:  
`ERACompiler -h`  -  get help message  
`ERACompiler -s test.txt folder1/file1.java`  -  will compile source files to `compiled_test.txt` and `folder1/compiled_file1.java`  
`ERACompiler --lex -o out.a`  -  will compile default `code.era` file to the `out.a` using only tokenization

### ERA Simulator

Here is the link to the ERA Simulator GitHub repo:  
[https://github.com/egorklementev/era_simulator ](https://github.com/egorklementev/era_simulator)  
  
It is aslo a Windows x64 command-line application with similar arguments and way of usage.  

### Vim syntax highlight
Since no one want to program in the "notepad.exe" (why not?), there is a special `era.vim` file that adds a support of 
ERA language syntax highlighting. You may find it in the ../DOCS folder. How to use Vim and its syntax highlighting, you may find in Google.  
  
The support for VS/VSCode, Intellij IDEA, and etc. will be added later.  

## ATTENTION:

It is highly recommended to read `example.era` file which gives basic understanding of the ERA language.  
[example.era](../master/ERACompiler/ERACompilerUnitTests/example.era)
  
Also, read documentation files in ../DOCS folder for more information about ERA language.
