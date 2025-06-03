using System;
using Core.Audio;
using Core.GameLoop;
using Core.Libraries;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using Essential;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Components
{
    public class UIButton : Essential.Mono ,IPointerDownHandler, IPointerUpHandler, IInitializeListener
    {
        public event Action Clicked;
        public bool IsInitialized { get; set; }
        
        [SerializeField] private Button _button;
        
        private AudioService _audio;

        public UniTask Initialize()
        {
            _audio = Container.Instance.GetService<AudioService>();
          
            Log.Info($"_audio != null {_audio != null}", this);
            
            _button.onClick.AddListener(Click);
            
            return UniTask.CompletedTask;
        }

        protected virtual void Click()
        {
            Clicked?.Invoke();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _audio.OneShot(AudioEventLibrary.BUTTON_DOWN);   
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _audio.OneShot(AudioEventLibrary.BUTTON_UP);
        }
    }
}