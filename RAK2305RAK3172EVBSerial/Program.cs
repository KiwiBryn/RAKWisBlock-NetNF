//---------------------------------------------------------------------------------
// Copyright (c) August 2022, devMobile Software
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
//
namespace devMobile.IoT.RAK.Wisblock.RAK2305RAK3172
{
    using System;
    using System.Diagnostics;
    using System.IO.Ports;
    using System.Threading;

    using nanoFramework.Hardware.Esp32;

    public class Program
    {
        private static SerialPort _SerialPort;

        private const string SerialPortId = "COM3";

        public static void Main()
        {
            Debug.WriteLine("devMobile.IoT.RAK.Wisblock.RAK3172 EVB starting");

            Thread.Sleep(5000);

            try
            {
                // set GPIO functions for COM2 (this is UART1 on RAK2305)
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
                        //atCommand = "AT+DEVEUI=?";
                        //atCommand = "AT+APPEUI=?";
                        //atCommand = "AT+APPKEY=?";
                        //atCommand = "ATR";
                        //atCommand = "AT+SLEEP=4000";
                        //atCommand = "AT+ATM";
                        //atCommand = "AT?"; // Needed an AT Command with a really long response.
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