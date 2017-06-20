using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;
using System.IO;
using MySql.Data.MySqlClient;
using System.Windows.Forms;


namespace accessgranted
{
    class Program
    {
        static string server, database, uid, password;
        static MySqlConnection conn;
        static StreamWriter sw;
        static SerialPort _serialPort;
        static Thread readThread;
        static Thread readKeyThread;
        //static Thread curlThread;
        static string message;
        static string code;
        static int id_gate;
        static int cont;
        static string access;
        static char key = ' ';

        static void Main(string[] args)
        {
            InitializeConnection();
            getCont();
            _serialPort = new SerialPort();
            _serialPort.PortName = SetPortName(_serialPort.PortName);
            _serialPort.BaudRate = 19200;
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;

            _serialPort.Open();
            readThread = new Thread(Read);
            readKeyThread = new Thread(readKey);
            readThread.Start();
            readKeyThread.Start();
            
            
            readThread.Join();
            _serialPort.Close();

        }
        public static void getCont()
        {
            try
            {
                MySqlCommand cmd = new MySqlCommand("select max(cont) as max_cont from temp_auth;");
                cmd.Connection = conn;
                cont = (int)cmd.ExecuteScalar();
            }
            catch(Exception ex)
            {
                
            }
            finally
            {
            }
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

        public static void readKey()
        {
            key=Console.ReadKey().KeyChar;
        }
        public static void Read()
        {
            
            char ch;

            while (key==' ')
            {
                sw = new StreamWriter("log.txt");
                try
                {
                    code = "";
                    message = "";
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
                    sw.WriteLine(message);
                    
                    for(int i=1;i<15;i++)
                    {
                        if (message[i] == ';')
                            break;
                        code += message[i];
                    }
                    //da inserire codice per estrapolare id_gate per gate multipli
                    id_gate = 14;
                    if(checkPermission())
                    {
                        Console.WriteLine("Access Granted");
                        byte[] b = BitConverter.GetBytes(1);
                        _serialPort.Write(b, 0, 4);

                    }
                    else
                    {
                        Console.WriteLine("Access Denied, pending authorization...");
                        executeCurl(code);
                        while (!checkEntry()) ;
                        byte[] b;
                        switch (access)
                        {
                            case "grant":
                                Console.WriteLine("Authorized.");
                                Console.WriteLine();
                                b = BitConverter.GetBytes(1);
                                _serialPort.Write(b, 0, 4);
                                break;
                            case "deny":
                                Console.WriteLine("Access Denied.");
                                Console.WriteLine();
                                b = BitConverter.GetBytes(2);
                                _serialPort.Write(b, 0, 4);
                                break;
                        }
                        access = "";
                        
                    }
                    
                    System.Threading.Thread.Sleep(1000);
                    
                }
                //catch timeoutexception
                catch (Exception ex)
                {
                   // MessageBox.Show(ex.ToString());
                }
                sw.Close();
            }
        }
        public static void executeCurl(string code)
        {
            Process curl = new Process();
            curl.StartInfo.FileName = "curl.exe";
            curl.StartInfo.CreateNoWindow = true;
            //curl.StartInfo.Arguments = "-X POST -d \"message="+code+"\" https://anzepau.000webhostapp.com/accessgranted_notify/trigger.php";
            curl.StartInfo.Arguments = "-X POST -d \"message=" + code + "\" http://localhost/accessgranted/trigger.php";
            curl.Start();
            

            code = "";

        }

        public static bool checkEntry()
        {
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = "SELECT cont,status FROM accessgranted.temp_auth ORDER BY cont DESC LIMIT 1;";
            cmd.Connection = conn;
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (Convert.ToInt32(reader["cont"]) > cont)
                {
                    access = reader["status"].ToString();
                    cont = Convert.ToInt32(reader["cont"]);
                    reader.Close();
                    return true;
                }
            }
            reader.Close();
            return false;
        }
        public static void InitializeConnection()
        {
            server = "localhost";
            database = "accessgranted";
            uid = "root";
            password = "root";
            string connectionString;
            connectionString = "SERVER=" + server + ";" + "DATABASE=" +
            database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";

            conn = new MySqlConnection(connectionString);
            conn.Open();
        }
        public static bool checkPermission()
        {
            try
            {
                int count = 0; ;
                MySqlCommand cmd = new MySqlCommand();
                cmd.CommandText = "select count(*) C from autorizzazione JOIN gate JOIN utente on fk_idgate=id_gate AND fk_idutente=id_utente and id_utente='" + code + "' and id_gate=" + id_gate + ";";
                cmd.Connection = conn;
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    count = Convert.ToInt32(reader["c"]);
                }
                reader.Close();
                if (count == 0)
                    return false;
                else
                    return true;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return false;
            
        }
        
    }
    
}
