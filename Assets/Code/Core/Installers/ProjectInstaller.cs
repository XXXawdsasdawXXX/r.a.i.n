using System;
using Core.Audio;
using Core.GameLoop;
using Core.Input;
using Core.Network;
using Core.Save;
using Core.Scenes;
using Core.ServiceLocator;
using UnityEngine;

namespace Core.Installers
{
    [CreateAssetMenu(fileName = "Installer_Project", menuName = "Game/Installers/Project")]
    public sealed class ProjectInstaller : Installer
    {
        public override Type[] GetTypes()
        {
            return new[]
            {
                typeof(SaveService),
                typeof(GameModel),
                typeof(SceneService),
                typeof(MonoSpawnTracker),
                typeof(UserProvider),
                typeof(AudioService),
                typeof(AudioGlobalVolume),
                typeof(InputManager),
            };
        }
    }
}