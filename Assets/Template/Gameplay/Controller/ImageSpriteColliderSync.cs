using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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
			int shapeCount = sprite.GetPhysicsShapeCount();
			if (shapeCount <= 0)
			{
				// No physics shape authored on sprite → disable polygon to avoid wrong collisions
				polygonCollider.enabled = false;
				return;
			}

			// Map sprite physics shape (sprite local) -> RectTransform local (pixels relative to pivot) to match UI Image display
			var rt = image.rectTransform;
			Vector2 rectSize = rt.rect.size; // in UI pixels
			Vector2 spritePxSize = sprite.rect.size; // sprite pixels
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
	}
}


