using UnityEngine;

namespace Template.Core
{
    public class SystemManager : MonoBehaviourSingleton<SystemManager>
    {
        protected override void SingletonAwakened()
        {
            Application.targetFrameRate = 60;
        }

        protected override void SingletonStarted()
        {
            _ = GameManager.Instance;
            _ = AudioManager.Instance;
			_ = RankingManager.Instance;
        }
    }
}


