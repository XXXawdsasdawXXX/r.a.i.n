using Core.ServiceLocator;
using CoreGame.Card.Data;
using CoreGame.Card.Logic;
using CoreGame.Entities.Characters.Hero;
using Cysharp.Threading.Tasks;
using UI.World;
using UI.Windows.Base;
using UnityEngine;

namespace UI.Windows.Game.HUD
{
    public class HUDWindowController : UIWindowController<HUDWindowView>
    {
        private BattleService _battleService;
        private HeroContextMenuService _contextMenuService;
        private HeroWorldContextMenu _activeContextMenu;

        public override UniTask InitializeWindow(UIWindowManager manager)
        {
            _battleService = Container.Instance.GetService<BattleService>();
            _contextMenuService = Container.Instance.GetService<HeroContextMenuService>();

            return base.InitializeWindow(manager);
        }

        public override void SubscribeToEvents(bool flag)
        {
            base.SubscribeToEvents(flag);

            if (flag)
            {
                _battleService.BattleStarted += _closeHud;
                _battleService.BattleFinished += _openHud;
                _contextMenuService.OpenRequested += _onContextMenuOpenRequested;
                _contextMenuService.CloseRequested += _onContextMenuCloseRequested;
            }
            else
            {
                _battleService.BattleStarted -= _closeHud;
                _battleService.BattleFinished -= _openHud;
                _contextMenuService.OpenRequested -= _onContextMenuOpenRequested;
                _contextMenuService.CloseRequested -= _onContextMenuCloseRequested;
                _closeActiveContextMenu(notifyService: false);
            }
        }

        private void _openHud(BattleModel _)
        {
            view.Open();
        }

        private void _closeHud(BattleModel _)
        {
            _contextMenuService.RequestClose();
            view.Close();
        }

        private void _onContextMenuOpenRequested(HeroContextMenuRequest request)
        {
            _closeActiveContextMenu(notifyService: false);

            if (request.Target == null)
            {
                _contextMenuService.NotifyMenuClosed();
                return;
            }

            _activeContextMenu = request.Target.GetComponentInChildren<HeroWorldContextMenu>(true);
            if (_activeContextMenu == null)
            {
                Debug.LogWarning(
                    $"Hero '{request.DisplayName}' has no {nameof(HeroWorldContextMenu)} on world canvas.");
                _contextMenuService.NotifyMenuClosed();
                return;
            }

            _activeContextMenu.Closed += _onContextMenuClosed;
            _activeContextMenu.Open(request.Target);
        }

        private void _onContextMenuCloseRequested()
        {
            _closeActiveContextMenu(notifyService: false);
        }

        private void _onContextMenuClosed()
        {
            if (_activeContextMenu != null)
            {
                _activeContextMenu.Closed -= _onContextMenuClosed;
                _activeContextMenu = null;
            }

            _contextMenuService.NotifyMenuClosed();
        }

        private void _closeActiveContextMenu(bool notifyService)
        {
            if (_activeContextMenu == null)
            {
                return;
            }

            _activeContextMenu.Closed -= _onContextMenuClosed;
            _activeContextMenu.Close();
            _activeContextMenu = null;

            if (notifyService)
            {
                _contextMenuService.NotifyMenuClosed();
            }
        }
    }
}
