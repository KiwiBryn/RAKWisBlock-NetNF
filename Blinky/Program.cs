//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//
//
using System.Device.Gpio;
using System;
using System.Threading;
using nanoFramework.Hardware.Esp32;

namespace Blinky
{
    public class Program
    {
        private static GpioController s_GpioController;
        public static void Main()
        {
            s_GpioController = new GpioController();

            // RAK11200
            GpioPin led = s_GpioController.OpenPin(Gpio.IO12, PinMode.Output); // LED1 Green
            //GpioPin led = s_GpioController.OpenPin(Gpio.IO02, PinMode.Output); // LED2 Blue

            // RAK2305 Maybe

            led.Write(PinValue.Low);

            while (true)
            {
                led.Toggle();
                Thread.Sleep(125);
                led.Toggle();
                Thread.Sleep(125);
                led.Toggle();
                Thread.Sleep(125);
                led.Toggle();
                Thread.Sleep(525);
            }
        }
    }
}
