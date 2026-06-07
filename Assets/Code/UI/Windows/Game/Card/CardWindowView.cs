using UI.Components;
using UI.Windows.Base;
using UnityEngine;

namespace UI.Windows.Game.Card
{
    public class CardWindowView : UIWindowView
    {
        [SerializeField] private UIText _textCommandMessage;

        public void ShowCommandMessage(string message)
        {
            if (_textCommandMessage == null)
            {
                return;
            }

            bool hasMessage = !string.IsNullOrEmpty(message);
            _textCommandMessage.gameObject.SetActive(hasMessage);
            _textCommandMessage.SetText(hasMessage ? message : string.Empty);
        }

        public void ClearCommandMessage()
        {
            ShowCommandMessage(string.Empty);
        }
    }
}