using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace AdaptativeStreamingService
{
    /// <summary>
    /// Une instance de cette classe est créée pour chaque instance de service par le runtime Service Fabric.
    /// </summary>
    internal sealed class AdaptativeStreamingService : StatelessService
    {
        public AdaptativeStreamingService(StatelessServiceContext context)
            : base(context)
        { }

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
            // TODO: remplacez l'exemple de code suivant par votre propre logique 
            //       ou supprimez cette substitution de RunAsync, si elle n'est pas nécessaire dans votre service.

            long iterations = 0;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ServiceEventSource.Current.ServiceMessage(this.Context, "Working-{0}", ++iterations);

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }

        
    }
}
