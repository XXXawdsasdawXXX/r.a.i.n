using System;
using Core.ServiceLocator;
using CoreGame._Test;
using CoreGame.Camera;
using CoreGame.Card.Logic;
using CoreGame.Card.Logic.StateMachine;
using CoreGame.Entities.Characters.Controllers;
using CoreGame.Entities.Characters.Hero;
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
#if UNITY_EDITOR || DEBUG
                typeof(TestService),
#endif
                //world
                typeof(GameTime),
                typeof(GridService),
                typeof(WorldMaterialController),
                typeof(CameraController),
                //storages
                typeof(ResourceStorage),
                //cards
                typeof(BattleService),
                typeof(BattleStateMachine),
                typeof(NetworkBattleService),
                typeof(NetworkDuelService),
                typeof(HeroContextMenuService)
            };
        }
    }
}