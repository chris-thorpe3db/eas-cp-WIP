using System;
using System.IO;
using System.Net.Http;
using System.Threading;

using EASControlPanel.Authentication;

namespace EASControlPanel
{
    public class Program
    {
        public static readonly HttpClient httpClient= new HttpClient();
        
        static void Main(string[] args)
        {
            string nl = Environment.NewLine;
            string title = $" _____ _____ _____    _____ _____ _____ _____ _____ _____ __       _____ _____ _____ _____ __    {nl}|   __|  _  |   __|  |     |     |   | |_   _| __  |     |  |     |  _  |  _  |   | |   __|  |   {nl}|   __|     |__   |  |   --|  |  | | | | | | |    -|  |  |  |__   |   __|     | | | |   __|  |__ {nl}|_____|__|__|_____|  |_____|_____|_|___| |_| |__|__|_____|_____|  |__|  |__|__|_|___|_____|_____|{nl}";
            foreach (char c in title)
            {
                Console.Write(c);
            }
            Console.WriteLine();
            Console.WriteLine("UNAUTHORIZED ACCESS WILL RESULT IN FINES IN EXCESS OF $50,000 AND UP TO 5 YEARS IMPRISONMENT.");
            Thread.Sleep(10);
            Console.WriteLine("DO NOT PROCEED UNLESS YOU HAVE PROPER CREDENTIALS. PRESS ANY KEY TO CONTINUE.");
            Console.ReadKey();
            if (File.Exists(Environment.GetEnvironmentVariable("appdata") + @"\ECP\_2fa") == false)
            {
                Authenticator.New();
            } else {
                Authenticator.Authenticate();
            }
            
        }
    }
}
