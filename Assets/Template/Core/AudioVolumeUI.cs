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
			if (!initializeFromMixerOnStart) return;
			var audio = AudioManager.Instance;
			if (audio == null) return;
			if (bgmSlider != null)
			{
				float v = audio.GetBGMVolume01();
				bgmSlider.SetValueWithoutNotify(v);
			}
			if (seSlider != null)
			{
				float v = audio.GetSEVolume01();
				seSlider.SetValueWithoutNotify(v);
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


