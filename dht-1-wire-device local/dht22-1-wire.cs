using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Device.Gpio;
using Iot.Device;
using Iot.Device.CpuTemperature;
using System.Device.I2c;
using Iot.Device.Bmp180;
using Iot.Device.DHTxx;

namespace dht1wiredevicelocal
{ 
    public static class dht221wire
    {
        public static dynamic GetTempDHTxx1Wire(int maxNumTries, int dht22GPio=25)
        {
            int numTries = 0;
            dynamic datapoint = null;
            //1-Wire:

            Console.WriteLine("Using DH22-1-Wire1");
            bool lastResult = true;
            using (Dht22 dht = new Dht22(dht22GPio))
            {
                if (dht == null)
                {
                    Console.WriteLine("Dht22 instantiation failed");
                    return datapoint;

                }
                else
                {
                    while (true)
                    {
                        UnitsNet.Temperature temp;
                        UnitsNet.RelativeHumidity humid;
                        bool result1 = dht.TryReadTemperature(out temp);
                        bool result2 = dht.TryReadHumidity(out humid);
                        if (!result1 || !result2)
                        {
                            Console.Write(".");
                            lastResult = false;
                            if (++numTries >= maxNumTries)
                                return datapoint;
                        }
                        else
                        {
                            //Sanity Check
                            bool resultIsValid = true;
                            if (temp.DegreesCelsius is double.NaN)
                                resultIsValid = false;
                            else if (humid.Percent is double.NaN)
                                resultIsValid = false;
                            if (!resultIsValid)
                            {
                                Console.Write("#");
                                lastResult = false;
                                if (++numTries >= maxNumTries)
                                    return datapoint;
                            }
                            else
                            {
                                bool resultIsSane = true;
                                if ((temp.DegreesCelsius > 100) || (temp.DegreesCelsius < -20))
                                    resultIsSane = false;
                                else if ((humid.Percent > 100) || (humid.Percent < 0))
                                    resultIsSane = false;
                                if (!resultIsSane)
                                {
                                    Console.Write("x");
                                    lastResult = false;
                                    if (++numTries >= maxNumTries)
                                        return datapoint;
                                }
                                else
                                {

                                    if (!lastResult)
                                    {
                                        Console.WriteLine("");
                                    }
                                    lastResult = true;
                                    Console.Write($"Temperature: {temp.DegreesCelsius.ToString("0.0")} °C ");
                                    Console.Write($"Humidity: { humid.Percent.ToString("0.0")} % ");
                                    Console.WriteLine("");
                                    string result = $"Temperature equals {temp.DegreesCelsius.ToString("0.0")} °C ,";
                                    result += $"and Humidity equals { humid.Percent.ToString("0.0")} % ";

                                    datapoint = new
                                    {
                                        temperature = temp.DegreesCelsius,
                                        humidity = humid.Percent
                                    };
                                    return datapoint;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
