" ERA syntax file

if exists("b:current_syntax")
  finish
endif


" Keywords
syn keyword eraKeywordStruct struct
syn keyword eraKeywordRoutine routine
syn keyword eraKeywordConditionals if else 
syn keyword eraKeywordLabel break
syn keyword eraKeywords do end return data module code print
syn keyword eraKeywords asm skip stop format goto 
syn keyword eraKeywordRepeat for from to step while loop 
syn keyword eraKeywordPragma pragma

syn keyword eraTypes const int short byte nextgroup=eraTypeTails
syn match eraTypeTails '\(int\|short\|byte\)\@<=@'
syn match eraTypeTails '\(int\|short\|byte\)\@<=\[\]'
syn match eraTypeTails '\(int\|short\|byte\)\@<=@\[\]'

syn region eraString start='"' end='"'
"syn region eraPragmaRegion start='(' end=')' contains=eraString

syn match eraOperators '\v-'
syn match eraOperators '+' 
syn match eraOperators '-'
syn match eraOperators '*'
syn match eraOperators '>' 
syn match eraOperators '<'
syn match eraOperators '>='
syn match eraOperators '<='
syn match eraOperators 'skip'
syn match eraOperators 'stop' 
syn match eraOperators 'format'
syn match eraOperators '+='
syn match eraOperators '-='
syn match eraOperators '>>='
syn match eraOperators '<<='
syn match eraOperators '|='
syn match eraOperators '&='
syn match eraOperators '^='
syn match eraOperators '?='
syn match eraOperators '|'
syn match eraOperators '&'
syn match eraOperators '?'
syn match eraOperators '^'
syn match eraOperators '/='
syn match eraOperators '<-'
syn match eraOperators '->'

syn match eraIdentifier '[_a-zA-Z][_a-zA-Z0-9]*'

syn match eraNumber '[+-]\?\d\+'

syn match eraRegister 'R0'
syn match eraRegister 'r0'
syn match eraRegister 'R1'
syn match eraRegister 'r1'
syn match eraRegister 'R2'
syn match eraRegister 'r2'
syn match eraRegister 'R3'
syn match eraRegister 'r3'
syn match eraRegister 'R4'
syn match eraRegister 'r4'
syn match eraRegister 'R5'
syn match eraRegister 'r5'
syn match eraRegister 'R6'
syn match eraRegister 'r6'
syn match eraRegister 'R7'
syn match eraRegister 'r7'
syn match eraRegister 'R8'
syn match eraRegister 'r8'
syn match eraRegister 'R9'
syn match eraRegister 'r9'
syn match eraRegister 'R10'
syn match eraRegister 'r10'
syn match eraRegister 'R11'
syn match eraRegister 'r11'
syn match eraRegister 'R12'
syn match eraRegister 'r12'
syn match eraRegister 'R13'
syn match eraRegister 'r13'
syn match eraRegister 'R14'
syn match eraRegister 'r14'
syn match eraRegister 'R15'
syn match eraRegister 'r15'
syn match eraRegister 'R16'
syn match eraRegister 'r16'
syn match eraRegister 'R17'
syn match eraRegister 'r17'
syn match eraRegister 'R18'
syn match eraRegister 'r18'
syn match eraRegister 'R19'
syn match eraRegister 'r19'
syn match eraRegister 'R20'
syn match eraRegister 'r20'
syn match eraRegister 'R21'
syn match eraRegister 'r21'
syn match eraRegister 'R22'
syn match eraRegister 'r22'
syn match eraRegister 'R23'
syn match eraRegister 'r23'
syn match eraRegister 'R24'
syn match eraRegister 'r24'
syn match eraRegister 'R25'
syn match eraRegister 'r25'
syn match eraRegister 'R26'
syn match eraRegister 'r26'
syn match eraRegister 'R27'
syn match eraRegister 'r27'
syn match eraRegister 'FP'
syn match eraRegister 'fp'
syn match eraRegister 'SP'
syn match eraRegister 'sp'
syn match eraRegister 'SB'
syn match eraRegister 'sb'
syn match eraRegister 'PC'
syn match eraRegister 'pc'

syn keyword eraTodo TODO FIXME
syn match eraComment "//.*$" contains=eraTodo

let b:current_syntax = "era"

hi def link eraComment Comment
hi def link eraNumber Number
hi def link eraString String
hi def link eraRegister StorageClass
hi def link eraIdentifier Identifier
hi def link eraKeywordRoutine Function
hi def link eraKeywordStruct Structure
hi def link eraKeywordConditionals Conditional
hi def link eraKeywordRepeat Repeat
hi def link eraKeywordPragma Include
hi def link eraKeywordLabel Label
hi def link eraKeywords Keyword
hi def link eraOperators Operator
hi def link eraTypes Type
hi def link eraTypeTails Type
hi def link eraTodo Todo
