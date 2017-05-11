using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace accessgranted
{
    class Program
    {
        static SerialPort _serialPort;
        static Thread readThread;
        static string message;
        static string code;
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
                    for(int i=1;i<15;i++)
                    {
                        if (message[i] == ';')
                            break;
                        code += message[i];
                    }
                    executeCurl(code);
                    code = "";
                    message = "";
                }
                catch (TimeoutException) { }
                
            }
        }
        public static void executeCurl(string code)
        {
            Process curl = new Process();
            curl.StartInfo.FileName = "curl.exe";
            curl.StartInfo.Arguments = "-X POST -d \"message="+code+"\" https://anzepau.000webhostapp.com/accessgranted_notify/trigger.php";
            curl.Start();

            code = "";

        }
    }
}
