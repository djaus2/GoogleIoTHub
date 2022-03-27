# SendTelemetry2Hub

Send Telemetry to an Azure IoT Hub. Simple to include functionality in a Console app.

## Include in a .NET project

```
dotnet add package SendTelemetry2Hub
```

## Call

```
using SendTelemetry;

    ...
    ...

    await DeviceSendTelemetryToHub.SendDeviceToCloudMessageAsync(telemetryData, connectionString);

```

## Parameters

- telemetryData of type dynamic eg:  
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
- connectionString of type string
  - The Azure IoT Hub Device Connection string

## Further

See the code repository for sample Console app where it is part of a Triggercmd app for Google Nest.