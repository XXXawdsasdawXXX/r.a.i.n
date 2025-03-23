using System;
using Core.Audio;
using Core.GameLoop;
using Core.Libraries;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Components
{
    public class UIButton : Essential.Mono ,IPointerDownHandler, IPointerUpHandler, IInitializeListener
    {
        public event Action Clicked;
        
        [SerializeField] private Button _button;
        
        private Audio _audio;


        public UniTask GameInitialize()
        {
            _audio = Container.Instance.GetService<Audio>();
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