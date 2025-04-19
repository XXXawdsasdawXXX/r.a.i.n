using System;
using Core.Audio;
using Core.GameLoop;
using Core.Input;
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
                typeof(MonoSpawnTracker),
                typeof(AudioService),
                typeof(AudioGlobalVolume),
                typeof(InputManager),
                typeof(SceneService),
            };
        }
    }
}