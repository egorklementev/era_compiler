# ERA COMPILER GRAMMAR
Represents the grammar for the ERA system-level programming language.
  
* **Program** : {Annotation | Data | Module | Code | Structure | Routine}
* **Annotation** : `pragma` PragmaDeclaration {`,` PragmaDeclaration} `;`
* **Data** : `data` Identifier [Literal {`,` Literal}] `end`
* **Module** : `module` Identifier {VarDeclaration | Routine | Structure} `end`
* **Code** : `code` {VarDeclaration | Statement} `end`
* **Structure** : `struct` Identifier { VarDeclaration } `end`
---
* **PragmaDeclaration** : Identifier `(` [ Text ] `)`
* **VarDeclaration** : Variable | Constant
* **Variable** : Type VarDefinition { `,` VarDefinition } `;`
* **Type** : `int` | `short` | `byte` | Identifier
* **VarDefinition** : Identifier [ `:=` Expression ] | ArrayDeclaration
* **ArrayDeclaration** : Identifier `[` Expression `]`
* **Constant** : `const` ConstDefinition { `,` ConstDefinition } `;`
* **ConstDefinition** : Identifier `=` Expression
---
* **Routine** : [ Attribute ] `routine` Identifier [ Parameters ] [ Results ] ( `;` | RoutineBody )
* **Attribute** : `start` | `entry`
* **Parameters** : `(` Parameter { `,` Parameter } `)`
* **Parameter** : Type Identifier | Register
* **Results** : `:` Register { `,` Register }
* **RoutineBody** : `do` { VarDeclaration | Statement } `end`
* **Statement** : [ Label ] ( AssemblyBlock | ExtensionStatement )
* **Label** : `<` Identifier `>`
---
* **AssemblyBlock**: `asm`  
&emsp;( AssemblyStatement `;`  
&emsp;| AssemblyStatement { `,` AssemblyStatement } `end` )
* **AssemblyStatement**  
&emsp;: `skip` // NOP  
&emsp;| `stop` // STOP  
&emsp;| `format` ( 8 | 16 | 32 ) // format of next command  
&emsp;| Register `:=` `*`Register // Rj := \*Ri LD    
&emsp;| `*`Register `:=` Register // \*Rj := Ri ST  
&emsp;| Register `:=` Register // Rj := Ri MOV  
&emsp;| Register `:=` Expression // Rj := Const LDC    
&emsp;| Register `+=` Register // Rj += Ri ADD  
&emsp;| Register `-=` Register // Rj -= Ri SUB  
&emsp;| Register `>>=` Register // Rj >>= Ri ASR  
&emsp;| Register `<<=` Register // Rj <<= Ri ASL  
&emsp;| Register `|=` Register // Rj |= Ri OR  
&emsp;| Register `&=` Register // Rj &= Ri AND  
&emsp;| Register `^=` Register // Rj ^= Ri XOR  
&emsp;| Register `<=` Register // Rj <= Ri LSL  
&emsp;| Register `>=` Register // Rj >= Ri LSR  
&emsp;| Register `?=` Register // Rj ?= Ri CND  
&emsp;| `if` Register `goto` Register // if Ri goto Rj CBR
* **Register** : R0 | R1 | ... | R30 | R31
---
* **ExtensionStatement** : Assignment | Swap | Call | If | Loop | Break | Goto
* **Loop** : For | While | LoopBody
* **For** : `for` Identifier [ `from` Expression ] [ `to` Expression] [ `step` Expression ] LoopBody
* **While** : `while` Expression LoopBody
* **LoopBody** : `loop` BlockBody `end`
* **BlockBody** : { Statement }
* **Break** : `break` `;`
* **Goto** : `goto` Identifier `;`
* **Assignment** : Primary `:=` Expression `;`
* **Swap** : Primary `<=>` Primary `;`
---
* **If** : `if` Expression `do` BlockBody ( `end` | `else` BlockBody `end` )
* **Call** : [ Identifier`.` ] Identifier CallArgs `;`
* **CallArgs** : `(` [ Expression { , Expression } ] `)`
---
* **Expression** : Operand [ Operator Operand ]
* **Operator** : `+` | `-` | `*` | `&` | `|` | `^` | `?` | CompOperator
* **CompOperator** : `=` | `/=` | `<` | `>`
* **Operand** : Receiver | Reference | Literal
* **Primary** : Receiver | Dereference | ExplicitAddress
* **Receiver** : Identifier | ArrayAccess | Register | StructAccess
* **ArrayAccess** : Identifier`[`Expression`]`
* **StructAccess** : Identifier`.`[Identifier | StructAccess]
* **Reference** : `&` Identifier
* **Dereference** : `*` ( Identifier | Register )
* **ExplicitAddress** : `*` Literal
---
* **Identifier** : *(_a-zA-Z0-9)+*
* **Literal**: *(0-9)+*
* **Text**: *(\\,\\.\\-_a-zA-Z0-9)+*
---
