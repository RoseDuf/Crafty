using UnityEngine;

using Tenacious;
using Tenacious.UI;

namespace Game.UI
{
    [RequireComponent(typeof(WindowManager))]
    public class SystemUI : MBSingleton<SystemUI>
    {
        public WindowManager WindowManager
        {
            get { return this.GetComponent<WindowManager>(); }
        }
    }
}