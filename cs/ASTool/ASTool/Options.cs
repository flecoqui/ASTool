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
using System.IO;
using System.Text;
using System.Runtime.Serialization;

namespace ASTool
{
    [DataContract(Name = "Options")]
    /// <summary>
    /// Options that control the behavior of the program
    /// </summary>
    public class Options
    {
        [DataContract(Name = "Action")]
        public enum Action {
            [EnumMember]
            None = 0,
            [EnumMember]
            Help,
            [EnumMember]
            Pull,
            [EnumMember]
            Push,
            [EnumMember]
            PullPush,
            [EnumMember]
            Parse,
            [EnumMember]
            Import,
            [EnumMember]
            Export
        }
        [DataContract(Name = "LogLevel")]
        public enum LogLevel
        {
            [EnumMember]
            None = 0,

            [EnumMember]
            Error,
            [EnumMember]
            Information,
            [EnumMember]
            Warning,
            [EnumMember]
            Verbose
        }
        [DataContract(Name = "TheadStatus")]
        public enum TheadStatus
        {
            [EnumMember]
            Initializing = 0,
            [EnumMember]
            Running,
            [EnumMember]
            Stopping,
            [EnumMember]
            Stopped
        }
        [DataMember]
        public Action ASToolAction { get; set; }
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string InputUri { get; set; }
        [DataMember]
        public string OutputUri { get; set; }
        [DataMember]
        public int MinBitrate { get; set; }
        [DataMember]
        public int MaxBitrate { get; set; }
        [DataMember]
        public ulong MaxDuration { get; set; }
        [DataMember]
        public string AudioTrackName { get; set; }
        [DataMember]
        public string TextTrackName { get; set; }
        [DataMember]
        public int Loop { get; set; }
        [DataMember]
        public int BufferSize { get; set; }
        [DataMember]
        public int LiveOffset { get; set; }
        [DataMember]
        public int CounterPeriod { get; set; }
        [DataMember]
        public LogLevel ConsoleLevel { get; set; }
        [DataMember]
        public LogLevel TraceLevel { get; set; }
        [DataMember]
        public string TraceFile { get; set; }
        [DataMember]
        public int TraceSize { get; set; }
        [DataMember]
        public string ConfigFile { get; set; }

        public TheadStatus Status { get; set; }
        public DateTime ThreadStartTime { get; set; }

        public Dictionary<string, CounterDescription> ListCounters { get; set; }
        public string GetCountersInformation()
        {
            string result = string.Empty;

            if((ListCounters!=null)&& (ListCounters.Count>0))
            {
                foreach(var c in ListCounters)
                {
                    result += c.Value.Name + ": " + c.Value.value.ToString() + " " + c.Value.UnitString + "\r\n"; 
                }
            }
            return result;
        }
        public bool SetCounter(string key, string name, object value, string unit, string description )
        {
            bool result = false;
            if (ListCounters == null)
                ListCounters = new Dictionary<string, CounterDescription>();

            if(ListCounters!=null)
            {
                try
                {
                    if (ListCounters.ContainsKey(key))
                    {
                        ListCounters[key].Name = name;
                        ListCounters[key].value = value;
                        ListCounters[key].UnitString = unit;
                        ListCounters[key].Description = description;
                    }
                    else
                    {
                        CounterDescription c = new CounterDescription();
                        if (c != null)
                        {
                            c.Key = key;
                            c.Name = name;
                            c.value = value;
                            c.UnitString = unit;
                            c.Description = description;
                            ListCounters.Add(key, c);
                        }
                    }
                    result = true;
                }
                catch(Exception)
                {
                    result = false;
                }
            }
            return result;
        }
        public bool SetCounter(string key, object value)
        {
            bool result = false;
            if (ListCounters != null)
            {
                try
                {
                    if (ListCounters.ContainsKey(key))
                    {
                        ListCounters[key].value = value;
                        result = true;
                    }
                }
                catch (Exception)
                {
                    result = false;
                }
            }
            return result;
        }

