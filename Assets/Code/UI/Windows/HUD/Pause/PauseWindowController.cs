using Core.Audio;
using Core.Input;
using Core.Save;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using UI.Windows.Base;

namespace UI.Windows.HUD.Pause
{
    public class PauseWindowController : UIWindowController<PauseWindowView>
    {
        private AudioGlobalVolume _audioGlobalVolume;
        private InputManager _input;

        private bool _isOpened;

        public override UniTask InitializeWindow(UIWindowManager manager)
        {
            _audioGlobalVolume = Container.Instance.GetService<AudioGlobalVolume>();
            _input = Container.Instance.GetService<InputManager>();

            return base.InitializeWindow(manager);
        }

        public override void LoadWindow(GameModel model)
        {
            view.SliderMusic.SetValueWithoutNotify(_audioGlobalVolume.MusicValue);
            view.SliderSFX.SetValueWithoutNotify(_audioGlobalVolume.SFXValue);
            
            base.LoadWindow(model);
        }

        public override void SubscribeToEvents(bool flag)
        {
            if (flag)
            {
                _input.ActionEnded += InputOnActionEnded;
                view.SliderMusic.Changed += _audioGlobalVolume.SetMusicVolume;
                view.SliderSFX.Changed += _audioGlobalVolume.SetSFXVolume;
            }
            else
            {
                _input.ActionEnded -= InputOnActionEnded;
                view.SliderMusic.Changed -= _audioGlobalVolume.SetMusicVolume;
                view.SliderSFX.Changed -= _audioGlobalVolume.SetSFXVolume;
            }
        }

        private void InputOnActionEnded(EInputAction action)
        {
            if (action is EInputAction.Esc)
            {
                if (_isOpened)
                {
                    Close();
                }
                else
                {
                    Open();
                }

                _isOpened = !_isOpened;
            }
        }
    }
}