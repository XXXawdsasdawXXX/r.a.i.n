using Core.Data;
using Core.GameLoop;
using Core.Libraries.Assets;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CoreGame.GameTime
{
    public class WorldMaterialController : IMono, IInitializeListener, IUpdateListener, IExitListener
    {
        private static readonly int OVERLAY_BLEND = Shader.PropertyToID(MATERIAL_PARAM_NAME);

        private const string MATERIAL_PARAM_NAME = "_OverlayBlend";
        private const float MAX_VALUE = 0.675f;

        public bool IsInitialized { get; set; }
        public string RuntimeListenerName => "WorldMaterialController";
        
        private Cache<int> _lastUpdatedMinute;
        private Material _worldMaterial;
        private GameTime _gameTime;
        
        
        public UniTask Initialize()
        {
            _worldMaterial = Container.Instance.GetSO<AssetLibrary>().Material.Get(MaterialLibrary.WORLD);
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
    
                _worldMaterial.SetFloat(OVERLAY_BLEND, brightness);
            }
        }

        public void GameExit()
        {
            _worldMaterial.SetFloat(OVERLAY_BLEND, 0);
        }
    }
}