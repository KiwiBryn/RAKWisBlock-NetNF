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
// https://store.rakwireless.com/products/rak1920-sensor-adapter-module
//
// https://github.com/nanoframework/nanoFramework.IoT.Device/tree/develop/devices/Si7021
//
// https://www.seeedstudio.com/Grove-Temperature-Humidity-Sensor-High-Accuracy-Mini.html
//
//---------------------------------------------------------------------------------
namespace devMobile.IoT.RAK.Wisblock.RAK11200.SHT31
{
    using System;
    using System.Device.I2c;
    using System.Diagnostics;
    using System.Threading;

    using Iot.Device.Sht3x;
    using nanoFramework.Hardware.Esp32;

    public class Program
    {
        public static void Main()
        {
            Debug.WriteLine("devMobile.IoT.RAK.Wisblock.SHT31 starting");

            try
            {
                Configuration.SetPinFunction(Gpio.IO04, DeviceFunction.I2C1_DATA);
                Configuration.SetPinFunction(Gpio.IO05, DeviceFunction.I2C1_CLOCK);

                I2cConnectionSettings settings = new(1, (byte)I2cAddress.AddrLow);

                using (I2cDevice device = I2cDevice.Create(settings))
                using (Sht3x sht31 = new(device))
                {

                    while (true)
                    {
                        var temperature = sht31.Temperature;
                        var relativeHumidity = sht31.Humidity;

                        Debug.WriteLine($"Temperature {temperature.DegreesCelsius:F1}°C  Humidity {relativeHumidity.Value:F0}%");

                        Thread.Sleep(10000);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SHT31 initialisation or read failed {ex.Message}");

                Thread.Sleep(Timeout.Infinite);
            }
        }
    }
}
