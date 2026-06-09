using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using FishNet;
using FishNet.Transporting;
using FishNet.Transporting.Tugboat;
using UnityEngine;

namespace Core.Network
{
    public sealed class ConnectionHandler : Essential.Mono, IService
    {
        public const string SAVE_KEY = "last_connection_ip";
        public const float DEFAULT_CONNECT_TIMEOUT_SECONDS = 10f;

        public event Action<LocalConnectionState> ClientConnectionStateChanged;

        public string LastJoinedIP => _serverIP;

        [SerializeField] private string _serverIP = "192.168.1.100";
        [SerializeField] private ushort _port = 7777;

        private UniTaskCompletionSource<bool> _connectCompletionSource;

        private void OnEnable()
        {
            if (InstanceFinder.ClientManager != null)
            {
                InstanceFinder.ClientManager.OnClientConnectionState += _onClientConnectionState;
            }
        }

        private void OnDisable()
        {
            if (InstanceFinder.ClientManager != null)
            {
                InstanceFinder.ClientManager.OnClientConnectionState -= _onClientConnectionState;
            }
        }

        public void ConnectAsClient(string serverIP)
        {
            ConnectAsClientAsync(serverIP).Forget();
        }

        public async UniTask<bool> ConnectAsClientAsync(
            string serverIP,
            float timeoutSeconds = DEFAULT_CONNECT_TIMEOUT_SECONDS)
        {
            if (!_tryGetTugboat(out Tugboat tugboat))
            {
                return false;
            }

            serverIP = _normalizeAddress(serverIP);
            if (string.IsNullOrEmpty(serverIP))
            {
                Debug.LogError("[Client] IP сервера не указан.");
                return false;
            }

            _serverIP = serverIP;
            _stopClientConnection();

            _applyTransportSettings(tugboat, serverIP);
            _connectCompletionSource = new UniTaskCompletionSource<bool>();

            bool started = InstanceFinder.ClientManager.StartConnection();
            if (!started)
            {
                Debug.LogError("[Client] Не удалось запустить клиентское подключение.");
                _connectCompletionSource = null;
                return false;
            }

            Debug.Log($"[Client] Подключение к серверу {serverIP}:{_port}");

            try
            {
                bool connected = await _connectCompletionSource.Task.Timeout(
                    TimeSpan.FromSeconds(timeoutSeconds));
                if (!connected)
                {
                    Debug.LogError($"[Client] Не удалось подключиться к {serverIP}:{_port}. Проверьте IP, порт, firewall и что хост уже запущен.");
                    _stopClientConnection();
                }

                return connected;
            }
            catch (TimeoutException)
            {
                Debug.LogError($"[Client] Таймаут подключения к {serverIP}:{_port}.");
                _stopClientConnection();
                _connectCompletionSource = null;
                return false;
            }
        }

        public void StartHost()
        {
            if (!_tryGetTugboat(out Tugboat tugboat))
            {
                return;
            }

            _stopAllConnections();
            _applyTransportSettings(tugboat, "127.0.0.1");

            bool serverStarted = InstanceFinder.ServerManager.StartConnection();
            bool clientStarted = InstanceFinder.ClientManager.StartConnection();

            if (!serverStarted || !clientStarted)
            {
                Debug.LogError($"[Host] Не удалось запустить сервер/клиент на порту {_port}.");
                return;
            }

            Debug.Log($"[Host] Сервер запущен на {GetLocalIPAddress()}:{_port}");
        }

        public void StartServer()
        {
            if (!_tryGetTugboat(out Tugboat tugboat))
            {
                return;
            }

            _stopAllConnections();
            _applyTransportSettings(tugboat, "127.0.0.1");

            bool serverStarted = InstanceFinder.ServerManager.StartConnection();
            if (!serverStarted)
            {
                Debug.LogError($"[Server] Не удалось запустить сервер на порту {_port}.");
                return;
            }

            Debug.Log($"[Server] Сервер запущен на {GetLocalIPAddress()}:{_port}");
        }

        private void _applyTransportSettings(Tugboat tugboat, string clientAddress)
        {
            tugboat.SetServerBindAddress(string.Empty, IPAddressType.IPv4);
            tugboat.SetPort(_port);
            tugboat.SetClientAddress(clientAddress);
        }

