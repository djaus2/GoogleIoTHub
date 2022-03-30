# GoogleIoTHub
An Azure IoT Hub D2C Console app that can be activated by TRIGGERcmd to get telemetry from an Azure IoT Hub, 
forming a speakable string from it and then forwarding it to a Google Nest for enunciation.

This is a work in progress:

- D2C Console app **GoogleIoTHubD2C** that writes last telemetry send to Azure IoT Hub to temp file.
  - Optional arg: true/false(default true) if true runs app for just the next received telemetry only, false viz.
  - _2Do: Each time app runs it reads all telemetry sent in last 24hrs. Better to have a service locally that serves up last sent msg only. ... later_
- Device Simulator Console App **simulated-device**
  - Optional args: period(default 10sec)  deviceconnectionstring
    - deviceconnectionstring: defaults to environment value
    - Can Use dots to use defaults, as place holders
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
    - Optional args: period(default 10sec)  deviceconnectionstring
      - deviceconnectionstring: defaults to environment value
      - Can Use dots to use defaults, as place holders
- **bme280-device-local** Console app that reads telemetry from a BME280 and sends that telemetry.
  - Optional args: period(default 10sec)  deviceconnectionstring
    - deviceconnectionstring: defaults to environment value
    - Can Use dots to use defaults, as place holders
  - [Typical Circuit](https://github.com/djaus2/DNETCoreGPIO/blob/master/DNETCoreGPIO/Circuits/rpi-bmp280_i2c.png)
    - [More info from line 42 here](https://github.com/djaus2/DNETCoreGPIO/blob/master/DNETCoreGPIO/BMX280Sampler.cs)
- **dht22-1-wire-device-local** Console app that reads telemetry from a DHT22 1-Wire and sends that telemetry
  - Optional args: period(default 10sec)  deviceconnectionstring dht22Gpio(default 26)
    - deviceconnectionstring: defaults to environment value
    - Can Use dots to use defaults, as place holders 
  - [Typical circuit](https://github.com/djaus2/DNETCoreGPIO/blob/master/DNETCoreGPIO/Circuits/dht22.png) 
    - Nb: In that diagram to pinouts are Pwr-Signal-nc-Gnd  Pwr = 5V
    - On my (red) device they are Gnd-Pwr-Signal  I use Pwr = 3.3V
- **bme280**  is now option 31 for [djaus2/DNETCoreGPIO](https://github.com/djaus2/DNETCoreGPIO)
  - Parameters there are: 31 . \<Period\> \<DeviceConnectionString\>
  - The second parameter is a dot as a place holder as bme280 doesn't use GPIO. It uses I2C.
- [Integrations with Triggercmd](https://github.com/djaus2/TRIGGERcmdRPi)
  - Coming