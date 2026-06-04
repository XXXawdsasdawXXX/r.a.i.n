using System.Collections.Generic;
using System.Linq;
using Core.Save;
using Core.ServiceLocator;
using Cysharp.Threading.Tasks;
using Essential;
using FishNet;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;

namespace Core.GameLoop
{
    public sealed class GameEventDispatcher : Essential.Mono, IService
    {
        private static readonly ProfilerMarker _fixedUpdateProfilerMarker = new("_notifyGameFixedUpdate:");
        private static readonly ProfilerMarker _updateProfilerMarker = new("_notifyGameUpdate:");

        private readonly HashSet<IGameListener> _listeners = new();
        private readonly HashSet<IInitializeListener> _initListeners = new();
        private readonly HashSet<ILoadListener> _loadListeners = new();
        private readonly HashSet<IStartListener> _startListeners = new();
        private readonly HashSet<IUpdateListener> _updateListeners = new();
        private readonly HashSet<IFixedUpdateListener> _fixedUpdateListeners = new();
        private readonly HashSet<IExitListener> _exitListeners = new();
        private readonly HashSet<ISubscriber> _subscribers = new();

        private SaveService _saveService;
        private GameModel _gameModel;

        private bool _isStarted;

        public void Initialize()
        {
            _saveService = Container.Instance.GetService<SaveService>();
            _gameModel = Container.Instance.GetService<GameModel>();
        }

        private void Update()
        {
            if (_isStarted)
            {
                _notifyGameUpdate(UnityEngine.Time.deltaTime);
            }
        }

        private void FixedUpdate()
        {
            if (_isStarted)
            {
                _notifyGameFixedUpdate(UnityEngine.Time.fixedDeltaTime);
            }
        }

        private void OnApplicationQuit()
        {
            _notifyGameExit();
        }

        public async UniTask Register(List<IGameListener> listeners)
        {
            _addListeners(listeners);

            await _notifyGameInitialize();
            await _notifySubscribe();
            await _notifyGameLoad();
            await _notifyGameStart();

            _isStarted = true;
        }

        public void Dispose()
        {
            foreach (ISubscriber subscriber in _subscribers)
            {
                subscriber.Unsubscribe();
            }

            _saveService.Save();
            
            _listeners.Clear();
            _initListeners.Clear();
            _loadListeners.Clear();
            _startListeners.Clear();
            _updateListeners.Clear();
            _fixedUpdateListeners.Clear();
            _exitListeners.Clear();
            _subscribers.Clear();
            /*foreach (IGameListener listener in listeners)  
      {
          RemoveListener(listener);
      }*/

            _isStarted = false;
        }

        public async void InitializeListeners(IGameListener[] listeners)
        {
            foreach (IGameListener listener in listeners)
            {
                await InitializeListener(listener);
            }
        }

        public async UniTask InitializeListener(IGameListener listener)
        {
            if (!_listeners.Add(listener))
            {
                return;
            }

            Log.Info($"AddSListener {listener.GetType().Name}", Color.cyan, this);

            ProfilerMarker marker = new($"AddSpawnableListener: {listener.GetType().Name}");
            marker.Begin();

            if (listener is IInitializeListener initListener && !initListener.IsInitialized)
            {
                await initListener.Initialize();

                initListener.IsInitialized = true;
            }

            if (listener is ISubscriber subscriber)
            {
                subscriber.Subscribe();

                _subscribers.Add(subscriber);
            }

            if (listener is ILoadListener loadListener)
            {
                await loadListener.GameLoad(_gameModel);

                _loadListeners.Add(loadListener);
            }

            if (listener is IStartListener startListener)
            {
                await startListener.GameStart();
            }

            if (listener is IUpdateListener updateListener) _updateListeners.Add(updateListener);

            if (listener is IFixedUpdateListener fixedUpdateListener) _fixedUpdateListeners.Add(fixedUpdateListener);

            if (listener is IExitListener exitListener) _exitListeners.Add(exitListener);

            marker.End();
        }

        public void RemoveListeners(IGameListener[] listeners)
        {
            foreach (IGameListener listener in listeners)
            {
                RemoveListener(listener);
            }
        }

