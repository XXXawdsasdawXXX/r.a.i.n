using Cysharp.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IInitializable
    {
        bool IsInitialized { get; }
        UniTask Initialize();
    }
}