        private bool _tryGetTugboat(out Tugboat tugboat)
        {
            tugboat = null;

            if (InstanceFinder.NetworkManager == null)
            {
                Debug.LogError("[Network] NetworkManager не найден.");
                return false;
            }

            Transport transport = InstanceFinder.NetworkManager.TransportManager.Transport;
            tugboat = transport as Tugboat;
            if (tugboat == null)
            {
                Debug.LogError("[Network] Tugboat transport не найден на NetworkManager.");
                return false;
            }

            return true;
        }

        private void _stopClientConnection()
        {
            if (InstanceFinder.ClientManager != null && InstanceFinder.ClientManager.Started)
            {
                InstanceFinder.ClientManager.StopConnection();
            }
        }

        private void _stopAllConnections()
        {
            if (InstanceFinder.ServerManager != null && InstanceFinder.ServerManager.Started)
            {
                InstanceFinder.ServerManager.StopConnection(true);
            }

            _stopClientConnection();
        }

        private void _onClientConnectionState(ClientConnectionStateArgs args)
        {
            ClientConnectionStateChanged?.Invoke(args.ConnectionState);

            Debug.Log($"[Client] Состояние подключения: {args.ConnectionState}");

            if (_connectCompletionSource == null)
            {
                return;
            }

            switch (args.ConnectionState)
            {
                case LocalConnectionState.Started:
                    _connectCompletionSource.TrySetResult(true);
                    _connectCompletionSource = null;
                    break;
                case LocalConnectionState.Stopped:
                    _connectCompletionSource.TrySetResult(false);
                    _connectCompletionSource = null;
                    break;
            }
        }

        private static string _normalizeAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                return string.Empty;
            }

            address = address.Trim();

            if (address.Equals("localhost", StringComparison.OrdinalIgnoreCase))
            {
                return "127.0.0.1";
            }

            return address;
        }

        public static string GetLocalIPAddress()
        {
            try
            {
                string lanAddress = _findPrivateLanAddress();
                if (!string.IsNullOrEmpty(lanAddress))
                {
                    return lanAddress;
                }

                using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
                socket.Connect("8.8.8.8", 65530);
                return (socket.LocalEndPoint as IPEndPoint)?.Address.ToString() ?? "127.0.0.1";
            }
            catch
            {
                return "127.0.0.1";
            }
        }

        public static string GetAllLocalIPAddresses()
        {
            try
            {
                HashSet<string> addresses = new();

                foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (networkInterface.OperationalStatus != OperationalStatus.Up
                        || networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    {
                        continue;
                    }

                    foreach (UnicastIPAddressInformation addressInfo in networkInterface.GetIPProperties().UnicastAddresses)
                    {
                        if (addressInfo.Address.AddressFamily != AddressFamily.InterNetwork)
                        {
                            continue;
                        }

                        string address = addressInfo.Address.ToString();
                        if (_isPrivateLanAddress(address))
                        {
                            addresses.Add(address);
                        }
                    }
                }

                return addresses.Count > 0
                    ? string.Join(", ", addresses)
                    : GetLocalIPAddress();
            }
            catch
            {
                return GetLocalIPAddress();
            }
        }

        private static string _findPrivateLanAddress()
        {
            foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.OperationalStatus != OperationalStatus.Up
                    || networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                {
                    continue;
                }

                foreach (UnicastIPAddressInformation addressInfo in networkInterface.GetIPProperties().UnicastAddresses)
                {
                    if (addressInfo.Address.AddressFamily != AddressFamily.InterNetwork)
                    {
                        continue;
                    }

                    string address = addressInfo.Address.ToString();
                    if (_isPrivateLanAddress(address))
                    {
                        return address;
                    }
                }
            }

            return null;
        }

        private static bool _isPrivateLanAddress(string address)
        {
            if (!IPAddress.TryParse(address, out IPAddress ip))
            {
                return false;
            }

            byte[] bytes = ip.GetAddressBytes();
            if (bytes[0] == 10)
            {
                return true;
            }

            if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
            {
                return true;
            }

            if (bytes[0] == 192 && bytes[1] == 168)
            {
                return true;
            }

            return false;
        }
    }
}
