using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GyroShooterClient;

namespace GyroShooterNetTest
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            GyroClient.Listen();
            GyroClient.ClientConnected += GyroClient_ClientConnected;
            while (true)
            {
                Console.ReadLine();
                client.WriteCommand("hit", 1);
            }
        }


        static GyroClient client;

        static async void GyroClient_ClientConnected(object sender, GyroClient e)
        {
            client = e;
            Console.WriteLine("Connected!");
            while (true)
            {
                var res = await e.GetCommand();
                Console.WriteLine(res);
            }
        }
    }
}
