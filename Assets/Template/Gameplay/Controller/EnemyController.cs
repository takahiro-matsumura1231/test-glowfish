using UnityEngine;
using UnityEngine.UI;
using Template.Gameplay.Model;

namespace Template.Gameplay.Controller
{
    public class EnemyController : MonoBehaviour
    {
        public enum HorizontalDirection { LeftToRight, RightToLeft }

        [SerializeField] private RectTransform targetRect;
        [SerializeField] private float speed = 250f;
        [SerializeField] private HorizontalDirection direction = HorizontalDirection.RightToLeft;
        [SerializeField] private float despawnMarginPixels = 20f;
        [SerializeField] private bool flipHorizontally = true;

        [Header("Edible Visuals")]
        [SerializeField] private bool showEdibleOutline = true;
        [SerializeField] private Color edibleOutlineColor = new Color(0.2f, 1.0f, 0.2f, 1.0f);
        [SerializeField, Min(0f)] private float edibleOutlineSize = 6f;
        [SerializeField] private bool outlineUseGraphicAlpha = true;
        [SerializeField] private bool animateEdibleOutline = true;
        [SerializeField, Min(0.01f)] private float edibleOutlinePulseSpeed = 2.2f;
        [SerializeField, Range(0f, 1f)] private float pulseAlphaMultiplierMin = 0.35f;
        [SerializeField, Range(0f, 1f)] private float pulseAlphaMultiplierMax = 1.0f;
        [SerializeField, Min(0f)] private float pulseSizeMultiplierMin = 0.75f;
        [SerializeField, Min(0f)] private float pulseSizeMultiplierMax = 1.35f;

        private RectTransform parentRect;
        private Vector2 moveDir = Vector2.left;
        private Vector3 originalScale;

        private EnemyStatus enemyStatus;
        private Outline edibleOutline;
        private FishStatus subscribedFishStatus;
        private bool started;
        private static FishStatus cachedPlayerFishStatus;
        private bool edibleNow;

        public void Initialize() {}

        private void Awake()
        {
            if (targetRect == null)
                targetRect = GetComponent<RectTransform>();
            if (targetRect != null)
            {
                parentRect = targetRect.parent as RectTransform;
                originalScale = targetRect.localScale;
            }

            enemyStatus = GetComponent<EnemyStatus>();
            EnsureEdibleOutline();
            ApplyDirection(direction);
        }

        private void Start()
        {
            started = true;
            TrySubscribeToPlayerLevel();
            // Initial refresh should happen in Start, because EnemySpawner may override EnemyStatus level after Instantiate().
            RefreshEdibleOutline();
        }

        private void OnEnable()
        {
            TrySubscribeToPlayerLevel();
            // If re-enabled later (not just after Instantiate), ensure visuals are correct.
            if (started) RefreshEdibleOutline();
        }

        private void OnDisable()
        {
            UnsubscribeFromPlayerLevel();
        }

        private void Update()
        {
            if (targetRect == null) return;

            float dt = Time.deltaTime;
            Vector2 delta = moveDir * speed * dt;
            Vector2 newPos = targetRect.anchoredPosition + delta;
            targetRect.anchoredPosition = newPos;

            ApplyEdibleOutlinePulseIfNeeded();

            if (parentRect != null)
            {
                Vector2 parentHalf = parentRect.rect.size * 0.5f;
                Vector2 selfHalf = targetRect.rect.size * 0.5f;
                float rightExitX = parentHalf.x + selfHalf.x + despawnMarginPixels;
                float leftExitX = -parentHalf.x - selfHalf.x - despawnMarginPixels;

                if (moveDir.x > 0f && newPos.x > rightExitX)
                {
                    Destroy(gameObject);
                }
                else if (moveDir.x < 0f && newPos.x < leftExitX)
                {
                    Destroy(gameObject);
                }
            }
        }

        public void Setup(HorizontalDirection newDirection, float newSpeed)
        {
            direction = newDirection;
            speed = newSpeed;
            ApplyDirection(direction);
        }

        private void ApplyDirection(HorizontalDirection dir)
        {
            moveDir = (dir == HorizontalDirection.LeftToRight) ? Vector2.right : Vector2.left;
            if (flipHorizontally && targetRect != null)
            {
                // Assume original sprite faces left; flip on X when moving right to face travel.
                float sign = (moveDir.x > 0f) ? -1f : 1f;
                Vector3 scale = targetRect.localScale;
                scale.x = Mathf.Abs(originalScale.x) * sign;
                targetRect.localScale = scale;
            }
        }

