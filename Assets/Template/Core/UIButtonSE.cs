using UnityEngine;
using UnityEngine.UI;

namespace Template.Core
{
	[RequireComponent(typeof(Button))]
	public class UIButtonSE : MonoBehaviour
	{
		[SerializeField] private AudioClip customClickClip;
		[SerializeField] private float volume = 1f;

		private Button button;

		private void Awake()
		{
			button = GetComponent<Button>();
		}

		private void OnEnable()
		{
			if (button != null)
			{
				button.onClick.AddListener(OnClicked);
			}
		}

		private void OnDisable()
		{
			if (button != null)
			{
				button.onClick.RemoveListener(OnClicked);
			}
		}

		private void OnClicked()
		{
			if (customClickClip != null)
			{
				AudioManager.Instance?.PlaySE(customClickClip, volume);
			}
			else
			{
				AudioManager.Instance?.PlayButtonClickSE();
			}
		}
	}
}


