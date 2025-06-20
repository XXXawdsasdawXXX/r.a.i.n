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
    public class HUDQAController : UIWindowController<HUDQAView>
    {
        public bool IsInitialized { get; set; }

        private ResourceStorage _resourceStorage;
        private Health _heroHealth;
        private UserProvider _userProvider;

        public override UniTask InitializeWindow()
        {
            _resourceStorage = Container.Instance.GetService<ResourceStorage>();

            _userProvider = Container.Instance.GetService<UserProvider>();

            return UniTask.CompletedTask;
        }

        public override void StartWindow()
        {
            List<EResource> resourceTypes = Enum.GetValues(typeof(EResource)).Cast<EResource>().ToList();

            view.DropDownResourceType.DropDown.ClearOptions();

            view.DropDownResourceType.SetOptions(resourceTypes);
        }

        public override void SubscribeToEvents(bool flag)
        {
            if (flag)
            {
                _userProvider.HeroCreated += _onHeroCreated;

                view.ButtonOpen.Clicked += Open;
                view.ButtonClose.Clicked += Close;

                view.ButtonAddResource.Clicked += _addResource;

                view.ButtonAddHP.Clicked += _addHp;
                view.ButtonRemoveHP.Clicked += _removeHP;
            }
            else
            {
                _userProvider.HeroCreated -= _onHeroCreated;

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