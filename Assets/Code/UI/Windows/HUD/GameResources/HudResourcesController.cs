using System.Collections.Generic;
using Core.GameLoop;
using Core.ServiceLocator;
using CoreGame.Harvest;
using Cysharp.Threading.Tasks;
using UI.Components;
using UI.Windows.Base;

namespace UI.Windows.HUD.GameResources
{
    public class HudResourcesController : UIWindowController<HudResourcesView>
    {
        public bool IsInitialized { get; set; }
        
        private ResourceStorage _resourceStorage;
       
        private Dictionary<EResource, UIResourceBoxView> _resourcesView;

        public override UniTask InitializeWindow()
        {
            _resourceStorage = Container.Instance.GetService<ResourceStorage>();
            _resourcesView = new Dictionary<EResource, UIResourceBoxView>();
            
            foreach (UIResourceBoxView resourceBox in view.ResourceBoxViews)
            {
                resourceBox.UpdateIcon();
              
                _resourcesView.Add(resourceBox.ResourceType, resourceBox);
            }
            
            return UniTask.CompletedTask;
        }

        public override void StartWindow()
        {
            _updateResourcesView(_resourceStorage.Collection);
        }

        public void Subscribe()
        {
            _resourceStorage.ValueChanged += _updateResourceView;
            _resourceStorage.CollectionChanged += _updateResourcesView;
        }

        public void Unsubscribe()
        {
            _resourceStorage.ValueChanged -= _updateResourceView;
            _resourceStorage.CollectionChanged -= _updateResourcesView;
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