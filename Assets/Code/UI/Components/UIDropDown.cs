using System;
using System.Collections.Generic;
using System.Linq;
using Core.GameLoop;
using TMPro;
using UnityEngine;

namespace UI.Components
{
    public class UIDropDown : Essential.Mono, ISubscriber
    {
        public event Action<int> Changed;
        [field: SerializeField] public TMP_Dropdown DropDown { get; private set; }

        
        public void Subscribe()
        {
            DropDown.onValueChanged.AddListener(_onChanged);
        }

        public void Unsubscribe()
        {
            DropDown.onValueChanged.RemoveListener(_onChanged);
        }

        public void SetOptions(List<string> values)
        {
            DropDown.ClearOptions();

            List<TMP_Dropdown.OptionData> options = values.Select(v => new TMP_Dropdown.OptionData(v)).ToList();

            DropDown.AddOptions(options);
        }
        
        public void SetOptions<T>(T[] values)
        {
            DropDown.ClearOptions();

            List<TMP_Dropdown.OptionData> options = values
                .Select(v => new TMP_Dropdown.OptionData(v.ToString()))
                .ToList();

            DropDown.AddOptions(options);
        }

        public void SetOptions<T>(List<T> values)
        {
            DropDown.ClearOptions();

            List<TMP_Dropdown.OptionData> options = values
                .Select(v => new TMP_Dropdown.OptionData(v.ToString()))
                .ToList();

            DropDown.AddOptions(options);
        }

        public void SetCurrent(int id)
        {
            DropDown.value = id;
        }

        public void SetCurrentWithoutNotify(int id)
        {
            DropDown.SetValueWithoutNotify(id);
        }

        private void _onChanged(int id)
        {
            Changed?.Invoke(id);
        }
    }
}