using System.Threading;
using Cysharp.Threading.Tasks;
using UI.Windows.Game.Card.Unit.Fx;

namespace UI.Windows.Game.Card.Unit.Impacts
{
    public interface ICardImpact
    {
        UniTask Play(UnitFxSettings settings, CancellationToken cancellationToken);
    }
}
