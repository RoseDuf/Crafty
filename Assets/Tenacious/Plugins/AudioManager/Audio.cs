using UnityEngine;

namespace Tenacious.Audio
{
    [System.Serializable]
    public class Audio
    {
        public AudioClip audioClip;
        public bool loop = true;
        public bool fade;
        public float volumeScale = 1f;
    }
}
