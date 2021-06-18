using ERACompiler.Modules;
using ERACompiler.Modules.Generation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERACompiler.Structures
{
    public class CodeNode
    {
        /// <summary>
        /// Possible link to the AAST tree
        /// </summary>
        public AASTNode? AASTLink { get; } = null; // Used in 'goto' resolution

        /// <summary>
        /// Tabulation level used for fancy JSON output
        /// </summary>
        public int Level { get; set; } = 0;

        /// <summary>
        /// The name of this node (for usability)
        /// </summary>
        public string Name { get; set; } = "no_name";

        /// <summary>
        /// Possible token value (for usability)
        /// </summary>
        public string Token { get; set; } = "no_token";

        /// <summary>
        /// Possible Code Node parent
        /// </summary>
        public CodeNode? Parent { get; set; } = null;

        /// <summary>
        /// Possible label declaration link. Used for label resolution
        /// </summary>
        public CodeNode? LabelDecl { get; set; } = null; 

        /// <summary>
        /// The list of child Code Nodes. Can be empty.
        /// </summary>
        public LinkedList<CodeNode> Children { get; set; }

        /// <summary>
        /// The list of bytes related to this Code Node. Can be empty.
        /// </summary>
        public LinkedList<byte> Bytes { get; set; }

        /// <summary>
        /// Possible byte to return. Is needed, when returning some register information up the Code Node tree.
        /// </summary>
        public byte ByteToReturn { get; set; } = 255;

        /// <summary>
        /// Possible operand byte. Used for recursion resolution
        /// </summary>
        public byte OperandByte { get; set; } = 255; 

        public CodeNode(AASTNode aastNode) : this(aastNode.ASTType, null) 
        { 
            Token = aastNode.Token.Value;
            AASTLink = aastNode;
        }

        public CodeNode(AASTNode aastNode, CodeNode? parent) : this(aastNode.ASTType, parent) 
        {
            Token = aastNode.Token.Value;
            AASTLink = aastNode;
        }

        public CodeNode(string name, CodeNode? parent)
        {
            Name = name;
            Parent = parent;
            Level = parent == null ? 0 : parent.Level + 1;
            Bytes = new LinkedList<byte>();
            OperandByte = parent == null ? (byte)255 : parent.OperandByte;
            Children = new LinkedList<CodeNode>();
        }

        /// <summary>
        /// Adds given bytes to the list of bytes of this node.
        /// </summary>
        /// <param name="bytes">The list of bytes to add/append.</param>
        /// <returns>This node itself.</returns>
        public CodeNode Add(LinkedList<byte> bytes)
        {
            foreach (byte b in bytes)
            {
                Bytes.AddLast(b);
            }
            return this;
        }

        /// <summary>
        /// Adds given bytes to the list of bytes of this node.
        /// </summary>
        /// <param name="bytes">The list of bytes to add/append.</param>
        /// <returns>This node itself.</returns>
        public CodeNode Add(params byte[] bytes)
        {
            foreach (byte b in bytes)
            {
                Bytes.AddLast(b);
            }
            return this;
        }

        /// <summary>
        /// Replaces some bytes in the list of bytes of this Code Node.
        /// </summary>
        /// <param name="startIndex">From where to start replacement (inclusive).</param>
        /// <param name="newBytes">The list of bytes to replace with.</param>
        /// <returns>This node itself.</returns>
        public CodeNode Replace(int startIndex, LinkedList<byte> newBytes)
        {
            var anchor = Bytes.First;
            for (int i = 0; i < startIndex; i++)
            {
                anchor = anchor.Next;
            }
            foreach (byte b in newBytes)
            {
                anchor.Value = b;
                anchor = anchor.Next;
            }
            return this;
        }

        /// <returns>Whether node is leaf in Code Node tree or not.</returns>
        public bool IsLeaf()
        {
            return Children.Count == 0;
        }

        public CodeNode SetByteToReturn(byte b)
        {
            ByteToReturn = b;
            return this;
        }

        /// <summary>
        /// Recusively counts bytes number in this node and its children.
        /// </summary>
        /// <returns>The number of bytes.</returns>
        public int Count()
        {
            int count = Bytes.Count;
            foreach (CodeNode child in Children)
            {
                count += child.Count();
            }
            return count;
        }

        /// <summary>
        /// Used somewhere in JSON generation, I do not remember.
        /// </summary>
        /// <returns></returns>
        public int GetNodeCommandOffset()
        {
            int offset = 0;

            if (Parent != null)
            {
                int i = 0;
                foreach (CodeNode child in Parent.Children)
                {
                    if (child == this)
                    {
                        break;
                    }
                    i++;
                }
                var anchor = Parent.Children.First;
                for (int j = 0; j < i; j++)
                {
                    offset += anchor.Value.Count();
                    anchor = anchor.Next;
                }
                offset += Parent.GetNodeCommandOffset();
            }

            return offset;
        }

        public override string ToString()
        {

            string tabs_lvl = string.Concat(Enumerable.Repeat("\t", Level * 2));
            string tabs_lvl1 = string.Concat(Enumerable.Repeat("\t", Level * 2 + 1));
            string tabs_lvl2 = string.Concat(Enumerable.Repeat("\t", Level * 2 + 2));

            StringBuilder sb = new StringBuilder();

            sb.Append(tabs_lvl).Append("{\r\n");
            sb.Append(tabs_lvl1).Append("\"name\": \"").Append(Name).Append("\",\r\n");
            sb.Append(tabs_lvl1).Append("\"token\": \"").Append(Token).Append("\",\r\n");
            if (!IsLeaf())
            {
                sb.Append(tabs_lvl1).Append("\"bytes\": [],\r\n");
                sb.Append(tabs_lvl1).Append("\"children\": [\r\n");
                foreach (CodeNode child in Children)
                {
                    sb.Append(child.ToString()).Append(",\r\n");
                }
                sb.Remove(sb.Length - 3, 1); // Remove comma
                sb.Append(tabs_lvl1).Append("]\r\n");
            } 
            else
            {
                sb.Append(tabs_lvl1).Append("\"bytes\": [\r\n");
                if (Name.Equals("Version/padding/tech bytes") || Name.Equals("Static bytes"))
                {
                    foreach (byte b in Bytes)
                    {
                        sb.Append(tabs_lvl2);
                        sb.Append('\"').Append(BitConverter.ToString(new byte[] { b })).Append("\",\r\n");
                    }
                    sb.Remove(sb.Length - 3, 1);
                    sb.Append(tabs_lvl1).Append("],\r\n");
                } 
                else if (Bytes.Count > 0)
                {
                    sb.Append(Generator.ConvertToAssemblyCode(Bytes, GetNodeCommandOffset(), Level * 2 + 2)); ;
                    sb.Append(tabs_lvl1).Append("],\r\n");
                } 
                else
                {
                    sb.Remove(sb.Length - 2, 2);
                    sb.Append("],\r\n");
                }
                sb.Append(tabs_lvl1).Append("\"children\": []\r\n");
            }
            sb.Append(tabs_lvl).Append('}');

            return sb.ToString();
        }

    }
}
