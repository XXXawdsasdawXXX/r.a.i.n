using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Core.ServiceLocator;
using FishNet;
using FishNet.Transporting;
using FishNet.Transporting.Tugboat;
using UnityEngine;

namespace Core.Network
{
    public sealed class ConnectionHandler : Essential.Mono, IService
    {
        public const string SAVE_KEY = "last_connection_ip";
        public string LastJoinedIP => _serverIP;
        public ushort Port => _port;

        [SerializeField] private string _serverIP = "192.168.1.100";
        [SerializeField] private ushort _port = 7777;

        private void OnEnable()
        {
            InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionState;
            InstanceFinder.ServerManager.OnServerConnectionState += OnServerConnectionState;
        }

        private void OnDisable()
        {
            if (InstanceFinder.ClientManager != null)
            {
                InstanceFinder.ClientManager.OnClientConnectionState -= OnClientConnectionState;
            }

            if (InstanceFinder.ServerManager != null)
            {
                InstanceFinder.ServerManager.OnServerConnectionState -= OnServerConnectionState;
            }
        }

        public void ConnectAsClient(string serverAddress)
        {
            ParseServerAddress(serverAddress, out string serverIP, out ushort? port);
            _serverIP = serverIP;

            if (port.HasValue)
            {
                _port = port.Value;
            }

            ConfigureTransport(clientAddress: serverIP);

            InstanceFinder.ClientManager.StartConnection();

            Debug.Log($"[Client] Подключение к серверу {serverIP}:{_port}");
        }

        public void StartHost()
        {
            ConfigureTransport(clientAddress: "127.0.0.1", bindAllInterfaces: true);

            InstanceFinder.ServerManager.StartConnection();
            InstanceFinder.ClientManager.StartConnection();

            Debug.Log($"[Host] Сервер запущен на {GetLocalIPAddress()}:{_port}");
        }

        public void StartServer()
        {
            ConfigureTransport(bindAllInterfaces: true);

            InstanceFinder.ServerManager.StartConnection();

            Debug.Log($"[Server] Сервер запущен на {GetLocalIPAddress()}:{_port}");
        }

        public static string GetLocalIPAddress()
        {
            foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.OperationalStatus != OperationalStatus.Up)
                {
                    continue;
                }

                if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                {
                    continue;
                }

                foreach (UnicastIPAddressInformation address in networkInterface.GetIPProperties().UnicastAddresses)
                {
                    if (address.Address.AddressFamily != AddressFamily.InterNetwork)
                    {
                        continue;
                    }

                    string ip = address.Address.ToString();
                    if (ip.StartsWith("169.254."))
                    {
                        continue;
                    }

                    return ip;
                }
            }

            try
            {
                using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
                socket.Connect("8.8.8.8", 65530);
                return (socket.LocalEndPoint as IPEndPoint)?.Address.ToString() ?? "127.0.0.1";
            }
            catch
            {
                return "127.0.0.1";
            }
        }

        public string GetHostAddressForClients() => $"{GetLocalIPAddress()}:{_port}";

        private static void ParseServerAddress(string serverAddress, out string serverIP, out ushort? port)
        {
            serverIP = serverAddress?.Trim() ?? string.Empty;
            port = null;

            int separatorIndex = serverIP.LastIndexOf(':');
            if (separatorIndex <= 0 || separatorIndex >= serverIP.Length - 1)
            {
                return;
            }

            string ipPart = serverIP[..separatorIndex];
            string portPart = serverIP[(separatorIndex + 1)..];

            if (!ushort.TryParse(portPart, out ushort parsedPort))
            {
                return;
            }

            serverIP = ipPart;
            port = parsedPort;
        }

        private void ConfigureTransport(string clientAddress = null, bool bindAllInterfaces = false)
        {
            if (InstanceFinder.NetworkManager.TransportManager.Transport is not Tugboat tugboat)
            {
                Debug.LogError("[Network] Tugboat transport не найден.");
                return;
            }

            tugboat.SetPort(_port);

            if (bindAllInterfaces)
            {
                tugboat.SetServerBindAddress("0.0.0.0", IPAddressType.IPv4);
            }

            if (!string.IsNullOrWhiteSpace(clientAddress))
            {
                tugboat.SetClientAddress(clientAddress);
            }
        }

        private void OnClientConnectionState(ClientConnectionStateArgs args)
        {
            Debug.Log($"[Client] Состояние подключения: {args.ConnectionState}");
        }

        private void OnServerConnectionState(ServerConnectionStateArgs args)
        {
            Debug.Log($"[Server] Состояние сервера: {args.ConnectionState}");
        }
    }
}
