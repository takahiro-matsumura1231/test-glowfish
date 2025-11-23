using UnityEngine;
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

        private RectTransform parentRect;
        private Vector2 moveDir = Vector2.left;
        private Vector3 originalScale;

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
            ApplyDirection(direction);
        }

        private void Update()
        {
            if (targetRect == null) return;

            float dt = Time.deltaTime;
            Vector2 delta = moveDir * speed * dt;
            Vector2 newPos = targetRect.anchoredPosition + delta;
            targetRect.anchoredPosition = newPos;

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


