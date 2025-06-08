using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Components
{
    public class UIText : MonoBehaviour, IPointerDownHandler
    {
        public event Action Clicked;
        
        [SerializeField] private TextMeshProUGUI _textMeshPro;
        
        
        public void SetText(string text)
        {
            _textMeshPro.SetText(text);
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            Clicked?.Invoke();
        }
    }
}