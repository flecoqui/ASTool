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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASTool.CacheHelper
{
    class IndexCache
    {
        public ulong Time;
        public ulong Duration;
        public ulong Offset;
        public UInt32 Size;

        /// <summary>Construct the data by giving the parameters. This is for writing index data to storage</summary>
        public IndexCache(ulong time, ulong duration, ulong offset, UInt32 size)
        {
            Time = time;
            Duration = duration;
            Offset = offset;
            Size = size;
        }

        /// <summary>Construct the data by giving the Byte Array data. This is for reading index data from storage</summary>
        public IndexCache(Byte[] Data, ulong offset = 0)
        {
            Time = BitConverter.ToUInt64(Data, (int)offset + 0);
            Duration = BitConverter.ToUInt64(Data, (int)offset + 8);
            Offset = BitConverter.ToUInt64(Data, (int)offset + 16);
            Size = BitConverter.ToUInt32(Data, (int)offset + 24);
        }

        /// <summary> Convert all content index information to Byte Array in order to save it to file</summary>
        /// <returns> Byte array of each chunk index information.</returns>
        public Byte[] GetByteData()
        {
            Byte[] Data = new Byte[IndexCacheSize];
            BitConverter.GetBytes(Time).CopyTo(Data, 0);
            BitConverter.GetBytes(Duration).CopyTo(Data, 8);
            BitConverter.GetBytes(Offset).CopyTo(Data, 16);
            BitConverter.GetBytes(Size).CopyTo(Data, 24);

            return Data;
        }

        public const int IndexCacheSize = 28;
    }
}
