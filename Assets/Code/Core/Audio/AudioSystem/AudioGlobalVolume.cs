using Core.GameLoop;
using Core.Save;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using Essential;
using FMOD.Studio;
using FMODUnity;
using UnityEngine.Scripting;

namespace Core.Audio
{
    [Preserve]
    public class AudioGlobalVolume : IService, IInitializeListener, ILoadListener
    {
        public bool IsInitialized { get; set; }
        public float MusicValue => _saveService.ModelsContainer.UserSettings.MusicVolume;
        public float SFXValue => _saveService.ModelsContainer.UserSettings.SFXVolume;
        
        private const string MUSIC_VCA_PATH_VCA = "vca:/Music";
        private const string SFX_VCA_PATH = "vca:/SFX";

        private SaveService _saveService;
        
        private VCA _musicVCA;
        private VCA _sfxVCA;

        public UniTask Initialize()
        {
            _saveService = Container.Instance.GetService<SaveService>();
            
            _musicVCA = RuntimeManager.GetVCA(MUSIC_VCA_PATH_VCA);
            _sfxVCA = RuntimeManager.GetVCA(SFX_VCA_PATH);
                        
            return UniTask.CompletedTask;
        }
        
        public void SetMusicVolume(float volume)
        {
            Log.Info(this, $"music : {volume}");
            _musicVCA.setVolume(volume);
            _saveService.ModelsContainer.UserSettings.MusicVolume = volume;
        }

        public void SetSFXVolume(float volume)
        {
            Log.Info(this, $"sfx : {volume}");
            _sfxVCA.setVolume(volume);
            _saveService.ModelsContainer.UserSettings.SFXVolume = volume;
        }

        public UniTask GameLoad(GameModel model)
        {
            _musicVCA.setVolume(_saveService.ModelsContainer.UserSettings.MusicVolume);
            _sfxVCA.setVolume(_saveService.ModelsContainer.UserSettings.SFXVolume);
            return UniTask.CompletedTask;
        }
    }
}