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
    class Mp4BoxESDS : Mp4Box
    {
        static private byte[] GetESDescriptor(int mMaxFrameSize, int mBitrate, int mSampleRate, int mChannels)
        {

            int[] samplingFrequencies = new int[] {96000, 88200, 64000, 48000, 44100, 32000, 24000,

                22050, 16000, 12000, 11025, 8000, 7350};

            // First 5 bytes of the ES Descriptor.

            byte[] ESDescriptor_top = new byte[] { 0x03, 0x19, 0x00, 0x00, 0x00 };

            // First 4 bytes of Decoder Configuration Descriptor. Audio ISO/IEC 14496-3, AudioStream.

            byte[] decConfigDescr_top = new byte[] { 0x04, 0x11, 0x40, 0x15 };

            // Audio Specific Configuration: AAC LC, 1024 samples/frame/channel.

            // Sampling frequency and channels configuration are not set yet.

            byte[] audioSpecificConfig = new byte[] { 0x05, 0x02, 0x10, 0x00 };

            byte[] slConfigDescr = new byte[] { 0x06, 0x01, 0x02 };  // specific for MP4 file.

            int offset;

            int bufferSize = 0x300;

            while (bufferSize < 2 * mMaxFrameSize)
            {            
                // TODO(nfaralli): what should be the minimum size of the decoder buffer?
                // Should it be a multiple of 256?
                bufferSize += 0x100;
            }



            // create the Decoder Configuration Descriptor

            byte[] decConfigDescr = new byte[2 + decConfigDescr_top[1]];

            decConfigDescr_top.CopyTo(decConfigDescr, 0);
            Buffer.BlockCopy(decConfigDescr_top, 0, decConfigDescr, 0, decConfigDescr_top.Length);

            offset = decConfigDescr_top.Length;

            decConfigDescr[offset++] = (byte)((bufferSize >> 16) & 0xFF);

            decConfigDescr[offset++] = (byte)((bufferSize >> 8) & 0xFF);

            decConfigDescr[offset++] = (byte)(bufferSize & 0xFF);

            decConfigDescr[offset++] = (byte)((mBitrate >> 24) & 0xFF);

            decConfigDescr[offset++] = (byte)((mBitrate >> 16) & 0xFF);

            decConfigDescr[offset++] = (byte)((mBitrate >> 8) & 0xFF);

            decConfigDescr[offset++] = (byte)(mBitrate & 0xFF);

            decConfigDescr[offset++] = (byte)((mBitrate >> 24) & 0xFF);

            decConfigDescr[offset++] = (byte)((mBitrate >> 16) & 0xFF);

            decConfigDescr[offset++] = (byte)((mBitrate >> 8) & 0xFF);

            decConfigDescr[offset++] = (byte)(mBitrate & 0xFF);

            int index;

            for (index = 0; index < samplingFrequencies.Length; index++)
            {

                if (samplingFrequencies[index] == mSampleRate)
                {

                    break;

                }

            }

            if (index == samplingFrequencies.Length)
            {
                // TODO(nfaralli): log something here.
                // Invalid sampling frequency. Default to 44100Hz...
                index = 4;
            }

            audioSpecificConfig[2] |= (byte)((index >> 1) & 0x07);
            audioSpecificConfig[3] |= (byte)(((index & 1) << 7) | ((mChannels & 0x0F) << 3));

            Buffer.BlockCopy(
                    audioSpecificConfig, 0, decConfigDescr, offset, audioSpecificConfig.Length);

            audioSpecificConfig.CopyTo(decConfigDescr, offset);

            // create the ES Descriptor
            byte[] ESDescriptor = new byte[2 + ESDescriptor_top[1]];
            Buffer.BlockCopy(ESDescriptor_top, 0, ESDescriptor, 0, ESDescriptor_top.Length);
            ESDescriptor_top.CopyTo(ESDescriptor, 0);
            offset = ESDescriptor_top.Length;
            Buffer.BlockCopy(decConfigDescr, 0, ESDescriptor, offset, decConfigDescr.Length);
            decConfigDescr.CopyTo(ESDescriptor, offset);
            offset += decConfigDescr.Length;

            Buffer.BlockCopy(slConfigDescr, 0, ESDescriptor, offset, slConfigDescr.Length);
            slConfigDescr.CopyTo(ESDescriptor, offset);
            return ESDescriptor;

        }
        static private byte[] GetESDescriptor(int mMaxFrameSize, int mBitrate, int mSampleRate, int mChannels, string CodecPrivateData)
        {
            int BufferCodecPrivateDataLen = 0;
            byte[] BufferCodecPrivateData = null;
            byte[] ESDescriptorBuffer = null;

            try
            {
                BufferCodecPrivateData = Mp4Box.HexStringToByteArray(CodecPrivateData);
                if (BufferCodecPrivateData != null)
                    BufferCodecPrivateDataLen = BufferCodecPrivateData.Length;
                int Len = 1 + 3 + 1 + 2 + 1 + 1 + 3 + 1 + 1 /*Type*/ + 1 + 3 + 4 + 4 + 1 + 3 + 1 + BufferCodecPrivateDataLen + 3;
                ESDescriptorBuffer = new byte[Len];
                int offset = 0;
                ESDescriptorBuffer[offset++] = 0x03;
                ESDescriptorBuffer[offset++] = 0x80;
                ESDescriptorBuffer[offset++] = 0x80;
                ESDescriptorBuffer[offset++] = 0x80;
                ESDescriptorBuffer[offset++] = (byte)(Len - offset - 1);

                Mp4Box.WriteMp4BoxInt16(ESDescriptorBuffer, offset, 0);
                offset += 2;
                ESDescriptorBuffer[offset++] = 0x00;

                ESDescriptorBuffer[offset++] = 0x04;
                ESDescriptorBuffer[offset++] = 0x80;
                ESDescriptorBuffer[offset++] = 0x80;
                ESDescriptorBuffer[offset++] = 0x80;

                // Update Length for tag 0x04
                ESDescriptorBuffer[offset++] = (byte)(Len - offset - 1 - 2);
//                ESDescriptorBuffer[offset++] = (byte)(Len - offset - 1 - 4);

                // MPEG 4 Audio
                ESDescriptorBuffer[offset++] = 0x40;
                ESDescriptorBuffer[offset++] = 0x15;
                int bufferSize = 0x300;
                while (bufferSize < 2 * mMaxFrameSize)
                {
                    bufferSize += 0x100;
                }
                Mp4Box.WriteMp4BoxInt24(ESDescriptorBuffer, offset, bufferSize);
                offset += 3;

                Mp4Box.WriteMp4BoxInt32(ESDescriptorBuffer, offset, mBitrate);
                offset += 4;

                Mp4Box.WriteMp4BoxInt32(ESDescriptorBuffer, offset, mBitrate);
                offset += 4;

                ESDescriptorBuffer[offset++] = 0x05;
                ESDescriptorBuffer[offset++] = 0x80;
                ESDescriptorBuffer[offset++] = 0x80;
                ESDescriptorBuffer[offset++] = 0x80;

                ESDescriptorBuffer[offset++] = (byte)BufferCodecPrivateDataLen;
                if (BufferCodecPrivateData != null)
                    Mp4Box.WriteMp4BoxData(ESDescriptorBuffer, offset, BufferCodecPrivateData);
                offset += BufferCodecPrivateDataLen;
                ESDescriptorBuffer[offset++] = 0x06;
                ESDescriptorBuffer[offset++] = 0x01;
                ESDescriptorBuffer[offset++] = 0x02;
            }
            catch(Exception)
            {

            }

            return ESDescriptorBuffer;
        }

        static public Mp4BoxESDS CreateESDSBox(int mMaxFrameSize, int mBitrate, int mSampleRate, int mChannels, string CodecPrivateData)
        {
           
            Mp4BoxESDS box = new Mp4BoxESDS();
            if (box != null)
            {
//                byte[] desc = GetESDescriptor(mMaxFrameSize, mBitrate, mSampleRate, mChannels, CodecPrivateData);
                byte[] desc = GetESDescriptor(mMaxFrameSize, mBitrate, mSampleRate, mChannels);
                if (desc != null)
                {
                    box.Length = 8 + 4 + desc.Length;
                    box.Type = "esds";
                    byte[] Buffer = new byte[box.Length - 8];
                    if (Buffer != null)
                    {
                        byte version = 0;
                        Int32 flag = 0;
                        WriteMp4BoxByte(Buffer, 0, version);
                        WriteMp4BoxInt24(Buffer, 1, flag);
                        WriteMp4BoxData(Buffer, 4, desc);
                        box.Data = Buffer;
                        return box;
                    }
                }
            }
            return null;
        }
    }
}
