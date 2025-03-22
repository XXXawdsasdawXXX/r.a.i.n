using UnityEngine;
using UnityEngine.UI;

namespace UI.Components
{
    public class UIImage : MonoBehaviour
    {
        [SerializeField] private Image _image;

        public void SetFillAmount(float normalizedValue)
        {
            _image.fillAmount = normalizedValue;
        }
        
        public float GetFillAmount()
        {
            return _image.fillAmount;
        }
    }
}