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
// RAK Core WisBlock
// https://store.rakwireless.com/products/wiscore-esp32-module-rak11200
//
// RAK WisBlock Base
// https://store.rakwireless.com/products/rak5005-o-base-board
//
// RAK WisBlock Sensor
// https://store.rakwireless.com/products/rak1910-max-7q-gnss-location-sensor
//
// Uses the library
// https://github.com/mboud/TinyGPSPlusNF
//
// Inspired by
// https://github.com/RAKWireless/WisBlock/tree/master/examples/common/sensors/RAK1910_GPS_UBLOX7
//
// Flash device with
// nanoff --target ESP32_REV0 --serialport COM16 --update
//
//---------------------------------------------------------------------------------
namespace devMobile.IoT.RAK.Wisblock.Max7Q
{
    using System.Device.Gpio;
    using System.Diagnostics;
    using System.IO.Ports;
    using System.Threading;

    using nanoFramework.Hardware.Esp32;
    using TinyGPSPlusNF;

    public class Program
    {
        private static TinyGPSPlus _gps;

        public static void Main()
        {
            Debug.WriteLine($"devMobile.IoT.RAK.Wisblock.Max7Q starting TinyGPS {TinyGPSPlus.LibraryVersion}");

            Configuration.SetPinFunction(Gpio.IO21, DeviceFunction.COM2_TX);
            Configuration.SetPinFunction(Gpio.IO19, DeviceFunction.COM2_RX);

            _gps = new TinyGPSPlus();

            // UART1 with default Max7Q baudrate
            SerialPort serialPort = new SerialPort("COM2", 9600);

            serialPort.DataReceived += SerialDevice_DataReceived;
            serialPort.Open();
            serialPort.WatchChar = '\n';

            // Turn on GPS power GPS_3V3 - 3V3_S - IO2 - GPIO27
            GpioController gpioController = new GpioController();

            GpioPin Gps3V3 = gpioController.OpenPin(Gpio.IO27, PinMode.Output);
            Gps3V3.Write(PinValue.High);

            Debug.WriteLine("Waiting...");

            Thread.Sleep(Timeout.Infinite);
        }

        private static void SerialDevice_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // we only care if got EoL character
            if (e.EventType != SerialData.WatchChar)
            {
                return;
            }

            SerialPort serialDevice = (SerialPort)sender;

            string sentence = serialDevice.ReadExisting();

            if (_gps.Encode(sentence))
            {
                if (_gps.Date.IsValid)
                {
                    Debug.Write($"{_gps.Date.Year}-{_gps.Date.Month:D2}-{_gps.Date.Day:D2} ");
                }

                if (_gps.Time.IsValid)
                {
                    Debug.Write($"{_gps.Time.Hour:D2}:{_gps.Time.Minute:D2}:{_gps.Time.Second:D2}.{_gps.Time.Centisecond:D2} ");
                }

                if (_gps.Location.IsValid)
                {
                    Debug.Write($"Lat:{_gps.Location.Latitude.Degrees:F5}° Lon:{_gps.Location.Longitude.Degrees:F5}° ");
                }

                if (_gps.Altitude.IsValid)
                {
                    Debug.Write($"Alt:{_gps.Altitude.Meters:F1}M");
                }

                if (_gps.Date.IsValid || _gps.Time.IsValid || _gps.Location.IsValid || _gps.Altitude.IsValid)
                {
                    Debug.WriteLine("");
                }
            }
        }
    }
}
