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
//
/// NOTE: this demo uses the information outlined in https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-mqtt-support
//
//#define ESP32  // nanoff --target ESP32_REV0 --serialport COM17 --update
//
//---------------------------------------------------------------------------------
using System;
using System.Diagnostics;
using System.Threading;

namespace NFApp1
{
   public class Program
   {
      public static void Main()
      {
         Debug.WriteLine("Hello from nanoFramework!");

         Thread.Sleep(Timeout.Infinite);

         // Browse our samples repository: https://github.com/nanoframework/samples
         // Check our documentation online: https://docs.nanoframework.net/
         // Join our lively Discord community: https://discord.gg/gCyBu8T
      }
   }
}