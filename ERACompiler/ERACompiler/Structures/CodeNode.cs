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
        public AASTNode? AASTLink { get; } = null; // Used in 'goto' resolution
        public int Level { get; set; } = 0;
        public string Name { get; set; } = "no_name";
        public string Token { get; set; } = "no_token";
        public CodeNode? Parent { get; set; } = null;
        public CodeNode? LabelDecl { get; set; } = null; // Used for label resolution
        public LinkedList<CodeNode> Children { get; set; }
        public LinkedList<byte> Bytes { get; set; }
        public byte ByteToReturn { get; set; } = 255;
        public byte OperandByte { get; set; } = 255; // Used for recursion resolution

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

        public CodeNode Add(LinkedList<byte> bytes)
        {
            foreach (byte b in bytes)
            {
                Bytes.AddLast(b);
            }
            return this;
        }

        public CodeNode Add(params byte[] bytes)
        {
            foreach (byte b in bytes)
            {
                Bytes.AddLast(b);
            }
            return this;
        }

        public bool IsLeaf()
        {
            return Children.Count == 0;
        }

        public CodeNode SetByteToReturn(byte b)
        {
            ByteToReturn = b;
            return this;
        }

        public int Count()
        {
            int count = Bytes.Count;
            foreach (CodeNode child in Children)
            {
                count += child.Count();
            }
            return count;
        }

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
