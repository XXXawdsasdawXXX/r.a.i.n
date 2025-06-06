using System;
using System.Linq;
using Core.Scenes;
using Core.ServiceLocator;
using UnityEngine;

namespace Core.Libraries.Installers
{
    [CreateAssetMenu(fileName = "Storage_Installer", menuName = "Game/Library/Storage")]
    public class InstallerStorage : ScriptableObject
    {
        [Serializable]
        private struct SceneInstaller
        {
            public EScene Scene;
            public Installer Installer;
        }
        
        [field: SerializeField] public Installer ProjectsInstaller { get; private set; }
        [field: SerializeField] public Installer CoreGameInstaller { get; private set; }
        [field: SerializeField] public Installer MetaGameInstaller { get; private set; }

        [SerializeField] private SceneInstaller[] _sceneInstallers;
        
        public Installer GetSceneInstaller(EScene scene)
        {
            return _sceneInstallers.FirstOrDefault(i => i.Scene == scene).Installer;
        }
    }
}