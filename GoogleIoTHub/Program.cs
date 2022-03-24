using System;

namespace read_d2c_messages
{
    class Program
    {
        static  void Main(string[] args)
        {
           Console.WriteLine("Hello Google and Azure!");
           read_d2c_messages.ReadDeviceToCloudMessages.Run(new string[0]).GetAwaiter();
        }
    }
}
