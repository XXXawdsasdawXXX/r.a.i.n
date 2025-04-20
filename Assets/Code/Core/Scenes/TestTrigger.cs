using System;
using Essential;
using FishNet;
using FishNet.Object;
using UnityEngine;

namespace Core.Scenes
{
    public class TestTrigger : NetworkBehaviour
    {
        private void OnTriggerEnter2D(Collider2D col)
        {
            if (!IsOwner) return; // Только владелец может сообщить

            Log.Info(this, $"Client entered trigger: {col.name}");

            Server_NotifyTriggerEntered(col.gameObject.name); // или передавай нужные ID
        }

        [ServerRpc]
        private void Server_NotifyTriggerEntered(string targetName)
        {
            Log.Info(this, $"[SERVER] Client entered trigger with {targetName}", Color.green);
            // действия на сервере
        }
    }
}