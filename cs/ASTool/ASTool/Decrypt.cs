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

            if(((opt.TraceLevel>= Options.LogLevel.Verbose)&&(!string.IsNullOrEmpty(opt.TraceFile)))||(opt.ConsoleLevel >= Options.LogLevel.Verbose))
                opt.LogVerbose(Mp4Box.ParseFileVerbose(opt.InputUri));
            else
                opt.LogInformation(Mp4Box.ParseFile(opt.InputUri));
            opt.LogInformation("Decrypting file: " + opt.InputUri + " done");
            opt.Status = Options.TheadStatus.Stopped;
            return result;
        }
    }
}
