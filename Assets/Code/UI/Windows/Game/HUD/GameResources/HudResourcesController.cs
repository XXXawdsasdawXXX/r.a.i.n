using System.Collections.Generic;
using Core.GameLoop;
using Core.ServiceLocator;
using CoreGame.Harvest;
using Cysharp.Threading.Tasks;
using UI.Components;
using UI.Windows.Base;

namespace UI.Windows.Game.HUD.GameResources
{
    public class HudResourcesController : UIWindowController<HudResourcesView>, IStartListener
    {
        private readonly Dictionary<EResource, UIResourceBoxView> _resourcesView = new();
       
        private ResourceStorage _resourceStorage;

        
        public override UniTask InitializeWindow(UIWindowManager manager)
        {
            _resourceStorage = Container.Instance.GetService<ResourceStorage>();
            
            return base.InitializeWindow(manager);
        }

        
        public UniTask GameStart()
        {
            foreach (UIResourceBoxView resourceBox in view.ResourceBoxViews)
            {
                resourceBox.UpdateIcon();

                if (_resourcesView.ContainsKey(resourceBox.ResourceType))
                {
                    continue;
                }
                
                _resourcesView.Add(resourceBox.ResourceType, resourceBox);
            }
            
            return UniTask.CompletedTask;
        }
        
        public override void StartWindow()
        {
            _updateResourcesView(_resourceStorage.Collection);
        }


        public override void SubscribeToEvents(bool flag)
        {
            base.SubscribeToEvents(flag);

            if (flag)
            {
                _resourceStorage.ValueChanged += _updateResourceView;
                _resourceStorage.CollectionChanged += _updateResourcesView;
            }
            else
            {
                _resourceStorage.ValueChanged -= _updateResourceView;
                _resourceStorage.CollectionChanged -= _updateResourcesView;
            }
        }
        
        private void _updateResourcesView(Dictionary<EResource, int> collection)
        {
            foreach (KeyValuePair<EResource, int> resource in collection)
            {
                _updateResourceView(resource);
            }
        }

        private void _updateResourceView(KeyValuePair<EResource, int> resource)
        {
            if (_resourcesView.ContainsKey(resource.Key))
            {
                _resourcesView[resource.Key].SetValue(resource.Value.ToString());
            }
        }

    }
}