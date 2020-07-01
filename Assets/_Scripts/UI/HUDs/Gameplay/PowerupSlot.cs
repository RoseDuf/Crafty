using UnityEngine;
using UnityEngine.UI;

using Game.Objects.Powerups;

namespace Game.UI.HUDs.Gameplay
{
    public class PowerupSlot : MonoBehaviour
    {
        [SerializeField] private Image powerupImage;

        private Powerup powerup;
        private bool useable;

        public Powerup Powerup
        {
            get { return powerup; }
            set
            {
                powerup = value;
                if (powerup != null)
                    powerupImage.sprite = powerup.Sprite;
                else
                {
                    powerupImage.sprite = null;
                    Useable = false;
                }
            }
        }

        public bool Useable
        {
            get { return useable; }
            set
            {
                useable = value;
                if (useable)
                    powerupImage.color = new Color(1, 1, 1, 1);
                else
                    powerupImage.color = new Color(0, 0, 0, 0.5f);
            }
        }

        private void Awake()
        {
            Powerup = powerup;
        }
    }
}