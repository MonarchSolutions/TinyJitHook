﻿using System.Collections.Generic;
using System.IO;

namespace TinyJitHook.Extensions
{
    public static class InstructionByteHelper
    {
        public static IEnumerable<Instruction> GetInstructions(this byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            using (BinaryReader r = new BinaryReader(ms))
            {
                while (r.BaseStream.Position < bytes.Length)
                {
                    Instruction instruction = new Instruction { Offset = (int) r.BaseStream.Position };

                    short code = r.ReadByte();
                    if (code == 0xfe)
                    {
                        code = (short) (r.ReadByte() | 0xfe00);
                    }

                    instruction.OpCode = OpCodeShortHelper.GetOpCode(code);
                    instruction.Read(r);

                    yield return instruction;
                }
            }
        }

        public static byte[] GetBytes(this IEnumerable<Instruction> instructions)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter w = new BinaryWriter(ms))
            {
                foreach (var inst in instructions)
                {
                    if (inst.OpCode.Size == 1)
                    {
                        w.Write((byte)inst.OpCode.Value);
                    }
                    else
                    {
                        w.Write(inst.OpCode.Value);
                    }
                    
                    inst.Write(w);
                }

                return ms.ToArray();
            }
        }
    }
}