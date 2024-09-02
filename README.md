# AkkaStreamsEventHubIssue
Demo of possible issue of a single eventhub source reading from multiple partitions using Akka Streams

I was able to replicate this in Azure, this repo uses the azure event hub emulator to replicate the issue.

Start the emulator with

`docker compose -f .\docker-compose.yaml up`

Create events by running the EventGenerator console app, it will publish 10 events and exit

Run the non akka consumer  - PartitionPocReader

This should log reading from both partitions - total of 10 events

sample output
```
❯ dotnet run
Starting wiretap
Press [Enter] to stop the processor.
Received event: 1 :  1
Total events: 1
Received event: 0 :  2
Received event: 1 :  3
Total events: 2
Total events: 3
Received event: 1 :  5
Total events: 4
Received event: 1 :  7
Total events: 5
Received event: 0 :  4
Total events: 6
Received event: 1 :  9
Total events: 7
Received event: 0 :  6
Total events: 8
Received event: 0 :  8
Total events: 9
Received event: 0 :  10
Total events: 10
```

Run the akka consumer - PartitionPocReaderAkka

This often reads from a single partition or one event from one partition before getting stuck reading from the other - sample output

```
❯ dotnet run
Starting event listener
11:27:21 Event received from Parition: 1 : 1
Total messages received: 1
11:27:21 Event received from Parition: 0 : 2
Total messages received: 2
11:27:21 Event received from Parition: 0 : 4
Total messages received: 3
11:27:21 Event received from Parition: 0 : 6
Total messages received: 4
11:27:21 Event received from Parition: 0 : 8
Total messages received: 5
11:27:21 Event received from Parition: 0 : 10
Total messages received: 6

```


If you run the generator again, the non akka consumer reads from both partitions as expected, but the akka consumer still gets stuck reading from a single partition

