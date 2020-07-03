using UnityEngine;
using UnityEngine.SceneManagement;

using System;
using System.Collections;

namespace Tenacious.Scenes
{
    public class SceneLoader : MBSingleton<SceneLoader>
    {
        [SerializeField] private string loadingSceneName = "Loading";
        [SerializeField] private GameObject transitionsObject;

        public enum ETransitionType
        {
            None, // <-- must be first
            Fade, Circle, CircularFan, Box, Ellipse, Diamond, HorizontalSlide, VerticalSlide, DiagonalBlinds, HorizontalBlinds, VerticalBlinds,
            Random // <-- must be last
        }

        public enum ETransitionPhase { None, Out, In }

        public delegate void OnBeforeSceneLoadCallback();
        public event Action<float> OnLoadProgressUpdate;

        private Animator animator;
        private string sceneToLoad;
        private OnBeforeSceneLoadCallback beforeSceneLoadCallback;

        private ETransitionType transitionType;
        private ETransitionPhase transitionPhase;

        protected override void Awake()
        {
            base.Awake();

            animator = this.GetComponent<Animator>();
            transitionsObject.SetActive(false);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (scene.name.Equals(loadingSceneName))
                transitionsObject.SetActive(false);
        }

        public void LoadScene(string sceneName, ETransitionType type = ETransitionType.None, OnBeforeSceneLoadCallback onBeforeSceneLoadCallback = null)
        {
            sceneToLoad = sceneName;
            beforeSceneLoadCallback = onBeforeSceneLoadCallback;

            transitionsObject.SetActive(true);
            TransitionType = type;
            TransitionPhase = ETransitionPhase.Out;

            if (TransitionType == ETransitionType.None)
            {
                OnOutComplete();
                OnInComplete();
            }
        }

        public void OnOutComplete()
        {
            if (!string.IsNullOrWhiteSpace(sceneToLoad))
            {
                beforeSceneLoadCallback?.Invoke();

                if (SceneUtility.GetBuildIndexByScenePath(loadingSceneName) >= 0)
                    SceneManager.LoadScene(loadingSceneName);

                StartCoroutine(CRLoadSceneInBackground(sceneToLoad));
            }
            else
            {
                beforeSceneLoadCallback?.Invoke();
                TransitionPhase = ETransitionPhase.In;
            }

            sceneToLoad = null;
            beforeSceneLoadCallback = null;
        }

        public void OnInComplete()
        {
            transitionsObject.SetActive(false);
            TransitionType = ETransitionType.None;
            TransitionPhase = ETransitionPhase.None;
        }

        public ETransitionType TransitionType
        {
            get { return transitionType; }
            set
            {
                transitionType = value;
                if (value != ETransitionType.Random)
                    animator?.SetInteger("TransitionType", (int)value);
                else
                    animator?.SetInteger("TransitionType", UnityEngine.Random.Range((int)ETransitionType.None, (int)ETransitionType.Random));
            }
        }

        public ETransitionPhase TransitionPhase
        {
            get { return transitionPhase; }
            set
            {
                transitionPhase = value;
                animator?.SetInteger("TransitionPhase", (int)value);
            }
        }

        private IEnumerator CRLoadSceneInBackground(string sceneName)
        {
            yield return null; // continue running on the next frame

            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
            asyncOperation.allowSceneActivation = false;

            while (!asyncOperation.isDone)
            {
                float progress = asyncOperation.progress + 0.1f;

                OnLoadProgressUpdate?.Invoke(progress);

                // activate the new scene when progress is complete
                if (progress >= 1f)
                {
                    transitionsObject.SetActive(true);
                    asyncOperation.allowSceneActivation = true;
                }

                yield return null;
            }

            OnLoadProgressUpdate = null;
            TransitionPhase = ETransitionPhase.In;
        }
    }
}