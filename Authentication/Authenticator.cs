using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

namespace EASControlPanel.Authentication {
    public class Authenticator {
        public static string MFAKeyPath = $@"{Environment.GetEnvironmentVariable("appdata")}\ECP\_2fa";
        public static string MFAKeyDir = $@"{Environment.GetEnvironmentVariable("appdata")}\ECP";
        public static async Task New() {
            Directory.CreateDirectory($@"{MFAKeyPath}\..");
            File.Create(MFAKeyPath)
                .Close();
            XmlDocument pairBodyX = new XmlDocument();
            XmlDocument verifyBodyX = new XmlDocument();
            string verificationCode = null;
            string pairCode = null;

            Console.WriteLine("First launch detected! Please input a SECURE KEY to use for API authentication. You don't need to remember this.");
            string apiKey = Console.ReadLine();
            Console.WriteLine("Thank you! Please wait while the program registers and stores your code.");
            try {
                File.WriteAllText(MFAKeyPath, apiKey);
                HttpResponseMessage pairBody = await Program.httpClient.GetAsync($"https://authenticatorapi.com/api.asmx/Pair?appName=CTECP&appInfo=2021%20Chris%20Thorpe&secretCode={apiKey}");
                string pairResponse = await pairBody.Content.ReadAsStringAsync();
                pairBodyX.LoadXml(pairResponse);
                pairCode = pairBodyX.SelectSingleNode("ManualSetupCode").ToString();
            } catch (Exception e) {
                Console.WriteLine(e.Message);
                File.Delete(MFAKeyPath);
                Directory.Delete(MFAKeyDir);
                Environment.Exit(1);
            }
            Console.WriteLine($"Success! Key written to file. Now, open your authenticator app of choice and enter this code:");
            Console.WriteLine(pairCode);
            Console.WriteLine("Once you have successfully paired the app, type the 6-digit PIN below.");

            while (true) {
                verificationCode = Console.ReadLine();
                HttpResponseMessage verifyBody = await Program.httpClient.GetAsync($"https://authenticatorapi.com/api.asmx/ValidatePin?Pin={verificationCode}&secret={File.ReadAllText(MFAKeyPath)}");
                string verifyReponse = await verifyBody.Content.ReadAsStringAsync();
                verifyBodyX.LoadXml(verifyReponse);
                bool preCodeValid = Convert.ToBoolean(verifyBodyX.SelectSingleNode("boolean").ToString());
                if (preCodeValid) {
                    break;
                } else {
                    Console.WriteLine("That PIN didn't work! Double-check that you paired.");
                }
            }
        }

        public static async Task Authenticate() {
            string verificationCode = null;
            while (true) {
                XmlDocument verifyBodyX = new XmlDocument();
                verificationCode = Console.ReadLine();
                HttpResponseMessage verifyBody = await Program.httpClient.GetAsync($"https://authenticatorapi.com/api.asmx/ValidatePin?Pin={verificationCode}&secret={File.ReadAllText(MFAKeyPath)}");
                string verifyReponse = await verifyBody.Content.ReadAsStringAsync();
                verifyBodyX.LoadXml(verifyReponse);
                bool preCodeValid = Convert.ToBoolean(verifyBodyX.SelectSingleNode("boolean").ToString());
                if (preCodeValid) {
                    break;
                } else {
                    Console.WriteLine("Invalid pin.");
                }
            }
        }
    }
}