        public void RemoveListener(IGameListener listener)
        {
            if (!_listeners.Remove(listener))
            {
                return;
            }

            if (listener is ILoadListener loadListener)
            {
                _loadListeners.Remove(loadListener);
            }

            if (listener is IUpdateListener updateListener)
            {
                _updateListeners.Remove(updateListener);
            }

            if (listener is IFixedUpdateListener fixedUpdateListener)
            {
                _fixedUpdateListeners.Remove(fixedUpdateListener);
            }

            if (listener is ISubscriber subscriber)
            {
                subscriber.Unsubscribe();

                _subscribers.Remove(subscriber);
            }

            if (listener is IExitListener exitListener)
            {
                _exitListeners.Remove(exitListener);
            }
        }

        private void _addListeners(List<IGameListener> gameListeners)
        {
            foreach (IGameListener listener in gameListeners)
            {
                if (!_listeners.Add(listener)) continue;

                if (listener is IInitializeListener initListener) _initListeners.Add(initListener);

                if (listener is ISubscriber subscriber) _subscribers.Add(subscriber);

                if (listener is ILoadListener loadListener) _loadListeners.Add(loadListener);

                if (listener is IStartListener startListener) _startListeners.Add(startListener);

                if (listener is IUpdateListener updateListener) _updateListeners.Add(updateListener);

                if (listener is IFixedUpdateListener fixedUpdateListener)
                    _fixedUpdateListeners.Add(fixedUpdateListener);

                if (listener is IExitListener exitListener) _exitListeners.Add(exitListener);
            }
        }

        private async UniTask _notifyGameInitialize()
        {
            ProfilerMarker marker = new("_notifyGameInitialize");
            marker.Begin();

            foreach (IInitializeListener listener in _initListeners)
            {
                if (listener.IsInitialized)
                {
                    continue;
                }

                await listener.Initialize();

                listener.IsInitialized = true;
            }

            marker.End();

            Log.Info(this, $"_notifyGameInitialize", Color.red);
        }

        private async UniTask _notifyGameLoad()
        {
            ProfilerMarker marker = new("_notifyGameLoad");
            marker.Begin();

            foreach (ILoadListener listener in _loadListeners)
            {
                await listener.GameLoad(_gameModel);
            }

            marker.End();

            Log.Info(this, $"_notifyGameLoad", Color.red);
        }

        private UniTask _notifySubscribe()
        {
            ProfilerMarker marker = new("_notifySubscribe");
            marker.Begin();

            foreach (ISubscriber subscriber in _subscribers)
            {
                subscriber.Subscribe();
            }

            marker.End();

            Log.Info(this, $"_notifySubscribe", Color.red);

            return UniTask.CompletedTask;
        }

        private async UniTask _notifyGameStart()
        {
            await UniTask.WaitUntil(() => InstanceFinder.IsClientStarted);

            ProfilerMarker marker = new("_notifyGameStart");
            marker.Begin();

            foreach (IStartListener listener in _startListeners)
            {
                await listener.GameStart();
            }

            marker.End();

            Log.Info(this, $"_notifyGameStart", Color.red);
        }

        private void _notifyGameUpdate(float deltaTime)
        {
            using (_updateProfilerMarker.Auto())
            {
                foreach (IUpdateListener listener in _updateListeners)
                {
                    Profiler.BeginSample(listener.RuntimeListenerName);

                    listener.GameUpdate(deltaTime);

                    Profiler.EndSample();
                }
            }
        }

        private void _notifyGameFixedUpdate(float fixedDeltaTime)
        {
            using (_fixedUpdateProfilerMarker.Auto())
            {
                foreach (IFixedUpdateListener listener in _fixedUpdateListeners)
                {
                    Profiler.BeginSample(listener.RuntimeListenerName);

                    listener.GameFixedUpdate(fixedDeltaTime);

                    Profiler.EndSample();
                }
            }
        }

        private void _notifyGameExit()
        {
            ProfilerMarker marker = new("_notifyGameExit");
            marker.Begin();

            foreach (ISubscriber subscriber in _subscribers)
            {
                subscriber.Unsubscribe();
            }
            
            _saveService.Save();

            foreach (IExitListener listener in _exitListeners)
            {
                listener.GameExit();
            }

            Log.Info(this, $"_notifyGameExit", Color.red);
            marker.End();
        }
    }
}