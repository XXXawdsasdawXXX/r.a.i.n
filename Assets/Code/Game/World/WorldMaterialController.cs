using Core.Data;
using Core.GameLoop;
using Core.Libraries;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using LiteNetLib.Utils;
using UnityEngine;

namespace Game.World
{
    [Preserve]
    public class WorldMaterialController : IMono, IInitializeListener, IUpdateListener
    {
        private const string MATERIAL_PARAM_NAME = "_OverlayBlend";
        private const float MAX_VALUE = 0.675f;
        
        private static readonly int _overlayBlend = Shader.PropertyToID(MATERIAL_PARAM_NAME);

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
                float timeOfDay = (_gameTime.Current.Hours * 60 + _gameTime.Current.Minutes) / 1440f; // 0-1 интервал
                float shiftedTime = (timeOfDay - 0.2f) * 2 * Mathf.PI; // Сдвиг темноты на 10% дня (144 мин)
    
                float brightness = (Mathf.Cos(shiftedTime) + 1) / 2 * MAX_VALUE;
    
                _worldMaterial.SetFloat(_overlayBlend, brightness);
            }
        }
    }
}