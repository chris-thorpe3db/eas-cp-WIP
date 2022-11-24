using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

using EASControlPanel;

namespace EAS_control_panel.Authentication {
    public class Authenticator {
        static async Task NewAuthenticator() {
            File.Create(MFAKeyPath);
            XmlDocument pairBodyX = new XmlDocument();
            XmlDocument verifyBodyX = new XmlDocument();
            string verificationCode = null;


            Console.WriteLine("First launch detected! Please input a SECURE KEY to use for API authentication. You don't need to remember this.");
            string apiKey = Console.ReadLine();
            Console.WriteLine("Thank you! Please wait while the program registers and stores your code.");

            File.WriteAllText(MFAKeyPath, apiKey);
            HttpResponseMessage pairBody = await client.GetAsync($"https://authenticatorapi.com/api.asmx/Pair?appName=CTECP&appInfo=2021%20Chris%20Thorpe&secretCode={apiKey}");
            string pairResponse = await pairBody.Content.ReadAsStringAsync();
            pairBodyX.LoadXml(pairResponse);
            string pairCode = pairBodyX.SelectSingleNode("ManualSetupCode").ToString();

            Console.WriteLine($"Success! Key written to file. Now, open your authenticator app of choice and enter this code:");
            Console.WriteLine(pairCode);
            Console.WriteLine("Once you have successfully paired the app, type the 6-digit PIN below.");

            while (true) {
                verificationCode = Console.ReadLine();
                HttpResponseMessage verifyBody = await client.GetAsync($"https://authenticatorapi.com/api.asmx/ValidatePin?Pin={verificationCode}&secret={File.ReadAllText(MFAKeyPath)}");
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
    }
}
