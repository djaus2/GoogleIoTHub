// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// This application uses the Azure IoT Hub device SDK for .NET
// For samples see: https://github.com/Azure/azure-iot-sdk-csharp/tree/master/iothub/device/samples

using System;
using Microsoft.Azure.Devices.Client;
using System.Threading.Tasks;
using SendTelemetry;

namespace simulated_device
{
    class SimulatedDeviceLocal
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
        private static async void SendDeviceToCloudMessagesAsync(int maxReties, int dht22Gpio)
        {

            while (true)
            {

                // Create JSON message
                var telemetryDataPoint = dht1wiredevicelocal.dht221wire.GetTempDHTxx1Wire(maxReties, dht22Gpio);

                if (telemetryDataPoint != null)
                {
                    // Send the telemetry message
                    await DeviceSendTelemetryToHub.SendDeviceToCloudMessageAsync(telemetryDataPoint, s_connectionString);
                }

                await Task.Delay(period*1000);
            }
        }
        private static void Main(string[] args)
        {
            period = 10; //sec
            int maxReties = 10;
            // Set this to the RPi GPIO connection for DHT22 1-Wire
            // eg https://github.com/djaus2/DNETCoreGPIO/blob/master/DNETCoreGPIO/Circuits/dht22.png  .. Its 26 there
            int dht22Gpio = 26;
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
                if (args.Length > 2)
                    if (int.TryParse(args[2], out int igpio))
                        dht22Gpio = igpio;
            }

            Console.WriteLine("Using Env Var IOTHUB_DEVICE_CONN_STRING = " + s_connectionString);

            SendDeviceToCloudMessagesAsync(maxReties, dht22Gpio);
            Console.ReadLine();
        }
    }
}
