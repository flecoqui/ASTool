using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ASTool
{
    /// <summary>
    /// Options that control the behavior of the program
    /// </summary>
    public class Options
    {
        public enum Action {
            None = 0,
            Help,
            Pull,
            Push,
            PullPush,
            Parse
        }
        public enum LogLevel
        {
            None = 0,
            Information,
            Error,
            Warning,
            Verbose
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
            "                 [--tracefile <path> --tracesize <size in bytes> --tracelevel <none|error|warning|debug>]\r\n" +
            "                 [--consolelevel <none|error|warning|verbose>]\r\n" +
            "ASTool --pull     --input <inputVODUri>       --output <outputLocalDirectory> \r\n" + 
            "                 [--minbitrate <bitrate b/s>  --maxbitrate <bitrate b/s> --maxduration <duration ms>]\r\n" +
            "                 [--audiotrackname <name>  --texttrackname <name>\r\n" +
            "                 [--liveoffset <value in seconds>]\r\n" +
            "                 [--tracefile <path> --tracesize <size in bytes> --tracelevel <none|error|warning|debug>]\r\n" +
            "                 [--consolelevel <none|error|warning|verbose>]\r\n" +
            "ASTool --push     --input <inputLocalISMFile> --output <outputLiveUri> \r\n" + 
            "                 [--minbitrate <bitrate b/s>  --maxbitrate <bitrate b/s> --loop <loopCounter>]\r\n" +
            "                 [--tracefile <path> --tracesize <size in bytes> --tracelevel <none|error|warning|debug>]\r\n" +
            "                 [--consolelevel <none|error|warning|verbose>]\r\n" +
            "ASTool --parse    --input <inputLocalISMFile|inputLocalISMCFile|inputLocalISMV|inputLocalISMA>  [--recursive]\r\n" +
            "ASTool --help";
        private string ErrorMessage = string.Empty;
        public string InputUri { get; set; }        
        public string OutputUri { get; set; }
        public int MinBitrate { get; set; }
        public int MaxBitrate { get; set; }
        public ulong Duration { get; set; }
        public string AudioTrackName { get; set; }
        public string TextTrackName { get; set; }
        public bool Recursive { get; set; }
        public int Loop { get; set; }
        public int BufferSize { get; set; }
        public int LiveOffset { get; set; }
        public Action ASToolAction { get; set; }
        public Int32 version { get; set; }

        public LogLevel TraceLevel { get; set; }

        public string TraceFile { get; set; }

        public int TraceSize { get; set; }

        public LogLevel ConsoleLevel { get; set; }
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception while append in file:" + fullPath + " Exception: " + ex.Message);
            }
            return retVal;
        }
        static public LogLevel GetLogLevel(string text)
        {
            LogLevel level = LogLevel.None;
            switch(text)
            {
                case "none":
                    level = LogLevel.None;
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

        public static Options InitializeOptions(string[] args)
        { 
            Options options = new Options();
            if (options == null)
            {
                return null;
            }
            try
            {
                options.Loop = 0;
                options.TraceFile = "ASTOOL.log";
                options.TraceSize = 524280;
                options.TraceLevel = LogLevel.Information;
                options.ConsoleLevel = LogLevel.Information;
                options.Recursive = false;
                options.MaxBitrate = 0;
                options.MinBitrate = 0;
                options.Duration = 0;
                options.AudioTrackName = string.Empty;
                options.TextTrackName = string.Empty;
                options.BufferSize = 0;
                options.LiveOffset = 0;
                options.ASToolAction = Action.None;
                if (args!=null)
                {
                    int i = 0;
                    while((i < args.Length)&&(string.IsNullOrEmpty(options.ErrorMessage)))
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
                            case "--input":
                                if ((i < args.Length) && (!string.IsNullOrEmpty(args[i])))
                                    options.InputUri = args[i++];
                                else
                                    options.ErrorMessage = "Input URI not set";
                                break;
                            case "--output":
                                if ((i < args.Length) && (!string.IsNullOrEmpty(args[i])))
                                    options.OutputUri = args[i++];
                                else
                                    options.ErrorMessage = "Output URI not set";
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
                                        options.Duration = duration;
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
                            case "--recursive":
                                options.Recursive = true;
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
                                if(options.ASToolAction != Action.None)
                                {
                                    options.ErrorMessage = "wrong parameter: " + args[i-1];
                                }
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                options.ErrorMessage = "Exception while analyzing the options: " + ex.Message;
                return options;
            }

            if(!string.IsNullOrEmpty(options.ErrorMessage))
            {
                return options;
            }

            if (options.ASToolAction == Action.Help)
                return options;
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
                    catch(Exception)
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
                    if(bDirExists==false)
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
                        options.ErrorMessage = "Input ISM file doesn't exist:" + options.OutputUri;
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
                    options.ErrorMessage = "Missing paramters for Parse feature";
            }
            if (!string.IsNullOrEmpty(options.ErrorMessage))
            {
                return options;
            }
            return null;
        }


    }
}
