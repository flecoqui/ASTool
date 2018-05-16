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

        private static Task mainTask;


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

            AstoolCli_Main(args);
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

                // TODO : replace with CancellationToken handling
            }
        }


        void  AstoolCli_Main(object genericArgs)
        {
            string[] args = (string[]) genericArgs;
            // HACK : reusings Main from CLI for first tests

            int Version = ASVersion.SetVersion(0x01, 0x00, 0x00, 0x00);

            Options opt = Options.InitializeOptions(args);
            if (opt == null)
            {
                Options o = new Options();
                o.LogInformation("ASTool: Internal Error");
                return;
            }
            List<Options> list = new List<Options>();
            list.Add(opt);


            if (list == null)
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, "ASTool: Internal Error : empty parameter list");
                return;
            }

            foreach (Options option in list)
            {
                if (option.ASToolAction == Options.Action.PullPush)
                {
                    mainTask = ASTool.Core.ASToolEngine.PullPush(option);
                    //OptionsLauncher.LaunchThread(ASTool.Core.ASToolEngine.PullPush, option);
                }
            }

            //bool bCompleted = false;
            //while (bCompleted == false)
            //{
            //    int StoppedThreadCounter = 0;
            //    foreach (Options option in list)
            //    {
            //        if (option.Status == Options.TheadStatus.Stopped)
            //        {
            //            StoppedThreadCounter++;
            //        }
            //        else
            //        {
            //            if ((DateTime.Now - option.ThreadCounterTime).TotalSeconds > option.CounterPeriod)
            //            {
            //                option.ThreadCounterTime = DateTime.Now;
            //                option.LogInformation("\r\nCounters for feature : " + option.ASToolAction.ToString() + " " + option.Name + "\r\n" + option.GetCountersInformation());
            //            }
            //        }
            //    }
            //    if (StoppedThreadCounter == list.Count)
            //        bCompleted = true;
            //}

        }


    }
}