        public string GetErrorMessage()
        {
            return ErrorMessage;
        }
        public string GetInformationMessage(Int32 version)
        {
            return string.Format(InformationMessage, ASVersion.GetVersionString(version));
        }
        public string GetErrorMessagePrefix()
        {
            return ErrorMessagePrefix;
        }
        private string ErrorMessagePrefix = "ASTool Error: \r\n";
        private string InformationMessage = "ASTool:\r\n" + "Version: {0} \r\n"  + "Syntax:\r\n"+
            "ASTool --pullpush --input <inputLiveUri>      --output <outputLiveUri>  \r\n" +
            "                 [--minbitrate <bitrate b/s>  --maxbitrate <bitrate b/s> --maxduration <duration ms>]\r\n" +
            "                 [--audiotrackname <name>  --texttrackname <name>]\r\n" +
            "                 [--liveoffset <value in seconds>]\r\n" +
            "                 [--name <service name>]\r\n" +
            "                 [--tracefile <path> --tracesize <size in bytes> --tracelevel <none|error|warning|debug>]\r\n" +
            "                 [--consolelevel <none|error|warning|verbose>]\r\n" +
            "ASTool --pull     --input <inputVODUri>       --output <outputLocalDirectory> \r\n" + 
            "                 [--minbitrate <bitrate b/s>  --maxbitrate <bitrate b/s> --maxduration <duration ms>]\r\n" +
            "                 [--audiotrackname <name>  --texttrackname <name>\r\n" +
            "                 [--liveoffset <value in seconds>]\r\n" +
            "                 [--name <service name>]\r\n" +
            "                 [--tracefile <path> --tracesize <size in bytes> --tracelevel <none|error|warning|debug>]\r\n" +
            "                 [--consolelevel <none|error|warning|verbose>]\r\n" +
            "ASTool --push     --input <inputLocalISMFile> --output <outputLiveUri> \r\n" + 
            "                 [--minbitrate <bitrate b/s>  --maxbitrate <bitrate b/s> --loop <loopCounter>]\r\n" +
            "                 [--name <service name>]\r\n" +
            "                 [--tracefile <path> --tracesize <size in bytes> --tracelevel <none|error|warning|debug>]\r\n" +
            "                 [--consolelevel <none|error|warning|verbose>]\r\n" +
            "ASTool --parse    --input <inputLocalISMV|inputLocalISMA>  \r\n" +
            "ASTool --import   --configfile <configFile> \r\n" +
            "ASTool --export   --configfile <configFile> \r\n" +
            "ASTool --help";
        private string ErrorMessage = string.Empty;


