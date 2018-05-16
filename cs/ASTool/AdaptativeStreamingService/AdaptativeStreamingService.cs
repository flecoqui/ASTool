using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ASTool;
using ASTool_cli;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace AdaptativeStreamingService
{
    /// <summary>
    /// Une instance de cette classe est créée pour chaque instance de service par le runtime Service Fabric.
    /// </summary>
    internal sealed class AdaptativeStreamingService : StatelessService
    {
        readonly byte[] initializationData;
        public AdaptativeStreamingService(StatelessServiceContext context)
            : base(context)
        {
            this.initializationData = context.InitializationData;
        }

        
        /// <summary>
        /// Substitution facultative pour créer des écouteurs (par exemple, TCP, HTTP) pour ce réplica de service afin de gérer les requêtes des clients ou des utilisateurs.
        /// </summary>
        /// <returns>Collection d'écouteurs.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[0];
        }

        /// <summary>
        /// Point d'entrée principal de votre instance de service.
        /// </summary>
        /// <param name="cancellationToken">Annulé quand Service Fabric doit arrêter cette instance de service.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // HACK : reusing Main from CLI for first tests

            // retrieving parameters from InitializationData (idem as command line but base64 encoded)
            string cliArg = System.Text.Encoding.UTF8.GetString(this.initializationData);

#if DEBUG
            if (string.IsNullOrEmpty(cliArg))
            {
            }
#endif

            var args = cliArg.Split(' ');

            Thread astoolEngineThread = new Thread(new ParameterizedThreadStart(AstoolCli_Main));
            astoolEngineThread.Start(args);

            try
            {
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
                }
            }
            catch (OperationCanceledException oce)
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, "Cancel async received !!");
                astoolEngineThread.Abort();
            }
            //long iterations = 0;
            //while (true)
            //{
            //    cancellationToken.ThrowIfCancellationRequested();

            //    ServiceEventSource.Current.ServiceMessage(this.Context, "Working-{0}", ++iterations);

            //    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            //}
        }


        void  AstoolCli_Main(object genericArgs)
        {
            string[] args = (string[]) genericArgs;
            // HACK : reusings Main from CLI for first tests

            // TODO remove pure CLI code (help, ...) and implement statis code return.

            int Version = ASVersion.SetVersion(0x01, 0x00, 0x00, 0x00);

            Options opt = Options.InitializeOptions(args);
            if (opt == null)
            {
                Options o = new Options();
                o.LogInformation("ASTool: Internal Error");
                return;
            }
            List<Options> list = new List<Options>();
            if (opt.ASToolAction == Options.Action.Import)
            {
                opt.LogInformation("Importing configuration file in " + opt.ConfigFile);
                var result = Options.ReadConfigFile(opt.ConfigFile);
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
                    opt.LogError("Error while reading ConfigFile " + opt.ConfigFile);
                    return;
                }
                if (list.Count > 0)
                {
                    opt.LogInformation("Importing configuration file done: " + list.Count.ToString() + " features imported");
                }
                else
                {
                    opt.LogError("Importing configuration file done: 0 features imported");
                    return;
                }
            }
            else if (opt.ASToolAction == Options.Action.Export)
            {
                Options optPush = new Options();
                Options optPullDVR = new Options();
                Options optPullVOD = new Options();
                Options optPullPush = new Options();

                optPush.ASToolAction = Options.Action.Push;
                optPush.Name = "Service1";
                optPush.InputUri = "C:\\folder\\asset\\asset.ism";
                optPush.OutputUri = "http://myserver/live/live1.isml";
                optPush.Loop = 0;

                optPullDVR.ASToolAction = Options.Action.Pull;
                optPullDVR.Name = "Service2";
                optPullDVR.InputUri = "http://3rdpartyserver/live/live1.isml/manifest";
                optPullDVR.OutputUri = "C:\\folder\\asset\\assetdvr";
                optPullDVR.MaxBitrate = 1000000;
                optPullDVR.MinBitrate = 100000;
                optPullDVR.LiveOffset = 10;
                optPullDVR.MaxDuration = 3600000;

                optPullVOD.ASToolAction = Options.Action.Pull;
                optPullVOD.Name = "Service3";
                optPullVOD.InputUri = "http://3rdpartyserver/vod/asset/asset.ism/manifest";
                optPullVOD.OutputUri = "C:\\folder\\asset\\assetdvr";
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
                opt.LogInformation("Exporting a sample configuration file in " + opt.ConfigFile);
                Options.WriteConfigFile(opt.ConfigFile, list);
                opt.LogInformation("Exporting a sample configuration file done");
                return;
            }
            else
                list.Add(opt);


            if (list == null)
            {
                opt.LogError("ASTool: Internal Error");
                return;
            }

            foreach (Options option in list)
            {
                if (!string.IsNullOrEmpty(option.GetErrorMessage()))
                {
                    option.LogInformation(opt.GetErrorMessagePrefix() + option.GetErrorMessage());
                    option.LogInformation(opt.GetInformationMessage(Version));
                    return;
                }
                if (option.ASToolAction == Options.Action.Help)
                {
                    option.LogInformation(option.GetInformationMessage(Version));
                    return;
                }
                else if (option.ASToolAction == Options.Action.PullPush)
                {
                    OptionsLauncher.LaunchThread(ASTool.Core.ASToolEngine.PullPush, option);
                }
                if (option.ASToolAction == Options.Action.Pull)
                {
                    OptionsLauncher.LaunchThread(ASTool.Core.ASToolEngine.Pull, option);
                }
                if (option.ASToolAction == Options.Action.Push)
                {
                    OptionsLauncher.LaunchThread(ASTool.Core.ASToolEngine.Push, option);
                }
                if (option.ASToolAction == Options.Action.Parse)
                {
                    OptionsLauncher.LaunchThread(ASTool.Core.ASToolEngine.Parse, option);
                }
            }
            bool bCompleted = false;
            while (bCompleted == false)
            {
                int StoppedThreadCounter = 0;
                foreach (Options option in list)
                {
                    if (option.Status == Options.TheadStatus.Stopped)
                    {
                        StoppedThreadCounter++;
                    }
                    else
                    {
                        if ((DateTime.Now - option.ThreadCounterTime).TotalSeconds > option.CounterPeriod)
                        {
                            option.ThreadCounterTime = DateTime.Now;
                            option.LogInformation("\r\nCounters for feature : " + option.ASToolAction.ToString() + " " + option.Name + "\r\n" + option.GetCountersInformation());
                        }
                    }
                }
                if (StoppedThreadCounter == list.Count)
                    bCompleted = true;
            }

        }


    }
}
