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
// https://docs.rakwireless.com/Product-Categories/WisBlock/RAK1901
//
// https://github.com/nanoframework/nanoFramework.IoT.Device/tree/develop/devices/Shtc3
//
// Builds on 
// https://github.com/KiwiBryn/RAKWisBlock-NetNF/tree/master/AzureIoTHubRAK11200PowerBaseline
//
// nanoff --platform esp32 --serialport COM29 --update
//
//  To configure how the device sleeps one of the following should be defined.
//  SLEEP_LIGHT
//      OR
//  SLEEP_DEEP
//
//  SLEEP_SHT3C
//
//---------------------------------------------------------------------------------
//#define SLEEP_LIGHT
#define SLEEP_DEEP
//#define SLEEP_SHT3C
namespace devMobile.IoT.RAK.Wisblock.AzureIoTHub.RAK11200.PowerSleep
{
    using System;
    using System.Device.Adc;
    using System.Device.I2c;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading;
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

            Thread.Sleep(5000);

            try
            {
                double batteryVoltage;

                Configuration.SetPinFunction(Gpio.IO04, DeviceFunction.I2C1_DATA);
                Configuration.SetPinFunction(Gpio.IO05, DeviceFunction.I2C1_CLOCK);

                Debug.WriteLine($"{DateTime.UtcNow:HH:mm:ss} Battery voltage measurement");

                // Configure Analog input (AIN0) port then read the "battery charge"
                AdcController adcController = new AdcController();

                using (AdcChannel batteryVoltageAdcChannel = adcController.OpenChannel(AdcControllerChannel))
                {

                    // https://forum.rakwireless.com/t/custom-li-ion-battery-voltage-calculation-in-rak4630/4401/7
                    // When I checked with multimeter I had to increase 1.72 to 1.9
                    batteryVoltage = batteryVoltageAdcChannel.ReadValue() * (3.0 / 4096) * 1.9;

                    Debug.WriteLine($" BatteryVoltage {batteryVoltage:F2}");

                    if (batteryVoltage < Config.BatteryVoltageBrownOutThreshold)
                    {
                        Sleep.EnableWakeupByTimer(Config.FailureRetryInterval);
                        Sleep.StartDeepSleep();
                    }
                }

                Debug.WriteLine($"{DateTime.UtcNow:HH:mm:ss} Wifi connecting");

                if (!WifiNetworkHelper.ConnectDhcp(Config.Ssid, Config.Password, requiresDateTime: true))
                {
                    if (NetworkHelper.HelperException != null)
                    {
                        Debug.WriteLine($"{DateTime.UtcNow:HH:mm:ss} WifiNetworkHelper.ConnectDhcp failed {NetworkHelper.HelperException}");
                    }

                    Sleep.EnableWakeupByTimer(Config.FailureRetryInterval);
                    Sleep.StartDeepSleep();
                }
                Debug.WriteLine($"{DateTime.UtcNow:HH:mm:ss} Wifi connected");

                // Configure the SHTC3 
                I2cConnectionSettings settings = new(I2cDeviceBusID, Shtc3.DefaultI2cAddress);

                string payload ;

                using (I2cDevice device = I2cDevice.Create(settings))
                using (Shtc3 shtc3 = new(device))
                {
                    if (shtc3.TryGetTemperatureAndHumidity(out var temperature, out var relativeHumidity))
                    {
                        Debug.WriteLine($" Temperature {temperature.DegreesCelsius:F1}°C Humidity {relativeHumidity.Value:F0}% BatteryVoltage {batteryVoltage:F2}");

                        payload = $"{{\"RelativeHumidity\":{relativeHumidity.Value:F0},\"Temperature\":{temperature.DegreesCelsius:F1}, \"BatteryVoltage\":{batteryVoltage:F2}}}";
                    }
                    else
                    {
                        Debug.WriteLine($" BatteryVoltage {batteryVoltage:F2}");

                        payload = $"{{\"BatteryVoltage\":{batteryVoltage:F2}}}";
                    }

#if SLEEP_SHT3C
                    shtc3.Sleep();
#endif
                }

                // Configure the HttpClient uri, certificate, and authorization
                string uri = $"{Config.AzureIoTHubHostName}.azure-devices.net/devices/{Config.DeviceID}";

                HttpClient httpClient = new HttpClient()
                {
                    SslProtocols = System.Net.Security.SslProtocols.Tls12,
                    HttpsAuthentCert = new X509Certificate(Config.DigiCertBaltimoreCyberTrustRoot),
                    BaseAddress = new Uri($"https://{uri}/messages/events?api-version=2020-03-13"),
                };
                httpClient.DefaultRequestHeaders.Add("Authorization", SasTokenGenerate(uri, Config.Key, DateTime.UtcNow.Add(Config.SasTokenRenewFor)));

                Debug.WriteLine($"{DateTime.UtcNow:HH:mm:ss} Azure IoT Hub device {Config.DeviceID} telemetry update start");

                HttpResponseMessage response = httpClient.Post("", new StringContent(payload));

                Debug.WriteLine($"{DateTime.UtcNow:HH:mm:ss} Response code:{response.StatusCode}");

                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{DateTime.UtcNow:HH:mm:ss} Azure IoT Hub telemetry update failed:{ex.Message} {ex?.InnerException?.Message}");

                Sleep.EnableWakeupByTimer(Config.FailureRetryInterval);
                Sleep.StartDeepSleep();
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