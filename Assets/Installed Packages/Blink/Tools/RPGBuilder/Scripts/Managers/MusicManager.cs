using System;
using System.Collections;
using System.Collections.Generic;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.Templates;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BLINK.RPGBuilder.Managers
{
    public class MusicManager : MonoBehaviour
    {
        public static MusicManager Instance { get; private set; }

        public float fadeSpeed = 0.5f;
        public AudioSource audioSource;
        public float normalMusicVolume = 0.3f;

        private Coroutine musicFadeCoroutine;

        public bool dynamicMusicEnabled = true;
        private void Awake()
        {
            if (Instance != null) return;
            Instance = this;
        }

        public void InitializeSceneMusic()
        {
            if (Camera.main == null) return;
            AudioSource camAudioSource = Camera.main.gameObject.GetComponent<AudioSource>();
            audioSource = camAudioSource != null ? camAudioSource : Camera.main.gameObject.AddComponent<AudioSource>();
            audioSource.volume = normalMusicVolume;
        }

        public void HandleMusicFadeCoroutine(AudioClip clip)
        {
            if (musicFadeCoroutine != null) StopCoroutine(musicFadeCoroutine);
            musicFadeCoroutine = StartCoroutine(PlayGameMusic(clip));
        }

        void FixedUpdate()
        {
            if (!RPGBuilderEssentials.Instance.isInGame || !dynamicMusicEnabled) return;
            HandleNextMusic();
        }

        private void HandleNextMusic()
        {
            if (audioSource == null || audioSource.isPlaying || RegionManager.Instance.CurrentRegion == null) return;
            if (RegionManager.Instance.CurrentRegion.musicClips.Count == 0) return;
            HandleMusicFadeCoroutine(RegionManager.Instance.CurrentRegion.musicClips[
                GETRandomMusicIndex(0, RegionManager.Instance.CurrentRegion.musicClips.Count)]);
        }

        public int GETRandomMusicIndex(int min, int max)
        {
            return Random.Range(min, max);
        }

        private IEnumerator PlayGameMusic(AudioClip clip)
        {
            if(audioSource == null) yield break;
            if (audioSource.clip != null)
            {
                while (Math.Abs(audioSource.volume - 0f) > 0.05f)
                {
                    audioSource.volume = Mathf.Lerp(audioSource.volume, 0f, Time.deltaTime * fadeSpeed);
                    yield return new WaitForEndOfFrame();
                }
            }
            else
            {
                audioSource.volume = 0;
            }

            audioSource.clip = clip;
            audioSource.Play();
            
            while (audioSource != null && Math.Abs(audioSource.volume - normalMusicVolume) > 0.05f)
            {
                audioSource.volume = Mathf.Lerp(audioSource.volume, normalMusicVolume, Time.deltaTime * fadeSpeed);
                yield return new WaitForEndOfFrame();
            }

            if(audioSource == null) yield break;
            audioSource.volume = normalMusicVolume;
        }

        public void StopGameMusic()
        {
            audioSource.Stop();
        }
    }
}
