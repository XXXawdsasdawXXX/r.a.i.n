using System;
using UnityEngine;

namespace UI.Windows.Game.Card.Unit.Fx
{
    [Serializable]
    public sealed class UnitFxSettings
    {
        [SerializeField] private Color _color = new Color(1f, 1f, 1f, 0.5f);
        [SerializeField] private float _duration = 0.35f;
        [SerializeField] private float _scale = 1.08f;
        [SerializeField] private float _intensity = 0.18f;
        [SerializeField] private int _steps = 6;

        public Color Color => _color;
        public float Duration => _duration;
        public float Scale => _scale;
        public float Intensity => _intensity;
        public int Steps => _steps;
    }
}
