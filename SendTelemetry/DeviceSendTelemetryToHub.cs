using System;
using Microsoft.Azure.Devices.Client;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SendTelemetry
{
    // Copyright (c) Microsoft. All rights reserved.
    // Licensed under the MIT license. See LICENSE file in the project root for full license information.

    // This application uses the Azure IoT Hub device SDK for .NET
    // For samples see: https://github.com/Azure/azure-iot-sdk-csharp/tree/master/iothub/device/samples


    public static class DeviceSendTelemetryToHub
    {
        private static DeviceClient s_deviceClient;

        // The device connection string to authenticate the device with your IoT hub.
        // Using the Azure CLI:
        // az iot hub device-identity connection-string show --hub-name {YourIoTHubName} --device-id MyDotnetDevice --output table
        ////private readonly static string s_connectionString = "{Your device connection string here}";

        // For this sample either
        // - pass this value as a command-prompt argument
        // - set the IOTHUB_DEVICE_CONN_STRING environment variable 
        // - create a launchSettings.json (see launchSettings.json.template) containing the variable
        //private static string s_connectionString = Environment.GetEnvironmentVariable("IOTHUB_DEVICE_CONN_STRING");



        // Async method to send simulated telemetry
        public static async Task SendDeviceToCloudMessageAsync(dynamic telemetryDataPoint, string s_connectionString)
        {
            try
            {
                s_deviceClient = DeviceClient.CreateFromConnectionString(s_connectionString, TransportType.Mqtt);
                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                Message message = new Message(Encoding.ASCII.GetBytes(messageString));

                // Send the telemetry message
                await s_deviceClient.SendEventAsync(message);
                Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

    }
}

