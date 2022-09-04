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
// https://store.rakwireless.com/products/rak1901-shtc3-temperature-humidity-sensor
//
// https://github.com/nanoframework/nanoFramework.IoT.Device/tree/develop/devices/Shtc3
//
//---------------------------------------------------------------------------------
namespace devMobile.IoT.RAK.Wisblock.Sht3C
{
   using System;
   using System.Diagnostics;
   using System.Device.I2c;
   using System.Threading;

   using nanoFramework.Hardware.Esp32;

   using Iot.Device.Shtc3;

   public class Program
   {
      public static void Main()
      {
         Debug.WriteLine("devMobile.IoT.RAK.Wisblock.SHTC3 starting");

         try
         {
            // RAK11200 & RAK2305
            Configuration.SetPinFunction(Gpio.IO04, DeviceFunction.I2C1_DATA);
            Configuration.SetPinFunction(Gpio.IO05, DeviceFunction.I2C1_CLOCK);

            I2cConnectionSettings settings = new(1, Shtc3.DefaultI2cAddress);

            using (I2cDevice device = I2cDevice.Create(settings))
            using (Shtc3 shtc3 = new(device))
            {
               while (true)
               {
                  if (shtc3.TryGetTemperatureAndHumidity(out var temperature, out var relativeHumidity))
                  {
                     Debug.WriteLine($"Temperature {temperature.DegreesCelsius:F1}°C  Humidity {relativeHumidity.Value:F0}%");
                  }

                  Thread.Sleep(10000);
               }
            }
         }
         catch (Exception ex)
         {
            Debug.WriteLine($"SHTC3 initialisation or read failed {ex.Message}");

            Thread.Sleep(Timeout.Infinite);
         }
      }
   }
}
