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
using System.Net;
using System.Collections.Generic;

namespace ASTool.ISMHelper
{
    public class Mp4BoxHelper
    {
        const int kBoxHeaderSize = 8;
        static public bool IsBoxType(byte[] buffer, int offset, string box)
        {
            if ((offset + 8) > buffer.Length)
            {
                return false;
            }
            offset += 4;
            for (int i = 0; i < 4; i++)
            {
                if (buffer[offset + i] != (byte)box[i])
                {
                    return false;
                }
            }
            return true;
        }

        static public int FindChild(byte[] buffer, int offset, string box)
        {
            int endOfBoxOffset = offset + ReadMp4BoxLength(buffer, offset);
            offset += kBoxHeaderSize;
            while (offset < endOfBoxOffset)
            {
                if (IsBoxType(buffer, offset, box))
                {
                    return offset;
                }
                offset += ReadMp4BoxLength(buffer, offset);
            }
            return -1;
        }
        static public byte[] CreateBox(string Type, Int32 length, byte[] data)
        {

            if ((!string.IsNullOrEmpty(Type) && (Type.Length == 4)) &&
                (data != null) && (data.Length > 0) &&
                (length <= (data.Length + 8))
                )
            {
                byte[] Buffer =  new byte[length];
                if(Buffer!=null)
                {
                    if( WriteMp4BoxLength(Buffer, 0,length)&&
                        WriteMp4BoxType(Buffer, 0, Type)&&
                        WriteMp4BoxData(Buffer, 0, data, length - 8))
                    {
                        return Buffer;
                    }
                }
            }
            return null;
        }
        static public byte[] CreateFTYPBox(string major_brand, Int32 minor_version, List<string> compatible_brands)
        {
            if( (!string.IsNullOrEmpty(major_brand) && (major_brand.Length == 4)) &&
                (compatible_brands !=null) && (compatible_brands.Count>0)
                )
            {
                byte[] Buffer = new byte[8 + compatible_brands.Count*4];
                if(Buffer!=null)
                {
                    WriteMp4BoxString(Buffer, 0, major_brand, 4);
                    WriteMp4BoxInt32(Buffer, 4, minor_version);
                    for(int i = 0; i < compatible_brands.Count;i++)
                        WriteMp4BoxString(Buffer, 8 + 4*i, compatible_brands[i], 4);
                }
            }
            return null;
        }
        static public int ReadMp4BoxLength(byte[] buffer, int offset)
        {
            int mp4BoxLen = 0;
            mp4BoxLen |= (int)(buffer[offset + 0] << 24);
            mp4BoxLen |= (int)(buffer[offset + 1] << 16);
            mp4BoxLen |= (int)(buffer[offset + 2] << 8);
            mp4BoxLen |= (int)(buffer[offset + 3] << 0);
            return mp4BoxLen;
        }
        static public string ReadMp4BoxTitle(byte[] buffer, int offset)
        {
            string s = string.Empty;
            if ((offset + 8) > buffer.Length)
            {
                return s;
            }
            char[] array = new char[5];
            for(int i = 0; i< 4 ; i ++)
                array[0+i] = (char) buffer[4+i];
            array[4] = (char)0;
            s = new string(array);
            return s;
        }

