using System;
using System.Collections.Generic;
using System.Linq;
using Core.Data;
using Core.ServiceLocator;
using Newtonsoft.Json;

namespace Core.Save
{
    [Serializable]
    public class GameModel : IService
    {
        public DateTime GameEnterTime;
        public DateTime GameExitTime;
        
        public ReactiveProperty<int> LastHeroIndex;
        public ReactiveProperty<int> LastWorldIndex;
        
        public List<HeroModel> Heroes;
        public List<WorldModel> Worlds;
        

        [JsonIgnore]
        public WorldModel World
        {
            get
            {
                if (LastWorldIndex?.Value >= 0  && Worlds?.Count > LastWorldIndex?.Value)
                {
                    return Worlds[LastWorldIndex.Value];
                }

                if (LastWorldIndex != null)
                {
                    LastWorldIndex.Value = 0;
                }

                return null;
            }
        }
        
        [JsonIgnore]
        public HeroModel Hero
        {
            get
            {
                if (LastHeroIndex?.Value >= 0  && Heroes?.Count > LastHeroIndex?.Value)
                {
                    return Heroes[LastHeroIndex.Value];
                }

                if (LastHeroIndex != null)
                {
                    LastHeroIndex.Value = 0;
                }

                return null;
            }
        }
        
        
        public GameModel()
        {
            Heroes = new List<HeroModel>();
            Worlds = new List<WorldModel>();
            
            LastWorldIndex = new ReactiveProperty<int>(0);
            LastHeroIndex = new ReactiveProperty<int>(0);
        }
        
        public void CopyFrom(GameModel model)
        {
            Heroes = model?.Heroes ?? new List<HeroModel>();
            Worlds = model?.Worlds ?? new List<WorldModel>();
            
            LastWorldIndex = model?.LastWorldIndex ?? new ReactiveProperty<int>(0);
            LastHeroIndex = model?.LastHeroIndex ?? new ReactiveProperty<int>(0);

            GameEnterTime = model?.GameEnterTime ?? default;
            GameExitTime = model?.GameExitTime ?? default;
        }
        
        public HeroModel GetNearestHeroByExitTime()
        {
            if (Heroes == null || Heroes.Count == 0)
            {
                return null;
            }

            return Heroes.OrderBy(h => Math.Abs((GameEnterTime - h.ExitTime).Ticks)).FirstOrDefault();
        }

        public WorldModel GetNearestWorldByExitTime()
        {
            if (Worlds == null || Worlds.Count == 0)
            {
                return null;
            }

            return Worlds.OrderBy(w => Math.Abs((GameEnterTime - w.ExitTime).Ticks)).FirstOrDefault();
        }
    }
}