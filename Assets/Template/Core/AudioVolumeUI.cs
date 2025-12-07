using UnityEngine;
using UnityEngine.UI;

namespace Template.Core
{
	// Attach this to the same GameObject as AudioManager (or anywhere in the UI).
	// Assign bgmSlider and seSlider in the inspector.
	public class AudioVolumeUI : MonoBehaviour
	{
		[SerializeField] private Slider bgmSlider;
		[SerializeField] private Slider seSlider;
		[SerializeField] private bool initializeFromMixerOnStart = true;
		[SerializeField] private float initialBGMVolume = 0.5f;
		[SerializeField] private float initialSEVolume = 0.5f;

		private void OnEnable()
		{
			if (bgmSlider != null)
			{
				bgmSlider.onValueChanged.AddListener(OnBGMChanged);
			}
			if (seSlider != null)
			{
				seSlider.onValueChanged.AddListener(OnSEChanged);
			}
		}

		private void Start()
		{
			var audio = AudioManager.Instance;
			if (audio == null) return;

			if (bgmSlider != null)
			{
				float v;
				if (initializeFromMixerOnStart)
				{
					v = audio.GetBGMVolume01();
					// If mixer value is 0 or invalid, use initial value
					if (v <= 0f) v = Mathf.Clamp01(initialBGMVolume);
				}
				else
				{
					v = Mathf.Clamp01(initialBGMVolume);
				}
				bgmSlider.SetValueWithoutNotify(v);
				audio.SetBGMVolume01(v);
			}

			if (seSlider != null)
			{
				float v;
				if (initializeFromMixerOnStart)
				{
					v = audio.GetSEVolume01();
					// If mixer value is 0 or invalid, use initial value
					if (v <= 0f) v = Mathf.Clamp01(initialSEVolume);
				}
				else
				{
					v = Mathf.Clamp01(initialSEVolume);
				}
				seSlider.SetValueWithoutNotify(v);
				audio.SetSEVolume01(v);
			}
		}

		private void OnDisable()
		{
			if (bgmSlider != null)
			{
				bgmSlider.onValueChanged.RemoveListener(OnBGMChanged);
			}
			if (seSlider != null)
			{
				seSlider.onValueChanged.RemoveListener(OnSEChanged);
			}
		}

		// These can also be hooked directly from Slider.OnValueChanged in the Inspector
		public void OnBGMChanged(float normalized01)
		{
			AudioManager.Instance?.SetBGMVolume01(normalized01);
		}

		public void OnSEChanged(float normalized01)
		{
			AudioManager.Instance?.SetSEVolume01(normalized01);
		}
	}
}


