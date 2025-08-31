using System;
using TMPro;
using UnityEngine;

namespace UI.Components
{
    public sealed class UIInputField : Essential.Mono
    {
        public event Action<string> Changed;
        public string Value => _inputField.text;
        
        [SerializeField] private TMP_InputField _inputField;

        private void Awake()
        {
            _inputField.onEndEdit.AddListener(EndEdit);
        }

        public void SetTextWithoutNotify(string text)
        {
            _inputField.SetTextWithoutNotify(text);
        }

        private void EndEdit(string value)
        {
            Changed?.Invoke(value);
        }
    }
}