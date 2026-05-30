using System;
using System.Collections.Generic;
using Core.GameLoop;
using Core.Save;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using Essential;

namespace UI.Windows.Base
{
    public class UIWindowManager : Essential.Mono, IService, IInitializeListener, IStartListener, ILoadListener, ISubscriber
    {
        public bool IsInitialized { get; set; }

        private IWindowController[] _windows; 
        
        private readonly Dictionary<Type, IWindowController> _windowControllers = new();
        
        private IWindowController _lastOpenedWindow;


        public async UniTask Initialize()
        {
            _windows = GetComponentsInChildren<IWindowController>(true);
            
            foreach (IWindowController windowController in _windows)
            {
                Type type = windowController.GetType();
                
                _windowControllers.Add(type, windowController);
            }
            
            foreach (IWindowController windowController in _windows)
            {
                await windowController.InitializeWindow(this);
            }
        }

        public UniTask GameStart()
        {
            foreach ((Type _, IWindowController windowController) in _windowControllers)
            {
                windowController.StartWindow();
            }
            return UniTask.CompletedTask;
        }

        public UniTask GameLoad(GameModel model)
        {
            foreach ((Type _, IWindowController windowController) in _windowControllers)
            {
                windowController.LoadWindow(model);
            }
            return UniTask.CompletedTask;
        }

        public void Subscribe()
        {
            foreach ((Type _, IWindowController windowController) in _windowControllers)
            {
                windowController.SubscribeToEvents(true);
            }
        }

        public void Unsubscribe()
        {
            foreach ((Type _, IWindowController windowController) in _windowControllers)
            {
                windowController.SubscribeToEvents(false);
            }
        }

        public void OpenWindow<T>() where T : class, IWindowController
        {
            Type type = typeof(T);
            
            _checkCollection<T>(type);

            _windowControllers[type].Open();
        }
        
        public void CloseWindow<T>() where T : class, IWindowController
        {
            Type type = typeof(T);
            
            _checkCollection<T>(type);

            _windowControllers[type].Close();
        }
        
        public void SwitchWindow<T>() where T : class, IWindowController
        {
            Type type = typeof(T);
            
            _checkCollection<T>(type);
            
            _lastOpenedWindow?.Close();
                    
            _lastOpenedWindow = _windowControllers[type];
                    
            _lastOpenedWindow.Open();
        }

        private void _checkCollection<T>(Type type) where T : class, IWindowController
        {
            if (!_windowControllers.ContainsKey(type))
            {
                foreach (IWindowController window in _windows)
                {
                    if (window is T)
                    {
                        _windowControllers.Add(type, window);
                    }
                }
            }
        }

        public T GetWindow<T>() where T : class, IWindowController
        {
            Type type = typeof(T);
            
            if (_windowControllers.ContainsKey(type))
            {
                return _windowControllers[type] as T;
            }

            foreach (IWindowController window in _windows)
            {
                if (window is T windowController)
                {
                    _windowControllers.Add(type, window);
                        
                    return windowController;
                }
            }

            Log.Error(this, $"Has not window with type {type.Name}");
            
            return null;
        }
    }
}