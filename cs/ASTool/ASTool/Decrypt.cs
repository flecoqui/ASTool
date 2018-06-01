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
using ASTool.CacheHelper;
using ASTool.ISMHelper;
using ASTool.Decrypt;
using System.Security.Cryptography;

namespace ASTool
{
    public partial class Program
    {

        public static byte[] ConvertHexaStringToByteArray(string str)
        {
            if (string.IsNullOrEmpty(str))
                return null;
            Dictionary<string, byte> hexindex = new Dictionary<string, byte>();
            for (int i = 0; i <= 255; i++)
                hexindex.Add(i.ToString("X2"), (byte)i);

            List<byte> hexres = new List<byte>();
            for (int i = 0; i < str.Length; i += 2)
                hexres.Add(hexindex[str.Substring(i, 2)]);

            return hexres.ToArray();
        }
        static public byte[] GeneratePlayReadyContentKey(byte[] keySeed, Guid keyId)
        {
            const int DRM_AES_KEYSIZE_128 = 16;
            byte[] contentKey = new byte[DRM_AES_KEYSIZE_128];
            //
            // Truncate the key seed to 30 bytes, key seed must be at least 30 bytes long.
            //
            byte[] truncatedKeySeed = new byte[30];
            Array.Copy(keySeed, truncatedKeySeed, truncatedKeySeed.Length);
            //
            // Get the keyId as a byte array
            //
            byte[] keyIdAsBytes = keyId.ToByteArray();
            //
            // Create sha_A_Output buffer. It is the SHA of the truncatedKeySeed and the keyIdAsBytes
            //
            SHA256Managed sha_A = new SHA256Managed();
            sha_A.TransformBlock(truncatedKeySeed, 0, truncatedKeySeed.Length, truncatedKeySeed, 0);
            sha_A.TransformFinalBlock(keyIdAsBytes, 0, keyIdAsBytes.Length);
            byte[] sha_A_Output = sha_A.Hash;
            //
            // Create sha_B_Output buffer. It is the SHA of the truncatedKeySeed, the keyIdAsBytes, and
            // the truncatedKeySeed again.
            //
            SHA256Managed sha_B = new SHA256Managed();
            sha_B.TransformBlock(truncatedKeySeed, 0, truncatedKeySeed.Length, truncatedKeySeed, 0);
            sha_B.TransformBlock(keyIdAsBytes, 0, keyIdAsBytes.Length, keyIdAsBytes, 0);
            sha_B.TransformFinalBlock(truncatedKeySeed, 0, truncatedKeySeed.Length);
            byte[] sha_B_Output = sha_B.Hash;
            //
            // Create sha_C_Output buffer. It is the SHA of the truncatedKeySeed, the keyIdAsBytes,
            // the truncatedKeySeed again, and the keyIdAsBytes again.
            //
            SHA256Managed sha_C = new SHA256Managed();
            sha_C.TransformBlock(truncatedKeySeed, 0, truncatedKeySeed.Length, truncatedKeySeed, 0);
            sha_C.TransformBlock(keyIdAsBytes, 0, keyIdAsBytes.Length, keyIdAsBytes, 0);
            sha_C.TransformBlock(truncatedKeySeed, 0, truncatedKeySeed.Length, truncatedKeySeed, 0);
            sha_C.TransformFinalBlock(keyIdAsBytes, 0, keyIdAsBytes.Length);
            byte[] sha_C_Output = sha_C.Hash;
            for (int i = 0; i < DRM_AES_KEYSIZE_128; i++)
            {
                contentKey[i] = Convert.ToByte(sha_A_Output[i] ^ sha_A_Output[i + DRM_AES_KEYSIZE_128]
                ^ sha_B_Output[i] ^ sha_B_Output[i + DRM_AES_KEYSIZE_128]
                ^ sha_C_Output[i] ^ sha_C_Output[i + DRM_AES_KEYSIZE_128]);
            }
            return contentKey;
        }
        static public string DumpHex(byte[] data, bool bExt = true)
        {
            string result = string.Empty;
            string resultHex = " ";
            string resultASCII = " ";
            if (bExt == true)
            {
                int Len = ((data.Length % 16 == 0) ? (data.Length / 16) : (data.Length / 16) + 1) * 16;
                for (int i = 0; i < Len; i++)
                {
                    if (i < data.Length)
                    {
                        resultASCII += string.Format("{0}", ((data[i] >= 32) && (data[i] < 127)) ? (char)data[i] : '.');
                        resultHex += string.Format("{0:X2} ", data[i]);
                    }
                    else
                    {
                        resultASCII += " ";
                        resultHex += "   ";
                    }
                    if (i % 16 == 15)
                    {
                        result += string.Format("{0:X8} ", i - 15) + resultHex + resultASCII + "\r\n";
                        resultHex = " ";
                        resultASCII = " ";
                    }
                }
            }
            else
            {
                if (data != null)
                {

                    for(int i = 0; i < data.Length; i++)
                        result += string.Format("{0:X2} ", data[i]);
                }


            }


            return result;
        }

