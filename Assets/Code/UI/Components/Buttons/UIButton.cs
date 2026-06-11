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
    public class UIButton : UISelectable, IPointerUpHandler, IInitializeListener
    {
        public bool IsInitialized { get; set; }

        [SerializeField] private Button _button;
        [SerializeField] private UIText _uiText;
        
        private AudioService _audio;

        
        public UniTask Initialize()
        {
            _audio = Container.Instance.GetService<AudioService>();
            
            return UniTask.CompletedTask;
        }

        public override void SetInteractable(bool isInteractable)
        {
            _button.interactable = isInteractable;
        }

        protected override void onClick()
        {
            _audio?.OneShot(AudioEventLibrary.BUTTON_DOWN);   
        }

        protected override void onEnter()
        {
            base.onEnter();
        }

        protected override void onExit()
        {
            base.onExit();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _audio?.OneShot(AudioEventLibrary.BUTTON_UP);
        }
    }
}