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
- **bme280-device-local** console app that reads telemetry from a BME280 and sends that telemetry.
- **dht22-1-wire-device-nuget** [Coming]
- [Integrations with Triggercmd](https://github.com/djaus2/TRIGGERcmdRPi)
  - Coming