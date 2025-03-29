using Core.Data;
using Core.GameLoop;
using Core.Libraries;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using LiteNetLib.Utils;
using UnityEngine;

namespace Game.Time
{
    [Preserve]
    public class WorldMaterialController : IMono, IInitializeListener, IUpdateListener, IExitListener
    {
        private static readonly int _overlayBlend = Shader.PropertyToID(MATERIAL_PARAM_NAME);
        
        private const string MATERIAL_PARAM_NAME = "_OverlayBlend";
        private const float MAX_VALUE = 0.675f;
        
        private Cache<int> _lastUpdatedMinute;
        private Material _worldMaterial;
        private GameTime _gameTime;
        
        public UniTask GameInitialize()
        {
            _worldMaterial = Container.Instance.GetConfig<AssetLibrary>().Material.Get(MaterialLibrary.WORLD);
            _gameTime = Container.Instance.GetService<GameTime>();
          
            _lastUpdatedMinute = new Cache<int>();
            
            return UniTask.CompletedTask;
        }

        public void GameUpdate(float deltaTime)
        {
            if (_lastUpdatedMinute.Update(_gameTime.Current.Hours * 60 + _gameTime.Current.Minutes))
            {
                float timeOfDay = (_gameTime.Current.Hours * 60 + _gameTime.Current.Minutes) / 1440f;
                float shiftedTime = (timeOfDay - 0.2f) * 2 * Mathf.PI;
    
                float brightness = (Mathf.Cos(shiftedTime) + 1) / 2 * MAX_VALUE;
    
                _worldMaterial.SetFloat(_overlayBlend, brightness);
            }
        }

        public void GameExit()
        {
            _worldMaterial.SetFloat(_overlayBlend, 0);
        }
    }
}