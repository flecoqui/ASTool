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
    class Mp4BoxVMHD : Mp4Box
    {
        static public Mp4BoxVMHD CreateVMHDBox()
        {
            byte version = 0x00;
            Int32 flag = 1;
            Mp4BoxVMHD box = new Mp4BoxVMHD();
            if (box != null)
            {
                box.Length = 8 + 12 ;
                box.Type = "vmhd";
                byte[] Buffer = new byte[box.Length - 8 ];
                if (Buffer != null)
                {
                    WriteMp4BoxByte(Buffer, 0, version);
                    WriteMp4BoxInt24(Buffer, 1,flag);

                    WriteMp4BoxInt16(Buffer, 4, 0);
                    WriteMp4BoxInt16(Buffer, 6, 0);
                    WriteMp4BoxInt16(Buffer, 8, 0);
                    WriteMp4BoxInt16(Buffer, 10, 0);

                    box.Data = Buffer;
                    return box;
                }
            }
            return null;
        }
    }
}
