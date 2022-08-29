namespace devMobile.IoT.LoRaWAN.nanoFramework.RAK.LoraWAN
{ 
   using System;
   using System.Diagnostics;
   using System.IO.Ports;
   using System.Threading;
   using global::nanoFramework.Hardware.Esp32; //need NuGet nanoFramework.Hardware.Esp32

   public class Program
   {
      private static SerialPort _SerialPort;

      private const string SerialPortId = "COM2";

      public static void Main()
      {
         Debug.WriteLine("devMobile.IoT.LoRaWAN.nanoFramework.RAK.LoraWAN RAK3172/RAK4630 EVB starting");

         try
         {
            // set GPIO functions for COM2 (this is UART1 on ESP32)
            Configuration.SetPinFunction(Gpio.IO21, DeviceFunction.COM2_TX);
            Configuration.SetPinFunction(Gpio.IO19, DeviceFunction.COM2_RX);

            Debug.Write("Ports:");
            foreach (string port in SerialPort.GetPortNames())
            {
               Debug.Write($" {port}");
            }
            Debug.WriteLine("");

            using (_SerialPort = new SerialPort(SerialPortId))
            {
               // set parameters
               _SerialPort.BaudRate = 115200;
               _SerialPort.Parity = Parity.None;
               _SerialPort.DataBits = 8;
               _SerialPort.StopBits = StopBits.One;
               _SerialPort.Handshake = Handshake.None;
               _SerialPort.NewLine = "\r\n";
               _SerialPort.ReadTimeout = 1000;

               //_SerialPort.WatchChar = '\n'; // May 2022 WatchChar event didn't fire github issue https://github.com/nanoframework/Home/issues/1035

               _SerialPort.DataReceived += SerialDevice_DataReceived;

               _SerialPort.Open();

               _SerialPort.WatchChar = '\n';

               _SerialPort.ReadExisting(); // Running at 115K2 this was necessary


               for (int i = 0; i < 5; i++)
               {
                  string atCommand;
                  atCommand = "AT+VER=?";
                  //atCommand = "AT+SN=?"; // Empty response?
                  //atCommand = "AT+HWMODEL=?";
                  //atCommand = "AT+HWID=?";
                  atCommand = "AT+DEVEUI=?";
                  //atCommand = "AT+APPEUI=?";
                  //atCommand = "AT+APPKEY=?";
                  //atCommand = "ATR";
                  //atCommand = "AT+SLEEP=4000";
                  //atCommand = "AT+ATM";
                  Debug.WriteLine("");
                  Debug.WriteLine($"{DateTime.UtcNow:hh:mm:ss} {i} TX:{atCommand} bytes:{atCommand.Length}--------------------------------");
                  _SerialPort.WriteLine(atCommand);

                  Thread.Sleep(5000);
               }
            }
            Debug.WriteLine("Done");
         }
         catch (Exception ex)
         {
            Debug.WriteLine(ex.Message);
         }
      }

      private static void SerialDevice_DataReceived(object sender, SerialDataReceivedEventArgs e)
      {
         SerialPort serialPort = (SerialPort)sender;

         switch (e.EventType)
         {
            case SerialData.Chars:
               break;

            case SerialData.WatchChar:
               string response = serialPort.ReadExisting();
               //Debug.Write($"{DateTime.UtcNow:hh:mm:ss} RX:{response} bytes:{response.Length}");
               Debug.Write(response);
               break;
            default:
               Debug.Assert(false, $"e.EventType {e.EventType} unknown");
               break;
         }
      }

   }
}