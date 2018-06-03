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
        public delegate bool CallBack(Options opt, string message);

        public static string ParseFile(string Path, Options opt, CallBack callback)
        {
            string result = "\r\n";
            try
            {
                FileStream fs = new FileStream(Path, FileMode.Open, FileAccess.Read);
                if (fs != null)
                {
                    long offset = 0;
                    fs.Seek((long)offset, SeekOrigin.Begin);
                    while (offset < fs.Length)
                    {
                        Mp4Box box = Mp4Box.ReadMp4Box(fs);
                        if (box != null)
                        {
                            result += box.ToString() + "\tat offset: " + offset.ToString() + "\r\n";
                            if (box.GetBoxType() != "mdat\0")
                                result += Mp4Box.GetBoxChildrenString(0, box);


                            offset += box.GetBoxLength();
                            if (callback != null)
                                callback(opt,result);
                            result = "\r\n";
                        }
                        else
                            break;
                    }
                    fs.Close();

                }

            }
            catch (Exception ex)
            {
                result += "ERROR: Exception while parsing the file: " + ex.Message;
            }
            return result;
        }

        public static bool ParseFile(string Path, Options opt)
        {
            bool result = false;
            string log = "\r\n";
            try
            {
                FileStream fs = new FileStream(Path, FileMode.Open, FileAccess.Read);
                if (fs != null)
                {
                    long offset = 0;
                    fs.Seek((long)offset, SeekOrigin.Begin);
                    while (offset < fs.Length)
                    {
                        Mp4Box box = Mp4Box.ReadMp4Box(fs);
                        if (box != null)
                        {
                            log += box.ToString() + "\tat offset: " + offset.ToString() + "\r\n";
                            if (box.GetBoxType()!= "mdat\0")
                                log += Mp4Box.GetBoxChildrenString(0, box);
                            if (((opt.TraceLevel >= Options.LogLevel.Verbose) && (!string.IsNullOrEmpty(opt.TraceFile))) || (opt.ConsoleLevel >= Options.LogLevel.Verbose))
                                log += Options.DumpHex(box.GetBoxBytes());
                            offset += box.GetBoxLength();
                            opt.LogInformation(log);
                            log = "\r\n";
                        }
                        else
                            break;
                    }
                    fs.Close();
                    result = true;
                }

            }
            catch (Exception ex)
            {
                opt.LogError("ERROR: Exception while parsing the file: " + ex.Message);
            }
            return result;
        }
        static bool DisplayParseInformation(Options opt, string message)
        {
            opt.LogInformation(message);
            return true;
        }
        static bool DisplayParseVerbose(Options opt, string message)
        {
            opt.LogVerbose(message);
            return true;
        }
        static bool Parse(Options opt)
        {
            bool result = true;
            opt.Status = Options.TheadStatus.Running;
            opt.ThreadStartTime = DateTime.Now;
            opt.LogInformation("Parsing file: " + opt.InputUri);
            ParseFile(opt.InputUri, opt);
            opt.LogInformation("Parsing file: " + opt.InputUri + " done");
            opt.Status = Options.TheadStatus.Stopped;
            return result;
        }
    }
}
