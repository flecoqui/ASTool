﻿//*********************************************************
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



        static bool Decrypt(Options opt)
        {
            bool result = true;
            opt.Status = Options.TheadStatus.Running;
            opt.ThreadStartTime = DateTime.Now;
            opt.LogInformation("Decrypting file: " + opt.InputUri);

            //if(((opt.TraceLevel>= Options.LogLevel.Verbose)&&(!string.IsNullOrEmpty(opt.TraceFile)))||(opt.ConsoleLevel >= Options.LogLevel.Verbose))
            //    opt.LogVerbose(Mp4Box.ParseFileVerbose(opt.InputUri));
            //else
            //    opt.LogInformation(Mp4Box.ParseFile(opt.InputUri));


//            byte[] kid = { 0xAE, 0x2A, 0x8B, 0x76, 0xF9, 0xC3, 0x4E, 0xAB, 0x9D, 0xC4, 0x55, 0xB7, 0x32, 0xB8, 0x40, 0xE9 };
            byte[] kid = { 0xAE, 0x2A, 0x8B, 0x76, 0xF9, 0xC3, 0x4E, 0xAB, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            byte[] key = { 0xD4, 0x72, 0x45, 0x9A, 0x00, 0x77, 0xE7, 0x05, 0x9F, 0x33, 0x58, 0x3B, 0xCC, 0x5C, 0x47, 0xCD };
            byte[] input = {
            0xC4, 0xF2, 0x0E, 0x3A, 0x4F, 0x90, 0xE9, 0x5C,
            0x02, 0x16, 0x2D, 0xB0, 0x4F, 0x2C, 0x44, 0x87, 0x8C, 0x90, 0xC6, 0x06, 0x7A, 0x82, 0x85, 0x74, 0x60, 0x4E, 0x70, 0x49, 0xB1, 0x9D, 0x16, 0xCA, 0x1B, 0x29, 0xC2, 0x31, 0xD8, 0x09, 0xE7, 0xEE,
            0xCC, 0xFD, 0x38, 0x83, 0x4D, 0x48, 0x23, 0x45, 0xD1, 0xC0, 0x26, 0x6E, 0xEC, 0x6C, 0x87, 0xE7, 0xAB, 0x2F, 0x59, 0x13, 0xE0, 0x0D, 0x95, 0xBF, 0x18, 0xA3, 0x88, 0x72, 0xB3, 0xA4, 0x98, 0x6F,
            0xD9, 0xBE, 0x2C, 0x9B, 0x00, 0xB0, 0x85, 0x1D, 0xA7, 0x0C, 0x5C, 0xBD, 0x38, 0x6B, 0x67, 0x4C, 0x90, 0xF8, 0xBE, 0xB2, 0x81, 0x03, 0x4A, 0x63, 0x44, 0xFD, 0xEA, 0xD1, 0xBB, 0x1B, 0xE8, 0x0F,
            0xC3, 0x30, 0xA0, 0x9A, 0xD7, 0xA4, 0xFE, 0x0B, 0x0B, 0x3A, 0xF8, 0x4B, 0x47, 0x95, 0xBE, 0x4B, 0x82, 0xAA, 0x5C, 0x3D, 0xC8, 0x08, 0x18, 0x9C, 0xE9, 0xD4, 0xB9, 0xE2, 0xB6, 0x81, 0x15, 0x2C,
            0xD2, 0xD8, 0x49, 0xE8, 0x62, 0xB7, 0x06, 0x56, 0xE9, 0xEC, 0x34, 0xC5, 0xD4, 0xA9, 0xEA, 0x5C, 0x82, 0xE3, 0x3D, 0x9D, 0xFF, 0x78, 0x20, 0x4B, 0xF1, 0x2C, 0x34, 0x0B, 0xCA };
           // 0x6C, 0x97, 0xD3,
           // 0x1B, 0x6E, 0x85, 0xB5, 0xA4, 0x64, 0x2A, 0xF8, 0x50, 0xB2, 0xF0, 0x55, 0xAD, 0xA2, 0x8B, 0xDC };

            byte[] resultArray = new byte[165];
            Aes128CounterMode am;
            CounterModeCryptoTransform ict;
            am = new Aes128CounterMode(kid);
            ict =(CounterModeCryptoTransform) am.CreateDecryptor(key, null);
            ict.TransformBlock(input, 0, input.Length, resultArray, 0);
            opt.LogInformation("Decrypting file: " + opt.InputUri + " done");

            opt.LogInformation(Options.DumpHex(resultArray));
            opt.Status = Options.TheadStatus.Stopped;
            return result;
        }
    }
}
