using System;

using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Collections.Concurrent;


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

        public Messenger(NetworkStream stream)
        {
            Stream = stream;
        }

        public void StartMessenger()
        {
            SenderThread = new Thread(() => SendMessage() );
            SenderThread.Start();

            ReceiverThread = new Thread(() => ReceiveMessage() );
            ReceiverThread.Start();
        }

        void SendMessage()
        {
            while(true)
            {
                SleepForAWhile();
                MonitorInput();
            }
        }

        void MonitorInput()
        {
            string text = Console.ReadLine();
            WriteMessage(Encoder.GetBytes(text));
        }

        void WriteMessage(byte[] bytes)
        {
            Stream.Write(bytes);
            Console.WriteLine($"Sent : {Encoder.GetString(bytes)}");
        }

        void ReceiveMessage()
        {
            byte[] bytes = new byte[MessageLength];
            while(Stream.Read(bytes) > 0)
            {
                string text = Encoder.GetString(bytes);
                Console.WriteLine($"Received : {text}");
            }
        }

        void SleepForAWhile()
        {
            Thread.Sleep(SleepDuration);
        }
    }
}