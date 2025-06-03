using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace UI.Components
{
    public class UIDropDown : Essential.Mono
    {
        [field: SerializeField] public TMP_Dropdown DropDown { get; private set; }

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
    }
}