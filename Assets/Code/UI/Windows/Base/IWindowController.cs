using Core.Save;
using Cysharp.Threading.Tasks;

namespace UI.Windows.Base
{
    public interface IWindowController
    {
        public UniTask InitializeWindow(UIWindowManager manager);
        
        public void LoadWindow(GameModel game);

        public void StartWindow();

        public void SubscribeToEvents(bool flag);
        
        public void Open();

        public void Close();
    }
}