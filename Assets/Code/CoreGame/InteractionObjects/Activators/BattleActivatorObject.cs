using Core.GameLoop;
using Cysharp.Threading.Tasks;

namespace CoreGame.InteractionObjects.Activators
{
    public class BattleActivatorObject : ActivatorObject, IInitializeListener
    {
        public bool IsInitialized { get; set; }

        public UniTask Initialize()
        {
            
            
            return UniTask.CompletedTask;
        }
        
        public override void StartInteraction()
        {
            base.StartInteraction();
            
        }
    }
}