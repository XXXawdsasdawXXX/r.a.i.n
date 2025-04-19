using System;
using Code.CoreGame._Test;
using Code.CoreGame.Grid;
using Code.CoreGame.Time;
using Core.Network;
using Core.ServiceLocator;
using UnityEngine;

namespace Code.CoreGame.Installers
{
    [CreateAssetMenu(fileName = "Installer_CoreGame", menuName = "Game/Installers/CoreGame")]
    public  class CoreGameInstaller : Installer
    {
        public override Type[] GetTypes()
        {
            return new[]
            {
                typeof(TestService),
                typeof(GameTime),
                typeof(GridService),
                typeof(WorldMaterialController)
            };
        }
    }
}