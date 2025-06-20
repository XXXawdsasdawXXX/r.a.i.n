using System;
using System.Collections.Generic;
using Core.GameLoop;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UI.Components
{

    public abstract class UIRadioGroup<T> : Essential.Mono, IInitializeListener, ISubscriber where T : UISelectable
    {
        public event Action<int> Selected; 
        public event Action<int> Deselected; 
        
        public bool IsInitialized { get; set; }
        
        [field: SerializeField, Min(0)] public int MaxSelectedCount { get; private set; } = 1;
        [field: SerializeField] public UIElementPool<T> Pool { get; private set; }
        [field: SerializeField] public Queue<T> SelectedButtons { get; private set; } = new();
        

        public UniTask Initialize()
        {
            Pool.Initialize();
            
            return UniTask.CompletedTask;
        }
        
        public void Subscribe()
        {
            Pool.Changed += _updateSubscriptionToButtons;
        }

        public void Unsubscribe()
        {
            Pool.Changed -= _updateSubscriptionToButtons;
        }

        public void Select(int index)
        {
            if (SelectedButtons.Count >= MaxSelectedCount)
            {
                T element = SelectedButtons.Dequeue();
                element.Deselect();
            }
            
            T newSelectedElement = Pool.Enabled[index];
            SelectedButtons.Enqueue(newSelectedElement);
            newSelectedElement.Select();
        }

        public void Clear()
        {
            foreach (T selectable in Pool.All)
            {
                selectable.Deselect();
            }
            
            Pool.DisableAll();
        }

        private void _updateSubscriptionToButtons()
        {
            foreach (T element in Pool.All)
            {
                element.Dispose();
                
                element.Clicked += () =>
                {
                    if (SelectedButtons.Count < MaxSelectedCount)
                    {
                        SelectedButtons.Enqueue(element);
                        element.Select();
                    }
                    else
                    {
                        T firstSelected = SelectedButtons.Dequeue();
                        firstSelected.Deselect();
                        
                        Deselected?.Invoke(firstSelected.Index);
                        
                        SelectedButtons.Enqueue(element);
                        element.Select();
                    }
                    
                    Selected?.Invoke(element.Index);
                };
            }
        }
    }
}