using Akka.Streams.Azure.EventHub;
using Azure.Messaging.EventHubs;
using Azure.Storage.Blobs;

namespace PartitionReaderPocAkka;

internal class ProcessorFactory : IProcessorFactory<EventProcessorClient>
{
    const string ehubNamespaceConnectionString = "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;";
    const string eventHubName = "defaultevents";
    const string blobStorageConnectionString = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;";
    const string blobContainerName = "wiretappocakka";
    const string consumerGroup = "wiretapakka";
    public EventProcessorClient CreateProcessor()
    {
        //check container is present
        var blobClient = new BlobServiceClient(blobStorageConnectionString);
        var containerClient = blobClient.GetBlobContainerClient(blobContainerName);
        if (!containerClient.Exists())
        {
            containerClient.Create();
        }

        var storageClient = new BlobContainerClient(blobStorageConnectionString, blobContainerName);
        var processor = new EventProcessorClient(storageClient, consumerGroup, ehubNamespaceConnectionString, eventHubName);

        return processor;
    }
}
