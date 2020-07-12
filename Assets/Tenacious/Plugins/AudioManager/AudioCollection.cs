using UnityEngine;

using System.Collections.Generic;

namespace Tenacious.Audio
{
    [CreateAssetMenu(fileName = "AudioCollection", menuName = "Tenacious/Audio/AudioCollection")]
    public class AudioCollection : ScriptableObject
    {
        [Tooltip("List of music audio (do not modify in editor while playing)")]
        [SerializeField] private List<Audio> musicList;

        [Tooltip("List of sound audio (do not modify in editor while playing)")]
        [SerializeField] private List<Audio> soundList;

        private Dictionary<string, int> musicDictionary;
        private Dictionary<string, int> soundDictionary;

        protected virtual void OnEnable()
        {
            if (musicList == null) musicList = new List<Audio>();
            if (soundList == null) soundList = new List<Audio>();

            musicDictionary = new Dictionary<string, int>();
            for (int i = 0; i < musicList.Count; i++)
            {
                if (musicList[i].audioClip != null)
                    musicDictionary.Add(musicList[i].audioClip.name, i);
            }

            soundDictionary = new Dictionary<string, int>();
            for (int i = 0; i < soundList.Count; i++)
            {
                if (soundList[i].audioClip != null)
                    soundDictionary.Add(soundList[i].audioClip.name, i);
            }
        }

        public virtual Audio GetMusic(string name)
        {
            if (musicDictionary.ContainsKey(name))
                return musicList[musicDictionary[name]];
            else
            {
                Debug.LogError("AudioCollection : Music with AudioClip name '" + name + "' not found in music list.");
                return null;
            }
        }

        public virtual Audio GetSound(string name)
        {
            if (soundDictionary.ContainsKey(name))
                return soundList[soundDictionary[name]];
            else
            {
                Debug.LogError("AudioCollection : Sound with AudioClip name '" + name + "' not found in sound list.");
                return null;
            }
        }


    }
}
