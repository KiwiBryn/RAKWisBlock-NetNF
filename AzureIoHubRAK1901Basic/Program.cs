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
// https://docs.rakwireless.com/Product-Categories/WisBlock/RAK2305
//
// https://docs.rakwireless.com/Product-Categories/WisBlock/RAK11200
//
// https://store.rakwireless.com/products/rak1901-shtc3-temperature-humidity-sensor
//
// https://github.com/nanoframework/nanoFramework.IoT.Device/tree/develop/devices/Shtc3
//
//---------------------------------------------------------------------------------
namespace devMobile.IoT.RAK.Wisblock.AzureIoHub.RAK1901
{
    using System;
    using System.Device.I2c;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;

    using Iot.Device.Shtc3;

    using nanoFramework.Hardware.Esp32;
    using nanoFramework.Json;
    using nanoFramework.Networking;

    public class Program
    {
        private static HttpClient _httpClient;

        public static void Main()
        {
            Debug.WriteLine("devMobile.IoT.RAK.Wisblock.AzureIoHub.RAK1901 starting");

            Configuration.SetPinFunction(Gpio.IO04, DeviceFunction.I2C1_DATA);
            Configuration.SetPinFunction(Gpio.IO05, DeviceFunction.I2C1_CLOCK);

            if (!WifiNetworkHelper.ConnectDhcp(Config.Ssid, Config.Password, requiresDateTime: true))
            {
                if (NetworkHelper.HelperException != null)
                {
                    Debug.WriteLine($"WifiNetworkHelper.ConnectDhcp failed {NetworkHelper.HelperException}");
                }

                Thread.Sleep(Timeout.Infinite);
            }

            _httpClient = new HttpClient
            {
                SslProtocols = System.Net.Security.SslProtocols.Tls12,
                HttpsAuthentCert = new X509Certificate(Config.DigiCertBaltimoreCyberTrustRoot),
                BaseAddress = new Uri($"https://{Config.AzureIoTHubHostName}.azure-devices.net/devices/{Config.DeviceID}/messages/events?api-version=2020-03-13"),
            };
            _httpClient.DefaultRequestHeaders.Add("Authorization", Config.SasKey);

            I2cConnectionSettings settings = new(1, Shtc3.DefaultI2cAddress);
            I2cDevice device = I2cDevice.Create(settings);
            Shtc3 shtc3 = new(device);

            while (true)
            {
                if (shtc3.TryGetTemperatureAndHumidity(out var temperature, out var relativeHumidity))
                {
                    Debug.WriteLine($"Temperature {temperature.DegreesCelsius:F1}°C  Humidity {relativeHumidity.Value:F0}%");

                    TelemetryPayload telemetryPayload = new()
                    {
                        Temperature = temperature.DegreesCelsius.ToString("F1"),
                        RelativeHumidity = relativeHumidity.Value.ToString("F0"),
                    };

                    string output = JsonConvert.SerializeObject(telemetryPayload);

                    StringContent content = new StringContent(output);

                    try
                    {
                        var response = _httpClient.Post("", content);

                        response.EnsureSuccessStatusCode();
                    }
                    catch(Exception ex)
                    {
                        Debug.WriteLine($"Azure IoT Hub POST failed:{ex.Message}");
                    }
                }

                Thread.Sleep(30 * 60 * 1000);
            }
        }
    }

    internal class TelemetryPayload
    {
        public string Temperature { get; set; }
        public string RelativeHumidity { get; set; }
    }
}
