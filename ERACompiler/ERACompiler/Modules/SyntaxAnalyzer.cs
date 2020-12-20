using System.Collections.Generic;
using ERACompiler.Structures;
using ERACompiler.Structures.Rules;
using ERACompiler.Utilities;

namespace ERACompiler.Modules
{
    /// <summary>
    /// The purpose of this class it to check the code for syntax errors and build Abstract Syntax Tree that can be used by Semantic Analyzer.
    /// </summary>
    public class SyntaxAnalyzer
    {
        private readonly SyntaxRule programRule; // The main rule of the syntax

        /// <summary>
        /// Used for initialization of some variables.
        /// </summary>
        public SyntaxAnalyzer()
        {
            // All the tokens that can appear in the program

            // Keywords
            Token tAsm = new Token(TokenType.KEYWORD, "asm", new TokenPosition(0, 0));
            Token tBreak = new Token(TokenType.KEYWORD, "break", new TokenPosition(0, 0));
            Token tByte = new Token(TokenType.KEYWORD, "byte", new TokenPosition(0, 0));
            Token tCode = new Token(TokenType.KEYWORD, "code", new TokenPosition(0, 0));
            Token tConst = new Token(TokenType.KEYWORD, "const", new TokenPosition(0, 0));
            Token tData = new Token(TokenType.KEYWORD, "data", new TokenPosition(0, 0));
            Token tDo = new Token(TokenType.KEYWORD, "do", new TokenPosition(0, 0));
            Token tElse = new Token(TokenType.KEYWORD, "else", new TokenPosition(0, 0));
            Token tEnd = new Token(TokenType.KEYWORD, "end", new TokenPosition(0, 0));
            Token tFor = new Token(TokenType.KEYWORD, "for", new TokenPosition(0, 0));
            Token tFormat = new Token(TokenType.KEYWORD, "format", new TokenPosition(0, 0));
            Token tFrom = new Token(TokenType.KEYWORD, "from", new TokenPosition(0, 0));
            Token tGoto = new Token(TokenType.KEYWORD, "goto", new TokenPosition(0, 0));
            Token tIf = new Token(TokenType.KEYWORD, "if", new TokenPosition(0, 0));
            Token tInt = new Token(TokenType.KEYWORD, "int", new TokenPosition(0, 0));
            Token tLoop = new Token(TokenType.KEYWORD, "loop", new TokenPosition(0, 0));
            Token tModule = new Token(TokenType.KEYWORD, "module", new TokenPosition(0, 0));
            Token tPragma = new Token(TokenType.KEYWORD, "pragma", new TokenPosition(0, 0));
            Token tReturn = new Token(TokenType.KEYWORD, "return", new TokenPosition(0, 0));
            Token tRoutine = new Token(TokenType.KEYWORD, "routine", new TokenPosition(0, 0));
            Token tShort = new Token(TokenType.KEYWORD, "short", new TokenPosition(0, 0));
            Token tSkip = new Token(TokenType.KEYWORD, "skip", new TokenPosition(0, 0));
            Token tStep = new Token(TokenType.KEYWORD, "step", new TokenPosition(0, 0));
            Token tStop = new Token(TokenType.KEYWORD, "stop", new TokenPosition(0, 0));
            Token tStruct = new Token(TokenType.KEYWORD, "struct", new TokenPosition(0, 0));
            Token tTo = new Token(TokenType.KEYWORD, "to", new TokenPosition(0, 0));
            Token tWhile = new Token(TokenType.KEYWORD, "while", new TokenPosition(0, 0));


            // Itentifiers / Literals / Registers
            Token tIdentifier = new Token(TokenType.IDENTIFIER, "SOME_IDENTIFIER", new TokenPosition(0, 0));
            Token tLiteral = new Token(TokenType.NUMBER, "SOME_LITERAL", new TokenPosition(0, 0));
            Token tRegister = new Token(TokenType.REGISTER, "SOME_REGISTER", new TokenPosition(0, 0));

            // Delimiters
            Token tAt = new Token(TokenType.DELIMITER, "@", new TokenPosition(0, 0));
            Token tColon = new Token(TokenType.DELIMITER, ":", new TokenPosition(0, 0));
            Token tComma = new Token(TokenType.DELIMITER, ",", new TokenPosition(0, 0));
            Token tDot = new Token(TokenType.DELIMITER, ".", new TokenPosition(0, 0));
            Token tLeftBracket = new Token(TokenType.DELIMITER, "[", new TokenPosition(0, 0));
            Token tLeftParen = new Token(TokenType.DELIMITER, "(", new TokenPosition(0, 0));
            Token tRightBracket = new Token(TokenType.DELIMITER, "]", new TokenPosition(0, 0));
            Token tRightParen = new Token(TokenType.DELIMITER, ")", new TokenPosition(0, 0));
            Token tSemicolon = new Token(TokenType.DELIMITER, ";", new TokenPosition(0, 0));
            Token tQuote = new Token(TokenType.DELIMITER, "\"", new TokenPosition(0, 0));

            // Operators
            Token tAnd = new Token(TokenType.OPERATOR, "&", new TokenPosition(0, 0));
            Token tAndEquals = new Token(TokenType.OPERATOR, "&=", new TokenPosition(0, 0));
            Token tAsr = new Token(TokenType.OPERATOR, ">>=", new TokenPosition(0, 0));
            Token tAsl = new Token(TokenType.OPERATOR, "<<=", new TokenPosition(0, 0));
            Token tAssign = new Token(TokenType.OPERATOR, ":=", new TokenPosition(0, 0));
            Token tCnd = new Token(TokenType.OPERATOR, "?", new TokenPosition(0, 0));
            Token tCndEquals = new Token(TokenType.OPERATOR, "?=", new TokenPosition(0, 0));
            Token tEquals = new Token(TokenType.OPERATOR, "=", new TokenPosition(0, 0));
            Token tGreater = new Token(TokenType.OPERATOR, ">", new TokenPosition(0, 0));
            Token tLess = new Token(TokenType.OPERATOR, "<", new TokenPosition(0, 0));
            Token tLsl = new Token(TokenType.OPERATOR, "<=", new TokenPosition(0, 0));
            Token tLsr = new Token(TokenType.OPERATOR, ">=", new TokenPosition(0, 0));
            Token tMinus = new Token(TokenType.OPERATOR, "-", new TokenPosition(0, 0));
            Token tMinusEquals = new Token(TokenType.OPERATOR, "-=", new TokenPosition(0, 0));
            Token tMult = new Token(TokenType.OPERATOR, "*", new TokenPosition(0, 0));
            Token tNotEquals = new Token(TokenType.OPERATOR, "/=", new TokenPosition(0, 0));
            Token tOr = new Token(TokenType.OPERATOR, "|", new TokenPosition(0, 0));
            Token tOrEquals = new Token(TokenType.OPERATOR, "|=", new TokenPosition(0, 0));
            Token tPlus = new Token(TokenType.OPERATOR, "+", new TokenPosition(0, 0));
            Token tPlusEquals = new Token(TokenType.OPERATOR, "+=", new TokenPosition(0, 0));
            Token tSwap = new Token(TokenType.OPERATOR, "<=>", new TokenPosition(0, 0));
            Token tTakeAddr = new Token(TokenType.OPERATOR, "<-", new TokenPosition(0, 0));
            Token tTakeVal = new Token(TokenType.OPERATOR, "->", new TokenPosition(0, 0));
            Token tXor = new Token(TokenType.OPERATOR, "^", new TokenPosition(0, 0));
            Token tXorEquals = new Token(TokenType.OPERATOR, "^=", new TokenPosition(0, 0));

            // All terminal syntax rules of the language
            RuleTerminal kAsmRule = new RuleTerminal(tAsm);
            RuleTerminal kBreakRule = new RuleTerminal(tBreak);
            RuleTerminal kByteRule = new RuleTerminal(tByte);
            RuleTerminal kCodeRule = new RuleTerminal(tCode);
            RuleTerminal kConstRule = new RuleTerminal(tConst);
            RuleTerminal kDataRule = new RuleTerminal(tData);
            RuleTerminal kDoRule = new RuleTerminal(tDo);
            RuleTerminal kElseRule = new RuleTerminal(tElse);
            RuleTerminal kEndRule = new RuleTerminal(tEnd);
            RuleTerminal kForRule = new RuleTerminal(tFor);
            RuleTerminal kFormatRule = new RuleTerminal(tFormat);
            RuleTerminal kFromRule = new RuleTerminal(tFrom);
            RuleTerminal kGotoRule = new RuleTerminal(tGoto);
            RuleTerminal kIfRule = new RuleTerminal(tIf);
            RuleTerminal kIntRule = new RuleTerminal(tInt);
            RuleTerminal kLoopRule = new RuleTerminal(tLoop);
            RuleTerminal kModuleRule = new RuleTerminal(tModule);
            RuleTerminal kPragmaRule = new RuleTerminal(tPragma);
            RuleTerminal kReturnRule = new RuleTerminal(tReturn);
            RuleTerminal kRoutineRule = new RuleTerminal(tRoutine);
            RuleTerminal kShortRule = new RuleTerminal(tShort);
            RuleTerminal kSkipRule = new RuleTerminal(tSkip);
            RuleTerminal kStepRule = new RuleTerminal(tStep);
            RuleTerminal kStopRule = new RuleTerminal(tStop);
            RuleTerminal kStructRule = new RuleTerminal(tStruct);
            RuleTerminal kToRule = new RuleTerminal(tTo);
            RuleTerminal kWhileRule = new RuleTerminal(tWhile);

            RuleTerminal identifierRule = new RuleTerminal(tIdentifier);
            RuleTerminal literalRule = new RuleTerminal(tLiteral);
            RuleTerminal registerRule = new RuleTerminal(tRegister);

            RuleTerminal atRule = new RuleTerminal(tAt);
            RuleTerminal colonRule = new RuleTerminal(tColon);
            RuleTerminal commaRule = new RuleTerminal(tComma);
            RuleTerminal dotRule = new RuleTerminal(tDot);
            RuleTerminal leftBracketRule = new RuleTerminal(tLeftBracket);
            RuleTerminal leftParenRule = new RuleTerminal(tLeftParen);
            RuleTerminal rightBracketRule = new RuleTerminal(tRightBracket);
            RuleTerminal rightParenRule = new RuleTerminal(tRightParen);
            RuleTerminal semicolonRule = new RuleTerminal(tSemicolon);
            RuleTerminal quoteRule = new RuleTerminal(tQuote);

            RuleTerminal opAndRule = new RuleTerminal(tAnd);
            RuleTerminal opAndEqualsRule = new RuleTerminal(tAndEquals);
            RuleTerminal opAslRule = new RuleTerminal(tAsl);
            RuleTerminal opAsrRule = new RuleTerminal(tAsr);
            RuleTerminal opAssignRule = new RuleTerminal(tAssign);
            RuleTerminal opCndRule = new RuleTerminal(tCnd);
            RuleTerminal opCndEqualsRule = new RuleTerminal(tCndEquals);
            RuleTerminal opEqualsRule = new RuleTerminal(tEquals);
            RuleTerminal opGreaterRule = new RuleTerminal(tGreater);
            RuleTerminal opLessRule = new RuleTerminal(tLess);
            RuleTerminal opLslRule = new RuleTerminal(tLsl);
            RuleTerminal opLsrRule = new RuleTerminal(tLsr);
            RuleTerminal opMinusRule = new RuleTerminal(tMinus);
            RuleTerminal opMinusEqualsRule = new RuleTerminal(tMinusEquals);
            RuleTerminal opMultRule = new RuleTerminal(tMult);
            RuleTerminal opNotEqualsRule = new RuleTerminal(tNotEquals);
            RuleTerminal opOrRule = new RuleTerminal(tOr);
            RuleTerminal opOrEqualsRule = new RuleTerminal(tOrEquals);
            RuleTerminal opPlusRule = new RuleTerminal(tPlus);
            RuleTerminal opPlusEqualsRule = new RuleTerminal(tPlusEquals);
            RuleTerminal opSwapRule = new RuleTerminal(tSwap);
            RuleTerminal opTakeAddrRule = new RuleTerminal(tTakeAddr);
            RuleTerminal opTakeValRule = new RuleTerminal(tTakeVal);
            RuleTerminal opXorRule = new RuleTerminal(tXor);
            RuleTerminal opXorEqualsRule = new RuleTerminal(tXorEquals);


            // All syntax rules of the language
            SyntaxRule operatorRule = new SyntaxRule()
                .SetName("Operator")
                .SetType(SyntaxRule.SyntaxRuleType.OR)
                .AddRule(opPlusRule)
                .AddRule(opMinusRule)
                .AddRule(opMultRule)
                .AddRule(opAndRule)
                .AddRule(opOrRule)
                .AddRule(opXorRule)
                .AddRule(opCndRule)
                .AddRule(opEqualsRule)
                .AddRule(opNotEqualsRule)
                .AddRule(opGreaterRule)
                .AddRule(opLessRule);

            SyntaxRule referenceRule = new SyntaxRule()
                .SetName("Reference")
                .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                .AddRule(opTakeAddrRule)
                .AddRule(identifierRule);

            SyntaxRule primaryRule = new SyntaxRule()
                .SetName("Primary")
                .SetType(SyntaxRule.SyntaxRuleType.OR);
            // other rules are added after 'Expression' rule

            SyntaxRule explicitAddrRule = new SyntaxRule()
                .SetName("Explicit address")
                .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                .AddRule(opTakeValRule)
                .AddRule(literalRule);

            SyntaxRule dereferenceRule = new SyntaxRule()
                .SetName("Dereference")
                .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                .AddRule(opTakeValRule)
                .AddRule(
                    new SyntaxRule()
                    .SetName("Identifier | Register")
                    .SetType(SyntaxRule.SyntaxRuleType.OR)
                    .AddRule(identifierRule)
                    .AddRule(registerRule)
                );

            SyntaxRule operandRule = new SyntaxRule()
                .SetName("Operand")
                .SetType(SyntaxRule.SyntaxRuleType.OR)
                .AddRule(primaryRule)
                .AddRule(dereferenceRule)
                .AddRule(referenceRule)
                .AddRule(explicitAddrRule)
                .AddRule(literalRule);

            SyntaxRule expressionRule = new SyntaxRule()
                .SetName("Expression")
                .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                .AddRule(operandRule)
                .AddRule(
                    new SyntaxRule()
                    .SetName("{ Operator Operand }")
                    .SetType(SyntaxRule.SyntaxRuleType.ZERO_OR_MORE)
                    .AddRule(operatorRule)
                    .AddRule(operandRule)
                );

            primaryRule
                .AddRule(
                    new SyntaxRule()
                    .SetName("Identifier { '.' Identifier } [ '[' Expression ']' ]")
                    .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                    .AddRule(identifierRule)
                    .AddRule(
                        new SyntaxRule()
                        .SetName("{ '.' Identifier }")
                        .SetType(SyntaxRule.SyntaxRuleType.ZERO_OR_MORE)
                        .AddRule(dotRule)
                        .AddRule(identifierRule)
                        )
                    .AddRule(
                        new SyntaxRule()
                        .SetName("[ '[' Expression ']' ]")
                        .SetType(SyntaxRule.SyntaxRuleType.ZERO_OR_ONE)
                        .AddRule(leftBracketRule)
                        .AddRule(expressionRule)
                        .AddRule(rightBracketRule)
                        )
                )
                .AddRule(registerRule);

            SyntaxRule typeRule = new SyntaxRule()
                .SetName("Type")
                .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                .AddRule(
                    new SyntaxRule()
                    .SetName("int | byte | short | SOME_IDENTIFIER")
                    .SetType(SyntaxRule.SyntaxRuleType.OR)
                    .AddRule(kIntRule)
                    .AddRule(kShortRule)
                    .AddRule(kByteRule)
                    .AddRule(identifierRule)
                )
                .AddRule(
                    new SyntaxRule()
                    .SetName("[ [] | @ ]")
                    .SetType(SyntaxRule.SyntaxRuleType.ZERO_OR_ONE)
                    .AddRule(
                        new SyntaxRule()
                        .SetName("[] | @")
                        .SetType(SyntaxRule.SyntaxRuleType.OR)
                        .AddRule(atRule)
                        .AddRule(
                            new SyntaxRule()
                            .SetName("[]")
                            .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                            .AddRule(leftBracketRule)
                            .AddRule(rightBracketRule)
                            )
                        )
                );

            SyntaxRule constDefinitionRule = new SyntaxRule()
                .SetName("Constant definition")
                .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                .AddRule(identifierRule)
                .AddRule(opAssignRule)
                .AddRule(expressionRule)
                ;

            SyntaxRule constantRule = new SyntaxRule()
                .SetName("Constant")
                .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                .AddRule(kConstRule)
                .AddRule(typeRule)
                .AddRule(constDefinitionRule)
                .AddRule(
                    new SyntaxRule()
                    .SetName("Constant definitions")
                    .SetType(SyntaxRule.SyntaxRuleType.ZERO_OR_MORE)
                    .AddRule(commaRule)
                    .AddRule(constDefinitionRule)
                )
                .AddRule(semicolonRule);

            SyntaxRule varDefinitionRule = new SyntaxRule()
                .SetName("Variable definition")
                .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                .AddRule(identifierRule)
                .AddRule(
                    new SyntaxRule()
                    .SetName("[ ( := Expression ) | ( '[' Expression ']' ) ]")
                    .SetType(SyntaxRule.SyntaxRuleType.ZERO_OR_ONE)
                    .AddRule(
                        new SyntaxRule()
                        .SetName("( := Expression ) | ( '[' Expression ']' )")
                        .SetType(SyntaxRule.SyntaxRuleType.OR)
                        .AddRule(
                            new SyntaxRule()
                            .SetName(":= Expression")
                            .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                            .AddRule(opAssignRule)
                            .AddRule(expressionRule)
                            )
                        .AddRule(
                            new SyntaxRule()
                            .SetName("'[' Expression ']'")
                            .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                            .AddRule(leftBracketRule)
                            .AddRule(expressionRule)
                            .AddRule(rightBracketRule)
                            )
                        )
                );

            SyntaxRule variableRule = new SyntaxRule()
                .SetName("Variable")
                .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                .AddRule(typeRule)
                .AddRule(varDefinitionRule)
                .AddRule(
                    new SyntaxRule()
                    .SetName("Variable definitions")
                    .SetType(SyntaxRule.SyntaxRuleType.ZERO_OR_MORE)
                    .AddRule(commaRule)
                    .AddRule(varDefinitionRule)
                )
                .AddRule(semicolonRule);

            SyntaxRule varDeclarationRule = new SyntaxRule()
                .SetName("Variable declaration")
                .SetType(SyntaxRule.SyntaxRuleType.OR)
                .AddRule(variableRule)
                .AddRule(constantRule);

            SyntaxRule assemblyStatementRule = new SyntaxRule()
                .SetName("Assembly statement")
                .SetType(SyntaxRule.SyntaxRuleType.OR)
                .AddRule(kSkipRule)
                .AddRule(kStopRule)
                .AddRule(
                    new SyntaxRule()
                    .SetName("format ( 8 | 16 | 32 )")
                    .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                    .AddRule(kFormatRule)
                    .AddRule(literalRule) // The actual check of a number is done in the SemanticsAnalyzer
                )
                .AddRule(
                    new SyntaxRule()
                    .SetName("Register := -> Register")
                    .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                    .AddRule(registerRule)
                    .AddRule(opAssignRule)
                    .AddRule(opTakeValRule)
                    .AddRule(registerRule)
                )
                .AddRule(
                    new SyntaxRule()
                    .SetName("-> Register := Register")
                    .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                    .AddRule(opTakeValRule)
                    .AddRule(registerRule)
                    .AddRule(opAssignRule)
                    .AddRule(registerRule)
                )
                .AddRule(
                    new SyntaxRule()
                    .SetName("Register := Register")
                    .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                    .AddRule(registerRule)
                    .AddRule(opAssignRule)
                    .AddRule(registerRule)
                )
                .AddRule(
                    new SyntaxRule()
                    .SetName("Register := Expression")
                    .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                    .AddRule(registerRule)
                    .AddRule(opAssignRule)
                    .AddRule(expressionRule)
                ).AddRule(
                    new SyntaxRule()
                    .SetName("Register += Register")
                    .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                    .AddRule(registerRule)
                    .AddRule(opPlusEqualsRule)
                    .AddRule(registerRule)
                ).AddRule(
                    new SyntaxRule()
                    .SetName("Register -= Register")
                    .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                    .AddRule(registerRule)
                    .AddRule(opMinusEqualsRule)
                    .AddRule(registerRule)
                ).AddRule(
                    new SyntaxRule()
                    .SetName("Register >>= Register")
                    .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                    .AddRule(registerRule)
                    .AddRule(opAsrRule)
                    .AddRule(registerRule)
                ).AddRule(
                    new SyntaxRule()
                    .SetName("Register <<= Register")
                    .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                    .AddRule(registerRule)
                    .AddRule(opAslRule)
                    .AddRule(registerRule)
                ).AddRule(
                    new SyntaxRule()
                    .SetName("Register |= Register")
                    .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                    .AddRule(registerRule)
                    .AddRule(opOrEqualsRule)
                    .AddRule(registerRule)
                ).AddRule(
                    new SyntaxRule()
                    .SetName("Register &= Register")
                    .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                    .AddRule(registerRule)
                    .AddRule(opAndEqualsRule)
                    .AddRule(registerRule)
                ).AddRule(
                    new SyntaxRule()
                    .SetName("Register ^= Register")
                    .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                    .AddRule(registerRule)
                    .AddRule(opXorEqualsRule)
                    .AddRule(registerRule)
                ).AddRule(
                    new SyntaxRule()
                    .SetName("Register >= Register")
                    .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                    .AddRule(registerRule)
                    .AddRule(opLsrRule)
                    .AddRule(registerRule)
                ).AddRule(
                    new SyntaxRule()
                    .SetName("Register <= Register")
                    .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                    .AddRule(registerRule)
                    .AddRule(opLslRule)
                    .AddRule(registerRule)
                ).AddRule(
                    new SyntaxRule()
                    .SetName("Register ?= Register")
                    .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                    .AddRule(registerRule)
                    .AddRule(opCndEqualsRule)
                    .AddRule(registerRule)
                ).AddRule(
                    new SyntaxRule()
                    .SetName("if Register goto Register")
                    .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                    .AddRule(kIfRule)
                    .AddRule(registerRule)
                    .AddRule(kGotoRule)
                    .AddRule(registerRule)
                );

            SyntaxRule assemblyBlockRule = new SyntaxRule()
                .SetName("Assembly block")
                .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                .AddRule(kAsmRule)
                .AddRule(assemblyStatementRule)
                .AddRule(semicolonRule)
                .AddRule(
                    new SyntaxRule()
                    .SetName("{ AssemblyStatement ; }")
                    .SetType(SyntaxRule.SyntaxRuleType.ZERO_OR_MORE)
                    .AddRule(assemblyStatementRule)
                    .AddRule(semicolonRule)
                )
                .AddRule(kEndRule);
            
            SyntaxRule receiverRule = new SyntaxRule()
                .SetName("Receiver")
                .SetType(SyntaxRule.SyntaxRuleType.OR)
                .AddRule(primaryRule)
                .AddRule(dereferenceRule)
                .AddRule(explicitAddrRule);

            SyntaxRule assignmentRule = new SyntaxRule()
                .SetName("Assignment")
                .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                .AddRule(receiverRule)
                .AddRule(opAssignRule)
                .AddRule(expressionRule)
                .AddRule(semicolonRule);

            SyntaxRule swapRule = new SyntaxRule()
                .SetName("Swap")
                .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                .AddRule(receiverRule)
                .AddRule(opSwapRule)
                .AddRule(receiverRule)
                .AddRule(semicolonRule);

            SyntaxRule callArgsRule = new SyntaxRule()
                .SetName("Call arguments")
                .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                .AddRule(leftParenRule)
                .AddRule(
                    new SyntaxRule()
                    .SetName("[ Expression { , Expression } ]")
                    .SetType(SyntaxRule.SyntaxRuleType.ZERO_OR_ONE)
                    .AddRule(expressionRule)
                    .AddRule(
                        new SyntaxRule()
                        .SetName("{ , Expression }")
                        .SetType(SyntaxRule.SyntaxRuleType.ZERO_OR_MORE)
                        .AddRule(commaRule)
                        .AddRule(expressionRule)
                        )
                )
                .AddRule(rightParenRule);

            SyntaxRule callRule = new SyntaxRule()
                .SetName("Call")
                .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                .AddRule(identifierRule)
                .AddRule(callArgsRule)
                .AddRule(semicolonRule);

            SyntaxRule blockBodyRule = new SyntaxRule()
                .SetName("Block body")
                .SetType(SyntaxRule.SyntaxRuleType.ZERO_OR_MORE)
                ;
            // other rulesadded after 'Statement' rule

            SyntaxRule ifRule = new SyntaxRule()
                .SetName("If")
                .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                .AddRule(kIfRule)
                .AddRule(expressionRule)
                .AddRule(kDoRule)
                .AddRule(blockBodyRule)
                .AddRule(
                    new SyntaxRule()
                    .SetName("end | else BlockBody end")
                    .SetType(SyntaxRule.SyntaxRuleType.OR)
                    .AddRule(kEndRule)
                    .AddRule(
                        new SyntaxRule()
                        .SetName("else BlockBody end")
                        .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                        .AddRule(kElseRule)
                        .AddRule(blockBodyRule)
                        .AddRule(kEndRule)
                        )
                );

            SyntaxRule loopBodyRule = new SyntaxRule()
                .SetName("Loob body")
                .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                .AddRule(kLoopRule)
                .AddRule(blockBodyRule)
                .AddRule(kEndRule);

            SyntaxRule whileRule = new SyntaxRule()
                .SetName("While")
                .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                .AddRule(kWhileRule)
                .AddRule(expressionRule)
                .AddRule(loopBodyRule);

            SyntaxRule forRule = new SyntaxRule()
                .SetName("For")
                .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                .AddRule(kForRule)
                .AddRule(identifierRule)
                .AddRule(
                    new SyntaxRule()
                    .SetName("[ from Expression ]")
                    .SetType(SyntaxRule.SyntaxRuleType.ZERO_OR_ONE)
                    .AddRule(kFromRule)
                    .AddRule(expressionRule)
                )
                .AddRule(
                    new SyntaxRule()
                    .SetName("[ to Expression ]")
                    .SetType(SyntaxRule.SyntaxRuleType.ZERO_OR_ONE)
                    .AddRule(kToRule)
                    .AddRule(expressionRule)
                    )
                .AddRule(
                    new SyntaxRule()
                    .SetName("[ step Expression ]")
                    .SetType(SyntaxRule.SyntaxRuleType.ZERO_OR_ONE)
                    .AddRule(kStepRule)
                    .AddRule(expressionRule)
                )
                .AddRule(loopBodyRule);

            SyntaxRule loopRule = new SyntaxRule()
                .SetName("Loop")
                .SetType(SyntaxRule.SyntaxRuleType.OR)
                .AddRule(forRule)
                .AddRule(whileRule)
                .AddRule(loopBodyRule);

            SyntaxRule breakRule = new SyntaxRule()
                .SetName("Break")
                .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                .AddRule(kBreakRule)
                .AddRule(semicolonRule);

            SyntaxRule returnRule = new SyntaxRule()
                .SetName("Return")
                .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                .AddRule(kReturnRule)
                .AddRule(
                    new SyntaxRule()
                    .SetName("( Expression ; | Call ) | ; ")
                    .SetType(SyntaxRule.SyntaxRuleType.OR)
                    .AddRule(
                        new SyntaxRule()
                        .SetName("Expression ; | Call")
                        .SetType(SyntaxRule.SyntaxRuleType.OR)
                        .AddRule(
                            new SyntaxRule()
                            .SetName("Expression ;")
                            .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                            .AddRule(expressionRule)
                            .AddRule(semicolonRule)
                            )
                        .AddRule(callRule)
                        )
                    .AddRule(semicolonRule)
                );

            SyntaxRule extensionStatementRule = new SyntaxRule()
                .SetName("Extension statement")
                .SetType(SyntaxRule.SyntaxRuleType.OR)
                .AddRule(assignmentRule)
                .AddRule(swapRule)
                .AddRule(callRule)
                .AddRule(ifRule)
                .AddRule(loopRule)
                .AddRule(breakRule)
                .AddRule(returnRule);

            SyntaxRule statementRule = new SyntaxRule()
                .SetName("Statement")
                .SetType(SyntaxRule.SyntaxRuleType.OR)
                .AddRule(assemblyBlockRule)
                .AddRule(extensionStatementRule);

            blockBodyRule
                .AddRule(statementRule);

            SyntaxRule routineBodyRule = new SyntaxRule()
                .SetName("Routine body")
                .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                .AddRule(kDoRule)
                .AddRule(
                    new SyntaxRule()
                    .SetName("{ VarDeclaration | Statement }")
                    .SetType(SyntaxRule.SyntaxRuleType.ZERO_OR_MORE)
                    .AddRule(
                        new SyntaxRule()
                        .SetName("VarDeclaration | Statement")
                        .SetType(SyntaxRule.SyntaxRuleType.OR)
                        .AddRule(varDeclarationRule)
                        .AddRule(statementRule)
                        )
                )
                .AddRule(kEndRule);

            SyntaxRule parameterRule = new SyntaxRule()
                .SetName("Parameter")
                .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                .AddRule(typeRule)
                .AddRule(identifierRule);

            SyntaxRule parametersRule = new SyntaxRule()
                .SetName("Parameters")
                .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                .AddRule(leftParenRule)
                .AddRule(
                    new SyntaxRule()
                    .SetName("[ Parameter { , Parameter } ]")
                    .SetType(SyntaxRule.SyntaxRuleType.ZERO_OR_ONE)
                    .AddRule(parameterRule)
                    .AddRule(
                        new SyntaxRule()
                        .SetName("{ , Parameter }")
                        .SetType(SyntaxRule.SyntaxRuleType.ZERO_OR_MORE)
                        .AddRule(commaRule)
                        .AddRule(parameterRule)
                        )
                )
                .AddRule(rightParenRule);

            SyntaxRule routineRule = new SyntaxRule()
                .SetName("Routine")
                .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                .AddRule(kRoutineRule)
                .AddRule(identifierRule)
                .AddRule(
                    new SyntaxRule()
                    .SetName("[ Parameters ]")
                    .SetType(SyntaxRule.SyntaxRuleType.ZERO_OR_ONE)
                    .AddRule(parametersRule)
                )
                .AddRule(
                    new SyntaxRule()
                        .SetName("[ : Type ]")
                        .SetType(SyntaxRule.SyntaxRuleType.ZERO_OR_ONE)
                        .AddRule(colonRule)
                        .AddRule(typeRule)
                )
                .AddRule(routineBodyRule);

            SyntaxRule structureRule = new SyntaxRule()
                .SetName("Structure")
                .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                .AddRule(kStructRule)
                .AddRule(identifierRule)
                .AddRule(
                    new SyntaxRule()
                    .SetName("{ Variable declarations }")
                    .SetType(SyntaxRule.SyntaxRuleType.ZERO_OR_MORE)
                    .AddRule(varDeclarationRule)
                )
                .AddRule(kEndRule);

            SyntaxRule moduleRule = new SyntaxRule()
                .SetName("Module")
                .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                .AddRule(kModuleRule)
                .AddRule(identifierRule)
                .AddRule(
                    new SyntaxRule()
                    .SetName("Module statements")
                    .SetType(SyntaxRule.SyntaxRuleType.ZERO_OR_MORE)
                    .AddRule(
                        new SyntaxRule()
                        .SetName("Some module statement")
                        .SetType(SyntaxRule.SyntaxRuleType.OR)
                        .AddRule(varDeclarationRule)
                        .AddRule(routineRule)
                        .AddRule(structureRule)
                        )
                    )
                .AddRule(kEndRule);

            SyntaxRule dataRule = new SyntaxRule()
                .SetName("Data")
                .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                .AddRule(kDataRule)
                .AddRule(identifierRule)
                .AddRule(literalRule)
                .AddRule(
                    new SyntaxRule()
                    .SetName("Data literals")
                    .SetType(SyntaxRule.SyntaxRuleType.ZERO_OR_MORE)
                    .AddRule(literalRule)
                )
                .AddRule(kEndRule);

            SyntaxRule pragmaDeclRule = new SyntaxRule()
                .SetName("Pragma declaration")
                .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                .AddRule(identifierRule)
                .AddRule(leftParenRule)
                .AddRule(
                    new SyntaxRule()
                    .SetName("Pragma parameter")
                    .SetType(SyntaxRule.SyntaxRuleType.ZERO_OR_ONE)
                    .AddRule(quoteRule)
                    .AddRule(identifierRule)
                    .AddRule(quoteRule)
                )
                .AddRule(rightParenRule);

            SyntaxRule annotationsRule = new SyntaxRule()
                .SetName("Annotations")
                .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                .AddRule(kPragmaRule)
                .AddRule(pragmaDeclRule)
                .AddRule(
                    new SyntaxRule()
                    .SetName("Pragma declarations")
                    .SetType(SyntaxRule.SyntaxRuleType.ZERO_OR_MORE)
                    .AddRule(pragmaDeclRule)
                )
                .AddRule(kEndRule);

            SyntaxRule codeRule = new SyntaxRule()
                .SetName("Code")
                .SetType(SyntaxRule.SyntaxRuleType.SEQUENCE)
                .AddRule(kCodeRule)
                .AddRule(
                    new SyntaxRule()
                    .SetName("{ VarDeclaration | Statement }")
                    .SetType(SyntaxRule.SyntaxRuleType.ZERO_OR_MORE)
                    .AddRule(
                        new SyntaxRule()
                        .SetName("VarDeclaration | Statement")
                        .SetType(SyntaxRule.SyntaxRuleType.OR)
                        .AddRule(varDeclarationRule)
                        .AddRule(statementRule)
                        )
                )
                .AddRule(kEndRule);

            programRule = new SyntaxRule()
                .SetName("Program")
                .SetType(SyntaxRule.SyntaxRuleType.ONE_OR_MORE)
                .AddRule(
                    new SyntaxRule()
                    .SetName("Some unit")
                    .SetType(SyntaxRule.SyntaxRuleType.OR)
                    .AddRule(annotationsRule)
                    .AddRule(dataRule)
                    .AddRule(moduleRule)
                    .AddRule(codeRule)
                    .AddRule(structureRule)
                    .AddRule(routineRule)
                    );
        }

        /// <summary>
        /// Validates the syntax correctness of the token stream
        /// and returns the AST root node if success.
        /// </summary>
        /// <param name="tokens">Tokens of a whole program to be validated.</param>
        /// <returns>A syntax responce instance which contains the AST root node.</returns>
        public SyntaxRule.SyntaxResponse CheckSyntax(List<Token> tokens)
        {
            tokens.RemoveAll(token => token.Type == TokenType.WHITESPACE);
            ASTNode root = new ASTNode
                (null, 
                new List<ASTNode>(), 
                new Token (TokenType.NO_TOKEN, "no_token", new TokenPosition(0, 0)), 
                "root");
            SyntaxRule.SyntaxResponse sr = programRule.Verify(tokens, root);
            if (!sr.Success)
            {
                Logger.LogError(programRule.GetErrors());
            }
            return sr;
        }
    }
}