﻿using ERACompiler.Structures;
using System;
using System.Linq;

namespace ERACompiler.Modules
{
    /// <summary>
    /// Generates the actual assembly code
    /// </summary>
    public class Generator
    {
        // The length of these blocks can be obtained by difference
        private int staticDataAddrBase = 18;
        private int codeAddrBase = 22;

        public Generator()
        {

        }

        public byte[] GetAssemblyCode(AASTNode root)
        {
            return new byte[] 
            {
                0x01,
                0x00,

                0x00, // Static data address
                0x00,
                0x00,
                0x12,
                
                0x00, // Static data length
                0x00,
                0x00,
                0x02,

                0x00, // Code data address
                0x00,
                0x00,
                0x16,

                0x00, // Code data length
                0x00,
                0x00,
                0x04,

                0x00, // Static data itself
                0x00,
                0x00,
                0xf0,

                // Code data itself 
                0xd0, 0x1e, // r0 := sb;
                0xc8, 0x61, // r1 := 3;
                0xd4, 0x20, // r0 += r1;
                0xc4, 0x00  // r0 := ->r0;
            };
            //return ConstructCode(root);
        }

        private byte[] ConstructCode(AASTNode node, bool lookingForStatic = false)
        {
            switch (node.ASTType)
            {
                case "Program":
                    return ConstructProgram(node);
                case "Assembly block":
                    return ConstructAssemblyBlock(node);
                default: // Just go for children nodes
                    {
                        byte[] bytes = new byte[0];
                        foreach (AASTNode child in node.Children)
                        {
                            bytes = MergeByteArrays(bytes, ConstructCode(child));
                        }
                        return bytes;
                    }
            }
        }

        private byte[] ConstructAssemblyBlock(AASTNode node)
        {
            byte[] asmBytes = new byte[0];
            foreach (AASTNode child in node.Children)
            {
                switch (child.ASTType)
                {
                    case "Register := Expression":
                        {
                            // TODO:
                            break;
                        }
                    default:
                        {
                            break;
                        }
                        
                }

            }
            return asmBytes;
        }

        private byte[] ConstructProgram(AASTNode node)
        {
            byte[] programBytes = new byte[18]; // 2 technical + 8 static + 8 code
            programBytes[0] = 0x01; // Version
            programBytes[1] = 0x00; // Padding
            
            // First descent - identify all static data
            byte[] staticBytes = new byte[4] 
            { 0x00, 0x00, 0x00, 0x00 }; // Don't know why, but we need at least two words of static data for interpreter
            foreach (AASTNode child in node.Children)
            {
                staticBytes = MergeByteArrays(staticBytes, ConstructCode(child, true));
            }
            int staticLength = staticBytes.Length / 2; // We count in words (16 bits)
            
            // Move code data by the static data length
            codeAddrBase += staticBytes.Length;

            // Second descent - identify all code data
            byte[] codeBytes = new byte[0];
            foreach (AASTNode child in node.Children)
            {
                codeBytes = MergeByteArrays(codeBytes, ConstructCode(child));
            }
            int codeLength = codeBytes.Length / 2;

            // Convert static data and code lengths to chunks of four bytes
            byte[] sdlb = new byte[4]; // Static data length (in bytes)
            byte[] cdlb = new byte[4]; // Code data length (in bytes)
            byte[] sdab = new byte[4]; // Static data address (in bytes)
            byte[] cdab = new byte[4]; // Code data address (in bytes)
            BitConverter.GetBytes(staticLength).CopyTo(sdlb, 0);
            BitConverter.GetBytes(codeLength).CopyTo(cdlb, 0);
            BitConverter.GetBytes(staticDataAddrBase).CopyTo(sdab, 0);
            BitConverter.GetBytes(codeAddrBase).CopyTo(cdab, 0);

            for (int i = 3; i >= 0; i--)
            {
                programBytes[2 + (3 - i)] = sdab[i]; // Static data address
                programBytes[6 + (3 - i)] = sdlb[i]; // Static data length
                programBytes[10 + (3 - i)] = cdab[i]; // Code data address
                programBytes[14 + (3 - i)] = cdlb[i]; // Code data length
            }

            // Merge previosly constructed bytes
            programBytes = MergeByteArrays(programBytes, staticBytes);
            programBytes = MergeByteArrays(programBytes, codeBytes);

            return programBytes;
        }

        private byte IdentifyRegister(string reg)
        {
            if (reg[0] == 'r')
            {
                if (reg.Length > 2)
                {
                    return (byte)(reg[1] - '0' + (reg[0] - '0') * 10);
                }
                else
                {
                    return (byte)(reg[1] - '0');
                }
            }
            else
            {
                return reg switch
                {
                    "pc" => 0xFF,
                    "sb" => 0xFE,
                    "sp" => 0xFD,
                    "fp" => 0xFC,
                    _ => 0x00,
                };
            }
        }

        private byte[] MergeByteArrays(byte[] arr1, byte[] arr2)
        {
            return arr1.Concat(arr2).ToArray(); // TODO: check different approaches for better performance
        }

    }
}
