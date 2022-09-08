//---------------------------------------------------------------------------------
// Copyright (c) September 2022, devMobile Software
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
// https://store.rakwireless.com/products/wiscore-esp32-module-rak11200
//
// https://store.rakwireless.com/products/rak1906-bme680-environment-sensor
//
// https://store.rakwireless.com/products/rak19007-wisblock-base-board-2nd-gen
//
// https://github.com/nanoframework/nanoFramework.IoT.Device/tree/develop/devices/Bmxx80
//
//---------------------------------------------------------------------------------
namespace devMobile.IoT.RAK.Wisblock.RAK1906
{
   using System;
   using System.Device.I2c;
   using System.Diagnostics;
   using System.Threading;

   using Iot.Device.Bmxx80;
   using Iot.Device.Common;
   using nanoFramework.Hardware.Esp32;
   using UnitsNet;

   public class Program
   {
      public static void Main()
      {
         Debug.WriteLine("devMobile.IoT.RAK.Wisblock.BME680 starting");

         Thread.Sleep(5000);

         try
         {
            Configuration.SetPinFunction(Gpio.IO04, DeviceFunction.I2C1_DATA);
            Configuration.SetPinFunction(Gpio.IO05, DeviceFunction.I2C1_CLOCK);

            // set this to the current sea level pressure in the area for correct altitude readings
            Pressure defaultSeaLevelPressure = WeatherHelper.MeanSeaLevel;

            I2cConnectionSettings i2cSettings = new(1, Bme680.DefaultI2cAddress);
            I2cDevice i2cDevice = I2cDevice.Create(i2cSettings);

            using (I2cDevice device = I2cDevice.Create(i2cSettings))
            {
               using (Bme680 bme680 = new Bme680(i2cDevice))
               {
                  bme680.Reset();

                  while (true)
                  {
                     // Perform a synchronous measurement
                     var readResult = bme680.Read();

                     // Print out the measured data
                     Debug.WriteLine($"Temperature {readResult.Temperature.DegreesCelsius:F1}°C  Humidity {readResult.Humidity.Value:F0}%  Pressure: {readResult.Pressure.Hectopascals:F1}hPa  GasResistance: {readResult.GasResistance.Value:F1} ");

                     Thread.Sleep(10000);
                  }
               }
            }
         }
         catch (Exception ex)
         {
            Debug.WriteLine($"BME680 initialisation or read failed {ex.Message}");

            Thread.Sleep(Timeout.Infinite);
         }
      }
   }
}
