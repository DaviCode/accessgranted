using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;

namespace accessgranted
{
    class Program
    {
        static SerialPort _serialPort;
        static Thread readThread;
        static string message;
        static void Main(string[] args)
        {
            _serialPort = new SerialPort();
            _serialPort.PortName = SetPortName(_serialPort.PortName);
            _serialPort.BaudRate = 19200;
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;

            _serialPort.Open();
            readThread = new Thread(Read);
            readThread.Start();


            
            readThread.Join();
            _serialPort.Close();

        }
        public static string SetPortName(string defaultPortName)
        {
            string portName;

            Console.WriteLine("Available Ports:");
            foreach (string s in SerialPort.GetPortNames())
            {
                Console.WriteLine("   {0}", s);
            }

            Console.Write("Enter COM port value (Default: {0}): ", defaultPortName);
            portName = Console.ReadLine();

            if (portName == "" || !(portName.ToLower()).StartsWith("com"))
            {
                portName = defaultPortName;
            }
            return portName;
        }

        public static void Read()
        {
            char ch;
            while (true)
            {
                try
                {
                    do
                    {
                        
                        ch = Convert.ToChar(_serialPort.ReadChar());
                        message += ch;
                        Console.Write(ch);
                        if (ch == '!')
                        {
                            Console.WriteLine();
                        }
                    }
                    while (ch != '!');
                }
                catch (TimeoutException) { }
            }
        }
    }
}
