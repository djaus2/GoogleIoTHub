// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//Ref: https://github.com/dotnet/iot/blob/master/src/devices/Bmp180/samples/Bmp180.Sample.cs


using System;
using System.Device.I2c;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.Bmxx80;
using Iot.Device.Common;
using SendTelemetry;
using UnitsNet;

namespace DotNetCoreCoreGPIO
{
    /// <summary>
    /// Test program main class
    /// </summary>
    public static class BME280Sampler
    {
        /// <summary>
        /// Entry point for example program
        /// </summary>
        /// <param name="args">Command line arguments</param>
        public static dynamic Get()
        {
            Console.WriteLine("Using BME280!");
            Console.WriteLine(Bme280.DefaultI2cAddress);
            string result = "";
            dynamic telemetryDataPoint = null;
            try
            {
                // set this to the current sea level pressure in the area for correct altitude readings
                Pressure defaultSeaLevelPressure = WeatherHelper.MeanSeaLevel;
                // bus id on the raspberry pi 3 for Pins 3 (SDA) and 5 (SCL)
                const int busId = 1;
                // Note for units with more than 4 pins (Vin, GND and SDA, SCL) ...
                // Bme280.DefaultI2cAddress = 0x77 Need SD0 set to hi for that. If low its 0x76
                // The Bosch documentation says make CSB hi for I2C, but many units have /CS which I treat as a chip set (ie Gnd it)
                // Also for I2C to work need to enable the interafce in Menu Preferences/Raspberry Pi Configuration/Interfaces
                // A check: i2cdetect -y 1
                // Should show 77
                // Ref: https://www.electroniclinic.com/i2c-serial-communication-bus-in-raspberry-pi/#:~:text=I2C%20bus%20in%20Raspberry%20pi%3A%20I2C%20bus%20represents,unlike%20the%20SPI%20bus%2C%20only%20uses%20two%20wires.
                I2cConnectionSettings i2cSettings = new(busId, Bme280.DefaultI2cAddress);
                using I2cDevice i2cDevice = I2cDevice.Create(i2cSettings);
                using Bme280 bme80 = new Bme280(i2cDevice)
                {
                    // set higher sampling
                    TemperatureSampling = Sampling.LowPower,
                    PressureSampling = Sampling.UltraHighResolution,
                    HumiditySampling = Sampling.Standard,

                };

                // Perform a synchronous measurement
                var readResult = bme80.Read();

                // Note that if you already have the pressure value and the temperature, you could also calculate altitude by using
                // var altValue = WeatherHelper.CalculateAltitude(preValue, defaultSeaLevelPressure, tempValue) which would be more performant.
                bme80.TryReadAltitude(defaultSeaLevelPressure, out var altValue);


                Console.WriteLine($"Temperature: {readResult.Temperature?.DegreesCelsius:0.#}\u00B0C");
                Console.WriteLine($"Pressure: {readResult.Pressure?.Hectopascals:0.##}hPa");
                Console.WriteLine($"Altitude: {altValue.Meters:0.##}m");
                Console.WriteLine($"Relative humidity: {readResult.Humidity?.Percent:0.#}%");

                result += $"Temperature: {readResult.Temperature?.DegreesCelsius:0.#}\u00B0C ,";
                result += $"Pressure: {readResult.Pressure?.Hectopascals:0.##}hPa ,";
                result += $"Altitude: {altValue.Meters:0.##}m ,";
                result += $"Relative humidity: {readResult.Humidity?.Percent:0.#}% .";

                telemetryDataPoint = new
                {
                    temperature = (double)readResult.Temperature?.DegreesCelsius,
                    pressure = (double)readResult.Pressure?.Hectopascals,
                    humidity = readResult.Humidity?.Percent
                };

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Failed: Probably no hw.");
                result = $"Failed: Probably no hw.{ex.Message}";
                telemetryDataPoint = null;
            }
            return telemetryDataPoint;
        }

    }

}