        static bool Decrypt(Options opt, bool bDecrypt = true)
        {
            bool result = true;
            opt.Status = Options.TheadStatus.Running;
            opt.ThreadStartTime = DateTime.Now;
            opt.LogInformation("Decrypting file: " + opt.InputUri);

            byte[] ContentKey;
            if (string.IsNullOrEmpty(opt.ContentKey))
            {
                Guid KidGuid = new Guid(ConvertHexaStringToByteArray(opt.KeyID));
                ContentKey = GeneratePlayReadyContentKey(ConvertHexaStringToByteArray(opt.KeySeed), KidGuid);
                opt.LogInformation("Generating ContentKey from KeyID and KeySeed");
                opt.LogInformation("KeyID: " + KidGuid.ToString());
                opt.LogInformation("KeySeed: " + opt.KeySeed);
                opt.LogInformation("ContentKey: " + DumpHex(ContentKey,false));

            }
            else
            {
                ContentKey = ConvertHexaStringToByteArray(opt.ContentKey);
                opt.LogInformation("Using directly ContentKey");
                opt.LogInformation("ContentKey: " + DumpHex(ContentKey));
            }
            //if(((opt.TraceLevel>= Options.LogLevel.Verbose)&&(!string.IsNullOrEmpty(opt.TraceFile)))||(opt.ConsoleLevel >= Options.LogLevel.Verbose))
            //    opt.LogVerbose(Mp4Box.ParseFileVerbose(opt.InputUri));
            //else
            //    opt.LogInformation(Mp4Box.ParseFile(opt.InputUri));


                         byte[] kid = { 0xAE, 0x2A, 0x8B, 0x76, 0xF9, 0xC3, 0x4E, 0xAB, 0x9D, 0xC4, 0x55, 0xB7, 0x32, 0xB8, 0x40, 0xE9 };
            //byte[] key = { 0xD4, 0x72, 0x45, 0x9A, 0x00, 0x77, 0xE7, 0x05, 0x9F, 0x33, 0x58, 0x3B, 0xCC, 0x5C, 0x47, 0xCD };
            // byte[] input = {
            // 0xC4, 0xF2, 0x0E, 0x3A, 0x4F, 0x90, 0xE9, 0x5C,
            // 0x02, 0x16, 0x2D, 0xB0, 0x4F, 0x2C, 0x44, 0x87, 0x8C, 0x90, 0xC6, 0x06, 0x7A, 0x82, 0x85, 0x74, 0x60, 0x4E, 0x70, 0x49, 0xB1, 0x9D, 0x16, 0xCA, 0x1B, 0x29, 0xC2, 0x31, 0xD8, 0x09, 0xE7, 0xEE,
            // 0xCC, 0xFD, 0x38, 0x83, 0x4D, 0x48, 0x23, 0x45, 0xD1, 0xC0, 0x26, 0x6E, 0xEC, 0x6C, 0x87, 0xE7, 0xAB, 0x2F, 0x59, 0x13, 0xE0, 0x0D, 0x95, 0xBF, 0x18, 0xA3, 0x88, 0x72, 0xB3, 0xA4, 0x98, 0x6F,
            // 0xD9, 0xBE, 0x2C, 0x9B, 0x00, 0xB0, 0x85, 0x1D, 0xA7, 0x0C, 0x5C, 0xBD, 0x38, 0x6B, 0x67, 0x4C, 0x90, 0xF8, 0xBE, 0xB2, 0x81, 0x03, 0x4A, 0x63, 0x44, 0xFD, 0xEA, 0xD1, 0xBB, 0x1B, 0xE8, 0x0F,
            // 0xC3, 0x30, 0xA0, 0x9A, 0xD7, 0xA4, 0xFE, 0x0B, 0x0B, 0x3A, 0xF8, 0x4B, 0x47, 0x95, 0xBE, 0x4B, 0x82, 0xAA, 0x5C, 0x3D, 0xC8, 0x08, 0x18, 0x9C, 0xE9, 0xD4, 0xB9, 0xE2, 0xB6, 0x81, 0x15, 0x2C,
            // 0xD2, 0xD8, 0x49, 0xE8, 0x62, 0xB7, 0x06, 0x56, 0xE9, 0xEC, 0x34, 0xC5, 0xD4, 0xA9, 0xEA, 0x5C, 0x82, 0xE3, 0x3D, 0x9D, 0xFF, 0x78, 0x20, 0x4B, 0xF1, 0x2C, 0x34, 0x0B, 0xCA };



            byte[] input =             {
                            0x86,0x2B,0xDE,0x8D,0x3A,0x61,0x2B,0x29,0x02,0x0F,0xBA,0xDF,0x43,0xBF,0x05,0x15,0x28,0xCD,0x4E,0x8E,0xBF,0xD6,0x6F,0x84,
            0xB1,0x82,0x0D,0x12,0xD1,0x88,0xAD,0xB7,0x79,0x19,0xE5,0xF8,0x4C,0x3E,0x36,0xFF,
            0x80,0x7B,0x71,0xAB,0x4F,0xA8,0xE0,0x9B,0x9C,0xFD,0x32,0x52,0x09,0x68,0x2F,0xB4,
            0xC2,0x6E,0x99,0x9B,0x15,0x84,0xE6,0x6E,0x8A,0xD8,0x34,0x39,0x5A,0xE1,0x1D,0x29,
            0xA5,0x6F,0x9F,0x52,0x40,0x71,0x43,0x14,0x1C,0x13,0xAE,0x1E,0x95,0x1E,0xDF,0x21,
            0xDA,0x92,0xA4,0x79,0x4C,0x9B,0xAB,0x54,0xB8,0xE0,0x4D,0x2F,0x43,0x2A,0x29,0xE8,
            0x69,0x48,0x24,0xE0,0x1F,0xE5,0x39,0x3C,0x78,0xEB,0xEE,0x6F,0xAA,0xCA,0x62,0xDD,
            0x04,0x71,0x9F,0x66,0x37,0xA5,0xD9,0x23,0x12,0x32,0xD9,0xB8,0xB5,0xF9,0x3E,0xED,
            0xF7,0xAB,0x36,0xDF,0xA9,0xA3,0x3D,0x67,0x2F,0xBB,0x26,0x7A,0x4C,0x9B,0x02,0x9C,
            0x27,0xF0,0x46,0xDB,0x58,0x9D,0x89,0xF4,0x84,0xCF,0x8B,0x09,0x4A };

            byte[] key = { 0x78, 0xDA, 0x95, 0x9D, 0x7A, 0xC4, 0x96, 0x6A, 0x36, 0x35, 0x2B, 0x27, 0xEC, 0x85, 0xDE, 0x10 };
          //  byte[] kid = { 0x76, 0x8B, 0x2A, 0xAE, 0xC3, 0xF9, 0xAB, 0x4E, 0x9D, 0xC4, 0x55, 0xB7, 0x32, 0xB8, 0x40, 0xE9 };


            byte[] resultArray = new byte[165];
            byte[] finalresultArray = new byte[165];
            AESCTR aesctrdec = AESCTR.CreateDecryptor(key, kid);
            AESCTR aesctrenc = AESCTR.CreateEncryptor(key, kid);
            if ((aesctrdec!=null)&& (aesctrenc != null))
            {
                //byte[] nkid = new byte[16];
                //kid.CopyTo(nkid,0);

                //for (int i = 8; (i < 16) && (i < nkid.Length); i++)
                //    nkid[i] = 0;

                //opt.LogInformation("\r\n" + DumpHex(input));
                //Aes128CounterMode am = new Aes128CounterMode(nkid);
                //ICryptoTransform ict = am.CreateEncryptor(key, null);
                //ict.TransformBlock(input, 0, input.Length, resultArray, 0);
                //opt.LogInformation("\r\n" +DumpHex( resultArray));

                //nkid = new byte[16];
                //kid.CopyTo(nkid, 0);

                //for (int i = 8; (i < 16) && (i < nkid.Length); i++)
                //    nkid[i] = 0;

                //opt.LogInformation("\r\n" + DumpHex(input));
                //am = new Aes128CounterMode(nkid);
                //ict = am.CreateEncryptor(key, null);
                //ict.TransformBlock(input, 0, input.Length, resultArray, 0);
                //opt.LogInformation("\r\n" + DumpHex(resultArray));

                aesctrdec.TransformBlock(input, 0, input.Length, resultArray, 0);
                opt.LogInformation(DumpHex(resultArray));
                aesctrdec.TransformBlock(input, 0, input.Length, resultArray, 0);
                opt.LogInformation(DumpHex(resultArray));

                aesctrenc.TransformBlock(resultArray, 0, resultArray.Length, finalresultArray, 0);
                opt.LogInformation(DumpHex(finalresultArray));
                aesctrdec.TransformBlock(finalresultArray, 0, finalresultArray.Length, resultArray, 0);
                opt.LogInformation(DumpHex(resultArray));


            }


            opt.Status = Options.TheadStatus.Stopped;
            return result;
        }
    }
}
