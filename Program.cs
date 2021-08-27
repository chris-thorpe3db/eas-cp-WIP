using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace EAS_Control_Panel
{
    class Program
    {
        static readonly HttpClient client = new HttpClient();
        public static string MFAKeyPath = $@"{Environment.GetEnvironmentVariable("appdata")}\EPC\_2fa";
        static void Main(string[] args)
        {
            string nl = Environment.NewLine;
            string title = $" _____ _____ _____    _____ _____ _____ _____ _____ _____ __       _____ _____ _____ _____ __    {nl}|   __|  _  |   __|  |     |     |   | |_   _| __  |     |  |     |  _  |  _  |   | |   __|  |   {nl}|   __|     |__   |  |   --|  |  | | | | | | |    -|  |  |  |__   |   __|     | | | |   __|  |__ {nl}|_____|__|__|_____|  |_____|_____|_|___| |_| |__|__|_____|_____|  |__|  |__|__|_|___|_____|_____|{nl}";
            foreach (Char c in title)
            {
                Console.Write(c);
                Thread.Sleep(1);
            }
            Console.WriteLine();
            Console.WriteLine("UNAUTHORIZED ACCESS WILL RESULT IN FINES IN EXCESS OF $50,000 AND UP TO 5 YEARS IMPRISONMENT.");
            Thread.Sleep(10);
            Console.WriteLine("DO NOT PROCEED UNLESS YOU HAVE PROPER CREDENTIALS. PRESS ANY KEY TO CONTINUE.");
            Console.ReadKey();
            if (File.Exists(Environment.GetEnvironmentVariable("appdata") + @"\ECP\_2fa") == false)
            {
                NewAuthenticator();
            } else {
                Authenticate();
            }
            
        }

        static async Task NewAuthenticator()
        {
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

            while (true) 
            {
                verificationCode = Console.ReadLine();
                HttpResponseMessage verifyBody = await client.GetAsync($"https://authenticatorapi.com/api.asmx/ValidatePin?Pin={verificationCode}&secret={File.ReadAllText(MFAKeyPath)}");
                string verifyReponse = await verifyBody.Content.ReadAsStringAsync();
                verifyBodyX.LoadXml(verifyReponse);
                bool preCodeValid = Convert.ToBoolean(verifyBodyX.SelectSingleNode("boolean").ToString());
                if (preCodeValid)
                {
                    break;
                } else {
                    Console.WriteLine("That PIN didn't work! Double-check that you paired.");
                }
            }

        }

        static async Task Authenticate() {
            string verificationCode = null;
            while (true)
            {
                XmlDocument verifyBodyX = new XmlDocument();
                verificationCode = Console.ReadLine();
                HttpResponseMessage verifyBody = await client.GetAsync($"https://authenticatorapi.com/api.asmx/ValidatePin?Pin={verificationCode}&secret={File.ReadAllText(MFAKeyPath)}");
                string verifyReponse = await verifyBody.Content.ReadAsStringAsync();
                verifyBodyX.LoadXml(verifyReponse);
                bool preCodeValid = Convert.ToBoolean(verifyBodyX.SelectSingleNode("boolean").ToString());
                if (preCodeValid)
                {
                    break;
                } else {
                    Console.WriteLine("Invalid pin.");
                }
            }
        }
    }
}
