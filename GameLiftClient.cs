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
    class GameLiftClient
    {
        // All the GameLift related things will be processed by this AmazonGameLiftClient
        private AmazonGameLiftClient gameLiftClient = null;

        // Sessions contain general information
        private GameSession gameSession = null;
        private PlayerSession playerSession = null;
        private string playerId = string.Empty;

        // A instance's status flag of this class 
        public bool IsAlive { get; private set; } = false;

        public GameLiftClient()
        {
            IsAlive = true;

            playerId = Guid.NewGuid().ToString();

            Console.WriteLine($"Client : playerId {playerId}");

            gameLiftClient = new AmazonGameLiftClient("fake", "fake", new AmazonGameLiftConfig() { ServiceURL = "http://localhost:9080" });

            // CognitoAWSCredentials credentials = new CognitoAWSCredentials(
            //     "Your-Identity-Pool-ID", // Identity pool ID
            //     RegionEndpoint.APNortheast1 // Region
            // );

            // gameLiftClient = new AmazonGameLiftClient(credentials, RegionEndpoint.APNortheast1);
        }

        public void Start()
        {
            // Create GameSession(async) -> Create PlayerSession(async) -> Connect ()
            CreateSessionsAndConnect();
        }

        async private Task CreateSessionsAndConnect()
        {
            await SearchGameSessionAsync();
            await CreateGameSessionAsync();
            await CreatePlayerSessionAsync();

            // Connect to the IP provided by PlayerSession
            Connect();
        }

        async private Task SearchGameSessionAsync()
        {
            Console.WriteLine($"Client : SearchGameSessionAsync() start");
            var request = new DescribeGameSessionsRequest();
            request.FleetId = "fleet-id";

            Console.WriteLine($"Client : Sending request and await");
            var response = await gameLiftClient.DescribeGameSessionsAsync(request);
            Console.WriteLine($"Client : request sent");

            foreach (var session in response.GameSessions)
            {
                if (session.MaximumPlayerSessionCount > session.CurrentPlayerSessionCount && session.Status == GameSessionStatus.ACTIVE)
                {
                    gameSession = session;
                    break;
                }
            }
        }

        async private Task CreateGameSessionAsync()
        {
            if (gameSession != null) return;

            Console.WriteLine($"Client : CreateGameSessionAsync() start");
            // GameSession gameSession = await CreateGameSessionAsync();
            var request = new CreateGameSessionRequest();
            request.FleetId = "fleet-id";
            request.CreatorId = playerId;
            request.MaximumPlayerSessionCount = 2;

            Console.WriteLine($"Client : Sending request and await");
            var response = await gameLiftClient.CreateGameSessionAsync(request);
            Console.WriteLine($"Client : request sent");

            if (response.GameSession != null)
            {
                Console.WriteLine($"Client : GameSession Created!");
                Console.WriteLine($"Client : GameSession ID {response.GameSession.GameSessionId}!");
                gameSession = response.GameSession;
            }
            else
            {
                Console.Error.WriteLine($"Client : Failed creating GameSession!");
                IsAlive = false;
            }
        }

        async private Task CreatePlayerSessionAsync()
        {
            Console.WriteLine($"Client : CreatePlayerSessionAsync() start");
            if (gameSession == null) return;

            while (gameSession.Status != GameSessionStatus.ACTIVE)
            {
                Console.WriteLine($"Client : Wait for GameSession to be ACTIVE, Sleep for a while");
                Thread.Sleep(100);
                await UpdateGameSession();
            }

            var request = new CreatePlayerSessionRequest();
            request.GameSessionId = gameSession.GameSessionId;
            request.PlayerId = playerId;

            Console.WriteLine($"Client : Sending request and await");
            var response = await gameLiftClient.CreatePlayerSessionAsync(request);
            Console.WriteLine($"Client : request sent");

            if (response.PlayerSession != null)
            {
                Console.WriteLine($"Client : PlayerSession Created!");
                Console.WriteLine($"Client : PlayerSession ID {response.PlayerSession.PlayerSessionId}!");
                playerSession = response.PlayerSession;
            }
            else
            {
                Console.Error.WriteLine($"Client : Failed creating PlayerSession!");
                IsAlive = false;
            }
        }

        async private Task UpdateGameSession()
        {
            DescribeGameSessionsRequest request = new DescribeGameSessionsRequest();
            request.GameSessionId = gameSession.GameSessionId;

            Console.WriteLine($"Client : Describe GameSession {gameSession.GameSessionId}...");
            DescribeGameSessionsResponse response = await gameLiftClient.DescribeGameSessionsAsync(request);

            if (response != null)
            {
                if (response.GameSessions.Count == 1)
                {
                    // Update GameSession
                    gameSession = response.GameSessions[0];
                }
                else
                {
                    Console.WriteLine($"Client : Warning, describe GameSession count {response.GameSessions.Count}");
                }
            }
            else
            {
                Console.WriteLine($"Client : Warning, describe GameSession no response");
            }
        }



        // Connect to the IP which PlayerSession provides
        // When client connects : 
        // 1) Receive the msg sent by server
        // 2) Send another msg back
        // 3) Close the connection
        private void Connect()
        {
            Console.WriteLine($"Client : Connect() start");
            if (playerSession == null) return;

            Console.WriteLine($"Client : Try to connect {playerSession.IpAddress}:{playerSession.Port}");
            TcpClient client = new TcpClient(playerSession.IpAddress, playerSession.Port);

            if (client.Connected)
            {
                Console.WriteLine($"Client : Connected");

                ChatClient chatClient = new ChatClient(client);

                chatClient.StartClient();
            }
        }
    }
}