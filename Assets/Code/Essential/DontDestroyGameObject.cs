namespace Essential
{
    public class DontDestroyGameObject : Mono
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);       
        }
    }
}