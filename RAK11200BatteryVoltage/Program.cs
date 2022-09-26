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
// https://docs.rakwireless.com/Product-Categories/WisBlock/RAK11200
//
// https://docs.rakwireless.com/Product-Categories/WisBlock/RAK5005-O
//
// https://docs.rakwireless.com/Product-Categories/WisBlock/RAK19001/
//
namespace devMobile.IoT.RAK.Wisblock.RAK11200BatteryVoltage
{
   using System;
   using System.Device.Gpio;
   using System.Threading;
   using System.Device.Adc;
   using nanoFramework.Hardware.Esp32;
   using System.Diagnostics;
   using System.Globalization;

   public class Program
   {
      private static GpioController s_GpioController;
      private static AdcController s_AdcController;

      public static void Main()
      {
         s_GpioController = new GpioController();

         // RAK11200 on RAK19007
         GpioPin ledGreen = s_GpioController.OpenPin(Gpio.IO12, PinMode.Output); // LED1 Green
         GpioPin ledBlue = s_GpioController.OpenPin(Gpio.IO02, PinMode.Output); // LED2 Blue

         s_AdcController = new AdcController();

         Debug.WriteLine($"min:{s_AdcController.MinValue} max:{s_AdcController.MaxValue}");

         AdcChannel ac0 = s_AdcController.OpenChannel(0);

         int batteryVoltageInitial = ac0.ReadValue();
         Debug.WriteLine($"BatteryVoltage Initial:{batteryVoltageInitial}");

         ledGreen.Write(PinValue.Low);
         ledBlue.Write(PinValue.Low);

         while (true)
         {
            int batteryVoltage = ac0.ReadValue();
            Debug.WriteLine($"BatteryVoltage Current:{batteryVoltage}");

            ledGreen.Write(PinValue.High);
            ledBlue.Write(PinValue.High);

            for (int i = 0; i < batteryVoltageInitial; i++)
            {
               if (batteryVoltage < i)
               {
                  ledBlue.Write(PinValue.Low);
               }

               Thread.Sleep(5);
            }
            ledGreen.Write(PinValue.Low);
            ledBlue.Write(PinValue.Low);

            Thread.Sleep(10000);
         }
      }
   }
}
