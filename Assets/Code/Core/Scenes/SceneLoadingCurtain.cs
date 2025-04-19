using UnityEngine;

namespace Core.Scenes
{
    public class SceneLoadingCurtain : Essential.Mono
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private GameObject _object;
        
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void Show()
        {
            _object.SetActive(true);
        }

        public void Hide()
        {
            _object.SetActive(false);
        }
    }
}