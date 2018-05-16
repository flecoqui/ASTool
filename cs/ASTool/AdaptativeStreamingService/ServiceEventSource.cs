using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Fabric;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Runtime;

namespace AdaptativeStreamingService
{
    [EventSource(Name = "MyCompany-ASTool_SF-AdaptativeStreamingService")]
internal sealed class ServiceEventSource : EventSource
{
    public static readonly ServiceEventSource Current = new ServiceEventSource();

    // Le constructeur d'instance est privé pour appliquer la sémantique du singleton
    private ServiceEventSource() : base() { }

    #region Mots clés
    // Les mots clés d'événement peuvent servir à classer les événements. 
    // Chaque mot clé est un indicateur de bit. Un seul événement peut être associé à plusieurs mots clés (via la propriété EventAttribute.Keywords).
    // Les mots clés doivent être définis en tant que classe publique nommée 'Keywords' dans l'EventSource qui les utilise.
    public static class Keywords
    {
        public const EventKeywords Requests = (EventKeywords)0x1L;
        public const EventKeywords ServiceInitialization = (EventKeywords)0x2L;
    }
    #endregion

    #region Événements
    // Définissez une méthode d'instance pour chaque événement à enregistrer, puis appliquez-lui un attribut [Event].
    // Le nom de la méthode est le nom de l'événement.
    // Passez les paramètres à enregistrer avec l'événement (seuls les types entiers primitifs, DateTime, Guid et string sont autorisés).
    // Chaque implémentation de méthode d'événement doit vérifier si la source d'événement est activée. Si tel est le cas, elle doit appeler la méthode WriteEvent() pour déclencher l'événement.
    // Le nombre et les types des arguments passés à chaque méthode d'événement doivent correspondre exactement à ce qui est passé à WriteEvent().
    // Placez l'attribut [NonEvent] sur toutes les méthodes qui ne définissent pas un événement.
    // Pour plus d'informations, consultez https://msdn.microsoft.com/fr-fr/library/system.diagnostics.tracing.eventsource.aspx

    [NonEvent]
    public void Message(string message, params object[] args)
    {
        if (this.IsEnabled())
        {
            string finalMessage = string.Format(message, args);
            Message(finalMessage);
        }
    }

    private const int MessageEventId = 1;
    [Event(MessageEventId, Level = EventLevel.Informational, Message = "{0}")]
    public void Message(string message)
    {
        if (this.IsEnabled())
        {
            WriteEvent(MessageEventId, message);
        }
    }

    [NonEvent]
    public void ServiceMessage(StatelessServiceContext serviceContext, string message, params object[] args)
    {
        if (this.IsEnabled())
        {
            string finalMessage = string.Format(message, args);
            ServiceMessage(
                serviceContext.ServiceName.ToString(),
                serviceContext.ServiceTypeName,
                serviceContext.InstanceId,
                serviceContext.PartitionId,
                serviceContext.CodePackageActivationContext.ApplicationName,
                serviceContext.CodePackageActivationContext.ApplicationTypeName,
                serviceContext.NodeContext.NodeName,
                finalMessage);
        }
    }

