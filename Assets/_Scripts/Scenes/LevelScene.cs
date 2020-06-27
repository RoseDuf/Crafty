using UnityEngine;

using System.Collections.Generic;

using Game.Objects.Creatures.Targets;

using Tenacious;

namespace Game.Scenes
{
    public class LevelScene : MBSingleton<LevelScene>
    {
        private List<Target> targets;

        protected override void Awake()
        {
            targets = new List<Target>();

            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Target"))
            {
                Target target = obj.GetComponent<Target>();
                target.OnCapture += OnTargetCapture;
                targets.Add(target);
            }
        }

        private void OnTargetCapture(Target target)
        {
            targets.Remove(target);
            target.OnCapture -= OnTargetCapture;

            if (targets.Count == 1)
                targets[0].TurnIntoHole();
        }
    }
}
