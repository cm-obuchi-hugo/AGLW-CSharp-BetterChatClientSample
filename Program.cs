using System;

namespace AGLW_CSharp_BetterChatClientSample
{
    class Program
    {
        static private GameLiftClient client = new GameLiftClient();
        static void Main(string[] args)
        {
            client.Start();

            while (client.IsAlive)
            {

            }

            Console.WriteLine("Program ends.");
        }
    }
}
