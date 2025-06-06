using CoreGame.Entities.Animation;

namespace CoreGame.Entities.Characters.Interfaces
{
    public interface IHarvestAnimator
    {
        void StartMine(AnimatorKey.EHarvestType harvestType);
        void StopMine();
    }
}