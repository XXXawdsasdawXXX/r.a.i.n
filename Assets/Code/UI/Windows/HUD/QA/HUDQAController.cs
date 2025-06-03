using System;
using System.Collections.Generic;
using System.Linq;
using Core.GameLoop;
using Core.Network;
using Core.ServiceLocator;
using CoreGame.Entities.Characters.Hero;
using CoreGame.Entities.Params;
using CoreGame.Harvest;
using Cysharp.Threading.Tasks;
using Essential;
using TMPro;
using UI.Windows.Base;

namespace UI.Windows.HUD.QA
{
    public class HUDQAController : UIWindowController<HUDQAView>, IInitializeListener, IStartListener, ISubscriber
    {
        public bool IsInitialized { get; set; }
       
        private ResourceStorage _resourceStorage;
        private Health _heroHealth;
        private UserProvider _userProvider;

        public UniTask Initialize()
        {
            _resourceStorage = Container.Instance.GetService<ResourceStorage>();

            _userProvider = Container.Instance.GetService<UserProvider>();
            
            return UniTask.CompletedTask;
        }

        public UniTask GameStart()
        {
            EResource[] resourceTypes = Enum.GetValues(typeof(EResource)).Cast<EResource>().ToArray();

            view.DropDownResourceType.DropDown.ClearOptions();

           view.DropDownResourceType.SetOptions(resourceTypes);

            return UniTask.CompletedTask;
        }
        
        public void Subscribe()
        {
            _userProvider.HeroCreated += _onHeroCreated;
        }

        public void Unsubscribe()
        {
            _userProvider.HeroCreated -= _onHeroCreated;
        }

        protected override void subscribeToEvents(bool flag)
        {
            if (flag)
            {
                view.ButtonOpen.Clicked += Open;
                view.ButtonClose.Clicked += Close;
             
                view.ButtonAddResource.Clicked += _addResource;
                
                view.ButtonAddHP.Clicked += _addHp;
                view.ButtonRemoveHP.Clicked += _removeHP;
            }
            else
            {
                view.ButtonOpen.Clicked -= Open;
                view.ButtonClose.Clicked -= Close;
             
                view.ButtonAddResource.Clicked -= _addResource;
                
                view.ButtonAddHP.Clicked -= _addHp;
                view.ButtonRemoveHP.Clicked -= _removeHP;
            }
        }

        private void _onHeroCreated()
        {
            _heroHealth = _userProvider.GetHeroComponent<Hero>().Health;
        }

        private void _removeHP()
        {
            if (int.TryParse(view.InputFieldHP.Value.ToString(), out int healthValue))
            {
                _heroHealth.UpdateHealth(-healthValue);
            }
        }

        private void _addHp()
        {
            if (int.TryParse(view.InputFieldHP.Value.ToString(), out int healthValue))
            {
                Log.Info(this, $"int parse {healthValue}");
                _heroHealth.UpdateHealth(healthValue);
            }
            else
            {
                
                Log.Info(this, $"int parse lose {view.InputFieldHP.Value}");
            }
        }

        private void _addResource()
        {
            Log.Info(this, $"{view.DropDownResourceType.DropDown.value}");

            if (Enum.TryParse(view.DropDownResourceType.DropDown.value.ToString(), out EResource resourceType))
            {
                if (int.TryParse(view.InputFieldResource.Value, out int resourceCount))
                {
                    _resourceStorage.Add(resourceType, resourceCount);
                }
            }
        }
    }
}