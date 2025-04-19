using System.Net;
using FishNet;
using FishNet.Transporting;
using FishNet.Transporting.Tugboat;
using UnityEngine;

namespace Core.Network
{
    public sealed class ConnectionHandler : MonoBehaviour
    {
        public string LastJoinedIP => _serverIP;

        [SerializeField] private string _serverIP = "192.168.1.100";
        [SerializeField] private ushort _port = 7777;
     
        private void OnEnable()
        {
            InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionState;
        }

        private void OnDisable()
        {
            if (InstanceFinder.ClientManager != null)
            {
                InstanceFinder.ClientManager.OnClientConnectionState -= OnClientConnectionState;
            }
        }

        public void ConnectAsClient(string serverIP)
        {
            if (InstanceFinder.NetworkManager.TransportManager.Transport is Tugboat tugboat)
            {
                tugboat.SetClientAddress(serverIP);
                tugboat.SetPort(_port);
            }

            InstanceFinder.ClientManager.StartConnection();
     
            Debug.Log($"[Client] Подключение к серверу {_serverIP}:{_port}");
        }

        public void StartHost()
        {
            if (InstanceFinder.NetworkManager.TransportManager.Transport is Tugboat tugboat)
            {
                tugboat.SetClientAddress(GetLocalIPAddress());
                tugboat.SetPort(_port);
                Debug.Log($"[Host] set local ip {GetLocalIPAddress()}:{_port}");
            }

            InstanceFinder.ServerManager.StartConnection();
            InstanceFinder.ClientManager.StartConnection();
      
            Debug.Log($"[Host] Сервер запущен на {GetLocalIPAddress()}:{_port}");
        }

        private void OnClientConnectionState(ClientConnectionStateArgs args)
        {
            if (args.ConnectionState == LocalConnectionState.Stopping)
            {
                UnityEditor.EditorApplication.isPlaying = false;
            }
        }

        public static string GetLocalIPAddress()
        {
            string localIP = "127.0.0.1"; 
            
            foreach (IPAddress ip in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            
            return localIP;
        }
    }
}
