using UnityEngine;

using Game.UI.HUDs.Gameplay;

namespace Game.UI.HUDs
{
    public class GameplayHUD : MonoBehaviour
    {
        [SerializeField] private HealthBar healthBar;
        [SerializeField] private StarBar starBar;
        [SerializeField] private PowerupSlot powerupSlot;
        [SerializeField] private ShotMetricBars shotMetricsBars;

        public HealthBar HealthBar
        {
            get { return healthBar; }
        }

        public StarBar StarBar
        {
            get { return starBar; }
        }

        public PowerupSlot PowerupSlot
        {
            get { return powerupSlot; }
        }

        public ShotMetricBars ShotMetricBars
        {
            get { return shotMetricsBars; }
        }
    }
}