using System;

using System.Threading;
using System.Net.Sockets;


namespace AGLW_CSharp_BetterChatClientSample
{
    class Messenger
    {
        public readonly System.Text.Encoding Encoder = System.Text.Encoding.UTF8;
        public static int SleepDuration = 100;  // 100ms
        public static int MessageLength = 256;

        public NetworkStream Stream { get; private set; } = null;

        public Thread SenderThread { get; private set; } = null;
        public Thread ReceiverThread { get; private set; } = null;

        public string Username { get; private set; } = string.Empty;

        public Messenger(NetworkStream stream)
        {
            Stream = stream;
        }

        public void StartMessenger()
        {
            Console.WriteLine("Enter your username: ");
            Username = Console.ReadLine();

            Console.Clear();

            SenderThread = new Thread(() => SendMessage());
            SenderThread.Start();

            ReceiverThread = new Thread(() => ReceiveMessage());
            ReceiverThread.Start();
        }

        void SendMessage()
        {
            while (true)
            {
                SleepForAWhile();
                MonitorInput();
            }
        }

        void MonitorInput()
        {
            string text = Console.ReadLine();
            string message = new string($"{Username}: {text}");
            WriteMessage(Encoder.GetBytes(message));
        }

        void WriteMessage(byte[] bytes)
        {
            ClearCurrentConsoleLine();
            Stream.Write(bytes);
            // Console.WriteLine($"Sent : {Encoder.GetString(bytes)}");
        }

        void ReceiveMessage()
        {
            byte[] bytes = new byte[MessageLength];
            while (Stream.Read(bytes) > 0)
            {
                string text = Encoder.GetString(bytes);
                // Console.WriteLine($"Received : {text}");
                Console.WriteLine($"{text}");
            }
        }

        void SleepForAWhile()
        {
            Thread.Sleep(SleepDuration);
        }

        void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor - 1);
        }
    }
}