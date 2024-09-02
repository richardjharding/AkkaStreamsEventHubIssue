// See https://aka.ms/new-console-template for more information
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;
using System.Collections.Concurrent;
using System.Text;

Console.WriteLine("Starting wiretap");


const string ehubNamespaceConnectionString = "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;";
const string eventHubName = "defaultevents";
const string blobStorageConnectionString = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;";
const string blobContainerName = "wiretappoc";
const string consumerGroup = "wiretap";
var totals = new ConcurrentDictionary<string, int>();

async Task ProcessEventHandler(ProcessEventArgs eventArgs)
{
    string data = Encoding.UTF8.GetString(eventArgs.Data.Body.ToArray());
    Console.WriteLine($"Received event: {eventArgs.Partition.PartitionId} :  {data}");

    var totalEvents = totals.AddOrUpdate("total", 1, (key, value) => value + 1);
    Console.WriteLine($"Total events: {totalEvents}");
    // Update checkpoint in the blob storage so that the processor remembers the last read event.
    await eventArgs.UpdateCheckpointAsync(eventArgs.CancellationToken);
}

Task ProcessErrorHandler(ProcessErrorEventArgs eventArgs)
{
    Console.WriteLine($"Error on partition {eventArgs.PartitionId}: {eventArgs.Exception.Message}");
    return Task.CompletedTask;
}

async Task ProcessEvents()
{
    var blobClient = new BlobServiceClient(blobStorageConnectionString);
    var containerClient = blobClient.GetBlobContainerClient(blobContainerName);
    if (!containerClient.Exists())
    {
        containerClient.Create();
    }

    var storageClient = new BlobContainerClient(blobStorageConnectionString, blobContainerName);
    var processor = new EventProcessorClient(storageClient, consumerGroup, ehubNamespaceConnectionString, eventHubName);

    processor.ProcessEventAsync += ProcessEventHandler;
    processor.ProcessErrorAsync += ProcessErrorHandler;

    await processor.StartProcessingAsync();

    Console.WriteLine("Press [Enter] to stop the processor.");
    Console.ReadLine();

    await processor.StopProcessingAsync();
}

await ProcessEvents();