        private void EnsureEdibleOutline()
        {
            if (!showEdibleOutline) return;
            if (edibleOutline == null || edibleOutline.Equals(null))
            {
                edibleOutline = GetComponent<Outline>();
                if (edibleOutline == null || edibleOutline.Equals(null))
                {
                    // Outline requires a Graphic (Image) on the same GameObject. Enemy prefabs are UGUI Images, so this is safe.
                    edibleOutline = gameObject.AddComponent<Outline>();
                }
            }

            edibleOutline.effectColor = edibleOutlineColor;
            edibleOutline.effectDistance = new Vector2(edibleOutlineSize, edibleOutlineSize);
            edibleOutline.useGraphicAlpha = outlineUseGraphicAlpha;
            edibleOutline.enabled = false;
            edibleNow = false;
        }

        private static FishStatus GetPlayerFishStatus()
        {
            if (cachedPlayerFishStatus != null && !cachedPlayerFishStatus.Equals(null))
            {
                return cachedPlayerFishStatus;
            }
            cachedPlayerFishStatus = FindObjectOfType<FishStatus>();
            return cachedPlayerFishStatus;
        }

        private void TrySubscribeToPlayerLevel()
        {
            FishStatus fish = GetPlayerFishStatus();
            if (fish == null || fish.Equals(null)) return;
            if (ReferenceEquals(subscribedFishStatus, fish)) return;

            UnsubscribeFromPlayerLevel();
            subscribedFishStatus = fish;
            subscribedFishStatus.LevelChanged += HandlePlayerLevelChanged;
        }

        private void UnsubscribeFromPlayerLevel()
        {
            if (subscribedFishStatus == null || subscribedFishStatus.Equals(null)) return;
            subscribedFishStatus.LevelChanged -= HandlePlayerLevelChanged;
            subscribedFishStatus = null;
        }

        private void HandlePlayerLevelChanged(int _newLevel)
        {
            RefreshEdibleOutline();
        }

        private void RefreshEdibleOutline()
        {
            if (edibleOutline == null || edibleOutline.Equals(null))
            {
                EnsureEdibleOutline();
            }
            if (!showEdibleOutline || edibleOutline == null || edibleOutline.Equals(null))
            {
                return;
            }

            if (enemyStatus == null || enemyStatus.Equals(null))
            {
                enemyStatus = GetComponent<EnemyStatus>();
                if (enemyStatus == null || enemyStatus.Equals(null))
                {
                    edibleOutline.enabled = false;
                    edibleNow = false;
                    return;
                }
            }

            FishStatus fish = GetPlayerFishStatus();
            if (fish == null || fish.Equals(null))
            {
                edibleOutline.enabled = false;
                edibleNow = false;
                return;
            }

            bool edible = fish.Level >= enemyStatus.Level;
            edibleOutline.enabled = edible;
            edibleNow = edible;

            if (!edible)
            {
                // Reset to base appearance when not edible (and when disabling pulse).
                edibleOutline.effectColor = edibleOutlineColor;
                edibleOutline.effectDistance = new Vector2(edibleOutlineSize, edibleOutlineSize);
            }
        }

        private void ApplyEdibleOutlinePulseIfNeeded()
        {
            if (!animateEdibleOutline) return;
            if (!edibleNow) return;
            if (!showEdibleOutline) return;
            if (edibleOutline == null || edibleOutline.Equals(null)) return;
            if (!edibleOutline.enabled) return;

            // 0..1 smooth pulse
            float t01 = (Mathf.Sin(Time.time * edibleOutlinePulseSpeed) + 1f) * 0.5f;

            float alphaMul = Mathf.Lerp(pulseAlphaMultiplierMin, pulseAlphaMultiplierMax, t01);
            float sizeMul = Mathf.Lerp(pulseSizeMultiplierMin, pulseSizeMultiplierMax, t01);

            Color c = edibleOutlineColor;
            c.a *= alphaMul;
            edibleOutline.effectColor = c;

            float size = edibleOutlineSize * sizeMul;
            edibleOutline.effectDistance = new Vector2(size, size);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Enemy doesn't actively eat; resolution is handled in FishController.
            // This method can be kept empty or used for additional effects if needed.
            FishStatus fish = other.GetComponent<FishStatus>();
            if (fish != null)
            {
                // Let FishController handle the resolution.
            }
        }
    }
}


