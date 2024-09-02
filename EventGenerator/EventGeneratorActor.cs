using Akka.Actor;
using Azure.Messaging.EventHubs;
using System.Text;

namespace EventGenerator;

internal class EventGeneratorActor : ReceiveActor
{
    public EventGeneratorActor()
    {
        Receive<string>(message =>
        {
            var eventMessage = new EventData(Encoding.UTF8.GetBytes(message));
            Sender.Tell(eventMessage);
        });
    }
}
