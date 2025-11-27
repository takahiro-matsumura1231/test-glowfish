using UnityEngine;
using TMPro;
using DG.Tweening;
using Template.Core;

namespace Template.Gameplay.Controller
{
	public class PraisePopupManager : MonoBehaviourSingleton<PraisePopupManager>
	{
		[Header("Parent/Canvas")]
		[SerializeField] private RectTransform popupParent;
		[SerializeField] private Canvas canvas;
		[SerializeField] private Camera uiCamera;

		[Header("Style")]
		[SerializeField] private TMP_FontAsset fontAsset;
		[SerializeField] private int fontSize = 64;
		[SerializeField] private Color baseColor = new Color(1f, 0.95f, 0.4f, 1f);
		[SerializeField] private Color outlineColor = new Color(0.15f, 0.05f, 0f, 1f);
		[SerializeField] private float outlineWidth = 0.2f;

		[Header("Animation")]
		[SerializeField] private float appearScale = 0.85f;
		[SerializeField] private float appearDuration = 0.12f;
		[SerializeField] private float holdDuration = 0.35f;
		[SerializeField] private float moveUpDistance = 80f;
		[SerializeField] private float fadeOutDuration = 0.35f;
		[SerializeField] private Ease appearEase = Ease.OutBack;
		[SerializeField] private Ease moveEase = Ease.OutSine;
		[SerializeField] private Ease fadeEase = Ease.InSine;

		[Header("Texts")]
		[SerializeField] private string[] praiseWords = new[] { "Yummy!", "Delicious!", "Tasty!", "Nice!" };

		protected override void SingletonAwakened()
		{
			if (canvas == null)
			{
				canvas = GetComponentInParent<Canvas>();
			}
			if (canvas != null && uiCamera == null)
			{
				uiCamera = canvas.renderMode == RenderMode.ScreenSpaceCamera ? canvas.worldCamera : null;
			}
			if (popupParent == null && canvas != null)
			{
				popupParent = canvas.transform as RectTransform;
			}
		}

		private void OnEnable()
		{
			EventBus.OnEnemyEaten += HandleEnemyEaten;
		}

		private void OnDisable()
		{
			EventBus.OnEnemyEaten -= HandleEnemyEaten;
		}

		private void HandleEnemyEaten(Vector3 worldPosition, int enemyLevel)
		{
			if (popupParent == null) return;

			Vector2 localPoint;
			Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, worldPosition);
			if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(popupParent, screenPoint, uiCamera, out localPoint))
			{
				return;
			}

			string text = SelectPraiseWord(enemyLevel);
			SpawnText(localPoint, text, enemyLevel);
		}

		private string SelectPraiseWord(int enemyLevel)
		{
			if (praiseWords == null || praiseWords.Length == 0) return "Yummy!";
			if (enemyLevel >= 3) return "Delicious!";
			if (enemyLevel == 2) return "Yummy!";
			int idx = Random.Range(0, praiseWords.Length);
			return praiseWords[idx];
		}

		private void SpawnText(Vector2 anchoredPosition, string content, int enemyLevel)
		{
			GameObject go = new GameObject("PraiseText", typeof(RectTransform));
			go.transform.SetParent(popupParent, false);
			RectTransform rt = go.GetComponent<RectTransform>();
			rt.anchoredPosition = anchoredPosition;
			rt.sizeDelta = new Vector2(600f, 200f);

			TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
			tmp.text = content;
			tmp.font = fontAsset != null ? fontAsset : tmp.font;
			tmp.fontSize = fontSize;
			tmp.alignment = TextAlignmentOptions.Center;
			tmp.color = baseColor;
			tmp.enableWordWrapping = false;
			tmp.enableAutoSizing = false;
			tmp.outlineWidth = outlineWidth;
			tmp.outlineColor = outlineColor;

			CanvasGroup cg = go.AddComponent<CanvasGroup>();
			cg.alpha = 0f;

			Vector3 startScale = Vector3.one * appearScale;
			rt.localScale = startScale;

			Sequence seq = DOTween.Sequence();
			seq.Append(cg.DOFade(1f, appearDuration).SetEase(appearEase));
			seq.Join(rt.DOScale(1f, appearDuration).SetEase(appearEase));

			float moveUp = moveUpDistance * Mathf.Lerp(1f, 1.4f, Mathf.Clamp01(enemyLevel - 1) / 2f);
			Vector2 targetPos = anchoredPosition + new Vector2(0f, moveUp);
			seq.AppendInterval(holdDuration);
			seq.Append(rt.DOAnchorPos(targetPos, fadeOutDuration).SetEase(moveEase));
			seq.Join(cg.DOFade(0f, fadeOutDuration).SetEase(fadeEase));
			seq.OnComplete(() =>
			{
				if (go != null) Destroy(go);
			});
		}
	}
}


