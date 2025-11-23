using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Template.Core;

namespace Template.Gameplay.Controller
{
	public class LevelUpController : MonoBehaviourSingleton<LevelUpController>
	{
		[SerializeField] private RectTransform levelUpRoot;
		[SerializeField] private Sprite cutInBackgroundSprite;

		[SerializeField] private float bgEnterDuration = 0.35f;
		[SerializeField] private float bgExitDuration = 0.35f;
		[SerializeField] private float fishEnterDuration = 0.45f;
		[SerializeField] private float evolvePopScale = 1.2f;
		[SerializeField] private float evolvePopDuration = 0.2f;
		[SerializeField] private float fishExitDuration = 0.4f;
		[SerializeField] private float delayBeforeFishEnter = 0.1f;
		[SerializeField] private float delayBeforeEvolve = 0.1f;
		[SerializeField] private float delayBeforeFishExit = 0.25f;
		[SerializeField] private float delayBeforeBgExit = 0.1f;

		[SerializeField] private Font levelUpFont;
		[SerializeField] private int levelUpFontSize = 64;
		[SerializeField] private Color levelUpTextColor = Color.yellow;
		[SerializeField] private string levelUpText = "LEVEL UP!";

		// Reusable UI (avoid destroy → MissingReference in Editor)
		private RectTransform cachedBgRect;
		private CanvasGroup cachedBgGroup;
		private Image cachedBgImage;

		private RectTransform cachedFishRect;
		private CanvasGroup cachedFishGroup;
		private Image cachedFishImage;

		private RectTransform cachedTextRect;
		private CanvasGroup cachedTextGroup;
		private Text cachedText;
		private bool isPlaying = false;

		public void PlayLevelUp(Sprite beforeSprite, Sprite afterSprite, Vector2 beforeSize, Vector2 afterSize)
		{
			if (!Application.isPlaying) return;
			if (levelUpRoot == null || levelUpRoot.Equals(null)) return;
			if (isPlaying) return;
			StartCoroutine(PlayRoutine(beforeSprite, afterSprite, beforeSize, afterSize));
		}

		private void OnDisable()
		{
			// Safety: ensure timescale resumes if component disabled mid-animation
			if (isPlaying && Mathf.Approximately(Time.timeScale, 0f)) Time.timeScale = 1f;
			isPlaying = false;

			// Null out cached references to avoid MissingReference in Editor after domain reload
			cachedBgRect = null;
			cachedBgGroup = null;
			cachedBgImage = null;

			cachedFishRect = null;
			cachedFishGroup = null;
			cachedFishImage = null;

			cachedTextRect = null;
			cachedTextGroup = null;
			cachedText = null;
		}

