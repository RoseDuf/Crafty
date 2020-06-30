using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using Game.Objects.Creatures.Targets;
using Game.UI;

namespace Game.Scenes
{
    public class LevelScene : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private PauseMenu pauseMenu;

        private bool paused;
        private List<Target> targets;

        private Dictionary<GameObject, bool> pausedObjects;

        private void Awake()
        {
            pausedObjects = new Dictionary<GameObject, bool>();
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
                
                //if (pauseMenu != null)
                //{
                //    pauseMenu?.gameObject.SetActive(true);
                //    Paused = true;
                //}
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

        //public bool Paused
        //{
        //    get { return paused; }
        //    set 
        //    {
        //        if (paused != value)
        //        {
        //            paused = value;
        //            if (paused)
        //            {
        //                StoreStateOfPausedObjects();

        //                StartCoroutine(CRPause());
        //            }
        //        }
        //    }
        //}

        //private void LateUpdate()
        //{
        //    if (paused)
        //    {
        //        foreach (GameObject go in pausedObjects.Keys)
        //            go.SetActive(pausedObjects[go]);
        //    }
        //}

        //private void StoreStateOfPausedObjects()
        //{
        //    pausedObjects.Clear();
        //    for (int i = 0; i < this.transform.root.childCount; i++)
        //    {
        //        GameObject rootGameObjectChild = this.transform.root.GetChild(i).gameObject;
        //        if (!rootGameObjectChild.name.Equals("UI") &&
        //            !rootGameObjectChild.name.Equals("Lights") && !pausedObjects.ContainsKey(rootGameObjectChild))
        //            pausedObjects.Add(rootGameObjectChild, rootGameObjectChild.activeSelf);
        //    }

        //    for (int i = 0; i < pauseMenu.transform.parent.childCount; i++)
        //    {
        //        GameObject uiGameObjectChild = pauseMenu.transform.parent.GetChild(i).gameObject;
        //        if (!uiGameObjectChild.Equals(pauseMenu.gameObject) && !pausedObjects.ContainsKey(uiGameObjectChild))
        //            pausedObjects.Add(uiGameObjectChild, uiGameObjectChild.activeSelf);
        //    }
        //}

        //private IEnumerator CRPause()
        //{
        //    while (paused)
        //    {
        //        yield return new WaitForEndOfFrame();

        //        foreach (GameObject go in pausedObjects.Keys)
        //            go.SetActive(false);
        //    }

        //    foreach (GameObject go in pausedObjects.Keys)
        //        go.SetActive(pausedObjects[go]);
        //}
    }
}
