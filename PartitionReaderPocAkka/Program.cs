// See https://aka.ms/new-console-template for more information
using Akka;
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Azure.EventHub;
using Akka.Streams.Dsl;
using PartitionReaderPocAkka;
using System;
using System.Collections.Concurrent;
using System.Text;

Console.WriteLine("Starting event listener");

//create actor system and materializer
using var sys = ActorSystem.Create("EventHubGeneratorSystem");
using var mat = sys.Materializer();
var totals = new ConcurrentDictionary<string, int>();

var (processor, streamTask) = EventHubSource.Create(new ProcessorFactory())
    .SelectAsync(1, async t =>
    {
        if (t.Data is not null)
        {
            string data = Encoding.UTF8.GetString(t.Data.Body.ToArray());
            Console.WriteLine($"{DateTime.Now.ToLongTimeString()} Event received from Parition: {t.Partition.PartitionId} : {data}");
            var totalEvents = totals.AddOrUpdate("total", 1, (key, value) => value + 1);
            Console.WriteLine($"Total messages received: {totalEvents}");
        }

        if (t.HasEvent && t.Data is not null)
        {
            await t.UpdateCheckpointAsync();
        }

        
        return Done.Instance;
    })
    .ToMaterialized(Sink.Ignore<Done>(), Keep.Both)
    .Run(mat);

await streamTask;
