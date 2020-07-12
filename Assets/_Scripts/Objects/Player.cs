using UnityEngine;

using Game.Objects.Powerups;
using Game.UI.HUDs;

namespace Game.Objects
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private GameplayHUD hud;
        [SerializeField] private Powerup powerup;

        public Powerup Powerup
        {
            get { return powerup; }
            set 
            {
                powerup = value;

                if (powerup != null)
                    powerup.Acquire(this);

                hud.PowerupSlot.Powerup = powerup;
            }
        }

        private void Awake()
        {
            Powerup = powerup;
        }

        private void Update()
        {
            if (powerup != null)
            {
                if (Input.GetButtonDown("Left Mouse Button"))
                {
                    hud.PowerupSlot.Useable = false;
                    powerup.Activate(this);
                }

                powerup.PlayerUpdate(this);
            }
        }
    }
}
