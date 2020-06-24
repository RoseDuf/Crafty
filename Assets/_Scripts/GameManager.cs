using UnityEngine;
using UnityEngine.SceneManagement;

using Tenacious;

using DG.Tweening;

namespace Game
{
    public class GameManager : MBSingleton<GameManager>
    {
        [SerializeField] private string firstScene = "Splash";

        protected override void Awake()
        {
            base.Awake();

            DOTween.Init();
        }

        private void Start()
        {
            // load first Scene
            if (!Application.isEditor)
                SceneManager.LoadScene(firstScene);
        }

        protected override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
        }
    }
}
