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
        public struct SampleProtection
        {
            public byte[] IV;
            public int BytesOfClearData;
            public int BytesOfEncryptedData;
        }
        public  Int32 GetFlag()
        {
            return ReadMp4BoxInt24(Data, 17);
        }
        public List<SampleProtection> GetIVList()
        {
            Guid id = GetUUID();
            Int32 Flag = GetFlag();
            Int32 IVSize = 8;
            int TableOffset = 20;

            if ((Flag & 0x000001) == 0x000001)
            {
                TableOffset += 4 + 16;
                IVSize = ReadMp4BoxInt32(this.Data,23);
            }
            int RowOffset = IVSize;
            if ((Flag & 0x000002) == 0x000002)
                RowOffset += 8;
            List<SampleProtection> list = new List<SampleProtection>();
            if (id == Mp4Box.kExtProtectHeaderMOOFBoxGuid)
            {
                if (list != null)
                {
                    int offset = 0;
                    int SampleCount = ReadMp4BoxInt32(this.Data, TableOffset);
                    for (int i = 0; i < SampleCount; i++)
                    {
                        byte[] iv = ReadMp4BoxBytes(this.Data, TableOffset + 4 + offset, IVSize);
                        if ((iv != null) && (iv.Length == IVSize))
                        {
                            SampleProtection sp = new SampleProtection();
                            {
                                if ((Flag & 0x000002) == 0x000002)
                                {
                                    int NumberOfEntries = ReadMp4BoxInt16(this.Data, TableOffset + 4 + offset + IVSize);
                                    sp.BytesOfClearData = ReadMp4BoxInt16(this.Data, TableOffset + 4 + offset + IVSize + 2 );
                                    sp.BytesOfEncryptedData = ReadMp4BoxInt32(this.Data, TableOffset + 4 + offset + IVSize + 2 + 2);
                                }
                                sp.IV = new byte[16];
                                iv.CopyTo(sp.IV, 0);
                                list.Add(sp);
                            }
                        }
                        offset += RowOffset;
                    }
                }
            }
            return list;
        }
        public Guid GetUUID()
        {
            byte[] buffer = ReadMp4BoxBytes(Data, 0, 16);
            Guid id = NetworkOrderArrayToGuid(buffer);
            return id;
        }
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
        static Guid NetworkOrderArrayToGuid(byte[] input)
        {

            byte[] ret = new byte[16];
            Buffer.BlockCopy(BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(BitConverter.ToInt32(input, 0))), 0, ret, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(BitConverter.ToInt16(input, 4))), 0, ret, 4, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(BitConverter.ToInt16(input, 6))), 0, ret, 6, 2);
            Buffer.BlockCopy(input, 8, ret, 8, 8);
            return new Guid(ret);
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
                        System.Buffer.BlockCopy(GuidToNetworkOrderArray(ProtectedGuid), 0, Buffer, 20, 16);

                        WriteMp4BoxInt32(Buffer, 36, base64EncodedBytes.Length);
                        WriteMp4BoxData(Buffer, 40, base64EncodedBytes);
                        box.Data = Buffer;
                        return box;
                    }
                }
            }
            return null;
        }
        static public Mp4BoxUUID CreateUUIDBox(Guid ExtendedGuid, int AlgorithmID, int SampleIDSize, Guid KID)
        {
            byte version = 0x00;
            Int32 flag = 0;
            Mp4BoxUUID box = new Mp4BoxUUID();
            if (box != null)
            {
                box.Length = 8 + 16 + 4 + 4 + 16  ;
                box.Type = "uuid";
                byte[] Buffer = new byte[box.Length - 8];
                if (Buffer != null)
                {
                    System.Buffer.BlockCopy(GuidToNetworkOrderArray(ExtendedGuid), 0, Buffer, 0, 16);
                    WriteMp4BoxByte(Buffer, 16, version);
                    WriteMp4BoxInt24(Buffer, 17, flag);
                    WriteMp4BoxInt24(Buffer, 20, AlgorithmID);
                    WriteMp4BoxByte(Buffer, 23, (byte) SampleIDSize);
                    System.Buffer.BlockCopy(GuidToNetworkOrderArray(KID), 0, Buffer, 24, 16);

                    box.Data = Buffer;
                    return box;
                }
            }
            return null;
        }
    }
}
