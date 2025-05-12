using System;
using Core.ServiceLocator;

namespace Core.Save
{
    [Serializable]
    public class GameModel : IService
    {

        public int test = 10;

        public void Copy(GameModel model)
        {
           
        }
    }
}