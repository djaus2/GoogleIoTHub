﻿    // Copyright (c) Microsoft. All rights reserved.
    // Licensed under the MIT license. See LICENSE file in the project root for full license information.

    // This application uses the Azure Event Hubs Client for .NET
    // For samples see: https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/eventhub/Azure.Messaging.EventHubs/samples/README.md
    // For documentation see: https://docs.microsoft.com/azure/event-hubs/

using Azure.Messaging.EventHubs.Consumer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GoogleIoTHubD2C
{
    /// <summary>
    /// A sample to illustrate reading Device-to-Cloud messages from a service app.
    /// </summary>
    internal class Program
    {
        const int hubRefreshPreiod = 10; //Sec
        private static bool DisplayData = false;
        private static bool runOnce = true;
        public static void WriteT2S(string txt)
        {
            if (File.Exists(@"c:\temp\temperature.txt"))
            {
                // If file found, delete it    
                File.Delete(@"c:\temp\temperature.txt");
                Console.WriteLine("File deleted.");
            }
            File.WriteAllText(@"c:\temp\temperature.txt", txt);
            return;
        }
        private static bool show_system_properties = true;
        // Event Hub-compatible endpoint
        // az iot hub show --query properties.eventHubEndpoints.events.endpoint --name {your IoT Hub name}
        ////private readonly static string s_eventHubsCompatibleEndpoint = "{your Event Hubs compatible endpoint}";

        private static string EventHubCompatibleEndpoint = Environment.GetEnvironmentVariable("EVENT_HUBS_COMPATIBILITY_ENDPOINT");


        // Event Hub-compatible name
        // az iot hub show --query properties.eventHubEndpoints.events.path --name {your IoT Hub name}
        //// private readonly static string s_eventHubsCompatiblePath = "{your Event Hubs compatible name}";

        private static string EventHubName = Environment.GetEnvironmentVariable("EVENT_HUBS_COMPATIBILITY_PATH");


        // az iot hub policy show --name service --query primaryKey --hub-name {your IoT Hub name}
        //private readonly static string s_iotHubSasKey = "{your service primary key}";
        //private readonly static string s_iotHubSasKeyName = "service";

        private static string SharedAccessKey = Environment.GetEnvironmentVariable("EVENT_HUBS_SAS_KEY");// "{your service primary key}";
        private static string IotHubSharedAccessKeyName = Environment.GetEnvironmentVariable("SHARED_ACCESS_KEY_NAME");// "service, iothubowner";

        private static string EventHubConnectionString = null;
        //The event hub-compatible name of your IoT Hub instance. Use `az iot hub show --query properties.eventHubEndpoints.events.path --name { your IoT Hub name}` to fetch via the Azure CLI.")]
        //private static string EventHubName = "";

        public static async Task Main(string[] args)
        {
            if (args.Length > 0)
            {
                if (args[0].ToLower() == "false")
                    runOnce = false;
                else
                    runOnce = true;
            }
            DisplayData = false;
            // Parse application parameters

            // Either the connection string must be supplied, or the set of endpoint, name, and shared access key must be.
            if (string.IsNullOrWhiteSpace(EventHubConnectionString)
                    && (string.IsNullOrWhiteSpace(EventHubCompatibleEndpoint)
                        || string.IsNullOrWhiteSpace(EventHubName)
                        || string.IsNullOrWhiteSpace(SharedAccessKey)))
            {
                Console.WriteLine("Bye");
                Environment.Exit(1);
            }

            Console.WriteLine("IoT Hub Quickstarts - Read device to cloud messages. Ctrl-C to exit.\n");

            // Set up a way for the user to gracefully shutdown
            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cts.Cancel();
                Console.WriteLine("Exiting...");
            };

            // Run the sample
            await ReceiveMessagesFromDeviceAsync(cts.Token);

            Console.WriteLine("Cloud message reader finished.");
        }

        internal static string GetEventHubConnectionString()
        {
            return EventHubConnectionString ?? $"Endpoint={EventHubCompatibleEndpoint};SharedAccessKeyName={IotHubSharedAccessKeyName};SharedAccessKey={SharedAccessKey}";
        }

        // Asynchronously create a PartitionReceiver for a partition and then start
        // reading any messages sent from the simulated client.
        private static async Task ReceiveMessagesFromDeviceAsync(CancellationToken ct)
        {
            string connectionString = GetEventHubConnectionString();

            // Create the consumer using the default consumer group using a direct connection to the service.
            // Information on using the client with a proxy can be found in the README for this quick start, here:
            // https://github.com/Azure-Samples/azure-iot-samples-csharp/tree/main/iot-hub/Quickstarts/ReadD2cMessages/README.md#websocket-and-proxy-support
            await using var consumer = new EventHubConsumerClient(
                EventHubConsumerClient.DefaultConsumerGroupName,
                connectionString,
                EventHubName);

            Console.WriteLine("Listening for messages on all partitions.");

            try
            {
            // Begin reading events for all partitions, starting with the first event in each partition and waiting indefinitely for
            // events to become available. Reading can be canceled by breaking out of the loop when an event is processed or by
            // signaling the cancellation token.
            //
            // The "ReadEventsAsync" method on the consumer is a good starting point for consuming events for prototypes
            // and samples. For real-world production scenarios, it is strongly recommended that you consider using the
            // "EventProcessorClient" from the "Azure.Messaging.EventHubs.Processor" package.
            //
            // More information on the "EventProcessorClient" and its benefits can be found here:
            //   https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/eventhub/Azure.Messaging.EventHubs.Processor/README.md
                await foreach (PartitionEvent partitionEvent in consumer.ReadEventsAsync(ct))
                {
                    //////////////////////////////////////////////////////////////////////////////////////////////////
                    //Console.WriteLine($"\nMessage received on partition {partitionEvent.Partition.PartitionId}:");
                    // Events are retained for 1 day (default. Can be up to 7 days.
                    // Have to skip through all events for last 24 hours to one that is within the Hub read period.
                    // Couldbe better to run this as a service.
                    //////////////////////////////////////////////////////////////////////////////////////////////////
                    var enqTime = partitionEvent.Data.EnqueuedTime;
                    var now = DateTimeOffset.UtcNow;
                    if ((now - enqTime) > new TimeSpan(0, 0, (hubRefreshPreiod-1)))
                        continue;
                    //////////////////////////////////////////////////////////////////////////////////////////////////

                    string data = Encoding.UTF8.GetString(partitionEvent.Data.Body.ToArray());

                    if (DisplayData)
                    {
                        Console.WriteLine($"\tMessage body: {data}");

                        Console.WriteLine("\tApplication properties (set by device):");
                        foreach (KeyValuePair<string, object> prop in partitionEvent.Data.Properties)
                        {
                            PrintProperties(prop);
                        }

                        Console.WriteLine("\tSystem properties (set by IoT Hub):");
                        foreach (KeyValuePair<string, object> prop in partitionEvent.Data.SystemProperties)
                        {
                            PrintProperties(prop);
                        }
                    }

                    int count1 = data.Count(f => f == '{');
                    int count2 = data.Count(f => f == '}');
                    int count3 = data.Count(f => f == '"');
                    // Check for braces
                    if ((count1! != 1) || (count2 != 1))
                        continue;
                    if (DisplayData)
                    {
                        //Console.WriteLine("Message received on partition {0}:", partitionEvent.Partition);
                        //Console.WriteLine("  {0}:", data);
                    }
                    string msg = "";
                    dynamic d = Newtonsoft.Json.Linq.JObject.Parse(data);
                    int pCount = 0;
                    foreach (var prop in d)
                    {
                        pCount++;
                    }
                    // If number of " = 2xNumber of properties then is number as each property name requires 2.
                    // If number of " = 4xNumber of properties then is string as each property name and value require 2 each.
                    // Could be more selective.
                    bool valsAreDouble = true;
                    bool valsAreString = false;
                    if (count3 != 2 * pCount)
                    {
                        valsAreDouble = false;
                        if (count3 != 4 * pCount)
                        {
                            valsAreString = true;
                        }
                    }
                    if (!(valsAreDouble | valsAreString))
                        continue;
                    foreach (var prop in d)
                    {
                        if (DisplayData)
                        {
                            Console.Write("property Name: " + prop.Name.ToString() + "\t");

                            Console.WriteLine("property Value: " + prop.Value);
                            //Could allow for more types, especially mixed.
                        }
                        if (valsAreDouble)
                        {
                            double val = (double)prop.Value;
                            msg += $" {prop.Name.ToString()} {val.ToString("0.##")}";
                        }
                        else if (valsAreString)
                            msg += $" {prop.Name.ToString()} {prop.Value}";
                    }

                    Console.WriteLine(msg);
                    WriteT2S(msg);
                    if (runOnce)
                        break;
                }
            }
            catch (TaskCanceledException)
            {
                // This is expected when the token is signaled; it should not be considered an
                // error in this scenario.
            }
        }

        private static void PrintProperties(KeyValuePair<string, object> prop)
        {
            string propValue = prop.Value is DateTime
                ? ((DateTime)prop.Value).ToString("O") // using a built-in date format here that includes milliseconds
                : prop.Value.ToString();

            Console.WriteLine($"\t\t{prop.Key}: {propValue}");
        }
    }
}
 
