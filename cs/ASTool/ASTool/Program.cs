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
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using ASTool;
using ASTool.CacheHelper;
namespace ASTool
{
    /*
    public class OptionsLauncher
    {
        public delegate bool OptionsThread(Options opt);
        public Options Option;
        public OptionsThread Proc;

        public OptionsLauncher (OptionsThread t, Options opt)
        {
            Proc = t;
            Option = opt;
        }
        public void ThreadEntry()
        {
        if((Proc!=null)&&(Option!=null))
            {
                Proc(Option);
            }
        }
        public static bool LaunchThread(OptionsThread ot, Options o)
        {
            bool result = false;
            OptionsLauncher ol = new OptionsLauncher(ot,o);
            System.Threading.ThreadStart threadStart = new System.Threading.ThreadStart(ol.ThreadEntry);

            System.Threading.Thread t = new System.Threading.Thread(new System.Threading.ThreadStart(ol.ThreadEntry));
            if(t!=null)
            {
                t.Start();
                result = true;
            }

            return result;

        }

    }
    */
    public partial class Program
    {
        static Int32 Version = ASVersion.SetVersion(0x01, 0x00, 0x00, 0x00);
        static Options ParseCommandLine(string[] args)
        {
            Options opt = Options.InitializeOptions(args);
            if (opt == null)
            {
                Options o = new Options();
                o.LogInformation("ASTool: Internal Error");
                return null;
            }
            return opt;
        }
        static List<Options> LaunchServices(Options inputOption , out bool bContinued)
        {
            List<Options> list = null;
            bContinued = false;

            int ThreadLaunched = 0;
            list = new List<Options>();
            if (list != null)
            {
                if (inputOption.ASToolAction == Options.Action.Import)
                {
                    inputOption.LogInformation("Importing configuration file in " + inputOption.ConfigFile);
                    var result = Options.ReadConfigFile(inputOption.ConfigFile);
                    if (result != null)
                    {
                        foreach (var c in result)
                        {
                            if ((c.ASToolAction == Options.Action.Pull) ||
                                (c.ASToolAction == Options.Action.Push) ||
                                (c.ASToolAction == Options.Action.PullPush))
                            {
                                var res = Options.CheckOptions(c);
                                if (res != null)
                                {
                                    string errorMessage = res.GetErrorMessage();
                                    if (string.IsNullOrEmpty(errorMessage))
                                        list.Add(c);
                                    else
                                        c.LogError("Error for feature " + c.ASToolAction.ToString() + ": " + errorMessage);
                                }
                            }
                        }
                    }
                    else
                    {
                        inputOption.LogError("Error while reading ConfigFile " + inputOption.ConfigFile);
                        return null;
                    }
                    if (list.Count > 0)
                    {
                        inputOption.LogInformation("Importing configuration file done: " + list.Count.ToString() + " features imported");
                    }
                    else
                    {
                        inputOption.LogError("Importing configuration file done: 0 features imported");
                        return null;
                    }
                }
                else if (inputOption.ASToolAction == Options.Action.Export)
                {
                    bool IsWindows = System.Runtime.InteropServices.RuntimeInformation
                   .IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);

                    Options optPush = new Options();
                    Options optPullDVR = new Options();
                    Options optPullVOD = new Options();
                    Options optPullPush = new Options();

                    optPush.ASToolAction = Options.Action.Push;
                    optPush.Name = "Service1";
                    if(IsWindows) 
                        optPush.InputUri = "C:\\folder\\asset\\asset.ism";
                    else
                        optPush.InputUri = "/folder/asset/asset.ism";
                    optPush.OutputUri = "http://myserver/live/live1.isml";
                    optPush.Loop = 0;

                    optPullDVR.ASToolAction = Options.Action.Pull;
                    optPullDVR.Name = "Service2";
                    optPullDVR.InputUri = "http://3rdpartyserver/live/live1.isml/manifest";
                    if (IsWindows)
                        optPush.OutputUri = "C:\\folder\\asset\\assetdvr";
                    else
                        optPush.OutputUri = "/folder/asset/assetdvr";
                    optPullDVR.MaxBitrate = 1000000;
                    optPullDVR.MinBitrate = 100000;
                    optPullDVR.LiveOffset = 10;
                    optPullDVR.MaxDuration = 3600000;

                    optPullVOD.ASToolAction = Options.Action.Pull;
                    optPullVOD.Name = "Service3";
                    optPullVOD.InputUri = "http://3rdpartyserver/vod/asset/asset.ism/manifest";
                    if (IsWindows)
                        optPush.OutputUri = "C:\\folder\\asset\\assetdvr";
                    else
                        optPush.OutputUri = "/folder/asset/assetdvr";
                    optPullVOD.MaxBitrate = 1000000;
                    optPullVOD.MinBitrate = 100000;
                    optPullVOD.LiveOffset = 0;

                    optPullPush.ASToolAction = Options.Action.PullPush;
                    optPullPush.Name = "Service4";
                    optPullPush.InputUri = "http://3rdpartyserver/live/live1.isml/manifest";
                    optPullPush.OutputUri = "http://myserver/live/live1.isml";
                    optPullPush.MaxBitrate = 1000000;
                    optPullPush.MinBitrate = 100000;
                    optPullPush.LiveOffset = 10;

                    list.Add(optPush);
                    list.Add(optPullDVR);
                    list.Add(optPullVOD);
                    list.Add(optPullPush);
                    inputOption.LogInformation("Exporting a sample configuration file in " + inputOption.ConfigFile);
                    Options.WriteConfigFile(inputOption.ConfigFile, list);
                    inputOption.LogInformation("Exporting a sample configuration file done");
                    return list;
                }
                else
                    list.Add(inputOption);

                foreach (Options option in list)
                {
                    if (option.ASToolAction == Options.Action.Help)
                    {
                        option.LogInformation(option.GetInformationMessage(Version));
                        return list;
                    }
                    else if (option.ASToolAction == Options.Action.PullPush)
                    {
                        //OptionsLauncher.LaunchThread(PullPush, option);
                        option.Task = System.Threading.Tasks.Task.Run(() => PullPush(option));
                        ThreadLaunched++;
                    }
                    if (option.ASToolAction == Options.Action.Pull)
                    {
                       
                        //OptionsLauncher.LaunchThread(Pull, option);
                        option.Task = System.Threading.Tasks.Task.Run(() => Pull(option));

                        ThreadLaunched++;

                    }
                    if (option.ASToolAction == Options.Action.Push)
                    {
                        //OptionsLauncher.LaunchThread(Push, option);
                        option.Task = System.Threading.Tasks.Task.Run(() => Push(option));
                        ThreadLaunched++;

                    }
                    if (option.ASToolAction == Options.Action.Parse)
                    {
                        //OptionsLauncher.LaunchThread(Parse, option);
                        option.Task = System.Threading.Tasks.Task.Run(() => Parse(option));
                        ThreadLaunched++;

                    }
                    if (option.ASToolAction == Options.Action.Install)
                    {
                        option.LogInformation("Installing ASTOOL Service");
                        if (InstallService(option) == true)
                            option.LogInformation("Installing ASTOOL Service done");
                        return list;
                    }
                    if (option.ASToolAction == Options.Action.Uninstall)
                    {
                        option.LogInformation("Uninstalling ASTOOL Service");
                        if (UninstallService(option) == true)
                            option.LogInformation("Uninstalling ASTOOL Service done");
                        return list;
                    }
                    if (option.ASToolAction == Options.Action.Stop)
                    {
                        option.LogInformation("Stopping ASTOOL Service");
                        if (StopService(option) == true)
                            option.LogInformation("Stopping ASTOOL Service done");
                        return list;
                    }
                    if (option.ASToolAction == Options.Action.Start)
                    {
                        option.LogInformation("Starting ASTOOL Service");
                        if (StartService(option) == true)
                            option.LogInformation("Starting ASTOOL Service done");
                        return list;
                    }
                }
            }
            if (ThreadLaunched > 0)
            {
                bContinued = true;
                return list;
            }
            return null;
        }
        static void WaitEndOfServices(List<Options> list)
        {
            
            bool bCompleted = false;
            while (bCompleted == false)
            {
                System.Threading.Tasks.Task.Delay(1000).Wait();
                int StoppedThreadCounter = 0;
                foreach (Options option in OptionsList)
                {
                    if (option.Status == Options.TheadStatus.Stopped)
                    {
                        StoppedThreadCounter++;
                    }
                    else
                    {
                        if (option.CounterPeriod > 0)
                        {
                            if ((option.ThreadCounterTime != DateTime.MinValue) && ((DateTime.Now - option.ThreadCounterTime).TotalSeconds > option.CounterPeriod))
                            {
                                option.ThreadCounterTime = DateTime.Now;
                                option.LogInformation("\r\nCounters for feature : " + option.ASToolAction.ToString() + " " + option.Name + "\r\n" + option.GetCountersInformation());
                            }
                        }
                    }
                }
                if (StoppedThreadCounter == list.Count)
                    bCompleted = true;
            }
        }
        static bool StopServices(TimeSpan span)
        {
            bool bCompleted = false;
            foreach (Options option in OptionsList)
            {
                if (option.Status == Options.TheadStatus.Running)
                {
                    option.LogInformation("Stopping feature: " + option.ASToolAction.ToString() + " " + option.Name);
                    option.Status = Options.TheadStatus.Stopping;
                }
            }
            DateTime start = DateTime.Now;
            while ((bCompleted == false)&&(DateTime.Now-start<span))
            {
                System.Threading.Thread.Sleep(500);
                int StoppedThreadCounter = 0;
                foreach (Options option in OptionsList)
                {
                    if (option.Status == Options.TheadStatus.Stopped)
                    {
                        StoppedThreadCounter++;
                        option.LogInformation("Feature: " + option.ASToolAction.ToString() + " " + option.Name );
                    }
                    else
                    {
                        option.LogInformation("Feature: " + option.ASToolAction.ToString() + " " + option.Name);
                    }
                    option.LogInformation("Counter: " + StoppedThreadCounter.ToString() + " List.Count: " + OptionsList.Count.ToString());
                }
                if (StoppedThreadCounter >= OptionsList.Count)
                    bCompleted = true;
            }
            return bCompleted;
        }
        //
        // List of options
        // Used in command line mode or in service mode
        //
        static List<Options> OptionsList = null;

        static void Main(string[] args)
        {
            Options opt = Options.InitializeOptions(args);
            if (opt == null)
            {
                Options o = new Options();
                o.LogInformation("ASTool: Internal Error");
                return;
            }
            string errorMessage = opt.GetErrorMessage();
            if(!string.IsNullOrEmpty(errorMessage))
            {
                opt.LogError(errorMessage);
                opt.LogInformation(opt.GetInformationMessage(Version));
                return;
            }
            List<Options> list = null;
            if (opt.ServiceMode == true)
                RunAsService(opt);
            else
            {
                bool bContinue = false;
                list = LaunchServices(opt, out bContinue);
                if((list !=null)&& (bContinue == true))
                {
                    OptionsList = list;
                    // if a service is still running 
                    // wait for the end of this service
                    WaitEndOfServices(list);
                }
            }

        }
    }

}
