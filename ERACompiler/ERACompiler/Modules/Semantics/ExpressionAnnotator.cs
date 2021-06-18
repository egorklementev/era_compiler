using ERACompiler.Structures;
using System.Collections.Generic;

namespace ERACompiler.Modules.Semantics
{
    class ExpressionAnnotator : NodeAnnotator
    {
        public override AASTNode Annotate(ASTNode astNode, AASTNode? parent)
        {
            AASTNode expr = new AASTNode(astNode, parent, SemanticAnalyzer.no_type);            
            List<ASTNode> children = astNode.Children;
            Context ctx = SemanticAnalyzer.FindParentContext(parent);

            // Special case -1: if we have constant expression - calculate it and return literal instead
            // ATTENTION: need to be tested. UPD: it's okay
            if (SemanticAnalyzer.IsExprConstant(astNode, ctx))
            {
                int exprValue = SemanticAnalyzer.CalculateConstExpr(astNode, ctx);             
                ASTNode number = new ASTNode(expr, new List<ASTNode>(), expr.Token, "NUMBER");                                
                ASTNode opMinus = new ASTNode(number, new List<ASTNode>(), expr.Token, "[ - ]");
                if (exprValue < 0)
                {
                    opMinus.Children.Add(new ASTNode(opMinus, new List<ASTNode>(), expr.Token, "OPERATOR"));
                    exprValue *= -1;
                }
                number.Children.Add(opMinus);
                ASTNode literal = new ASTNode(number, new List<ASTNode>(), new Token(TokenType.NUMBER, exprValue.ToString(), expr.Token.Position), "SOME_LITERAL");
                number.Children.Add(literal);
                expr.Children.Add(base.Annotate(number, expr));
                return expr;
            }

            // Special case 0: if we have "legal" or initial Expression from Syntax Analyzer
            if (astNode.Children.Count == 2 && astNode.Children[1].ASTType.Equals("{ Operator Operand }"))
            {
                children = astNode.Children[1].Children;
                children.Insert(0, astNode.Children[0]);
            }

            // Special case 1: only one operand
            if (children.Count == 1)
            {
                expr.Children.Add(base.Annotate(children[0], expr));
                return expr;
            }

            // Special case 2: operand, operator, and operand
            if (children.Count == 3)
            {
                foreach (var child in children)
                    expr.Children.Add(base.Annotate(child, expr));
                return expr;
            }

            // If more, we need to rearrange the operands and operators to follow the operation priority
            // --  Gospod' dast - srabotaet  --   
            // UPD: Gospod' smilovilsya nado mnoy, spasibo emu
            // Priority list
            List<string> ops = new List<string>() { "*", "+", "-", ">=", "<=", ">", "<", "=", "/=", "&", "^", "|", "?" };

            foreach (string op in ops)
            {
                if (children.Count <= 3)
                {
                    break;
                }
                for (int i = 1; i < children.Count; i += 2) // Iterate over operators
                {
                    if (children[i].ASTType.Equals("Operator") && children[i].Token.Value.Equals(op))
                    {
                        ASTNode child_expr = new ASTNode(astNode, new List<ASTNode>(), astNode.Token, "Expression"); // Create additional expression
                        child_expr.Children.Add(children[i - 1]);
                        child_expr.Children.Add(children[i]);
                        child_expr.Children.Add(children[i + 1]);
                        children.RemoveRange(i - 1, 3);
                        children.Insert(i - 1, child_expr);
                        i -= 2;                       
                    }
                }
            }
            
            // Annotate modified AST and put it to expression
            foreach (ASTNode child in children) expr.Children.Add(base.Annotate(child, expr));

            return expr;
        }
    }
}
