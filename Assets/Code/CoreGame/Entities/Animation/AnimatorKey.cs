using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CoreGame.Entities.Animation
{
    [InitializeOnLoad]
    public static class AnimatorKey
    {

        static AnimatorKey()
        {
#if UNITY_EDITOR
            _initCharacterStates();
#endif
        }

    

        #region characters
        
        public enum ECharacterAnimationState  
        {
            IDLE,
            MOVE,
            HARVEST,
            EAT,
        }

        public enum EHarvestType
        {
            NONE = 0,
            PICK_UP = 1,
            MINE = 2
        }
        
        public static readonly int PARAM_HARVEST_TYPE = Animator.StringToHash("HarvestType");
        public static readonly int PARAM_SPEED = Animator.StringToHash("Speed");
        public static readonly int PARAM_EAT = Animator.StringToHash("IsEat");

        private static readonly int STATE_IDLE = Animator.StringToHash("Idle");
        private static readonly int STATE_MOVE = Animator.StringToHash("Move");
        private static readonly int STATE_HARVEST = Animator.StringToHash("Harvest");
        private static readonly int STATE_EAT = Animator.StringToHash("Eat");
        private static readonly int STATE_MINE = Animator.StringToHash("Mine");
        private static readonly int STATE_PICK_UP = Animator.StringToHash("PickUp");
        
        public static Dictionary<int, ECharacterAnimationState> CHARACTER_STATES;
        
        private static void _initCharacterStates()
        {
            CHARACTER_STATES = new()
            {
                {STATE_IDLE, ECharacterAnimationState.IDLE},
                {STATE_MOVE, ECharacterAnimationState.MOVE},
                {STATE_HARVEST, ECharacterAnimationState.HARVEST},
                {STATE_MINE, ECharacterAnimationState.HARVEST},
                {STATE_PICK_UP, ECharacterAnimationState.HARVEST},
                {STATE_EAT, ECharacterAnimationState.EAT},
            };
        }
        
        #endregion
    }
}