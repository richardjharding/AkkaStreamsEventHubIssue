// See https://aka.ms/new-console-template for more information
using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Azure.EventHub;
using Akka.Streams.Dsl;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using EventGenerator;

Console.WriteLine("Starting generator, sending 100 events");

const string ehubNamespaceConnectionString = "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;";
const string eventHubName = "defaultevents";

using var sys = ActorSystem.Create("EventHubGeneratorSystem");
using var mat = sys.Materializer();
var generatorActor = sys.ActorOf(Props.Create(() => new EventGeneratorActor()), "generatorActor");


var producerClient = new EventHubProducerClient(ehubNamespaceConnectionString, eventHubName);

var generatorTask = Source.From(Enumerable.Range(1, 10))
                        .Select(i => i.ToString())
                        .Ask<EventData>(generatorActor, TimeSpan.FromSeconds(2), 1)
                        .WireTap(e =>
                        {
                            Console.WriteLine($"Sending event: {e.EventBody}");
                        })
                        .Grouped(1)
                        .ToEventHub(producerClient, mat);

await generatorTask;
                        