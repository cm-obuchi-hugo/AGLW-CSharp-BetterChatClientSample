using System;

using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Collections.Concurrent;

using Amazon;
using Amazon.GameLift;
using Amazon.GameLift.Model;
using Amazon.CognitoIdentity;

namespace AGLW_CSharp_BetterChatClientSample
{
    
    class ChatClient
    {
        public TcpClient ManagedConnection {get; private set;} = null;
        public Messenger Messenger {get;private set;} = null;
        public ChatClient(TcpClient client)
        {
            ManagedConnection = client;
            Messenger = new Messenger(ManagedConnection.GetStream());
        }

        public void StartClient()
        {
            if (ManagedConnection != null &&
                Messenger != null)
            {
                Messenger.StartMessenger();
            }
        }
    }
}