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

        public static string ParseFileVerbose(string Path, Options opt, CallBack callback)
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
                            if (box.GetBoxType()!= "mdat\0")
                                result += Mp4Box.GetBoxChildrenString(0, box);
                            result += Options.DumpHex(box.GetBoxBytes());
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
        static bool DisplayInformation(Options opt, string message)
        {
            opt.LogInformation(message);
            return true;
        }
        static bool DisplayVerbose(Options opt, string message)
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

            if(((opt.TraceLevel>= Options.LogLevel.Verbose)&&(!string.IsNullOrEmpty(opt.TraceFile)))||(opt.ConsoleLevel >= Options.LogLevel.Verbose))
                opt.LogVerbose(ParseFileVerbose(opt.InputUri, opt, DisplayVerbose));
            else
                opt.LogInformation(ParseFile(opt.InputUri, opt, DisplayInformation));
            opt.LogInformation("Parsing file: " + opt.InputUri + " done");
            opt.Status = Options.TheadStatus.Stopped;
            return result;
        }
    }
}
