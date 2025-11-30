using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Template.Gameplay.Model;

namespace Template.Gameplay.Controller
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Image))]
	public class ImageSpriteColliderSync : MonoBehaviour
	{
		[SerializeField] private PolygonCollider2D polygonCollider;
		[SerializeField] private bool addColliderIfMissing = true;
		[SerializeField] private bool autoSyncOnStart = true;
		[SerializeField] private bool makeTriggerIfUnset = true;

		private Image image;
		private readonly List<Vector2> shapeBuffer = new List<Vector2>(128);
		private float initialWidth;
		private bool initialWidthSet = false;
		private FishStatus fishStatus;
		
		// Level-based initial widths for fish
		private static readonly float[] InitialWidthByLevel = { 200f, 300f, 600f }; // Level 1, 2, 3

		private void Awake()
		{
			image = GetComponent<Image>();
			if (polygonCollider == null && addColliderIfMissing)
			{
				polygonCollider = GetComponent<PolygonCollider2D>();
				if (polygonCollider == null) polygonCollider = gameObject.AddComponent<PolygonCollider2D>();
			}
			// If there's an existing BoxCollider2D, mirror its trigger state; otherwise default to trigger for trigger-based logic
			var box = GetComponent<BoxCollider2D>();
			if (polygonCollider != null)
			{
				if (box != null)
				{
					polygonCollider.isTrigger = box.isTrigger;
				}
				else if (makeTriggerIfUnset)
				{
					polygonCollider.isTrigger = true;
				}
			}
			// Try to find FishStatus on this object or parent
			if (fishStatus == null)
			{
				fishStatus = GetComponent<FishStatus>();
				if (fishStatus == null && transform.parent != null)
				{
					fishStatus = transform.parent.GetComponent<FishStatus>();
				}
			}
			
			// Save initial width for aspect ratio preservation
			if (image != null && !initialWidthSet)
			{
				var rt = image.rectTransform;
				// If fish, use level-based width; otherwise use current width
				if (fishStatus != null)
				{
					initialWidth = GetInitialWidthForLevel();
				}
				else
				{
					initialWidth = rt.rect.width;
				}
				initialWidthSet = true;
			}
		}

		private void Start()
		{
			if (autoSyncOnStart) SyncNow();
		}

		public void SyncNow()
		{
			if (image == null) image = GetComponent<Image>();
			if (image == null || image.sprite == null || polygonCollider == null) return;

			Sprite sprite = image.sprite;
			
			// Get FishStatus if not already cached
			if (fishStatus == null)
			{
				fishStatus = GetComponent<FishStatus>();
				if (fishStatus == null && transform.parent != null)
				{
					fishStatus = transform.parent.GetComponent<FishStatus>();
				}
			}
			
			// Update RectTransform size based on sprite aspect ratio
			var rt = image.rectTransform;
			if (!initialWidthSet)
			{
				// If fish, use level-based width; otherwise use current width
				if (fishStatus != null)
				{
					initialWidth = GetInitialWidthForLevel();
				}
				else
				{
					initialWidth = rt.rect.width;
				}
				initialWidthSet = true;
			}
			else if (fishStatus != null)
			{
				// If fish and level might have changed, update initial width
				initialWidth = GetInitialWidthForLevel();
			}
			
			// Calculate aspect ratio from sprite
			Vector2 spritePxSize = sprite.rect.size; // sprite pixels
			if (spritePxSize.x > 0f && spritePxSize.y > 0f)
			{
				float aspectRatio = spritePxSize.x / spritePxSize.y;
				float newHeight = initialWidth / aspectRatio;
				rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, initialWidth);
				rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);
			}
			
			int shapeCount = sprite.GetPhysicsShapeCount();
			if (shapeCount <= 0)
			{
				// No physics shape authored on sprite → disable polygon to avoid wrong collisions
				polygonCollider.enabled = false;
				return;
			}

			// Map sprite physics shape (sprite local) -> RectTransform local (pixels relative to pivot) to match UI Image display
			Vector2 rectSize = rt.rect.size; // in UI pixels
			float ppu = sprite.pixelsPerUnit <= 0 ? 100f : sprite.pixelsPerUnit;
			Vector2 scale = new Vector2(
				(spritePxSize.x > 0f) ? (rectSize.x / spritePxSize.x) : 1f,
				(spritePxSize.y > 0f) ? (rectSize.y / spritePxSize.y) : 1f
			);

			polygonCollider.enabled = true;
			polygonCollider.pathCount = shapeCount;
			for (int i = 0; i < shapeCount; i++)
			{
				shapeBuffer.Clear();
				sprite.GetPhysicsShape(i, shapeBuffer); // points in sprite local units (world units), origin at sprite pivot
				// Convert: sprite units -> sprite pixels -> rect local pixels (relative to pivot) -> collider local units
				for (int p = 0; p < shapeBuffer.Count; p++)
				{
					Vector2 pt = shapeBuffer[p];           // sprite local units
					Vector2 ptPx = pt * ppu;               // sprite pixels
					Vector2 ptRelPivotPx = ptPx;           // since pt already relative to pivot per docs
					Vector2 ptRect = new Vector2(ptRelPivotPx.x * scale.x, ptRelPivotPx.y * scale.y); // rect local pixels
					shapeBuffer[p] = ptRect;
				}
				polygonCollider.SetPath(i, shapeBuffer);
			}
			// Keep offset at zero (paths already relative to pivot)
		}
		
		private float GetInitialWidthForLevel()
		{
			int level = 1; // default to level 1
			if (fishStatus != null)
			{
				level = fishStatus.Level;
			}
			int index = Mathf.Clamp(level - 1, 0, InitialWidthByLevel.Length - 1);
			return InitialWidthByLevel[index];
		}
	}
}


