using UnityEngine;
using UnityEngine.UI;

namespace UI.Components
{
    public class UIScroll : Essential.Mono
    {
        [field: SerializeField] public ScrollRect Scroll { get; private set; }

        public void SetViewPosition(float value)
        {
            Scroll.normalizedPosition = new Vector2(0, value);
        }
    }
}