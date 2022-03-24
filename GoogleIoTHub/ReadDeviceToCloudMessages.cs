// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// This application uses the Microsoft Azure Event Hubs Client for .NET
// For samples see: https://github.com/Azure/azure-event-hubs/tree/master/samples/DotNet
// For documenation see: https://docs.microsoft.com/azure/event-hubs/
using System;
using Microsoft.Azure.EventHubs;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ComponentModel;

namespace read_d2c_messages
{
    class ReadDeviceToCloudMessages
    {
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

        private static string s_eventHubsCompatibleEndpoint = Environment.GetEnvironmentVariable("EVENT_HUBS_COMPATIBILITY_ENDPOINT");


        // Event Hub-compatible name
        // az iot hub show --query properties.eventHubEndpoints.events.path --name {your IoT Hub name}
        //// private readonly static string s_eventHubsCompatiblePath = "{your Event Hubs compatible name}";

        private static string s_eventHubsCompatiblePath = Environment.GetEnvironmentVariable("EVENT_HUBS_COMPATIBILITY_PATH");


        // az iot hub policy show --name service --query primaryKey --hub-name {your IoT Hub name}
        //private readonly static string s_iotHubSasKey = "{your service primary key}";
        //private readonly static string s_iotHubSasKeyName = "service";

        private static string s_iotHubSasKey = Environment.GetEnvironmentVariable("EVENT_HUBS_SAS_KEY");// "{your service primary key}";
        private static string s_iotHubSasKeyName = Environment.GetEnvironmentVariable("SHARED_ACCESS_KEY_NAME");// "service, iothubowner";



        private static EventHubClient s_eventHubClient;

        // Asynchronously create a PartitionReceiver for a partition and then start 
        // reading any messages sent from the simulated client.
        private static async Task ReceiveMessagesFromDeviceAsync(string partition, CancellationToken ct)
        {
            // Create the receiver using the default consumer group.
            // For the purposes of this sample, read only messages sent since 
            // the time the receiver is created. Typically, you don't want to skip any messages.
            var eventHubReceiver = s_eventHubClient.CreateReceiver("$Default", partition, EventPosition.FromEnqueuedTime(DateTime.Now));
            Console.WriteLine("Create receiver on partition: " + partition);
            bool forever = true;
            while (forever)
            {
                if (ct.IsCancellationRequested) break;
                Console.WriteLine("Listening for messages on: " + partition);
                // Check for EventData - this methods times out if there is nothing to retrieve.
                var events = await eventHubReceiver.ReceiveAsync(100);

                // If there is data in the batch, process it.
                if (events == null) continue;

                foreach (EventData eventData in events)
                {

                    string data = Encoding.UTF8.GetString(eventData.Body.Array);
                    int count1 = data.Count(f => f == '{');
                    int count2 = data.Count(f => f == '}');
                    int count3 = data.Count(f => f == '"');
                    // Check for braces
                    if ((count1! != 1) || (count2 != 1))
                        continue;
                    Console.WriteLine("Message received on partition {0}:", partition);
                    Console.WriteLine("  {0}:", data);
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
                        Console.Write("property Name: " + prop.Name.ToString() + "\t");

                        Console.WriteLine("property Value: " + prop.Value);
                        // Could allow for more types, especially mixed.
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
                    forever = false;

                    if (show_system_properties)
                    {
                        Console.WriteLine("System properties (set by IoT Hub):");
                        foreach (var prop in eventData.SystemProperties)
                        {
                            Console.WriteLine("  {0}: {1}", prop.Key, prop.Value);
                        }
                    }
                }
            }
        }

        private static async Task Main(string[] args)
        {
            Console.WriteLine("IoT Hub Quickstarts - Read device to cloud messages. Ctrl-C to exit.\n");
            Console.WriteLine("Using Env Var EVENT_HUBS_COMPATIBILITY_ENDPOINT = " + s_eventHubsCompatibleEndpoint);
            Console.WriteLine("Using Env Var EVENT_HUBS_COMPATIBILITY_PATH = " + s_eventHubsCompatiblePath);
            Console.WriteLine("Using Env Var EVENT_HUBS_SAS_KEY = " + s_iotHubSasKey);
            Console.WriteLine("Using Env Var SHARED_ACCESS_KEY_NAME = " + s_iotHubSasKeyName);


            show_system_properties = false;


            var connectionString = new EventHubsConnectionStringBuilder(new Uri(s_eventHubsCompatibleEndpoint), s_eventHubsCompatiblePath, s_iotHubSasKeyName, s_iotHubSasKey);

            s_eventHubClient = EventHubClient.CreateFromConnectionString(connectionString.ToString());

            // Create a PartitionReciever for each partition on the hub.
            var runtimeInfo = await s_eventHubClient.GetRuntimeInformationAsync();
            var d2cPartitions = runtimeInfo.PartitionIds;

            CancellationTokenSource cts = new CancellationTokenSource();

            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
                Console.WriteLine("Exiting...");
            };

            var tasks = new List<Task>();
            foreach (string partition in d2cPartitions)
            {

                tasks.Add(ReceiveMessagesFromDeviceAsync(partition, cts.Token));
            }

            // Wait for all the PartitionReceivers to finish.
            Task.WaitAny(tasks.ToArray());
        }
    }
}
