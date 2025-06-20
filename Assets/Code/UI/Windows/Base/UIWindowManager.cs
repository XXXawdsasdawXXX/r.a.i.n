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
        
        private IWindowController _openedWindow;


        public UniTask Initialize()
        {
            _windows = GetComponentsInChildren<IWindowController>(true);
            
            foreach (IWindowController windowController in _windows)
            {
                Type type = windowController.GetType();
                
                _windowControllers.Add(type, windowController);

                windowController.InitializeWindow();
            }
            
            return UniTask.CompletedTask;
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
            
            _openedWindow?.Close();
                    
            _openedWindow = _windowControllers[type];
                    
            _openedWindow.Open();
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