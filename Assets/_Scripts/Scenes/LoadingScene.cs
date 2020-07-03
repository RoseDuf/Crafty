using UnityEngine;

using Tenacious.Scenes;

namespace Game.Scenes
{
    public class LoadingScene : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private RectTransform progressTransform;

        [Tooltip("Time to wait until Canvas is shown")]
        [SerializeField] private float canvasShowTime = 1;
        private float canvasTimer;

        private void Awake()
        {
            if (progressTransform != null)
                progressTransform.sizeDelta = new Vector2(0, progressTransform.sizeDelta.y);

            SceneLoader.Instance.OnLoadProgressUpdate += OnLoadProgressUpdate;

            canvas.gameObject.SetActive(false);
            canvasTimer = 0;
        }

        private void Update()
        {
            canvasTimer += Time.deltaTime;
            if (canvasTimer >= canvasShowTime)
                canvas.gameObject.SetActive(true);
        }

        private void OnLoadProgressUpdate(float progress)
        {
            if (progressTransform != null)
            {
                float barLength = ((RectTransform)progressTransform.parent).rect.width;
                progressTransform.sizeDelta = new Vector2(progress * barLength, progressTransform.sizeDelta.y);
            }
        }
    }
}