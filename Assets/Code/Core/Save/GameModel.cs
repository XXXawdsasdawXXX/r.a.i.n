using System;
using System.Collections.Generic;
using Core.Data;
using Core.ServiceLocator;

namespace Core.Save
{
    [Serializable]
    public class GameModel : IService
    {
        public HeroModel Hero;
        public WorldModel World;
        
        public List<HeroModel> Heroes;
        public List<WorldModel> Worlds;

        public ReactiveProperty<int> LastHeroIndex;
        public ReactiveProperty<int> LastWorldIndex;
        
        public DateTime GameEnterTime;
        public DateTime GameExitTime;
        
        public GameModel()
        {
            World = new WorldModel();
            Hero = new HeroModel();
            
            Heroes = new List<HeroModel>();
            Worlds = new List<WorldModel>();
            
            LastWorldIndex = new ReactiveProperty<int>(0);
            LastHeroIndex = new ReactiveProperty<int>(0);
        }
        
        public void CopyFrom(GameModel model)
        {
            World = model?.World ?? new WorldModel();
            Hero = model?.Hero ?? new HeroModel();
            
            Heroes = model?.Heroes ?? new List<HeroModel>();
            Worlds = model?.Worlds ?? new List<WorldModel>();
            
            LastWorldIndex = model?.LastWorldIndex ?? new ReactiveProperty<int>(0);
            LastHeroIndex = model?.LastHeroIndex ?? new ReactiveProperty<int>(0);

            GameEnterTime = model?.GameEnterTime ?? default;
            GameExitTime = model?.GameExitTime ?? default;
        }

        public HeroModel GetCurrentHeroModel()
        {
            if (LastHeroIndex.Value < Heroes.Count)
            {
                return Heroes[LastHeroIndex.Value];
            }

            throw new Exception($"Model has error with hero index. " +
                                $"Index = {LastWorldIndex.Value}. " +
                                $"Heroes count = {Heroes.Count}");
        }
    }
}