        void LogMessage(LogLevel level, string Message)
        {
            string Text = string.Empty;
            if (level <= TraceLevel)
            {
                Text = string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " " + Message + "\r\n";
                LogTrace(this.TraceFile, this.TraceSize, Text);
            }
            if (level <= ConsoleLevel)
            {
                if(string.IsNullOrEmpty(Text))
                    Text = string.Format("{0:d/M/yyyy HH:mm:ss.fff}", DateTime.Now) + " " + Message + "\r\n";
                Console.Write(Text);
            }
        }
        public void LogVerbose(string Message)
        {
            LogMessage(LogLevel.Verbose, Message);
        }
        public void LogInformation(string Message)
        {
            LogMessage(LogLevel.Information, Message);
        }
        public void LogWarning(string Message)
        {
            LogMessage(LogLevel.Warning, Message);
        }
        public void LogError(string Message)
        {
            LogMessage(LogLevel.Error, Message);
        }
        static public char GetChar(byte b)
        {
            if ((b >= 32) && (b < 127))
                return (char)b;
            return '.';
        }
        static public string DumpHex(byte[] data)
        {
            string result = string.Empty;
            string resultHex = " ";
            string resultASCII = " ";
            int Len = ((data.Length % 16 == 0) ? (data.Length / 16) : (data.Length / 16) + 1) * 16;
            for (int i = 0; i < Len; i++)
            {
                if (i < data.Length)
                {
                    resultASCII += string.Format("{0}", GetChar(data[i]));
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
            return result;
        }
        public ulong LogTrace(string fullPath, long Tracefile, string Message)
        {
            ulong retVal = 0;

            try
            {
                lock (this)
                {
                    FileStream fs = new FileStream(fullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    if (fs != null)
                    {
                        long pos = fs.Seek(0, SeekOrigin.End);
                        byte[] data = UTF8Encoding.UTF8.GetBytes(Message);
                        if (data != null)
                        {
                            if (pos + data.Length > Tracefile)
                                fs.SetLength(0);
                            fs.Write(data, 0, data.Length);
                            retVal = (ulong)data.Length;
                        }
                        fs.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception while append in file:" + fullPath + " Exception: " + ex.Message);
            }
            return retVal;
        }
        static public LogLevel GetLogLevel(string text)
        {
            LogLevel level = LogLevel.None;
            switch(text.ToLower())
            {
                case "none":
                    level = LogLevel.None;
                    break;
                case "information":
                    level = LogLevel.Information;
                    break;
                case "error":
                    level = LogLevel.Error;
                    break;
                case "warning":
                    level = LogLevel.Warning;
                    break;
                case "verbose":
                    level = LogLevel.Verbose;
                    break;
                default:
                    break;
            }
            return level;
        }
        public static  object ReadObjectByType(string filepath, Type type)
        {
            object retVal = null;
            try
            {
                FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read);
                if (fs != null)
                {
                    System.Runtime.Serialization.DataContractSerializer ser = new System.Runtime.Serialization.DataContractSerializer(type);
                    retVal = ser.ReadObject(fs);
                    fs.Close();                        
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception while reading config file: " + ex.Message);

            }
            return retVal;
        }
        public static bool WriteObjectByType(string filepath, Type type, object obj)
        {
            bool retVal = false;
            try
            {
                FileStream fs = new FileStream(filepath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                if (fs != null)
                {
                    System.Runtime.Serialization.DataContractSerializer ser = new System.Runtime.Serialization.DataContractSerializer(type);
                    ser.WriteObject(fs,obj);
                    fs.Close();
                    retVal = true;
                }
                    

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception while writing config file: " + ex.Message);

            }
            return retVal;
        }
        public static List<Options> ReadConfigFile(string ConfigFile)
        {
            List<Options> list = ReadObjectByType(ConfigFile, typeof(List<Options>)) as List<Options>;
            return list;
        }
        public static bool WriteConfigFile(string ConfigFile, List<Options> obj)
        {
            return WriteObjectByType(ConfigFile, typeof(List<Options>), obj);
        }
        public Options()
        {
            this.Loop = 0;
            this.Name = string.Empty;
            this.ConfigFile = string.Empty;
            this.TraceFile = string.Empty;
            this.TraceSize = 524280;
            this.TraceLevel = LogLevel.Information;
            this.ConsoleLevel = LogLevel.Information;
            this.MaxBitrate = 0;
            this.MinBitrate = 0;
            this.MaxDuration = 0;
            this.InputUri = string.Empty;
            this.OutputUri = string.Empty;
            this.AudioTrackName = string.Empty;
            this.TextTrackName = string.Empty;
            this.BufferSize = 0;
            this.LiveOffset = 10;
            this.ASToolAction = Action.None;
            this.CounterPeriod = 20;
            this.ListCounters = new Dictionary<string, CounterDescription>();
        }
        public static Options CheckOptions(Options options)
        {
            if (options.ASToolAction == Action.Help)
            {
                return options;
            }
            else if (options.ASToolAction == Action.Pull)
            {
                if ((!string.IsNullOrEmpty(options.InputUri)) &&
                    (!string.IsNullOrEmpty(options.OutputUri))
                    )
                {
                    try
                    {
                        Uri uri = new Uri(options.InputUri);
                    }
                    catch (Exception)
                    {
                        options.ErrorMessage = "Bad format Input Uri:" + options.InputUri;
                    }
                    bool bDirExists = false;
                    try
                    {
                        bDirExists = System.IO.Directory.Exists(options.OutputUri);
                    }
                    catch (Exception)
                    {
                        bDirExists = false;
                    }
                    if (bDirExists == false)
                        options.ErrorMessage = "Output directory doesn't exist:" + options.OutputUri;
                    return options;
                }
                else
                    options.ErrorMessage = "Missing paramters for Pull feature";
            }
            else if (options.ASToolAction == Action.PullPush)
            {
                if ((!string.IsNullOrEmpty(options.InputUri)) &&
                    (!string.IsNullOrEmpty(options.OutputUri))
                    )
                {
                    try
                    {
                        Uri uri = new Uri(options.InputUri);
                    }
                    catch (Exception)
                    {
                        options.ErrorMessage = "Bad format Input Uri:" + options.InputUri;
                    }
                    try
                    {
                        Uri uri = new Uri(options.OutputUri);
                    }
                    catch (Exception)
                    {
                        options.ErrorMessage = "Bad format Output Uri:" + options.OutputUri;
                    }
                    return options;
                }
                else
                    options.ErrorMessage = "Missing paramters for PullPush feature";
            }
            else if (options.ASToolAction == Action.Push)
            {
                if ((!string.IsNullOrEmpty(options.InputUri)) &&
                    (!string.IsNullOrEmpty(options.OutputUri))
                    )
                {

                    bool bFileExists = false;
                    try
                    {
                        bFileExists = System.IO.File.Exists(options.InputUri);
                    }
                    catch (Exception)
                    {
                        bFileExists = false;
                    }
                    if (bFileExists == false)
                        options.ErrorMessage = "Input ISM file doesn't exist:" + options.InputUri;
                    try
                    {
                        Uri uri = new Uri(options.OutputUri);
                    }
                    catch (Exception)
                    {
                        options.ErrorMessage = "Bad format Output Uri:" + options.OutputUri;
                    }
                    return options;
                }
                else
                    options.ErrorMessage = "Missing paramters for Push feature";
            }
            else if (options.ASToolAction == Action.Parse)
            {
                if (!string.IsNullOrEmpty(options.InputUri))
                {
                    return options;
                }
                else
                {
                    options.ErrorMessage = "Missing parameters for Parse feature";
                    return options;
                }
            }
            else if (options.ASToolAction == Action.Import)
            {
                if (!string.IsNullOrEmpty(options.ConfigFile))
                {
                    return options;
                }
                else
                {
                    options.ErrorMessage = "Missing parameters for Import feature";
                    return options;
                }
            }
            else if (options.ASToolAction == Action.Export)
            {
                if (!string.IsNullOrEmpty(options.ConfigFile))
                {
                    return options;
                }
                else
                {
                    options.ErrorMessage = "Missing parameters for Export feature";
                    return options;
                }
            }
            return null;
        }
        public static Options InitializeOptions(string[] args)
        {
            List<Options> list = new List<Options>();
            Options options = new Options();

            if ((options == null)||(list == null))
            {
                return null;
            }
            try
            {
                
                options.TraceFile = "ASTOOL.log";
                if (args!=null)
                {

                    int i = 0;
                    if(args.Length == 0)
                    {
                        options.ErrorMessage = "No parameter in the command line" ;
                        return options;
                    }
                    while ((i < args.Length)&&(string.IsNullOrEmpty(options.ErrorMessage)))
                    {
                        switch(args[i++])
                        {

                            case "--help":
                                options.ASToolAction = Action.Help;
                                break;
                            case "--pullpush":
                                options.LiveOffset = 10;
                                options.BufferSize = 0;
                                options.ASToolAction = Action.PullPush;
                                break;
                            case "--pull":
                                options.LiveOffset = 0;
                                options.BufferSize = 65535;
                                options.ASToolAction = Action.Pull;
                                break;
                            case "--push":
                                options.ASToolAction = Action.Push;
                                break;
                            case "--parse":
                                options.ASToolAction = Action.Parse;
                                break;
                            case "--import":
                                options.ASToolAction = Action.Import;
                                break;

                            case "--export":
                                options.ASToolAction = Action.Export;
                                break;
                            case "--output":
                                if ((i < args.Length) && (!string.IsNullOrEmpty(args[i])))
                                    options.OutputUri = args[i++];
                                else
                                    options.ErrorMessage = "Output URI not set";
                                break;
                            case "--input":
                                if ((i < args.Length) && (!string.IsNullOrEmpty(args[i])))
                                    options.InputUri = args[i++];
                                else
                                    options.ErrorMessage = "Input URI not set";
                                break;
                            case "--name":
                                if ((i < args.Length) && (!string.IsNullOrEmpty(args[i])))
                                    options.Name = args[i++];
                                else
                                    options.ErrorMessage = "Name not set";
                                break;

                            case "--configfile":
                                if ((i < args.Length) &&
                                    (!string.IsNullOrEmpty(args[i])))
                                    options.ConfigFile = args[i++];
                                else
                                    options.ErrorMessage = "ConfigFile not set";
                                break;
                            case "--loop":
                                if ((i < args.Length) && (!string.IsNullOrEmpty(args[i])))
                                {
                                    int loop = 0;
                                    if (int.TryParse(args[i++], out loop))
                                        options.Loop = loop;
                                    else
                                        options.ErrorMessage = "Loop value incorrect";
                                }
                                else
                                    options.ErrorMessage = "Loop not set";
                                break;
                            case "--maxduration":
                                if ((i < args.Length) && (!string.IsNullOrEmpty(args[i])))
                                {
                                    ulong duration = 0;
                                    if (ulong.TryParse(args[i++], out duration))
                                        options.MaxDuration = duration;
                                    else
                                        options.ErrorMessage = "Duration value incorrect";
                                }
                                else
                                    options.ErrorMessage = "Duration not set";
                                break;
                            case "--minbitrate":
                                if ((i < args.Length) && (!string.IsNullOrEmpty(args[i])))
                                {
                                    int bitrate = 0;
                                    if (int.TryParse(args[i++], out bitrate))
                                        options.MinBitrate = bitrate;
                                    else
                                        options.ErrorMessage = "MinBitrate value incorrect";
                                }
                                else
                                    options.ErrorMessage = "MinBitrate not set";
                                break;
                            case "--maxbitrate":
                                if ((i < args.Length) && (!string.IsNullOrEmpty(args[i])))
                                {
                                    int bitrate = 0;
                                    if (int.TryParse(args[i++], out bitrate))
                                        options.MaxBitrate = bitrate;
                                    else
                                        options.ErrorMessage = "MaxBitrate value incorrect";
                                }
                                else
                                    options.ErrorMessage = "MaxBitrate not set";
                                break;
                            case "--liveoffset":
                                if ((i < args.Length) && (!string.IsNullOrEmpty(args[i])))
                                {
                                    int offset = 0;
                                    if (int.TryParse(args[i++], out offset))
                                        options.LiveOffset = offset;
                                    else
                                        options.ErrorMessage = "Live Offset value incorrect";
                                }
                                else
                                    options.ErrorMessage = "Live Offset not set";
                                break;
                            case "--audiotrackname":
                                if ((i < args.Length) && (!string.IsNullOrEmpty(args[i])))
                                    options.AudioTrackName = args[i++];
                                else
                                    options.ErrorMessage = "AudioTrackName not set";
                                break;
                            case "--texttrackname":
                                if ((i < args.Length) && (!string.IsNullOrEmpty(args[i])))
                                    options.TextTrackName = args[i++];
                                else
                                    options.ErrorMessage = "TextTrackName not set";
                                break;
                            case "--buffersize":
                                if ((i < args.Length) && (!string.IsNullOrEmpty(args[i])))
                                {
                                    int buffersize= 0;
                                    if (int.TryParse(args[i++], out buffersize))
                                        options.BufferSize = buffersize;
                                    else
                                        options.ErrorMessage = "BufferSize value incorrect";
                                }
                                else
                                    options.ErrorMessage = "BufferSize not set";
                                break;

                            case "--tracefile":
                                if ((i < args.Length) && (!string.IsNullOrEmpty(args[i])))
                                    options.TraceFile = args[i++];
                                else
                                    options.ErrorMessage = "TraceFile not set";
                                break;
                            case "--tracesize":
                                if ((i < args.Length) && (!string.IsNullOrEmpty(args[i])))
                                {
                                    int tracesize = 0;
                                    if (int.TryParse(args[i++], out tracesize))
                                        options.TraceSize = tracesize;
                                    else
                                        options.ErrorMessage = "TraceSize value incorrect";
                                }
                                else
                                    options.ErrorMessage = "TraceSize not set";
                                break;
                            case "--tracelevel":
                                if ((i < args.Length) && (!string.IsNullOrEmpty(args[i])))
                                    options.TraceLevel = GetLogLevel(args[i++]);
                                else
                                    options.ErrorMessage = "TraceLevel not set";
                                break;
                            case "--consolelevel":
                                if ((i < args.Length) && (!string.IsNullOrEmpty(args[i])))
                                    options.ConsoleLevel = GetLogLevel(args[i++]);
                                else
                                    options.ErrorMessage = "ConsoleLevel not set";
                                break;

                            default:
                                if ((args[i - 1].ToLower() == "dotnet") ||
                                    (args[i - 1].ToLower() == "astool.dll") ||
                                    (args[i - 1].ToLower() == "astool.exe"))
                                    break;
                                options.ErrorMessage = "wrong parameter: " + args[i-1];
                                return options;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                options.ErrorMessage = "Exception while analyzing the options: " + ex.Message;
                return options;
            }

            if (!string.IsNullOrEmpty(options.ErrorMessage))
            {
                return options;
            }
            return CheckOptions(options);

        }


    }
}
