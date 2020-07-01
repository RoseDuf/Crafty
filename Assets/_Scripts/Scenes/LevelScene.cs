using UnityEngine;

using System.Collections.Generic;

using Game.Objects.Creatures.Targets;
using Game.UI.Menus;

namespace Game.Scenes
{
    public class LevelScene : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private PauseMenu pauseMenu;

        private bool paused;
        private List<Target> targets;

        private void Awake()
        {
            paused = false;
            targets = new List<Target>();

            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Target"))
            {
                Target target = obj.GetComponent<Target>();
                target.OnCapture += OnTargetCapture;
                targets.Add(target);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && !paused)
            {
                paused = true;
                pauseMenu?.gameObject.SetActive(true);
                Time.timeScale = 0;
            }
            else if (Input.GetKeyDown(KeyCode.Escape) && paused)
            {
                paused = false;
                pauseMenu?.gameObject.SetActive(false);
                Time.timeScale = 1;
            }
        }

        private void OnTargetCapture(Target target)
        {
            targets.Remove(target);
            target.OnCapture -= OnTargetCapture;

            if (targets.Count == 1)
                targets[0].SpawnHole();
        }

        private void OnDestroy()
        {
            Time.timeScale = 1;
        }
    }
}
