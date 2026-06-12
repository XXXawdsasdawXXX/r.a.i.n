using System;
using Core.Input;
using UnityEngine;

namespace CoreGame.Interaction
{
    public interface IWorldPointerTarget
    {
        bool IsHovered { get; }
        bool IsPointerEnabled { get; }
        Collider2D Collider { get; }
        int SortingPriority { get; }

        event Action<IWorldPointerTarget> HoverEntered;
        event Action<IWorldPointerTarget> HoverExited;
        event Action<IWorldPointerTarget> LeftClicked;
        event Action<IWorldPointerTarget> RightClicked;

        void NotifyHoverChanged(bool isHovered);
        void NotifyClicked(EInputAction action);
    }
}
