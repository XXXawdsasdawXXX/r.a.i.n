using System;
using Core.ServiceLocator;
using CoreGame._Test;
using CoreGame.Camera;
using CoreGame.Card.Logic;
using CoreGame.Entities.Characters.Controllers;
using CoreGame.Grid;
using CoreGame.Harvest;
using CoreGame.Time;
using UnityEngine;

namespace CoreGame
{
    [CreateAssetMenu(fileName = "Installer_CoreGame", menuName = "Game/Installers/CoreGame")]
    public class CoreGameInstaller : Installer
    {
        public override Type[] GetTypes()
        {
            return new[]
            {
                typeof(TestService),
                //world
                typeof(GameTime),
                typeof(GridService),
                typeof(WorldMaterialController),
                typeof(CameraController),
                //storages
                typeof(ResourceStorage),
                //hero
                typeof(Movement),
                typeof(Miner),
                //cards
                typeof(BattleService)
            };
        }
    }
}