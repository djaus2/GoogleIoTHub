# GoogleIoTHub
An Azure IoT Hub D2C Console app that can be activated by TRIGGERcmd to get telemetry from an Azure IoT Hub, form a speakable string from it  and then forward it to a Google Nest.

This is a work in progress. 2Do-ing:

- D2C Console app **GoogleIoTHubD2C** that writes last telemetry to temp file. [Done]
- Device Simulator Console App **simulated-device** [Done]
   - Extracted single generic telemetry send to class library **SendTelemetry2Hub**
   - Will publish lib to Nuget.
   - Added **simulated-device-local** that uses it locally.:  
    ```await DeviceSendTelemetryToHub.SendDeviceToCloudMessageAsync(telemetryDataPoint, s_connectionString);```  
    ```telemetryDataPoint``` is dynamic type

- [djaus2/DNETCoreGPIO option for device using sensors](https://github.com/djaus2/DNETCoreGPIO)
- [Integrations with Triggercmd](https://github.com/djaus2/TRIGGERcmdRPi)