		private IEnumerator PlayRoutine(Sprite beforeSprite, Sprite afterSprite, Vector2 beforeSize, Vector2 afterSize)
		{
			isPlaying = true;
			float prevTimeScale = Time.timeScale;
			Time.timeScale = 0f; // pause game while anim plays

			// Background (cut-in) - create or reuse
			if (cachedBgRect == null)
			{
				var bgGO = new GameObject("LevelUpBG", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
				cachedBgRect = bgGO.GetComponent<RectTransform>();
				cachedBgGroup = bgGO.GetComponent<CanvasGroup>();
				cachedBgImage = bgGO.GetComponent<Image>();
				cachedBgRect.SetParent(levelUpRoot, false);
				cachedBgRect.anchorMin = new Vector2(0.5f, 0.5f);
				cachedBgRect.anchorMax = new Vector2(0.5f, 0.5f);
			}
			if (cachedBgRect == null || cachedBgRect.Equals(null)) { isPlaying = false; Time.timeScale = prevTimeScale; yield break; }
			cachedBgRect.gameObject.SetActive(true);
			cachedBgRect.anchoredPosition = new Vector2(1800f, 0f);
			if (cachedBgImage != null && !cachedBgImage.Equals(null))
			{
				cachedBgImage.sprite = cutInBackgroundSprite;
				cachedBgImage.preserveAspect = true;
			}
			// Fit height to screen, width by sprite aspect (cover height)
			var parentRect = levelUpRoot as RectTransform;
			if (parentRect != null)
			{
				float h = parentRect.rect.height;
				float w = h;
				if (cachedBgImage != null && cachedBgImage.sprite != null && cachedBgImage.sprite.rect.height > 0f)
				{
					float aspect = cachedBgImage.sprite.rect.width / cachedBgImage.sprite.rect.height;
					w = h * aspect;
				}
				cachedBgRect.sizeDelta = new Vector2(w, h);
			}
			if (cachedBgGroup != null) cachedBgGroup.alpha = 0f;

			// Fish overlay - create or reuse
			if (cachedFishRect == null)
			{
				var fishGO = new GameObject("LevelUpFish", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
				cachedFishRect = fishGO.GetComponent<RectTransform>();
				cachedFishGroup = fishGO.GetComponent<CanvasGroup>();
				cachedFishImage = fishGO.GetComponent<Image>();
				cachedFishRect.SetParent(levelUpRoot, false);
			}
			if (cachedFishRect == null || cachedFishRect.Equals(null)) { isPlaying = false; Time.timeScale = prevTimeScale; yield break; }
			cachedFishRect.gameObject.SetActive(true);
			cachedFishRect.sizeDelta = (beforeSize != Vector2.zero) ? beforeSize : new Vector2(256f, 256f);
			cachedFishRect.anchoredPosition = new Vector2(900f, 0f);
			if (cachedFishImage != null && !cachedFishImage.Equals(null))
			{
				cachedFishImage.sprite = beforeSprite;
				cachedFishImage.preserveAspect = true;
			}
			if (cachedFishGroup != null) cachedFishGroup.alpha = 0f;

			// Text overlay - create or reuse
			if (cachedTextRect == null)
			{
				var textGO = new GameObject("LevelUpText", typeof(RectTransform), typeof(CanvasGroup), typeof(Text));
				cachedTextRect = textGO.GetComponent<RectTransform>();
				cachedTextGroup = textGO.GetComponent<CanvasGroup>();
				cachedText = textGO.GetComponent<Text>();
				cachedTextRect.SetParent(levelUpRoot, false);
				cachedTextRect.sizeDelta = new Vector2(800f, 160f);
			}
			if (cachedTextRect == null || cachedTextRect.Equals(null)) { isPlaying = false; Time.timeScale = prevTimeScale; yield break; }
			cachedTextRect.gameObject.SetActive(true);
			cachedTextRect.anchoredPosition = new Vector2(0f, -200f);
			if (cachedText != null && !cachedText.Equals(null))
			{
				cachedText.text = levelUpText;
				cachedText.alignment = TextAnchor.MiddleCenter;
				cachedText.color = levelUpTextColor;
				cachedText.fontSize = levelUpFontSize;
				cachedText.font = levelUpFont;
			}
			if (cachedTextGroup != null) cachedTextGroup.alpha = 0f;

			// Animate background enter (from x=1800 to 0), fade in
			if (cachedBgRect != null && cachedBgGroup != null)
				yield return StartCoroutine(AnimateMoveAndFade(cachedBgRect, cachedBgGroup, new Vector2(1800f, 0f), new Vector2(0f, 0f), 0f, 1f, bgEnterDuration));

			yield return new WaitForSecondsRealtime(delayBeforeFishEnter);

			// Fish enter from right and fade in
			if (cachedFishRect != null && cachedFishGroup != null)
				yield return StartCoroutine(AnimateMoveAndFade(cachedFishRect, cachedFishGroup, new Vector2(900f, 0f), new Vector2(0f, 0f), 0f, 1f, fishEnterDuration));

			yield return new WaitForSecondsRealtime(delayBeforeEvolve);

			// Evolve: swap sprite/size and do pop scale
			if (cachedFishImage != null && afterSprite != null) cachedFishImage.sprite = afterSprite;
			if (cachedFishRect != null && afterSize != Vector2.zero) cachedFishRect.sizeDelta = afterSize;

			if (cachedFishRect != null)
				yield return StartCoroutine(AnimatePopScale(cachedFishRect, evolvePopScale, evolvePopDuration));

			// Show text
			if (cachedTextGroup != null)
				yield return StartCoroutine(AnimateFade(cachedTextGroup, 0f, 1f, 0.25f));

			yield return new WaitForSecondsRealtime(delayBeforeFishExit);

			// Fish exit left and fade out
			if (cachedFishRect != null && cachedFishGroup != null)
				yield return StartCoroutine(AnimateMoveAndFade(cachedFishRect, cachedFishGroup, new Vector2(0f, 0f), new Vector2(-900f, 0f), 1f, 0f, fishExitDuration));

			yield return new WaitForSecondsRealtime(delayBeforeBgExit);

			// Background exit left and fade out
			if (cachedBgRect != null && cachedBgGroup != null)
				yield return StartCoroutine(AnimateMoveAndFade(cachedBgRect, cachedBgGroup, new Vector2(0f, 0f), new Vector2(-1800f, 0f), 1f, 0f, bgExitDuration));

			// Deactivate for reuse (avoid Destroy to prevent Editor MissingReference spam)
			if (cachedBgRect != null) cachedBgRect.gameObject.SetActive(false);
			if (cachedFishRect != null) cachedFishRect.gameObject.SetActive(false);
			if (cachedTextRect != null) cachedTextRect.gameObject.SetActive(false);

			Time.timeScale = prevTimeScale; // resume game
			isPlaying = false;
		}

		private IEnumerator AnimateMoveAndFade(RectTransform rect, CanvasGroup group, Vector2 from, Vector2 to, float aFrom, float aTo, float duration)
		{
			float t = 0f;
			rect.anchoredPosition = from;
			group.alpha = aFrom;
			while (t < duration)
			{
				t += Time.unscaledDeltaTime;
				float p = Mathf.Clamp01(t / duration);
				rect.anchoredPosition = Vector2.Lerp(from, to, p);
				group.alpha = Mathf.Lerp(aFrom, aTo, p);
				yield return null;
			}
			rect.anchoredPosition = to;
			group.alpha = aTo;
		}

		private IEnumerator AnimateFade(CanvasGroup group, float aFrom, float aTo, float duration)
		{
			float t = 0f;
			group.alpha = aFrom;
			while (t < duration)
			{
				t += Time.unscaledDeltaTime;
				float p = Mathf.Clamp01(t / duration);
				group.alpha = Mathf.Lerp(aFrom, aTo, p);
				yield return null;
			}
			group.alpha = aTo;
		}

		private IEnumerator AnimatePopScale(RectTransform rect, float popScale, float duration)
		{
			float half = duration * 0.5f;
			Vector3 baseScale = rect.localScale;
			// Up
			float t = 0f;
			while (t < half)
			{
				t += Time.unscaledDeltaTime;
				float p = Mathf.Clamp01(t / half);
				float s = Mathf.Lerp(1f, popScale, p);
				rect.localScale = baseScale * s;
				yield return null;
			}
			// Down
			t = 0f;
			while (t < half)
			{
				t += Time.unscaledDeltaTime;
				float p = Mathf.Clamp01(t / half);
				float s = Mathf.Lerp(popScale, 1f, p);
				rect.localScale = baseScale * s;
				yield return null;
			}
			rect.localScale = baseScale;
		}
	}
}


