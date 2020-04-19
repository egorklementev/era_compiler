# ERA COMPILER

This is system-level programming language compiler for processors with ERA architecture.

### Basic architecture
The most abstract architecture of the compiler:\\
\\
File with code -> Lexical Analyzer ->\\
-> Tokens -> Syntax Analyzer ->\\ 
-> Abstract Syntax Tree -> Semantic Analyzer ->\\
-> Annotated AST -> Generator ->\\
-> File with compiled code\\
