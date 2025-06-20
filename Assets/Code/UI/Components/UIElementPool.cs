using System;
using System.Collections.Generic;
using System.Linq;
using Core.GameLoop;
using Core.ServiceLocator;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UI.Components
{
    public interface IPoolableUIElement 
    {
        void Enable();
        void Disable();
    }

    [Serializable]
    public class UIElementPool<TUIElement> where TUIElement : UISelectable, IPoolableUIElement
    {
        public event Action Changed;
        
        [field: SerializeField] public List<TUIElement> All { get; private set; } = new();
        [field: SerializeField] public List<TUIElement> Enabled { get; private set; } = new();
     
        [SerializeField] private Transform _root;
        [SerializeField] private TUIElement _prefab;
        
        private GameEventDispatcher _gameEventDispatcher;

        public UIElementPool(Transform root, TUIElement prefab)
        {
            _root = root;
            _prefab = prefab;
        }

        public void Initialize()
        {
            _gameEventDispatcher = Container.Instance.GetService<GameEventDispatcher>();
        }
        
        public TUIElement GetNext()
        {
            TUIElement entity = _getDisabledEntity() ?? _addNewEntity();

            Enabled.Add(entity);

            entity.Enable();
            
            Changed?.Invoke();

            return entity;
        }

        public void SortByIndex()
        {
            Enabled.Sort((a, b) => a.Index.CompareTo(b.Index));

            for (int i = 0; i < Enabled.Count; i++)
            {
                Enabled[i].transform.SetSiblingIndex(i);
            }

            Changed?.Invoke();
        }
        
        public void Disable(TUIElement entity)
        {
            if (entity == null || !entity.gameObject.activeSelf)
            {
                return;
            }

            entity.Disable();

            Enabled.Remove(entity);
        }

        public void DisableAll()
        {
            foreach (TUIElement entity in All)
            {
                entity.Disable();
            }

            Enabled.Clear();
            
            Changed?.Invoke();
        }

        public TUIElement GetByIndex(int tabIndex)
        {
            return All[tabIndex];
        }
        
        private TUIElement _getDisabledEntity()
        {
            return All.FirstOrDefault(entity => entity != null && !entity.gameObject.activeSelf);
        }

        private TUIElement _addNewEntity()
        {
            TUIElement entity = Object.Instantiate(_prefab, _root);

            All.Add(entity);

            entity.SetIndex(All.Count);

            IGameListener[] listeners = entity.GetComponentsInChildren<IGameListener>(true);

            if (listeners is { Length: > 0 })
            {
                _gameEventDispatcher.InitializeListeners(listeners);
            }
            
            return entity;
        }

#if UNITY_EDITOR
        [Button]
        private void _findInChildren()
        {
            All = _root.GetComponentsInChildren<TUIElement>().ToList();
            
            Enabled = All.Where(entity => entity != null && !entity.gameObject.activeSelf).ToList();

            for (int index = 0; index < All.Count; index++)
            {
                TUIElement element = All[index];
                element.SetIndex(index);
            }

            AssetDatabase.SaveAssets();
        }
#endif
    }
}