using System;

namespace Core.Save
{
    [Serializable]
    public class HeroModel
    {
        public string Name;
        public float Health;
        public TimeSpan GameTime;
        public TimeSpan ExitTime;

        public HeroModel()
        {
            Name = "name";
            Health = 100;
            GameTime = new TimeSpan();
            ExitTime = new TimeSpan();
        }
    }
}