using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace CARVES.Core
{
    public class AudioManager : MonoBehaviour
    {
        public enum Types
        {
            BGM,
            SFX
        }
        [SerializeField] protected AudioSource bgm;
        [SerializeField] protected AudioSource[] sfx;
        Coroutine PlayCo { get; set; }
        AudioSource GetAudioSource(Types type)
        {
            return type switch
            {
                Types.BGM => bgm,
                Types.SFX => sfx.FirstOrDefault(s=>!s.isPlaying),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
        public void Play(Types type, AudioClip clip, Action onCompleteCallback = null)
        {
            var audioSource = GetAudioSource(type);
            if (!audioSource || audioSource == null || audioSource.clip == clip) return;
            audioSource.clip = clip;
            if(PlayCo!=null)
            {
                StopCoroutine(PlayCo);
                PlayCo = null;
            }
            PlayCo = StartCoroutine(OnComplete());

            IEnumerator OnComplete()
            {
                audioSource.Play();
                if (audioSource.loop && onCompleteCallback != null)
                    throw new Exception("AudioSource is looped, onCompleteCallback will never be called");
                yield return new WaitWhile(() => audioSource.isPlaying);
                onCompleteCallback?.Invoke();
            }
        }
        public void Stop(Types type)=> GetAudioSource(type)?.Stop();

        public void SetVolume(Types type, float volume)
        {
            var audioSource = GetAudioSource(type);
            if (!audioSource) return;
            audioSource.volume = volume;
        }

        public void AddVolume(Types type, float volume)
        {
            var audioSource = GetAudioSource(type);
            if (!audioSource) return;
            audioSource.volume += volume;
        }

        public void MuteAll(bool mute)
        {
            bgm.mute = mute;
            foreach (var fx in sfx) 
                fx.mute = mute;
        }

        public void Mute(Types type, bool mute)
        {
            var audioSource = GetAudioSource(type);
            if (!audioSource) return;
            audioSource.mute = mute;
        }

        public void Pause(Types type, bool pause)
        {
            var audioSource = GetAudioSource(type);
            if (pause)
                audioSource?.Pause();
            else
                audioSource?.UnPause();
        }
        public void Set(Types type, Action<AudioSource> setFunc) => setFunc(GetAudioSource(type));
    }
}