        static public bool WriteMp4BoxLength(byte[] buffer, int offset, int length)
        {
            if (buffer != null)
            { 
                buffer[offset++] = (byte)(length >> 24);
                buffer[offset++] = (byte)(length >> 16);
                buffer[offset++] = (byte)(length >> 8);
                buffer[offset++] = (byte)(length >> 0);
                return true;
            }
            return false;
        }
        static public bool WriteMp4BoxInt32(byte[] buffer, int offset, Int32 value)
        {
            if (buffer != null)
            {
                buffer[offset++] = (byte)(value >> 24);
                buffer[offset++] = (byte)(value >> 16);
                buffer[offset++] = (byte)(value >> 8);
                buffer[offset++] = (byte)(value >> 0);
                return true;
            }
            return false;
        }
        static public bool WriteMp4BoxString(byte[] buffer, int offset, string value, int Len)
        {
            if ((buffer != null) &&
                (!string.IsNullOrEmpty(value)) &&
                (value.Length >0)
                )
            {
                for (int i = 0; i < Len; i++)
                {
                    buffer[offset + i] = (byte)value[i];
                }
                return true;
            }
            return false;
        }
        static public bool WriteMp4BoxType(byte[] buffer, int offset, string boxType)
        {
            if ((buffer != null) &&
                (!string.IsNullOrEmpty(boxType)) &&
                (boxType.Length == 4)
                )
            {
                for (int i = 0; i < 4; i++)
                {
                    buffer[offset + 4 + i] = (byte)boxType[i];
                }
                return true;
            }
            return false;
        }
        static public bool WriteMp4BoxData(byte[] buffer, int offset, byte[] data, int len)
        {
            if ( (buffer!=null)&&
                (data != null) &&
                (len <= data.Length) &&
                (buffer.Length >= (offset + 8 + len))
                )
            { 
                for (int i = 0; i < len; i++)
                {
                    buffer[offset + 8 + i] = (byte)data[i];
                }
                return true;
            }
            return false;
        }
        static byte[] GuidToNetworkOrderArray(Guid input)
        {
            byte[] guidAsByteArray = input.ToByteArray();
            byte [] ret = new byte[16];
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(BitConverter.ToInt32(guidAsByteArray, 0))), 0, ret, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(BitConverter.ToInt16(guidAsByteArray, 4))), 0, ret, 4, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(BitConverter.ToInt16(guidAsByteArray, 6))), 0, ret, 6, 2);
            Buffer.BlockCopy(guidAsByteArray, 8, ret, 8, 8);
            return ret;
        }

        static private byte[] CreateExtendedData(Guid uuid, byte version, uint flags, byte[] boxData)
        {
            /*
            byte[] extendedData = new byte[16 + 5 + ((null == boxData) ? 0 : boxData.Length)];
            Buffer.BlockCopy(GuidToNetworkOrderArray(uuid), 0, extendedData, 0, 16);
            extendedData[16 + 0] = version;
            extendedData[16 + 1] = (byte)(flags >> 24);
            extendedData[16 + 2] = (byte)(flags >> 16);
            extendedData[16 + 3] = (byte)(flags >> 8);
            extendedData[16 + 4] = (byte)(flags >> 0);
            Buffer.BlockCopy(boxData, 0, extendedData, 16 + 5, boxData.Length);
             */
            byte[] extendedData = new byte[16 + 4 + ((null == boxData) ? 0 : boxData.Length)];
            Buffer.BlockCopy(GuidToNetworkOrderArray(uuid), 0, extendedData, 0, 16);
            extendedData[16 + 0] = version;
            extendedData[16 + 1] = (byte)(flags >> 16);
            extendedData[16 + 2] = (byte)(flags >> 8);
            extendedData[16 + 3] = (byte)(flags >> 0);
            Buffer.BlockCopy(boxData, 0, extendedData, 16 + 4, boxData.Length);
            return extendedData;
        }

        static public int InsertExtendedBoxHead(byte[] buffer, Guid uuid, byte version, uint flags, byte[] boxData, params int[] boxOffsets)
        {
            byte[] extendedData = CreateExtendedData(uuid, version, flags, boxData);
            return Mp4BoxHelper.InsertBoxHead(buffer, "uuid", extendedData, boxOffsets);
        }

        static public int WriteExtendedBox(byte[] buffer, Guid uuid, byte version, uint flags, byte[] boxData, int offset)
        {
            byte[] extendedData = CreateExtendedData(uuid, version, flags, boxData);
            return Mp4BoxHelper.WriteBox(buffer, "uuid", offset, extendedData);
        }

        static public int WriteBox(byte[] buffer, string boxType, int offset, byte[] boxData)
        {
            int newBoxSize = ((null == boxData) ? 0 : boxData.Length) + kBoxHeaderSize;
            // add our headers
            WriteMp4BoxType(buffer, offset, boxType);
            WriteMp4BoxLength(buffer, offset, newBoxSize);
            if (null != boxData)
            {
                // copy the new data in
                Buffer.BlockCopy(
                    boxData,
                    0,
                    buffer,
                    offset + kBoxHeaderSize, // newbox header
                    boxData.Length);
            }
            return newBoxSize;
        }

        static public int InsertBoxHead(byte[] buffer, string boxType, byte[] boxData, params int[] boxOffsets)
        {
            int newBoxSize = ((null == boxData) ? 0 : boxData.Length) + kBoxHeaderSize;
            int parentOffset = boxOffsets[boxOffsets.Length - 1];
            int rootSize = ReadMp4BoxLength(buffer, boxOffsets[0]);
            // first shift all the data down
            Buffer.BlockCopy(
                buffer, 
                parentOffset + kBoxHeaderSize, 
                buffer,
                parentOffset + kBoxHeaderSize + newBoxSize, // parent header + newbox header + box data
                (rootSize - (parentOffset + kBoxHeaderSize)));
            WriteBox(buffer, boxType, parentOffset + kBoxHeaderSize, boxData); // write just past the parent header
            // update our lengths
            for (int i = 0; i < boxOffsets.Length; i++)
            {
                WriteMp4BoxLength(buffer, boxOffsets[i], ReadMp4BoxLength(buffer, boxOffsets[i]) + newBoxSize);
            }
            return newBoxSize;
        }
    }
}
