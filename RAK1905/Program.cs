//---------------------------------------------------------------------------------
// Copyright (c) November 2022, devMobile Software
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
// https://docs.rakwireless.com/Product-Categories/WisBlock/RAK2305
//
// https://docs.rakwireless.com/Product-Categories/WisBlock/RAK11200
//
// https://store.rakwireless.com/products/9dof-motion-sensor-tdk-mpu9250-rak1905
//
// The .NET nanoFramework team have put a lot of work into supporting many devices this
// "inspired" by one of their samples.
// https://github.com/nanoframework/nanoFramework.IoT.Device/tree/develop/devices/Mpu9250
//
// The nanoFramework library has code for calibration and "wake on shake" which I
// intentionall left out to keep sample short.
//---------------------------------------------------------------------------------
namespace devMobile.IoT.RAK.Wisblock.RAK1905
{
    using System;
    using System.Diagnostics;
    using System.Device.I2c;
    using System.Numerics;
    using System.Threading;

    using nanoFramework.Hardware.Esp32;

    using Iot.Device.Imu;
    using UnitsNet;


    public class Program
    {
        public static void Main()
        {
            Debug.WriteLine("devMobile.IoT.RAK.Wisblock.RAK11200RAK19001RAK1901 starting");

            try
            {
                // RAK11200 & RAK2305
                Configuration.SetPinFunction(Gpio.IO04, DeviceFunction.I2C1_DATA);
                Configuration.SetPinFunction(Gpio.IO05, DeviceFunction.I2C1_CLOCK);

                I2cConnectionSettings settings = new(1, Mpu9250.DefaultI2cAddress);

                using (I2cDevice device = I2cDevice.Create(settings))
                using (Mpu9250 mpu9250 = new(device))
                {
                    while (true)
                    {
                        Temperature temperature = mpu9250.GetTemperature();
                        Debug.WriteLine($"Temperature {temperature.DegreesCelsius:F1} °C");

                        Vector3 gyroscope = mpu9250.GetGyroscopeReading();
                        Debug.WriteLine("Rotation");
                        Debug.WriteLine($"Gyro X = {gyroscope.X,15}");
                        Debug.WriteLine($"Gyro Y = {gyroscope.Y,15}");
                        Debug.WriteLine($"Gyro Z = {gyroscope.Z,15}");

                        Vector3 accelerometer = mpu9250.GetAccelerometer();
                        Debug.WriteLine("Acceleration");
                        Debug.WriteLine($" X = {accelerometer.X,15}");
                        Debug.WriteLine($" Y = {accelerometer.Y,15}");
                        Debug.WriteLine($" Z = {accelerometer.Z,15}");

                        Vector3 magnetometer = mpu9250.ReadMagnetometer(true);
                        Debug.WriteLine("Magnetism");
                        Debug.WriteLine($" X = {magnetometer.X,15}");
                        Debug.WriteLine($" Y = {magnetometer.Y,15}");
                        Debug.WriteLine($" Z = {magnetometer.Z,15}");

                        Thread.Sleep(10000);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MPU9250 initialisation or read failed {ex.Message}");

                Thread.Sleep(Timeout.Infinite);
            }
        }
    }
}
