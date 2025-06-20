using Core.Save;
using Cysharp.Threading.Tasks;

namespace UI.Windows.Base
{
    public interface IWindowController
    {
        public UniTask InitializeWindow();
        
        public void LoadWindow(GameModel model);

        public void StartWindow();

        public void SubscribeToEvents(bool flag);
        
        public void Open();

        public void Close();
    }
}