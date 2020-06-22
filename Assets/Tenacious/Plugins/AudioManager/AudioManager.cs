using UnityEngine;
using UnityEngine.Audio;

using System.Collections;

namespace Tenacious.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MBSingleton<AudioManager>
    {
        [SerializeField] private AudioMixer masterMixer;
        [SerializeField] private AudioMixerGroup masterGroup;
        [SerializeField] private AudioMixerGroup musicGroup;
        [SerializeField] private AudioMixerGroup soundGroup;

        [SerializeField] private AudioCollection audioCollection;

        [Tooltip("Default AudioSource for Music")]
        [SerializeField] private AudioSource musicSource;
        [Tooltip("Default AudioSource for Sound")]
        [SerializeField] private AudioSource soundSource;

        // variables used by the CRMusicFade coroutine
        private AudioClip musicFadeNewClip;
        private bool musicFadeRunning;

        private float Volume
        {
            get
            {
                float val;
                masterMixer.GetFloat("MasterVolume", out val);
                return val;
            }
            set { masterMixer.SetFloat("MasterVolume", value); }
        }
        private float MusicVolume
        {
            get
            {
                float val;
                masterMixer.GetFloat("MusicVolume", out val);
                return val;
            }
            set { masterMixer.SetFloat("MusicVolume", value); }
        }
        private float SoundVolume
        {
            get
            {
                float val;
                masterMixer.GetFloat("SoundVolume", out val);
                return val;
            }
            set { masterMixer.SetFloat("SoundVolume", value); }
        }

        private void Reset()
        {
            for (int i = 0; i < 2 - this.GetComponents<AudioSource>().Length; i++)
                gameObject.AddComponent<AudioSource>();
        }

        protected override void Awake()
        {
            if (musicSource != null)
               musicSource.priority = 0;
        }

        public void PlayMusic(string name, AudioSource source = null)
        {
            Audio musicAudio = audioCollection.GetMusic(name);
            if (musicAudio == null) return;

            if (source == null) source = musicSource;

            // do not restart if it's the same music clip
            if (source.clip != null && source.clip.name == musicAudio.audioClip.name) return;

            source.outputAudioMixerGroup = musicGroup;
            source.loop = musicAudio.loop;

            float targetVolume = source.volume;
            if (musicAudio.fade)
            {
                musicFadeNewClip = musicAudio.audioClip;
                if (!musicFadeRunning)
                    StartCoroutine(CRMusicFade(source, targetVolume));
            }
            else
            {
                StopMusic();
                source.clip = musicAudio.audioClip;
                source.Play();
            }
        }

        public void PlaySound(string name, AudioSource source = null)
        {
            Audio soundAudio = audioCollection.GetSound(name);
            if (soundAudio == null) return;

            if (source == null) source = soundSource;

            source.outputAudioMixerGroup = soundGroup;
            source.loop = soundAudio.loop;
            source.clip = soundAudio.audioClip;

            source.Stop();
            source.Play();
        }

        public void playSoundAtPosition(string name, Vector3 worldPosition, Transform parent = null)
        {
            if (parent == null) parent = this.transform;

            // create gameobject for the audio source
            GameObject audioObject = new GameObject();
            audioObject.transform.parent = parent;
            audioObject.name = "Sound-" + name;
            audioObject.transform.position = worldPosition;
            AudioSource source = audioObject.AddComponent<AudioSource>();

            source.volume = SoundVolume;
            PlaySound(name, source);

            // destroy on finish
            if (!source.loop && source.clip != null)
                Destroy(audioObject, source.clip.length);
        }

        public void StopMusic()
        {
            musicSource?.Stop();
        }

        public void PauseMusic()
        {
            musicSource?.Pause();
        }

        public void ResumeMusic()
        {
            musicSource?.UnPause();
        }

        private IEnumerator CRMusicFade(AudioSource source, float targetVolume)
        {
            musicFadeRunning = true;

            while (source.volume > 0f)
            {
                source.volume -= 0.1f;
                yield return new WaitForSeconds(0.05f);
            }

            source.Stop();
            source.clip = musicFadeNewClip;
            source.Play();

            while (source.volume < targetVolume)
            {
                source.volume += 0.1f;
                yield return new WaitForSeconds(0.05f);
            }

            musicFadeRunning = false;
        }
    }
}