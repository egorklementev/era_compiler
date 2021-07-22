using ERACompiler.Structures;
using ERACompiler.Utilities.Errors;
using System.Collections.Generic;

namespace ERACompiler.Modules.Generation
{
    class ProgramConstructor : CodeConstructor
    {
        public override CodeNode Construct(AASTNode aastNode, CodeNode? parent)
        {
            CodeNode programNode = new CodeNode(aastNode, parent);

            CodeNode vptNode = new CodeNode("Version/padding/tech bytes", programNode); // Used for simulator
            CodeNode staticNode = new CodeNode("Static bytes", programNode); // All static data
            CodeNode unitsNode = new CodeNode("Units' addresses node", programNode); // Code that puts proper unit addresses in static data
            CodeNode codeNode = new CodeNode("Actual program code", programNode);
            CodeNode skipStopNode = new CodeNode("Skip/Stop", programNode);
            programNode.Children.AddLast(vptNode);
            programNode.Children.AddLast(staticNode);
            programNode.Children.AddLast(unitsNode);
            programNode.Children.AddLast(codeNode);
            programNode.Children.AddLast(skipStopNode);

            staticNode.Add(GetConstBytes(Program.config.MemorySize))
                .Add(GetLList(new byte[aastNode.AASTValue]));

            int staticLength = (staticNode.Count() + staticNode.Count() % 2) / 2; // We count in words (2 bytes) (???) DECISION: ok

            // Identify all data blocks beforehand and populate static data with its values
            foreach (AASTNode child in aastNode.Children)
            {
                if (child.ASTType.Equals("Data"))
                {
                    LinkedList<byte> data = new LinkedList<byte>();
                    for (int i = 1; i < child.Children.Count; i++)
                    {
                        foreach (byte b in GetConstBytes(((AASTNode)child.Children[i]).AASTValue))
                        {
                            data.AddLast(b);
                        }
                    }
                    staticNode.Replace(aastNode.Context.GetStaticOffset(child.Children[0].Token.Value) + 4, data);
                }
            }

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
            int staticSize = staticNode.Count();
            int codeSize = 0;
            foreach (AASTNode child in aastNode.Children)
            {
                if (child.ASTType.Equals("Routine") || child.ASTType.Equals("Module") || child.ASTType.Equals("Code"))
                {
                    dummyNode.Add(GenerateLDA(SB, 27, aastNode.Context.GetStaticOffset(child.Context.Name)))
                        .Add(GenerateLDC(0, 26))
                        .Add(GenerateLDA(26, 26, staticSize + techOffset + codeSize))
                        .Add(GenerateST(26, 27));
                }

                codeNode.Children.AddLast(base.Construct(child, codeNode));
                codeSize += codeNode.Children.Last.Value.Count();
            }

            unitsNode.Bytes.Clear();
            unitsNode.Add(GenerateLDA(SB, SB, 4));

            // Put the actual units' code after it has been processed
            unitsNode.Add(dummyNode.Bytes);

            // Go to code module uncoditionally
            unitsNode.Add(GenerateLDA(SB, 27, aastNode.Context.GetStaticOffset("code")))
                .Add(GenerateLD(27, 27))
                .Add(GenerateCBR(27, 27));

            #region GOTO resolution
            List<CodeNode> gotoNodes = new List<CodeNode>(); 
            FindAllCodeNodesWithName(programNode, "Goto", gotoNodes);
            foreach (CodeNode gotoNode in gotoNodes)
            {
                int ctxNum = -1; // minus one since Program context does not have a stack allocated. It is static or global.
                string labelName = gotoNode.AASTLink.Children[0].Token.Value;
                AASTNode mainContextNode = gotoNode.AASTLink;
                // If we jump from some contexts, we have to deallocate stack, heap, and other related memory
                while (true)
                {
                    if (mainContextNode.Parent == null)
                    {
                        throw new CompilationErrorException("No label \'" + labelName + "\' found in the current context!!!\r\n" +
                            "  At (Line: " + gotoNode.AASTLink.Token.Position.Line +
                            ", Char: " + gotoNode.AASTLink.Token.Position.Char + ").");
                    }
                    if (mainContextNode.Context != null)
                    {
                        ctxNum++;
                        if (mainContextNode.Context.IsVarDeclaredInThisContext(labelName))
                        {
                            break;
                        }
                        gotoNode.Children.AddLast(GetDynamicMemoryDeallocationNode(mainContextNode, gotoNode));
                        if (mainContextNode.ASTType.Equals("For"))
                        {
                            gotoNode.Children.AddLast(GetHeapTopChangeNode(gotoNode, 16));
                        }
                        if (mainContextNode.ASTType.Equals("While"))
                        {
                            gotoNode.Children.AddLast(GetHeapTopChangeNode(gotoNode, 12));
                        }
                        if (mainContextNode.ASTType.Equals("Loop While"))
                        {
                            gotoNode.Children.AddLast(GetHeapTopChangeNode(gotoNode, 4));
                        }
                    }
                    mainContextNode = (AASTNode)mainContextNode.Parent;
                }
                for (int i = 0; i < ctxNum; i++)
                {
                    gotoNode.Children.AddLast(new CodeNode("Return stack back", gotoNode)
                        .Add(GenerateLDA(FP, FP, -4))
                        .Add(GenerateMOV(FP, SP))
                        .Add(GenerateLD(FP, FP)));
                }

                CodeNode? gotoLabelNode = gotoNode;
                while (true)
                {
                    if (gotoLabelNode == null)
                    {
                        throw new CompilationErrorException("No label \'" + labelName + "\' found in the current context!!!\r\n" +
                            "  At (Line: " + gotoNode.AASTLink.Token.Position.Line +
                            ", Char: " + gotoNode.AASTLink.Token.Position.Char + ").");
                    }
                    bool found = false;
                    foreach (CodeNode child in gotoLabelNode.Children)
                    {
                        if (child.Name.Equals("Statement") && child.Children.First.Value.Name.Equals("Goto label") && 
                            child.Children.First.Value.AASTLink.Children[0].Token.Value.Equals(labelName))
                        {
                            gotoLabelNode = child.Children.First.Value;
                            found = true;
                            break;
                        }
                    }
                    if (found)
                    {
                        break;
                    }
                    gotoLabelNode = gotoLabelNode.Parent;
                }

                CodeNode labelDecl = new CodeNode("Label declaration", gotoNode).Add(new byte[8]);
                labelDecl.ByteToReturn = 27;
                gotoNode.Children.AddLast(labelDecl);
                gotoNode.Children.AddLast(new CodeNode("goto jump", gotoNode).Add(GenerateCBR(27, 27)));

                CodeNode labelNode = new CodeNode("Label", gotoLabelNode);
                labelNode.LabelDecl = labelDecl;
                gotoLabelNode.Children.AddLast(labelNode);
                gotoLabelNode.Children.AddLast(GetRegisterAllocationNode((AASTNode)gotoLabelNode.AASTLink.Parent, gotoLabelNode));
            }
            #endregion

            #region Label resolution
            /*List<CodeNode> labels = new List<CodeNode>(); 
            FindAllCodeNodesWithName(programNode, "Label", labels);
            foreach (CodeNode label in labels)
            {
                int labelAddr = GetCurrentBinarySize(label); // Always the first child
                label.LabelDecl.Bytes.Clear();
                label.LabelDecl.Add(GenerateLDL(label.LabelDecl.ByteToReturn, labelAddr));
            }*/
            ResolveLabels(programNode);
            #endregion

            // Move code data by the static data length
            codeAddrBase += staticSize;
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

        private void FindAllCodeNodesWithName(CodeNode root, string nodeName, List<CodeNode> resultingList)
        {
            if (root.Name.Equals(nodeName))
            {
                resultingList.Add(root);
            }

            foreach (CodeNode child in root.Children)
            {
                FindAllCodeNodesWithName(child, nodeName, resultingList);
            }
        }

        private int curBinSize = 0;
        private void ResolveLabels(CodeNode root)
        {
            if (root.Name.Equals("Label"))
            {
                root.LabelDecl.Bytes.Clear();
                root.LabelDecl.Add(GenerateLDL(root.LabelDecl.ByteToReturn, curBinSize));
            }
            curBinSize += root.Bytes.Count;
            foreach (CodeNode child in root.Children)
            {
                ResolveLabels(child);
            }
        }

    }
}
