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
// https://store.rakwireless.com/products/rak1901-shtc3-temperature-humidity-sensor
//
// https://github.com/nanoframework/nanoFramework.IoT.Device/tree/develop/devices/Shtc3
//
// Builds on 
// https://github.com/KiwiBryn/RAKWisBlock-NetNF/tree/master/AzureIoHubRAK1901HttpSasKey
//
//---------------------------------------------------------------------------------
namespace AzureIoTHubRAK11200PowerConservation
{
   using System;
   using System.Diagnostics;
   using System.Threading;

   public class Program
   {
      public static void Main()
      {
         Debug.WriteLine("Hello from nanoFramework!");

         Thread.Sleep(Timeout.Infinite);
      }
   }
}
