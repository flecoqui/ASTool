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
        public static bool DecryptFile(Options opt, string InputPath, string OutputPath, byte[] ContentKey, bool bProtectCaption)
        {
            bool bResult = true;
            string log = string.Empty;
            List<int> ListTrackID = null;
            Dictionary<int, List<long>> MOOFOffsetDictionnary = new Dictionary<int, List<long>>();
            try
            {
                FileStream fsi = new FileStream(InputPath, FileMode.Open, FileAccess.Read);
                FileStream fso = new FileStream(OutputPath, FileMode.Create, FileAccess.ReadWrite);
                if ((fsi != null)&& (fso != null))
                {
                    long offset = 0;
                    fsi.Seek((long)offset, SeekOrigin.Begin);
                    while (offset < fsi.Length)
                    {
                        Mp4Box box = Mp4Box.ReadMp4Box(fsi);
                        if (box != null)
                        {
                            log += box.ToString() + "\tat offset: " + offset.ToString() + "\r\n";

                            if (box.GetBoxType() == "ftyp")
                            {
                                // No modification required
                                // Copy the box into the new file
                                if (Mp4Box.WriteMp4Box(box, fso) != true)
                                {
                                    bResult = false;
                                    opt.LogError("Unexpected error while writing ftyp box in the output file: " + OutputPath );
                                    break;
                                }
                                offset += box.GetBoxLength();
                            }
                            else if (box.GetBoxType() == "moov")
                            {
                                // Get the list of tracks which are encrypted
                                // Remove uuid box DO8A...83D3 which contains the protection header
                                // Replace encv box with avc1 and anca with mp4a
                                // Remove sinf box
                                // Calculate the new length and keep the difference with the previous lenght
                                // Copy the new box into the new file
                                Mp4BoxMOOV moov = box as Mp4BoxMOOV;
                                if (moov != null)
                                {
                                    int currentLength = moov.GetBoxLength();
                                    ListTrackID = moov.GetListTrackToDecrypt();
                                    if((ListTrackID!=null)&&(ListTrackID.Count>0))
                                    {
                                        // We need to decrypt some tracks

                                        // Create dictionnary for MOOF offset
                                        foreach(var track in ListTrackID)
                                        {
                                            MOOFOffsetDictionnary.Add(track, new List<long>());
                                        }
                                        bool result = moov.RemoveUUIDBox(Mp4Box.kExtProtectHeaderBoxGuid);
                                        result = moov.UpdateEncBoxes();
                                        byte[] newBuffer = moov.UpdateBoxBuffer();
                                        if (newBuffer != null)
                                        {
                                            int newLength = newBuffer.Length;
                                            int diff = newLength - currentLength;
                                            Mp4Box.WriteMp4BoxBuffer(newBuffer, fso);
                                            offset += newBuffer.Length;
                                        }
                                    }
                                    else
                                    {
                                        bResult = false;
                                        opt.LogError("No track found to decrypt in the moov box output file: " + OutputPath);
                                        break;
                                    }
                                }
                            }
                            else if (box.GetBoxType() == "moof")
                            {
                                // Remove uuid box A239...8DF4 which contains the IV (initialisation vectors for the encryption)
                                // Keep the list of IV (initialisation vectors for the encryption) included in this box
                                // Keep the list of sample size
                                // Calculate the new lenght and keep the difference with the previous lenght
                                // Open the next box (mdat) and decrypt sample by sample 
                                Mp4BoxMOOF moof = box as Mp4BoxMOOF;
                                if (moof != null)
                                {
                                    int currentLength = moof.GetBoxLength();
                                    int currentTrackID = moof.GetTrackID();
                                    Mp4BoxUUID uuidbox = moof.GetUUIDBox(Mp4Box.kExtProtectHeaderMOOFBoxGuid) as Mp4BoxUUID;
                                    Mp4BoxTRUN trunbox = moof.GetChild("trun") as Mp4BoxTRUN ;
                                    if ((trunbox != null) &&(uuidbox != null))
                                    {

                                        bool result = moof.RemoveUUIDBox(Mp4Box.kExtProtectHeaderMOOFBoxGuid);
                                        if (result == true)
                                        {
                                            if (MOOFOffsetDictionnary.ContainsKey(currentTrackID))
                                                MOOFOffsetDictionnary[currentTrackID].Add(offset);

                                            Int32 doff = trunbox.GetDataOffset();
                                            doff -= uuidbox.GetBoxLength();
                                            if(doff>0)
                                                trunbox.SetDataOffset(doff);
                                            else
                                            {
                                                bResult = false;
                                                opt.LogError("Error while updating the dataoffset in the TRUN box in the input file after a moof box: " + InputPath );
                                                break;
                                            }
                                            byte[] newBuffer = moof.UpdateBoxBuffer();
                                            if (newBuffer != null)
                                            {
                                                int newLength = newBuffer.Length;
                                                int diff = newLength - currentLength;

                                                Mp4Box.WriteMp4BoxBuffer(newBuffer, fso);
                                                offset += newBuffer.Length;

                                            }
                                        }
                                        else
                                        {
                                            offset += box.GetBoxLength();
                                            if (Mp4Box.WriteMp4Box(box, fso) != true)
                                            {
                                                bResult = false;
                                                opt.LogError("Unexpected error while writing moof box in the output file: " + OutputPath);
                                                break;
                                            }
                                        }
                                        Mp4Box mdatbox = Mp4Box.ReadMp4Box(fsi);
                                        if (mdatbox != null)
                                        {
                                            if (mdatbox.GetBoxType() != "mdat")
                                            {
                                                bResult = false;
                                                opt.LogError("Unexpected box read in the input file after a moof box: " + InputPath + " box: " + mdatbox.GetBoxType());
                                                break;
                                            }
                                            else
                                            {
                                                List<Mp4BoxUUID.SampleProtection> listIV = uuidbox.GetIVList();
                                                List<Int32> listSampleSize = trunbox.GetSampleSizeList();

                                                if ((listIV != null) &&
                                                    (listSampleSize != null) &&
                                                    (listIV.Count == listSampleSize.Count))
                                                {
                                                    // All the information to decrypt the mdat box are available
                                                    byte[] encryptedData = mdatbox.GetBoxData();
                                                    if (encryptedData != null)
                                                    {
                                                        byte[] clearData = new byte[encryptedData.Length + 8];
                                                        if (clearData != null)
                                                        {
                                                            Mp4Box.WriteMp4BoxInt32(clearData, 0, clearData.Length);
                                                            Mp4Box.WriteMp4BoxString(clearData, 4, "mdat");

                                                            int dataoffset = 0;
                                                            for (int i = 0; i < listIV.Count; i++)
                                                            {
                                                                AESCTR aesctrdec = AESCTR.CreateDecryptor(ContentKey, listIV[i].IV);
                                                                if (aesctrdec != null)
                                                                {
                                                                    int BytesOfClearData = listIV[i].BytesOfClearData;
                                                                    for (int j = 0; j < BytesOfClearData; j++)
                                                                        clearData[8 + dataoffset + j] = encryptedData[dataoffset + j];
                                                                    aesctrdec.TransformBlock(encryptedData, dataoffset+ BytesOfClearData, listSampleSize[i]-BytesOfClearData, clearData, 8+dataoffset+BytesOfClearData);
                                                                    dataoffset += listSampleSize[i];
                                                                }
                                                            }
                                                            if ((dataoffset + 8 )!= clearData.Length)
                                                                System.Diagnostics.Debug.WriteLine("Error");
                                                            Mp4Box.WriteMp4BoxBuffer(clearData, fso);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (Mp4Box.WriteMp4Box(mdatbox, fso) != true)
                                                    {
                                                        bResult = false;
                                                        opt.LogError("Unexpected error while writing mdat box in the output file: " + OutputPath);
                                                        break;
                                                    }
                                                }

                                            }
                                            offset += mdatbox.GetBoxLength();
                                        }
                                        else
                                            opt.LogError("Error while reading the mdat box in the input file after a moof box: " + InputPath);
                                    }
                                }
                            }
                            else if (box.GetBoxType() == "mfra")
                            {
                                // Update each tfra boxes with the new offset of each moof box
                                // Copy the new mfra box into the new file
                                Mp4BoxMFRA mfra = box as Mp4BoxMFRA;
                                if (mfra != null)
                                {
                                    if (MOOFOffsetDictionnary.Count > 0)
                                    {
                                        foreach (var v in MOOFOffsetDictionnary)
                                        {
                                            if(mfra.UpdateMOOFOffsetForTrack(v.Key, v.Value)!=true)
                                            {
                                                bResult = false;
                                                opt.LogError("Error while updating the MFRA/TFRA offset table in the output file: " + OutputPath);
                                                break;
                                            }

                                        }

                                        byte[] newBuffer = mfra.UpdateBoxBuffer();
                                        if (newBuffer != null)
                                        {
                                            Mp4Box.WriteMp4BoxBuffer(newBuffer, fso);
                                            offset += newBuffer.Length;
                                        }
                                    }
                                    else
                                    {
                                        if (Mp4Box.WriteMp4Box(box, fso) != true)
                                        {
                                            bResult = false;
                                            opt.LogError("Unexpected error while writing mfra box in the output file: " + OutputPath);
                                            break;
                                        }
                                        offset += box.GetBoxLength();
                                    }
                                }
                            }
                            else 
                            {
                                bResult = false;
                                opt.LogError("Unexpected box read in the input file: " + InputPath + " box: " + box.GetBoxType());
                                offset += box.GetBoxLength();
                                break;
                            }
                        }
                        else
                            break;
                    }
                    fsi.Close();
                    fso.Close();
                }

            }
            catch (Exception ex)
            {
                opt.LogError("ERROR: Exception while decrypting the file: " + InputPath + " Exception: " + ex.Message);
            }
            return bResult;
        }
        public static bool EncryptFile(Options opt, string InputPath, string OutputPath, byte[] ContentKey, bool bProtectCaption)
        {
            bool bResult = true;
            string log = string.Empty;
            try
            {
                FileStream fsi = new FileStream(InputPath, FileMode.Open, FileAccess.Read);
                FileStream fso = new FileStream(OutputPath, FileMode.Create, FileAccess.ReadWrite);
                if ((fsi != null) && (fso != null))
                {
                    long offset = 0;
                    fsi.Seek((long)offset, SeekOrigin.Begin);
                    while (offset < fsi.Length)
                    {
                        Mp4Box box = Mp4Box.ReadMp4Box(fsi);
                        if (box != null)
                        {
                            log += box.ToString() + "\tat offset: " + offset.ToString() + "\r\n";

                            if (box.GetBoxType() != "ftyp\0")
                            {
                                // No modification required
                                // Copy the box into the new file
                                if (Mp4Box.WriteMp4Box(box, fso) != true)
                                {
                                    bResult = false;
                                    opt.LogError("Unexpected error while writing ftyp box in the output file: " + OutputPath);
                                    break;
                                }
                                offset += box.GetBoxLength();
                            }
                            else if (box.GetBoxType() != "moov\0")
                            {
                                // Get the list of tracks which are encrypted
                                // Remove uuid box DO8A...83D3 which contains the protection header
                                // Replace encv box with avc1 and anca with mp4a
                                // Remove sinf box
                                // Calculate the new lenght and keep the difference with the previous lenght
                                // Copy the new box into the new file
                                if (Mp4Box.WriteMp4Box(box, fso) != true)
                                {
                                    bResult = false;
                                    opt.LogError("Unexpected error while writing moov box in the output file: " + OutputPath);
                                    break;
                                }

                                offset += box.GetBoxLength();
                            }
                            else if (box.GetBoxType() != "moof\0")
                            {
                                // Remove uuid box A239...8DF4 which contains the IV (initialisation vectors for the encryption)
                                // Keep the list of IV (initialisation vectors for the encryption) included in this box
                                // Keep the list of sample size
                                // Calculate the new lenght and keep the difference with the previous lenght
                                // Open the next box (mdat) and decrypt sample by sample 
                                offset += box.GetBoxLength();
                                Mp4Box mdatbox = Mp4Box.ReadMp4Box(fsi);
                                if (mdatbox != null)
                                {
                                    if (mdatbox.GetBoxType() != "mdat\0")
                                    {
                                        bResult = false;
                                        opt.LogError("Unexpected box read in the input file after a moof box: " + InputPath + " box: " + mdatbox.GetBoxType());
                                        break;
                                    }
                                    else
                                    {
                                        if (Mp4Box.WriteMp4Box(box, fso) != true)
                                        {
                                            bResult = false;
                                            opt.LogError("Unexpected error while writing moof box in the output file: " + OutputPath);
                                            break;
                                        }
                                        if (Mp4Box.WriteMp4Box(mdatbox, fso) != true)
                                        {
                                            bResult = false;
                                            opt.LogError("Unexpected error while writing mdat box in the output file: " + OutputPath);
                                            break;
                                        }

                                    }
                                    // Copy the new moof box into the new file
                                    // Copy the new mdat box into the new file

                                    offset += mdatbox.GetBoxLength();
                                }
                                else
                                    opt.LogError("Error while reading the mdat box in the input file after a moof box: " + InputPath);
                            }
                            else if (box.GetBoxType() != "mfra\0")
                            {
                                // Update each tfra boxes with the new offset of each moof box
                                // Copy the new mfra box into the new file
                                if (Mp4Box.WriteMp4Box(box, fso) != true)
                                {
                                    bResult = false;
                                    opt.LogError("Unexpected error while writing mfra box in the output file: " + OutputPath);
                                    break;
                                }

                                offset += box.GetBoxLength();
                            }
                            else
                            {
                                bResult = false;
                                opt.LogError("Unexpected box read in the input file: " + InputPath + " box: " + box.GetBoxType());
                                offset += box.GetBoxLength();
                                break;
                            }
                        }
                        else
                            break;
                    }
                    fsi.Close();
                    fso.Close();
                }

            }
            catch (Exception ex)
            {
                opt.LogError("ERROR: Exception while decrypting the file: " + InputPath + " Exception: " + ex.Message);
            }
            return bResult;
        }
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
        static bool SameBlock(byte[] input1, byte[] input2)
        {
            bool result = false;

            if((input1!=null)&&
                (input2 != null))
            {
                if(input1.Length == input2.Length)
                {
                    for (int i = 0; i < input1.Length; i++)
                        if (input1[i] != input2[i])
                            return false ;
                    return true;
                }

            }
            return result;
        }
        static bool TestEncryptDecrypt(Options opt)
        {
            bool result = true;
            //command line
            //dotnet ASTool.dll--decrypt--input C:\temp\astool\testvod\metisser_encrypted\metisser\metisser_500_enc.ismv--output C:\temp\astool\testvod\metisser_encrypted\metisser\metisser_500_dec.ismv--keyseed XVBovsmzhP9gRIZxWfFta3VVRPzVEWmJsazEJ46I --keyid AE2A8B76 - F9C3 - 4EAB - 9DC4 - 55B732B840E9

            byte[] iv2 = { 0xAE, 0x2A, 0x8B, 0x76, 0xF9, 0xC3, 0x4E, 0xAB, 0x9D, 0xC4, 0x55, 0xB7, 0x32, 0xB8, 0x40, 0xE9 };
            byte[] key2 = { 0xD4, 0x72, 0x45, 0x9A, 0x00, 0x77, 0xE7, 0x05, 0x9F, 0x33, 0x58, 0x3B, 0xCC, 0x5C, 0x47, 0xCD };
            byte[] input2 = {
             0xC4, 0xF2, 0x0E, 0x3A, 0x4F, 0x90, 0xE9, 0x5C,
             0x02, 0x16, 0x2D, 0xB0, 0x4F, 0x2C, 0x44, 0x87, 0x8C, 0x90, 0xC6, 0x06, 0x7A, 0x82, 0x85, 0x74, 0x60, 0x4E, 0x70, 0x49, 0xB1, 0x9D, 0x16, 0xCA, 0x1B, 0x29, 0xC2, 0x31, 0xD8, 0x09, 0xE7, 0xEE,
             0xCC, 0xFD, 0x38, 0x83, 0x4D, 0x48, 0x23, 0x45, 0xD1, 0xC0, 0x26, 0x6E, 0xEC, 0x6C, 0x87, 0xE7, 0xAB, 0x2F, 0x59, 0x13, 0xE0, 0x0D, 0x95, 0xBF, 0x18, 0xA3, 0x88, 0x72, 0xB3, 0xA4, 0x98, 0x6F,
             0xD9, 0xBE, 0x2C, 0x9B, 0x00, 0xB0, 0x85, 0x1D, 0xA7, 0x0C, 0x5C, 0xBD, 0x38, 0x6B, 0x67, 0x4C, 0x90, 0xF8, 0xBE, 0xB2, 0x81, 0x03, 0x4A, 0x63, 0x44, 0xFD, 0xEA, 0xD1, 0xBB, 0x1B, 0xE8, 0x0F,
             0xC3, 0x30, 0xA0, 0x9A, 0xD7, 0xA4, 0xFE, 0x0B, 0x0B, 0x3A, 0xF8, 0x4B, 0x47, 0x95, 0xBE, 0x4B, 0x82, 0xAA, 0x5C, 0x3D, 0xC8, 0x08, 0x18, 0x9C, 0xE9, 0xD4, 0xB9, 0xE2, 0xB6, 0x81, 0x15, 0x2C,
             0xD2, 0xD8, 0x49, 0xE8, 0x62, 0xB7, 0x06, 0x56, 0xE9, 0xEC, 0x34, 0xC5, 0xD4, 0xA9, 0xEA, 0x5C, 0x82, 0xE3, 0x3D, 0x9D, 0xFF, 0x78, 0x20, 0x4B, 0xF1, 0x2C, 0x34, 0x0B, 0xCA };


            //command line
            //dotnet ASTool.dll--decrypt--input C:\temp\astool\testvod\metisser_encrypted\metisser\metisser_500_enc.ismv--output C:\temp\astool\testvod\metisser_encrypted\metisser\metisser_500_dec.ismv --contentkey 78DA959D7AC4966A36352B27EC85DE10

            byte[] input1 = {
            0x86,0x2B,0xDE,0x8D,0x3A,0x61,0x2B,0x29,0x02,0x0F,0xBA,0xDF,0x43,0xBF,0x05,0x15,
            0x28,0xCD,0x4E,0x8E,0xBF,0xD6,0x6F,0x84,
            0xB1,0x82,0x0D,0x12,0xD1,0x88,0xAD,0xB7,0x79,0x19,0xE5,0xF8,0x4C,0x3E,0x36,0xFF,
            0x80,0x7B,0x71,0xAB,0x4F,0xA8,0xE0,0x9B,0x9C,0xFD,0x32,0x52,0x09,0x68,0x2F,0xB4,
            0xC2,0x6E,0x99,0x9B,0x15,0x84,0xE6,0x6E,0x8A,0xD8,0x34,0x39,0x5A,0xE1,0x1D,0x29,
            0xA5,0x6F,0x9F,0x52,0x40,0x71,0x43,0x14,0x1C,0x13,0xAE,0x1E,0x95,0x1E,0xDF,0x21,
            0xDA,0x92,0xA4,0x79,0x4C,0x9B,0xAB,0x54,0xB8,0xE0,0x4D,0x2F,0x43,0x2A,0x29,0xE8,
            0x69,0x48,0x24,0xE0,0x1F,0xE5,0x39,0x3C,0x78,0xEB,0xEE,0x6F,0xAA,0xCA,0x62,0xDD,
            0x04,0x71,0x9F,0x66,0x37,0xA5,0xD9,0x23,0x12,0x32,0xD9,0xB8,0xB5,0xF9,0x3E,0xED,
            0xF7,0xAB,0x36,0xDF,0xA9,0xA3,0x3D,0x67,0x2F,0xBB,0x26,0x7A,0x4C,0x9B,0x02,0x9C,
            0x27,0xF0,0x46,0xDB,0x58,0x9D,0x89,0xF4,0x84,0xCF,0x8B,0x09,0x4A };

            byte[] key1 = { 0x78, 0xDA, 0x95, 0x9D, 0x7A, 0xC4, 0x96, 0x6A, 0x36, 0x35, 0x2B, 0x27, 0xEC, 0x85, 0xDE, 0x10 };
            byte[] iv1 = { 0x2C, 0x53, 0xC8, 0x32, 0xA1, 0x2C, 0xC8, 0xBC, 0x9D, 0xC4, 0x55, 0xB7, 0x32, 0xB8, 0x40, 0xE9 };
            byte[] clearAsset = {
            0x21, 0x5B, 0xDD, 0x4D, 0x07, 0x8D, 0xCA, 0x20, 0x35, 0x12, 0x8A, 0xEB, 0x8D, 0x5C, 0x46, 0x57,
            0x32, 0xED, 0x43, 0x30, 0x40, 0x53, 0x8C, 0xDE, 0x13, 0x3A, 0x8F, 0x57, 0x2C, 0x29, 0x0E, 0xB0,
            0x09, 0x3A, 0xBC, 0x27, 0xBA, 0xD8, 0x08, 0x99, 0xD4, 0x94, 0x01, 0xA0, 0x14, 0x7E, 0xE0, 0x00,
            0x1B, 0x2A, 0xC5, 0xC5, 0xA9, 0xA8, 0x0A, 0x6B, 0x95, 0x54, 0x3E, 0xF2, 0x3E, 0x06, 0x15, 0xB5,
            0xAD, 0x6F, 0x9C, 0x00, 0x03, 0x65, 0xD7, 0xEE, 0xA5, 0x70, 0x95, 0xBB, 0xB1, 0xA9, 0xDD, 0xC7,
            0x6B, 0xB0, 0xC2, 0xF4, 0x6F, 0x5F, 0x44, 0x79, 0xDD, 0xC8, 0x59, 0x42, 0x6A, 0x10, 0x4A, 0x1C,
            0xFD, 0x35, 0x3A, 0xB2, 0xE5, 0xBF, 0x5F, 0x91, 0x40, 0xEF, 0x4D, 0xB9, 0x72, 0xCD, 0x97, 0xE3,
            0x5B, 0x29, 0xDC, 0x26, 0x09, 0x70, 0xF0, 0x08, 0xF6, 0x3E, 0x7D, 0x6B, 0x57, 0x50, 0xAC, 0x92,
            0xD5, 0x84, 0x94, 0x04, 0x05, 0xD1, 0x12, 0x0A, 0x85, 0x89, 0x7D, 0x6F, 0x0F, 0x25, 0xC2, 0x88,
            0x04, 0x7C, 0xB9, 0x8A, 0xC7, 0xB8, 0x22, 0xB9, 0x7B, 0x00, 0x03, 0xF9, 0x1E, 0x3C, 0x4B, 0xD6,
            0xE7, 0x75, 0x23, 0x50, 0xE0 };

            byte[] resultArray = new byte[165];
            byte[] finalresultArray = new byte[165];
            AESCTR aesctrdec = AESCTR.CreateDecryptor(key1, iv1);
            AESCTR aesctrenc = AESCTR.CreateEncryptor(key1, iv1);
            if ((aesctrdec != null) && (aesctrenc != null))
            {
                opt.LogInformation("Tests with the first set of assets");
                opt.LogInformation("\r\nFirst Test: Decrypt twice the same asset");
                aesctrdec.TransformBlock(input1, 0, input1.Length, resultArray, 0);
                opt.LogInformation("Result first decrypt: \r\n" + DumpHex(resultArray));
                if(SameBlock(resultArray,clearAsset))
                    opt.LogInformation("Test successful" );
                else
                {
                    result = false;
                    opt.LogInformation("Test failed");
                }

                aesctrdec.TransformBlock(input1, 0, input1.Length, resultArray, 0);
                opt.LogInformation("Result second decrypt: \r\n" + DumpHex(resultArray));
                if (SameBlock(resultArray, clearAsset))
                    opt.LogInformation("Test successful");
                else
                {
                    result = false;
                    opt.LogInformation("Test failed");
                }

                opt.LogInformation("\r\nSecond Test: Encrypt and Decrypt the same asset");
                aesctrenc.TransformBlock(resultArray, 0, resultArray.Length, finalresultArray, 0);
                opt.LogInformation("First Test: Encrypt \r\n" + DumpHex(finalresultArray));
                aesctrdec.TransformBlock(finalresultArray, 0, finalresultArray.Length, resultArray, 0);
                opt.LogInformation("\r\nSecond Test: Decrypt \r\n" + DumpHex(resultArray));
                if (SameBlock(resultArray, clearAsset))
                    opt.LogInformation("Test successful");
                else
                {
                    result = false;
                    opt.LogInformation("Test failed");
                }

            }

            aesctrdec = AESCTR.CreateDecryptor(key2, iv2);
            aesctrenc = AESCTR.CreateEncryptor(key2, iv2);
            if ((aesctrdec != null) && (aesctrenc != null))
            {
                opt.LogInformation("Tests with the second set of assets");

                opt.LogInformation("\r\nFirst Test: Decrypt twice the same asset");
                aesctrdec.TransformBlock(input2, 0, input2.Length, resultArray, 0);
                opt.LogInformation("Result first decrypt: \r\n" + DumpHex(resultArray));
                if (SameBlock(resultArray, clearAsset))
                    opt.LogInformation("Test successful");
                else
                {
                    result = false;
                    opt.LogInformation("Test failed");
                }

                aesctrdec.TransformBlock(input2, 0, input2.Length, resultArray, 0);
                opt.LogInformation("Result second decrypt: \r\n" + DumpHex(resultArray));
                if (SameBlock(resultArray, clearAsset))
                    opt.LogInformation("Test successful");
                else
                {
                    result = false;
                    opt.LogInformation("Test failed");
                }

                opt.LogInformation("\r\nSecond Test: Encrypt and Decrypt the same asset");
                aesctrenc.TransformBlock(resultArray, 0, resultArray.Length, finalresultArray, 0);
                opt.LogInformation("First Test: Encrypt \r\n" + DumpHex(finalresultArray));
                aesctrdec.TransformBlock(finalresultArray, 0, finalresultArray.Length, resultArray, 0);
                opt.LogInformation("\r\nSecond Test: Decrypt \r\n" + DumpHex(resultArray));
                if (SameBlock(resultArray, clearAsset))
                    opt.LogInformation("Test successful");
                else
                {
                    result = false;
                    opt.LogInformation("Test failed");
                }
            }
            return result;
        }
        static bool DisplayDecryptInformation(Options opt, string message)
        {
            opt.LogInformation(message);
            return true;
        }
        static bool DisplayDecryptVerbose(Options opt, string message)
        {
            opt.LogVerbose(message);
            return true;
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

            //if (TestEncryptDecrypt(opt) == false)
            //    opt.LogInformation("Encrypt Decrypt Tests failed");
            //else 
            //    opt.LogInformation("Encrypt Decrypt Tests successful");
            if (bDecrypt == true)
                DecryptFile(opt, opt.InputUri, opt.OutputUri, ContentKey,false);
            else
                EncryptFile(opt, opt.InputUri, opt.OutputUri, ContentKey, false);

            opt.Status = Options.TheadStatus.Stopped;
            return result;
        }
    }
}