    // Pour les événements très fréquents, il peut être avantageux de déclencher les événements à l'aide de l'API WriteEventCore.
    // Il en résulte une gestion des paramètres plus efficace, mais cela nécessite une allocation explicite de la structure EventData et du code unsafe.
    // Pour activer ce chemin de code, définissez le symbole de compilation conditionnelle UNSAFE, puis activez la prise en charge du code unsafe dans les propriétés du projet.
    private const int ServiceMessageEventId = 2;
    [Event(ServiceMessageEventId, Level = EventLevel.Informational, Message = "{7}")]
    private
#if UNSAFE
        unsafe
#endif
        void ServiceMessage(
        string serviceName,
        string serviceTypeName,
        long replicaOrInstanceId,
        Guid partitionId,
        string applicationName,
        string applicationTypeName,
        string nodeName,
        string message)
    {
#if !UNSAFE
        WriteEvent(ServiceMessageEventId, serviceName, serviceTypeName, replicaOrInstanceId, partitionId, applicationName, applicationTypeName, nodeName, message);
#else
            const int numArgs = 8;
            fixed (char* pServiceName = serviceName, pServiceTypeName = serviceTypeName, pApplicationName = applicationName, pApplicationTypeName = applicationTypeName, pNodeName = nodeName, pMessage = message)
            {
                EventData* eventData = stackalloc EventData[numArgs];
                eventData[0] = new EventData { DataPointer = (IntPtr) pServiceName, Size = SizeInBytes(serviceName) };
                eventData[1] = new EventData { DataPointer = (IntPtr) pServiceTypeName, Size = SizeInBytes(serviceTypeName) };
                eventData[2] = new EventData { DataPointer = (IntPtr) (&replicaOrInstanceId), Size = sizeof(long) };
                eventData[3] = new EventData { DataPointer = (IntPtr) (&partitionId), Size = sizeof(Guid) };
                eventData[4] = new EventData { DataPointer = (IntPtr) pApplicationName, Size = SizeInBytes(applicationName) };
                eventData[5] = new EventData { DataPointer = (IntPtr) pApplicationTypeName, Size = SizeInBytes(applicationTypeName) };
                eventData[6] = new EventData { DataPointer = (IntPtr) pNodeName, Size = SizeInBytes(nodeName) };
                eventData[7] = new EventData { DataPointer = (IntPtr) pMessage, Size = SizeInBytes(message) };

                WriteEventCore(ServiceMessageEventId, numArgs, eventData);
            }
#endif
    }

    private const int ServiceTypeRegisteredEventId = 3;
    [Event(ServiceTypeRegisteredEventId, Level = EventLevel.Informational, Message = "Service host process {0} registered service type {1}", Keywords = Keywords.ServiceInitialization)]
    public void ServiceTypeRegistered(int hostProcessId, string serviceType)
    {
        WriteEvent(ServiceTypeRegisteredEventId, hostProcessId, serviceType);
    }

    private const int ServiceHostInitializationFailedEventId = 4;
    [Event(ServiceHostInitializationFailedEventId, Level = EventLevel.Error, Message = "Service host initialization failed", Keywords = Keywords.ServiceInitialization)]
    public void ServiceHostInitializationFailed(string exception)
    {
        WriteEvent(ServiceHostInitializationFailedEventId, exception);
    }

    // Une paire d'événements partageant le même préfixe de nom avec un suffixe "Start"/"Stop" marque implicitement les limites d'une activité de suivi d'événement.
    // Ces activités peuvent être automatiquement sélectionnées à l'aide du débogage et des outils de profilage, qui peuvent calculer leur temps d'exécution, leurs activités enfants,
    // et d'autres statistiques.
    private const int ServiceRequestStartEventId = 5;
    [Event(ServiceRequestStartEventId, Level = EventLevel.Informational, Message = "Service request '{0}' started", Keywords = Keywords.Requests)]
    public void ServiceRequestStart(string requestTypeName)
    {
        WriteEvent(ServiceRequestStartEventId, requestTypeName);
    }

    private const int ServiceRequestStopEventId = 6;
    [Event(ServiceRequestStopEventId, Level = EventLevel.Informational, Message = "Service request '{0}' finished", Keywords = Keywords.Requests)]
    public void ServiceRequestStop(string requestTypeName, string exception = "")
    {
        WriteEvent(ServiceRequestStopEventId, requestTypeName, exception);
    }
    #endregion

    #region Méthodes privées
#if UNSAFE
        private int SizeInBytes(string s)
        {
            if (s == null)
            {
                return 0;
            }
            else
            {
                return (s.Length + 1) * sizeof(char);
            }
        }
#endif
    #endregion
}
}
