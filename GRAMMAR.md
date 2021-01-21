# ERA COMPILER GRAMMAR
#### Represents the grammar for the ERA system-level programming language.
  
* **Program** : { Annotations | Data | Module | Code | Structure | Routine }
* **Annotations** : `pragma` PragmaDeclaration { PragmaDeclaration } `end`
* **Data** : `data` Identifier Literal { Literal } `end`
* **Module** : `module` Identifier { VarDeclaration | Routine | Structure } `end`
* **Code** : `code` { VarDeclaration | Statement } `end`
* **Structure** : `struct` Identifier { VarDeclaration } `end`
---
* **PragmaDeclaration** : Identifier `(` [ `"` Text `"` ] `)`
* **VarDeclaration** : Type ( Variable | Array | Constant )
* **Variable** : VarDefinition { `,` VarDefinition } `;`
* **Constant** : `const` ConstDefinition { `,` ConstDefinition } `;`
* **Array** : `[]` ArrDefinition { `,` ArrDefinition } `;`
* **VarDefinition** : Identifier [ `:=` Expression ]
* **ConstDefinition** : Identifier `:=` Expression
* **ArrDefinition** : Identifier `[` Expression `]`
* **Type** : ( `int` | `short` | `byte` [ `@` ] ) | Identifier 
---
* **Routine** : `routine` Identifier `(` [ Parameters ] `)` [ `:` Type ] RoutineBody 
* **Parameters** : Parameter { `,` Parameter }
* **Parameter** : Type Identifier
* **RoutineBody** : `do` { Statement } `end`
* **Statement** : AssemblyBlock | ExtensionStatement
---
* **AssemblyBlock**: `asm` AssemblyStatement `;` { AssemblyStatement `;` } `end`   
* **AssemblyStatement**  
&emsp;: `skip` // NOP  
&emsp;| `stop` // STOP  
&emsp;| `format` ( 8 | 16 | 32 ) // format of next command  
&emsp;| Register `:=` `->` Register  
&emsp;| `->` Register `:=` Register  
&emsp;| Register `:=` Register  
&emsp;| Register `:=` Expression  
&emsp;| Register `+=` Register  
&emsp;| Register `-=` Register  
&emsp;| Register `>>=` Register  
&emsp;| Register `<<=` Register  
&emsp;| Register `|=` Register  
&emsp;| Register `&=` Register  
&emsp;| Register `^=` Register  
&emsp;| Register `<=` Register  
&emsp;| Register `>=` Register  
&emsp;| Register `?=` Register  
&emsp;| `if` Register `goto` Register  
* **Register** : R0 | R1 | ... | R30 | R31
---
* **ExtensionStatement** : Assignment | Swap | ( Call `;` ) | If | Loop | Break | Return | VarDeclaration
* **Loop** : For | While | LoopBody
* **For** : `for` Identifier [ `from` Expression ] [ `to` Expression] [ `step` Expression ] LoopBody
* **While** : `while` Expression LoopBody
* **LoopBody** : `loop` BlockBody `end`
* **BlockBody** : { Statement }
* **Break** : `break` `;`
* **Return** : `return` [ ( Expression | Call ) ] `;`
* **Assignment** : Receiver `:=` Expression `;`
* **Swap** : Reveiver `<=>` Receiver `;`
---
* **If** : `if` Expression `do` BlockBody ( `end` | `else` BlockBody `end` )
* **Call** : Identifier CallArgs
* **CallArgs** : `(` [ Expression { `,` Expression } ] `)`
---
* **Expression** : Operand { Operator Operand }
* **Operator** : `+` | `-` | `*` | `&` | `|` | `^` | `?` | `=` | `/=` | `<` | `>`
* **Operand** : Primary | Register | Dereference | Reference | Literal | ExplicitAddress | `(` Expression `)`
* **Receiver** : Primary | Dereference | ExplicitAddress | Register
* **Primary** : Identifier { `.` Identifier } [ ArrayAccess | CallArgs ]
* **ArrayAccess** : `[` Expression `]`
* **Reference** : `<-` Identifier
* **Dereference** : `->` ( Identifier | Register )
* **ExplicitAddress** : `->` Literal
---
* **Identifier** : *(_a-zA-Z0-9)+*
* **Literal**: [ `-` ] *(0-9)+*
* **Text**: *(\\,\\.\\-_a-zA-Z0-9)+*
---
