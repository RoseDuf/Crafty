#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

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
#if UNITY_EDITOR
            string previousScene = EditorPrefs.GetString("AutoPreloadScene.PreviousScene");
            if (!string.IsNullOrEmpty(previousScene) && previousScene != EditorPrefs.GetString("AutoPreloadScene.PreloadScene"))
            {
                EditorSceneManager.LoadSceneInPlayMode(
                    previousScene,
                    new LoadSceneParameters(
                        LoadSceneMode.Single,
                        EditorSettings.defaultBehaviorMode == EditorBehaviorMode.Mode3D ? LocalPhysicsMode.Physics3D : LocalPhysicsMode.Physics2D
                    )
                );
            }
#endif

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
