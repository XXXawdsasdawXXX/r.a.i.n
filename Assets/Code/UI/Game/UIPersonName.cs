using Code.CoreGame.Entities.Params;
using Core.GameLoop;
using Cysharp.Threading.Tasks;
using Essential;
using UI.Components;
using UnityEngine;

namespace UI.Game
{
    public class UIPersonName : Essential.Mono, IStartListener ,ISubscriber
    {
        [SerializeField] private PersonName _personName;
        [SerializeField] private UIText _uiText;

        public UniTask GameStart()
        {
            _uiText.SetText(_personName.Name);
            
            Log.Info($"game start set name {_personName.Name}");
            
            _personName.Changed += SetName;
            
            return UniTask.CompletedTask;
        }

        public UniTask Subscribe()
        {
            Log.Info($"2", this);
            
            
            return UniTask.CompletedTask;
        }

        public void Unsubscribe()
        {
            _personName.Changed -= SetName;
        }

        private void SetName(string objectName)
        {
            Log.Info($"3", this);
            
            _uiText.SetText(objectName);
        }
    }
}