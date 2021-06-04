using ERACompiler.Structures;

namespace ERACompiler.Modules.Generation
{
    class ProgramConstructor : CodeConstructor
    {
        public override CodeNode Construct(AASTNode aastNode, CodeNode? parent)
        {
            CodeNode programNode = new CodeNode(aastNode, parent);

            CodeNode vptNode = new CodeNode("Version/padding/tech bytes", programNode);
            CodeNode staticNode = new CodeNode("Static bytes", programNode);
            CodeNode unitsNode = new CodeNode("Units' addresses node", programNode);
            CodeNode codeNode = new CodeNode("Actual program code", programNode);
            CodeNode skipStopNode = new CodeNode("Skip/Stop", programNode);
            programNode.Children.AddLast(vptNode);
            programNode.Children.AddLast(staticNode);
            programNode.Children.AddLast(unitsNode);
            programNode.Children.AddLast(codeNode);
            programNode.Children.AddLast(skipStopNode);

            staticNode.Add(GetConstBytes(Program.config.MemorySize))
                .Add(GetLList(new byte[aastNode.AASTValue]));

            int staticLength = (staticNode.Count() + staticNode.Count() % 2) / 2; // We count in words (2 bytes)

            // First unit offset - to store correct addresses inside static frame
            int techOffset = 16; // LDA(SB), LDA(SB + codeOffset), 27 = ->27, if 27 goto 27

            // Identify all modules and routines
            int modulesAndRoutines = 0;
            foreach (AASTNode child in aastNode.Children)
            { 
                if (child.ASTType.Equals("Routine") || child.ASTType.Equals("Module") || child.ASTType.Equals("Code"))
                {
                    modulesAndRoutines++;
                }
            }

            techOffset += modulesAndRoutines * 16;
            CodeNode dummyNode = new CodeNode("dummy. Do not add it anywhere in a CodeNode tree. Used for label resolution.", null);
            unitsNode.Add(new byte[techOffset]); // Just fill bytes with zeros for a while (due to label resolution).

            // Identify all code data
            foreach (AASTNode child in aastNode.Children)
            {
                if (child.ASTType.Equals("Routine") || child.ASTType.Equals("Module") || child.ASTType.Equals("Code"))
                {
                    dummyNode.Add(GenerateLDA(SB, 27, aastNode.Context.GetStaticOffset(child.Context.Name)))
                        .Add(GenerateLDC(0, 26))
                        .Add(GenerateLDA(26, 26, staticNode.Count() + techOffset + codeNode.Count()))
                        .Add(GenerateST(26, 27));
                }
                codeNode.Children.AddLast(base.Construct(child, codeNode));
            }

            unitsNode.Bytes.Clear();
            unitsNode.Add(GenerateLDA(SB, SB, 4));

            // Put the actual units' code after it has been processed
            unitsNode.Add(dummyNode.Bytes);

            // Go to code module uncoditionally
            unitsNode.Add(GenerateLDA(SB, 27, aastNode.Context.GetStaticOffset("code")))
                .Add(GenerateLD(27, 27))
                .Add(GenerateCBR(27, 27));

            // Move code data by the static data length
            codeAddrBase += staticNode.Count();
            int codeLength = (unitsNode.Count() + codeNode.Count() + codeNode.Count() % 2) / 2 + 2;

            // Convert static data and code lengths to chunks of four bytes
            vptNode.Add(0x00, 0x01);
            vptNode.Add(GetConstBytes(staticDataAddrBase))
                .Add(GetConstBytes(staticLength))
                .Add(GetConstBytes(codeAddrBase))
                .Add(GetConstBytes(codeLength));
            
            skipStopNode.Add(GenerateSKIP()).Add(GenerateSTOP());

            return programNode;
        }
    }
}
