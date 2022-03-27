# GoogleIoTHub
An Azure IoT Hub D2C Console app that can be activated by TRIGGERcmd to get telemetry from an Azure IoT Hub, 
forming a speakable string from it and then forwarding it to a Google Nest for enunciation.

This is a work in progress:

- D2C Console app **GoogleIoTHubD2C** that writes last telemetry send to Azure IoT Hub to temp file.
- Device Simulator Console App **simulated-device**
- Extracted single generic telemetry send to class library **SendTelemetry2Hub**
  - Added **simulated-device-local** console app that uses it locally.:  
    ```await DeviceSendTelemetryToHub.SendDeviceToCloudMessageAsync(telemetryData, deviceconnectionString);```
    - ```deviceconnectionstring``` is the Device Connection String for the IOY Hub
    -  ```telemetryData``` is of dynamic type eg:  
```
    double currentTemperature = minTemperature + rand.NextDouble() * 15;
    double currentHumidity = minHumidity + rand.NextDouble() * 20;

    // Create JSON message
    var telemetryData = new
    {
        temperature = currentTemperature,
        humidity = currentHumidity
    };

```
- Published lib to Nuget as [SendTelemetery2Hub](https://www.nuget.org/packages/SendTelemetry2Hub/)
  - Added **simulated-device-nuget** console app that uses Nuget package.
- [djaus2/DNETCoreGPIO option for device using sensors](https://github.com/djaus2/DNETCoreGPIO) (on GitHub)
  - Option 31 with DNETCoreGPIO continuously sends BME280 telemetry to Azure IoT Hub.
    - Uses this SendTelemetry2Hub package.
  - Option 30 2Do for DHT22 1-Wire
  - The functionality for this is also a Nuget package: [DNETCoreGPIO](https://www.nuget.org/packages/DNETCoreGPIO/)
- [Integrations with Triggercmd](https://github.com/djaus2/TRIGGERcmdRPi)
  - Coming