using UnityEngine;
using UnityEngine.Audio;

namespace Template.Core
{
    public class AudioManager : MonoBehaviourSingleton<AudioManager>
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource seSource;
        [SerializeField] private AudioSource countdownSource; // カウントダウン用の専用AudioSource

        [Header("Audio Mixer Routing")]
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private AudioMixerGroup bgmGroup;
        [SerializeField] private AudioMixerGroup seGroup;
        [SerializeField] private string bgmVolumeParameter = "BGM";
        [SerializeField] private string seVolumeParameter = "SE";

		[Header("Default Clips")]
		[SerializeField] private AudioClip defaultBGMClip;
		[SerializeField] private AudioClip eatEnemySEClip;
		[SerializeField] private AudioClip eatFoodSEClip;
		[SerializeField] private float defaultBGMVolume = 1f;
		[SerializeField] private float defaultSEVolume = 1f;

		[Header("UI/Events Clips")]
		[SerializeField] private AudioClip buttonClickSEClip;
		[SerializeField] private AudioClip levelUpSEClip;
		[SerializeField] private AudioClip winSEClip;
		[SerializeField] private AudioClip loseSEClip;
		[SerializeField] private AudioClip countdownSEClip;
		[SerializeField] private AudioClip countdownEndSEClip;

        protected override void SingletonAwakened()
        {
            if (bgmSource == null)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
                bgmSource.playOnAwake = false;
                bgmSource.loop = true;
            }
            if (seSource == null)
            {
                seSource = gameObject.AddComponent<AudioSource>();
                seSource.playOnAwake = false;
                seSource.loop = false;
            }
            if (countdownSource == null)
            {
                countdownSource = gameObject.AddComponent<AudioSource>();
                countdownSource.playOnAwake = false;
                countdownSource.loop = true; // カウントダウンSEはループ再生
            }

            if (bgmGroup != null) bgmSource.outputAudioMixerGroup = bgmGroup;
            if (seGroup != null) seSource.outputAudioMixerGroup = seGroup;
            if (seGroup != null && countdownSource != null) countdownSource.outputAudioMixerGroup = seGroup;
        }

        public void PlayBGM(AudioClip clip, bool loop = true, float volume = 1f)
        {
            if (clip == null) return;
            bgmSource.clip = clip;
            bgmSource.loop = loop;
            bgmSource.volume = volume;
            bgmSource.Play();
        }

        public void StopBGM()
        {
            if (bgmSource.isPlaying) bgmSource.Stop();
        }

        public void PlaySE(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;
            seSource.PlayOneShot(clip, volume);
        }

		// Convenience wrappers
		public void PlayDefaultBGM()
		{
			PlayBGM(defaultBGMClip, true, defaultBGMVolume);
		}

		public void PlayEatEnemySE()
		{
			PlaySE(eatEnemySEClip, defaultSEVolume);
		}

		public void PlayEatFoodSE()
		{
			PlaySE(eatFoodSEClip, defaultSEVolume);
		}

		public void PlayButtonClickSE()
		{
			PlaySE(buttonClickSEClip, defaultSEVolume);
		}

		public void PlayLevelUpSE()
		{
			PlaySE(levelUpSEClip, defaultSEVolume);
		}

		public void PlayWinSE()
		{
			PlaySE(winSEClip, defaultSEVolume);
		}

		public void PlayLoseSE()
		{
			PlaySE(loseSEClip, defaultSEVolume);
		}

		/// <summary>
		/// カウントダウンSEをループ再生開始
		/// </summary>
		public void StartCountdownSE()
		{
			if (countdownSource == null || countdownSEClip == null) return;
			if (countdownSource.isPlaying) return; // 既に再生中の場合は何もしない
			countdownSource.clip = countdownSEClip;
			countdownSource.volume = defaultSEVolume;
			countdownSource.loop = true;
			countdownSource.Play();
		}

		/// <summary>
		/// カウントダウンSEを停止
		/// </summary>
		public void StopCountdownSE()
		{
			if (countdownSource == null) return;
			if (countdownSource.isPlaying)
			{
				countdownSource.Stop();
			}
		}

		/// <summary>
		/// カウントダウン終了SEを再生
		/// </summary>
		public void PlayCountdownEndSE()
		{
			PlaySE(countdownEndSEClip, defaultSEVolume);
		}

        // UI Slider bindings (0..1). Hook these from Slider OnValueChanged(float)
        public void SetBGMVolume01(float normalized)
        {
            SetMixerVolumeNormalized(bgmVolumeParameter, normalized);
        }

        public void SetSEVolume01(float normalized)
        {
            SetMixerVolumeNormalized(seVolumeParameter, normalized);
        }

        // Optional helpers to initialize sliders from current mixer values
        public float GetBGMVolume01()
        {
            return GetMixerVolumeNormalized(bgmVolumeParameter);
        }

        public float GetSEVolume01()
        {
            return GetMixerVolumeNormalized(seVolumeParameter);
        }

        private void SetMixerVolumeNormalized(string parameter, float normalized01)
        {
            if (audioMixer == null || string.IsNullOrEmpty(parameter)) return;
            float clamped = Mathf.Clamp01(normalized01);
            float dB = clamped <= 0.0001f ? -80f : Mathf.Log10(clamped) * 20f; // perceptual curve
            audioMixer.SetFloat(parameter, dB);
        }

        private float GetMixerVolumeNormalized(string parameter)
        {
            if (audioMixer == null || string.IsNullOrEmpty(parameter)) return 0f;
            if (!audioMixer.GetFloat(parameter, out float dB)) return 0f;
            // inverse of 20*log10(x), clamp to [0,1]
            float linear = Mathf.Pow(10f, dB / 20f);
            return Mathf.Clamp01(linear);
        }
    }
}


