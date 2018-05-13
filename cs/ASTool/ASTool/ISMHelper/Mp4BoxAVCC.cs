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
    class Mp4BoxAVCC : Mp4Box
    {
  
        static public Mp4BoxAVCC CreateAVCCBox(Byte ConfigurationVersion, Byte AVCProfileIndication, Byte ProfileCompatibility, Byte AVCLevelIndication, Byte[] SPSNALUContent, Byte[] PPSNALUContent)
        {

            Mp4BoxAVCC box = new Mp4BoxAVCC();
            if (box != null)
            {

                box.Length = 8 + 4 + 2 + 2 + SPSNALUContent.Length + 1 + 2 + PPSNALUContent.Length;
                box.Type = "avcC";
                byte[] Buffer = new byte[box.Length - 8];
                if (Buffer != null)
                {
                    WriteMp4BoxByte(Buffer, 0, ConfigurationVersion);
                    WriteMp4BoxByte(Buffer, 1, AVCProfileIndication);
                    WriteMp4BoxByte(Buffer, 2, ProfileCompatibility);
                    WriteMp4BoxByte(Buffer, 3, AVCLevelIndication);

                    WriteMp4BoxByte(Buffer, 4, 0xFF);
                    //SPS
                    WriteMp4BoxByte(Buffer, 5, 0xE1);
                    WriteMp4BoxInt16(Buffer, 6, (Int16) SPSNALUContent.Length);
                    WriteMp4BoxData(Buffer, 8, SPSNALUContent);
                    //PPS
                    WriteMp4BoxByte(Buffer, 8 + SPSNALUContent.Length, 0x01);
                    WriteMp4BoxInt16(Buffer, 8 + SPSNALUContent.Length + 1, (Int16)PPSNALUContent.Length);
                    WriteMp4BoxData(Buffer, 8 + SPSNALUContent.Length + 1 + 2, PPSNALUContent);

                    box.Data = Buffer;
                    return box;
                }
            }
            return null;
        }
    }
}
