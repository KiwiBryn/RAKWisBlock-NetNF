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
// https://docs.rakwireless.com/Product-Categories/WisBlock/RAK19007
//
// https://store.rakwireless.com/products/rak1901-shtc3-temperature-humidity-sensor
//
// https://github.com/nanoframework/nanoFramework.IoT.Device/tree/develop/devices/Shtc3
//
// Builds on 
// https://github.com/KiwiBryn/RAKWisBlock-NetNF/tree/master/AzureIoTHubRAK11200PowerBaseline
//
//  To configure how the device sleeps one of the following should be defined.
//  SLEEP_LIGHT
//      OR
//  SLEEP_DEEP
//
//---------------------------------------------------------------------------------
#define SLEEP_LIGHT
//#define SLEEP_DEEP
namespace devMobile.IoT.RAK.Wisblock.AzureIoTHub.RAK11200.PowerCpuSleep
{
    using System;
    using System.Device.Adc;
    using System.Device.I2c;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Web;

    using Iot.Device.Shtc3;

    using ElzeKool;

    using nanoFramework.Hardware.Esp32;
    using nanoFramework.Networking;

    public class Program
    {
        private const int I2cDeviceBusID = 1;
        private const int AdcControllerChannel = 0;

        public static void Main()
        {
            Debug.WriteLine($"{DateTime.UtcNow:HH:mm:ss} devMobile.IoT.RAK.Wisblock.AzureIoTHub.RAK11200.PowerSleep starting");

            Configuration.SetPinFunction(Gpio.IO04, DeviceFunction.I2C1_DATA);
            Configuration.SetPinFunction(Gpio.IO05, DeviceFunction.I2C1_CLOCK);

            try
            {
                if (!WifiNetworkHelper.ConnectDhcp(Config.Ssid, Config.Password, requiresDateTime: true))
                {
                    if (NetworkHelper.HelperException != null)
                    {
                        Debug.WriteLine($"{DateTime.UtcNow:HH:mm:ss} WifiNetworkHelper.ConnectDhcp failed {NetworkHelper.HelperException}");
                    }

                    Sleep.EnableWakeupByTimer(Config.FailureRetryInterval);
                    Sleep.StartLightSleep();
                }

                string uri = $"{Config.AzureIoTHubHostName}.azure-devices.net/devices/{Config.DeviceID}";

                HttpClient httpClient = new HttpClient()
                {
                    SslProtocols = System.Net.Security.SslProtocols.Tls12,
                    HttpsAuthentCert = new X509Certificate(Config.DigiCertBaltimoreCyberTrustRoot),
                    BaseAddress = new Uri($"https://{uri}/messages/events?api-version=2020-03-13"),
                };
                httpClient.DefaultRequestHeaders.Add("Authorization", SasTokenGenerate(uri, Config.Key, DateTime.UtcNow.Add(Config.SasTokenRenewFor)));

                I2cConnectionSettings settings = new(I2cDeviceBusID, Shtc3.DefaultI2cAddress);
                I2cDevice device = I2cDevice.Create(settings);
                Shtc3 shtc3 = new(device);

                AdcController adcController = new AdcController();
                AdcChannel batteryChargeAdcChannel = adcController.OpenChannel(AdcControllerChannel);

                Debug.WriteLine($"{DateTime.UtcNow:HH:mm:ss} Azure IoT Hub device {Config.DeviceID} telemetry update start");

                // Might not need this if just let it reference go pop then dealt with by try catch
                if (!shtc3.TryGetTemperatureAndHumidity(out var temperature, out var relativeHumidity))
                {
                    Debug.WriteLine($" Temperature and Humidity read failed");

                    Sleep.EnableWakeupByTimer(Config.FailureRetryInterval);
                    Sleep.StartLightSleep();
                }

                double batteryCharge = batteryChargeAdcChannel.ReadRatio() * 100.0;

                Debug.WriteLine($" Temperature {temperature.DegreesCelsius:F1}�C Humidity {relativeHumidity.Value:F0}% BatteryCharge {batteryCharge:F1}");

                string payload = $"{{\"RelativeHumidity\":{relativeHumidity.Value:F0},\"Temperature\":{temperature.DegreesCelsius.ToString("F1")}, \"BatteryCharge\":{batteryCharge:F1}}}";

                using (HttpContent content = new StringContent(payload))
                {
                    using (HttpResponseMessage response = httpClient.Post("", content))
                    {
                        Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss} Response code:{response.StatusCode}");

                        response.EnsureSuccessStatusCode();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{DateTime.UtcNow:HH:mm:ss} Azure IoT Hub telemetry update failed:{ex.Message} {ex?.InnerException?.Message}");

                Sleep.EnableWakeupByTimer(Config.FailureRetryInterval);
                Sleep.StartLightSleep();
            }

            Sleep.EnableWakeupByTimer(Config.TelemetryUploadInterval);
#if SLEEP_LIGHT
            Sleep.StartLightSleep();
#endif
#if SLEEP_DEEP
           Sleep.StartDeepSleep();
#endif
        }

        public static string SasTokenGenerate(string resourceUri, string key, DateTime sasKeyTokenUntilUtc)
        {
            long sasKeyvalidUntilUtcUnix = sasKeyTokenUntilUtc.ToUnixTimeSeconds();

            string stringToSign = $"{HttpUtility.UrlEncode(resourceUri)}\n{sasKeyvalidUntilUtcUnix}";

            var hmac = SHA.computeHMAC_SHA256(Convert.FromBase64String(key), Encoding.UTF8.GetBytes(stringToSign));

            string signature = Convert.ToBase64String(hmac);

            return $"SharedAccessSignature sr={HttpUtility.UrlEncode(resourceUri)}&sig={HttpUtility.UrlEncode(signature)}&se={sasKeyvalidUntilUtcUnix}";
        }
    }
}