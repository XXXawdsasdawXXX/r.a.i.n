using Code.CoreGame.Entities.Params;
using UI.Components;
using UnityEngine;

namespace UI.Game
{
    public class UIHealthBar : MonoBehaviour
    {
        [SerializeField] private Health _health;
        [SerializeField] private UIImage _fill;
        
        private void Awake()
        {
            _health.Changed += HealthOnChanged;
        }

        private void OnDestroy()
        {
            _health.Changed -= HealthOnChanged;
        }

        private void HealthOnChanged()
        {
            _fill.SetFillAmount(_health.GetNormalize());
        }
    }
}