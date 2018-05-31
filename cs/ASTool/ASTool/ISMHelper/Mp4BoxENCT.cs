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
    class Mp4BoxENCT : Mp4Box
    {
  
        static public Mp4BoxENCT CreateENCTBox(Int16  RefIndex)
        {

            Mp4BoxENCT box = new Mp4BoxENCT();
            if (box != null)
            {

                box.Length = 8 + 8;
                box.Type = "enct";
                byte[] Buffer = new byte[box.Length - 8];
                if (Buffer != null)
                {
                    WriteMp4BoxInt32(Buffer, 0, 0);
                    WriteMp4BoxInt16(Buffer, 4, 0);
                    WriteMp4BoxInt16(Buffer, 6, RefIndex);
                    box.Data = Buffer;
                    return box;
                }
            }
            return null;
        }
    }
}
