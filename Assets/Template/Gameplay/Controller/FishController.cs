using UnityEngine;
using Template.Gameplay.Model;
using Template.Core;

namespace Template.Gameplay.Controller
{
    [RequireComponent(typeof(FishStatus))]
    public class FishController : MonoBehaviour
    {
        [SerializeField] private FixedJoystick joystick;
        [SerializeField] private RectTransform targetRect;
        [SerializeField] private float maxSpeed = 300f;
        [SerializeField] private float acceleration = 400f;
        [SerializeField] private float deceleration = 300f;
        [SerializeField] private bool clampToParent = true;
        [SerializeField] private bool flipHorizontally = true;
        [SerializeField] private float flipDeadZone = 5f;
        [SerializeField] private float visibleEdgePixelsX = 12f;
        [SerializeField] private float visibleEdgePixelsY = 12f;
        [SerializeField] private FishStatus fishStatus;
        [SerializeField] private bool destroyEnemyOnEat = true;
        [SerializeField] private bool disableOnGameOver = true;

        private RectTransform parentRect;
        private Canvas canvas;
        private Vector2 currentVelocity = Vector2.zero;
        private Vector3 originalScale;
        private Vector2 initialAnchoredPosition;
        private bool movementLocked = false;

        public void Initialize() {}

        public void ResetState(Vector2? startAnchoredPosition = null, int startLevel = 1)
        {
            if (fishStatus == null) fishStatus = GetComponent<FishStatus>();
            fishStatus?.ResetProgress(startLevel);
            currentVelocity = Vector2.zero;
            enabled = true;
            if (joystick != null)
            {
                // Force joystick back to neutral so it doesn't remain tilted after game over
                joystick.OnPointerUp(null);
            }
            movementLocked = true;
            if (targetRect == null)
            {
                targetRect = GetComponent<RectTransform>();
                if (targetRect == null)
                    targetRect = GetComponentInChildren<RectTransform>(true);
            }
            if (targetRect != null)
            {
                targetRect.anchoredPosition = startAnchoredPosition.HasValue ? startAnchoredPosition.Value : initialAnchoredPosition;
            }
            else
            {
                Debug.LogError("FishController: targetRect is not assigned and no RectTransform was found on self/children.");
                return;
            }
        }

        private void Awake()
        {
            if (targetRect == null)
            {
                targetRect = GetComponent<RectTransform>();
                if (targetRect == null)
                    targetRect = GetComponentInChildren<RectTransform>(true);
            }

            if (targetRect != null)
            {
                parentRect = targetRect.parent as RectTransform;
                canvas = targetRect.GetComponentInParent<Canvas>();
                originalScale = targetRect.localScale;
                initialAnchoredPosition = targetRect.anchoredPosition;
            }
            if (fishStatus == null)
                fishStatus = GetComponent<FishStatus>();
        }

        private void Update()
        {
            if (joystick == null) return;
            if (targetRect == null) return;

            Vector2 input = joystick.Direction;
            if (input.sqrMagnitude > 1f) input.Normalize();

            float dt = Time.deltaTime;
            Vector2 desiredVelocity = input * maxSpeed;
            float step = ((input.sqrMagnitude > 0.0001f) ? acceleration : deceleration) * dt;
            if (movementLocked)
            {
                // Stay still until new input is applied after reset/gameover
                if (input.sqrMagnitude > 0.0001f)
                {
                    movementLocked = false;
                }
                else
                {
                    currentVelocity = Vector2.zero;
                    return;
                }
            }
            currentVelocity = Vector2.MoveTowards(currentVelocity, desiredVelocity, step);
            Vector2 delta = currentVelocity * dt;

            Vector2 newPos = targetRect.anchoredPosition + delta;

            if (clampToParent && parentRect != null)
            {
                Vector2 parentHalf = parentRect.rect.size * 0.5f;
                Vector2 selfHalf = targetRect.rect.size * 0.5f;
                float overflowX = Mathf.Max(0f, selfHalf.x - Mathf.Max(0f, visibleEdgePixelsX));
                float overflowY = Mathf.Max(0f, selfHalf.y - Mathf.Max(0f, visibleEdgePixelsY));

                float minX = -parentHalf.x + selfHalf.x - overflowX;
                float maxX = parentHalf.x - selfHalf.x + overflowX;
                float minY = -parentHalf.y + selfHalf.y - overflowY;
                float maxY = parentHalf.y - selfHalf.y + overflowY;

                newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
                newPos.y = Mathf.Clamp(newPos.y, minY, maxY);
            }

            targetRect.anchoredPosition = newPos;

            if (flipHorizontally)
            {
                float vx = currentVelocity.x;
                if (Mathf.Abs(vx) > flipDeadZone)
                {
                    // Initial sprite faces left at originalScale.x.
                    // Face right when moving right by flipping X.
                    float sign = (vx > 0f) ? -1f : 1f;
                    Vector3 scale = targetRect.localScale;
                    scale.x = Mathf.Abs(originalScale.x) * sign;
                    targetRect.localScale = scale;
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (fishStatus == null) return;

            EnemyStatus enemy = other.GetComponent<EnemyStatus>();
            if (enemy != null)
            {
                Debug.Log("Enemy eaten: " + enemy.Level);
                Debug.Log("My level: " + fishStatus.Level);
                int myLevel = fishStatus.Level;
                int enemyLevel = enemy.Level;
                if (myLevel >= enemyLevel)
                {
                    // Enemy is edible: count as food based on enemy level
                    fishStatus.AddFood(Mathf.Max(1, enemyLevel));
                    if (destroyEnemyOnEat)
                    {
                        Destroy(enemy.gameObject);
                    }
                }
                else
                {
                    Debug.Log("Game Over: Bigger enemy ate the player.");
                    GameManager.Instance.LoseGame();
                    if (joystick != null)
                    {
                        // Ensure joystick returns to neutral on game over
                        joystick.OnPointerUp(null);
                    }
                    movementLocked = true;
                }
                return;
            }

            if (other.CompareTag("Food"))
            {
                fishStatus.AddFood(1);
                Destroy(other.gameObject);
            }
        }
    }
}


