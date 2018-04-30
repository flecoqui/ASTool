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
            Help = 0,
            ListPlugin,
            Route
        }
        public string ErrorMessagePrefix = "ASTool Error: \r\n";
        public string InformationMessage = "ASTool:\r\nSyntax:\r\nASTool --route --inputuri <inputUri>  --outputuri <outputuri>  [--minbitrate <bitrate b/s> --maxbitrate <bitrate b/s> --plugindirectory <path>]\r\nASTool --listplugins [--plugindirectory <path>]\r\nASTool --help";
        public string ErrorMessage = string.Empty;
        public string InputUri { get; set; }

        public string InputPluginName { get; set; }
        public string OutputUri { get; set; }

        public string OutputPluginName { get; set; }

        public int MinBitrate { get; set; }
        public int MaxBitrate { get; set; }

        public int ByteBufferSize { get; set; }

        public string PluginDirectory { get; set; }

        public Action ASToolAction { get; set; }

        public static Options InitializeOptions(string[] args)
        { 
            Options options = new Options();
            if (options == null)
            {
                return null;
            }
            try
            {
                options.ByteBufferSize = 65536;
                if(args!=null)
                {
                    int i = 0;
                    while((i < args.Length)&&(string.IsNullOrEmpty(options.ErrorMessage)))
                    {
                        switch(args[i++])
                        {

                            case "--help":
                                options.ASToolAction = Action.Help;
                                break;
                            case "--listplugins":
                                options.ASToolAction = Action.ListPlugin;
                                break;
                            case "--route":
                                options.ASToolAction = Action.Route;
                                break;
                            case "--inputuri":
                                if ((i < args.Length) && (!string.IsNullOrEmpty(args[i])))
                                    options.InputUri = args[i++];
                                else
                                    options.ErrorMessage = "Input URI not set";
                                break;
                            //case "--inputplugin":
                            //    if ((i < args.Length) && (!string.IsNullOrEmpty(args[i])))
                            //        options.InputPluginName = args[i++];
                            //    else
                            //        options.ErrorMessage = "Input Plugin Name not set";
                            //    break;
                            case "--outputuri":
                                if ((i < args.Length) && (!string.IsNullOrEmpty(args[i])))
                                    options.OutputUri = args[i++];
                                else
                                    options.ErrorMessage = "Output URI not set";
                                break;
                            //case "--outputplugin":
                            //    if ((i < args.Length) && (!string.IsNullOrEmpty(args[i])))
                            //        options.OutputPluginName = args[i++];
                            //    else
                            //        options.ErrorMessage = "Output Plugin Name not set";
                            //    break;
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
                                    options.ErrorMessage = "MinBitrate Directory not set";
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
                                    options.ErrorMessage = "MaxBitrate Directory not set";
                                break;
                            case "--bytebuffersize":
                                if ((i < args.Length) && (!string.IsNullOrEmpty(args[i])))
                                {
                                    int buffersize= 0;
                                    if (int.TryParse(args[i++], out buffersize))
                                        options.ByteBufferSize = buffersize;
                                    else
                                        options.ErrorMessage = "ByteBufferSize value incorrect";
                                }
                                else
                                    options.ErrorMessage = "ByteBufferSize not set";
                                break;
                            case "--plugindirectory":
                                if ((i < args.Length) && (!string.IsNullOrEmpty(args[i])))
                                    options.PluginDirectory = args[i++];
                                else
                                    options.ErrorMessage = "Plugin Directory not set";
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                options.ErrorMessage = "Exception while analyzing the options: " + ex.Message;
                return null;
            }

            if(!string.IsNullOrEmpty(options.ErrorMessage))
            {
                return options;
            }

            if (options.ASToolAction == Action.Help)
                return options;
            else if (options.ASToolAction == Action.ListPlugin)
                return options;
            else if (options.ASToolAction == Action.Route)
            {
                if ((!string.IsNullOrEmpty(options.InputUri)) &&
                    //(!string.IsNullOrEmpty(options.InputPluginName)) &&
                    (!string.IsNullOrEmpty(options.InputUri)) 
                    //&&
                    //(!string.IsNullOrEmpty(options.InputPluginName))
                    )
                {
                    return options;
                }
                else
                    options.ErrorMessage = "Missing paramters for Route feature";
            }
            return null;
        }


    }
}
