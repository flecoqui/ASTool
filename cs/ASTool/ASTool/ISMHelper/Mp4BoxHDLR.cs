//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
namespace ASTool.ISMHelper
{
    class Mp4BoxHDLR : Mp4Box
    {
        static public Mp4BoxHDLR CreateHDLRBox(string handler_type, string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                byte version = 0x01;
                Mp4BoxHDLR box = new Mp4BoxHDLR();
                if (box != null)
                {
                    box.Length = 8 + 4 + 4 + 4 + 3 * 4 + name.Length;
                    box.Type = "hdlr";
                    byte[] Buffer = new byte[box.Length - 8];
                    if (Buffer != null)
                    {
                        WriteMp4BoxByte(Buffer, 0, version);
                        WriteMp4BoxInt24(Buffer, 1, 0);

                        WriteMp4BoxInt32(Buffer, 4, 0);
                        WriteMp4BoxString(Buffer, 8, handler_type, 4);

                        WriteMp4BoxInt32(Buffer, 12, 0);
                        WriteMp4BoxInt32(Buffer, 16, 0);
                        WriteMp4BoxInt32(Buffer, 20, 0);

                        WriteMp4BoxString(Buffer, 24, name);

                        box.Data = Buffer;
                        return box;
                    }
                }
            }
            return null;
        }
    }
}
