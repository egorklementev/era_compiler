using ERACompiler.Structures;
using System.Runtime.Serialization;
using System.Text;

namespace ERACompiler.Modules
{
    /// <summary>
    /// Generates the actual assembly code
    /// </summary>
    public class Generator
    {
        
        /// <summary>
        /// Generator constructor
        /// </summary>
        public Generator()
        {

        }

        /// <summary>
        /// Constructs actual assembly code from the annotated AST.
        /// </summary>
        /// <param name="root">The root node of the AAST.</param>
        /// <returns>A string containing assembly code build using AAST.</returns>
        public string GetAssemblyCode(AASTNode root)
        {
            return ConstructCode(root);
        }

        private string ConstructCode(AASTNode node)
        {
            StringBuilder asc = new StringBuilder();

            // Construct code corresponding to the node itself
            switch(node.NodeType)
            {
                case ASTNode.ASTNodeType.ASSEMBLER_BLOCK:
                    {
                        asc.Append(ConstructAssemblerBlock(node));
                        break;
                    }                
                default:
                    {
                        break;
                    }
            }


            // Construct code corresponding to nodes' children
            foreach (AASTNode child in node.Children)
            {
                asc.Append(ConstructCode(child));
            }

            return asc.ToString();
        }

        private string ConstructAssemblerBlock(AASTNode node)
        {
            StringBuilder asc = new StringBuilder();

            // Construct code from children
            foreach (AASTNode stmnt in node.Children)
            {
                asc.Append(ConstructAssemblerStatement(stmnt));
            }

            return asc.ToString();
        }

        private string ConstructAssemblerStatement(AASTNode node)
        {
            StringBuilder asc = new StringBuilder();

            if (node.Children.Count == 0) // Skip or Stop
            {
                asc.Append(node.CrspToken.Value);
            }
            else
            {
                // Special case for "format" statements
                if (node.Children[0].CrspToken.Value.Equals("format"))
                {
                    asc.Append("format(").Append(node.Children[1].CrspToken.Value).Append(")");
                }
                else
                {
                    bool isLDA = false;
                    foreach (AASTNode child in node.Children)
                    {
                        if (child.Children.Count > 0)
                        {
                            isLDA = true;
                            break;
                        }
                    }
                    if (isLDA)
                    {
                        asc.Append(GetLDA(node));
                        asc.Remove(asc.Length - 1, 1);
                    }
                    else
                    {
                        foreach (AASTNode child in node.Children)
                        {
                            asc.Append(child.CrspToken.Value).Append(" ");

                            // Special case for "RN := *RN;" type of statements
                            if (child.CrspToken.Value.Equals("*"))
                            {
                                asc.Remove(asc.Length - 1, 1);
                            }
                        }
                        asc.Remove(asc.Length - 1, 1);
                    }
                }
            }

            asc.Append(";\r\n");

            return asc.ToString();
        }

        private string GetLDA(AASTNode ldaNode)
        {
            StringBuilder asc = new StringBuilder();

            if (ldaNode.CrspToken.Type != TokenType.NO_TOKEN)
            {
                asc.Append(ldaNode.CrspToken.Value).Append(" ");
            }

            foreach (AASTNode child in ldaNode.Children)
            {
                asc.Append(GetLDA(child));
            }

            return asc.ToString();
        }

    }
}
