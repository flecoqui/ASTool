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
    class Mp4BoxUUID : Mp4Box
    {
        static byte[] GuidToNetworkOrderArray(Guid input)
        {
            byte[] guidAsByteArray = input.ToByteArray();
            byte[] ret = new byte[16];
            Buffer.BlockCopy(BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(BitConverter.ToInt32(guidAsByteArray, 0))), 0, ret, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(BitConverter.ToInt16(guidAsByteArray, 4))), 0, ret, 4, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(BitConverter.ToInt16(guidAsByteArray, 6))), 0, ret, 6, 2);
            Buffer.BlockCopy(guidAsByteArray, 8, ret, 8, 8);
            return ret;
        }
        static public Mp4BoxUUID CreateUUIDBox(Guid ExtendedGuid, Guid ProtectedGuid, string ProtectedData)
        {
            byte version = 0x00;
            Int32 flag = 0;
            Mp4BoxUUID box = new Mp4BoxUUID();
            if (box != null)
            {
                var base64EncodedBytes = System.Convert.FromBase64String(ProtectedData);
                if (base64EncodedBytes != null)
                {

                    box.Length = 8 + 16 + 4 + 16 + 4 + base64EncodedBytes.Length;
                    box.Type = "uuid";
                    byte[] Buffer = new byte[box.Length - 8];
                    if (Buffer != null)
                    {
                        System.Buffer.BlockCopy(GuidToNetworkOrderArray(ExtendedGuid), 0, Buffer, 0, 16);
                        WriteMp4BoxByte(Buffer, 16, version);
                        WriteMp4BoxInt24(Buffer, 17, flag);
                        System.Buffer.BlockCopy(GuidToNetworkOrderArray(ExtendedGuid), 0, Buffer, 20, 16);

                        WriteMp4BoxInt32(Buffer, 36, base64EncodedBytes.Length);
                        WriteMp4BoxData(Buffer, 40, base64EncodedBytes);
                        box.Data = Buffer;
                        return box;
                    }
                }
            }
            return null;
        }
    }
}
