using System;
using System.Collections.Generic;
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
            "                 [--minbitrate <bitrate b/s>  --maxbitrate <bitrate b/s> --duration <duration ms>]\r\n" +
            "ASTool --pull     --input <inputVODUri>       --output <outputLocalDirectory> \r\n" + 
            "                 [--minbitrate <bitrate b/s>  --maxbitrate <bitrate b/s> --duration <duration ms>]\r\n" +
            "ASTool --push     --input <inputLocalISMFile> --output <outputLiveUri> \r\n" + 
            "                 [--minbitrate <bitrate b/s>  --maxbitrate <bitrate b/s> --loop <loopCounter>]\r\n" +
            "ASTool --parse    --input <inputLocalISMFile|inputLocalISMCFile|inputLocalISMV|inputLocalISMA>  [--recursive]\r\n" +
            "ASTool --help";
        private string ErrorMessage = string.Empty;
        public string InputUri { get; set; }        
        public string OutputUri { get; set; }
        public int MinBitrate { get; set; }
        public int MaxBitrate { get; set; }
        public ulong Duration { get; set; }
        public bool Recursive { get; set; }
        public int Loop { get; set; }
        public int BufferSize { get; set; }
        public Action ASToolAction { get; set; }
        public Int32 version { get; set; }
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
                options.Recursive = false;
                options.MaxBitrate = 0;
                options.MinBitrate = 0;
                options.Duration = 0;
                options.BufferSize = 65536;
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
                                options.ASToolAction = Action.PullPush;
                                break;
                            case "--pull":
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
                            case "--duration":
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
