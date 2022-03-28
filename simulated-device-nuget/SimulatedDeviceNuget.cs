// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// This application uses the Azure IoT Hub device SDK for .NET
// For samples see: https://github.com/Azure/azure-iot-sdk-csharp/tree/master/iothub/device/samples

using System;
using System.Threading.Tasks;
using SendTelemetry;

namespace simulated_device
{
    class SimulatedDeviceNuget
    {

        // The device connection string to authenticate the device with your IoT hub.
        // Using the Azure CLI:
        // az iot hub device-identity connection-string show --hub-name {YourIoTHubName} --device-id MyDotnetDevice --output table
        ////private readonly static string s_connectionString = "{Your device connection string here}";
        
        // For this sample either
        // - pass this value as a command-prompt argument
        // - set the IOTHUB_DEVICE_CONN_STRING environment variable 
        // - create a launchSettings.json (see launchSettings.json.template) containing the variable
        private static string s_connectionString = Environment.GetEnvironmentVariable("IOTHUB_DEVICE_CONN_STRING");
        private static int period;



        // Async method to send simulated telemetry
        private static async Task SendDeviceToCloudMessagesAsync()
        {
            // Initial telemetry values
            double minTemperature = 20;
            double minHumidity = 60;
            Random rand = new Random();

            while (true)
            {
                double currentTemperature = minTemperature + rand.NextDouble() * 15;
                double currentHumidity = minHumidity + rand.NextDouble() * 20;

                // Create JSON message
                var telemetryDataPoint = new
                {
                    temperature = currentTemperature,
                    humidity = currentHumidity
                };

                // Send the telemetry message
                await DeviceSendTelemetryToHub.SendDeviceToCloudMessageAsync(telemetryDataPoint, s_connectionString);

                await Task.Delay(period*1000);
            }
        }
        private static async Task Main(string[] args)
        {
            period = 10;
            Console.WriteLine("IoT Hub Quickstarts #1 - Simulated device. Ctrl-C to exit.\n");

            if (args.Length > 0)
            {
                if (int.TryParse(args[0], out int iperiod))
                {
                    period = iperiod;
                }
                if (args.Length > 1)
                    if (args[1].Length > 20)
                        s_connectionString = args[1];
            }

            Console.WriteLine("Using Env Var IOTHUB_DEVICE_CONN_STRING = " + s_connectionString);

            await SendDeviceToCloudMessagesAsync();
            Console.WriteLine("Done");
        }
    }
}
