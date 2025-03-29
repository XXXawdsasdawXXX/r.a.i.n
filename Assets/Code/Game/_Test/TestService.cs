using Core.GameLoop;
using Core.Input;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using Essential;
using Game.Camera;
using Game.Grid;
using LiteNetLib.Utils;
using Unity.Mathematics;
using UnityEngine;

namespace Game._Test
{
    [Preserve]
    public class TestService : IMono, IInitializeListener, ISubscriber
    {
        private GridService _gridService;
        private InputManager _inputManager;
        private CameraView _cameraView;

        public UniTask GameInitialize()
        {
            _gridService = Container.Instance.GetService<GridService>();
            _inputManager = Container.Instance.GetService<InputManager>();
            _cameraView = Container.Instance.GetView<CameraView>();
            
            return UniTask.CompletedTask;
        }

        public UniTask Subscribe()
        {
            _inputManager.ActionEnded += _inputManagerOnActionEnded;
            
            return UniTask.CompletedTask;
        }

        public void Unsubscribe()
        {
            _inputManager.ActionEnded -= _inputManagerOnActionEnded;
        }

        private void _inputManagerOnActionEnded(EInputAction obj)
        {
            if (obj is EInputAction.LeftClick)
            {
                Vector3 worldPoint = _cameraView.ScreenToWorldPoint(_inputManager.MousePosition);
                
                worldPoint.z = 0;
                
                float2 position = _gridService.GetTilePosition(worldPoint);

                ETileType tileType = _gridService.GetTileType(worldPoint);
                
                Log.Info($"{position} {tileType}", Color.magenta, this);
            }
        }
    }
}