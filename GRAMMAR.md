# ERA COMPILER GRAMMAR
#### Represents the grammar for the ERA system-level programming language.
  
* **Program** : { Annotations | Data | Module | Code | Structure | Routine }
* **Annotations** : `pragma` PragmaDeclaration { PragmaDeclaration } `end`
* **Data** : `data` Identifier Literal { Literal } `end`
* **Module** : `module` Identifier { VarDeclaration | Routine | Structure } `end`
* **Code** : `code` { Statement } `end`
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
* **Statement** : [ Label ] ( AssemblyBlock | ExtensionStatement )
* **Label** : `<` Identifier `>`
---
* **AssemblyBlock**: `asm` AssemblyStatement `;` { AssemblyStatement `;` } `end`   
* **AssemblyStatement**  
&emsp;: `skip` // NOP  
&emsp;| `stop` // STOP  
&emsp;| `<` Identifier `>` // Label  
&emsp;| `format` ( 8 | 16 | 32 ) // format of next command  
&emsp;| Register `:=` `->` Register  
&emsp;| `->` Register `:=` Register  
&emsp;| Register `:=` Register  
&emsp;| Register `:=` Identifier // Label  
&emsp;| Register `:=` Expression // Should be in range [0-31]  
&emsp;| Register `:=` Register + Expression  
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
* **Loop** : For | While | LoopWhile
* **For** : `for` Identifier [ `from` Expression ] [ `to` Expression] [ `step` Expression ] LoopBody `end`
* **While** : `while` Expression LoopBody `end`
* **LoopWhile** : LoopBody `while` Expression `end`
* **LoopBody** : `loop` BlockBody
* **BlockBody** : { Statement }
* **Break** : `break` `;`
* **Return** : `return` [ ( Expression | Call ) ] `;`
* **Assignment** : Receiver `:=` Expression `;`
* **Swap** : Reveiver `<=>` Receiver `;`
* **Print** : `print` Expression `;`
---
* **If** : `if` Expression `do` BlockBody ( `end` | `else` BlockBody `end` )
* **Call** : Identifier CallArgs
* **CallArgs** : `(` [ Expression { `,` Expression } ] `)`
---
* **Expression** : Operand { Operator Operand }
* **Operator** : `+` | `-` | `*` | `&` | `|` | `^` | `?` | `=` | `/=` | `<` | `>` | `>=` | `<=`
* **Operand** : Primary | Register | Dereference | Reference | Literal | ExplicitAddress | `(` Expression `)`
* **Receiver** : Primary | Dereference | ExplicitAddress | Register
* **Primary** : Identifier { `.` Identifier } [ ArrayAccess | CallArgs ]
* **ArrayAccess** : `[` Expression `]`
* **Reference** : `<-` Primary
* **Dereference** : `->` ( Primary | Register )
* **ExplicitAddress** : `->` Literal
---
* **Identifier** : *(_a-zA-Z0-9)+*
* **Literal**: [ `-` ] *(0-9)+*
* **Text**: *(\\,\\.\\-_a-zA-Z0-9)+*
---
