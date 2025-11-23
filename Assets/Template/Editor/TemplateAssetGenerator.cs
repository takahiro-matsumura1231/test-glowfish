using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Template.Editor
{
    public static class TemplateAssetGenerator
    {
        [MenuItem("Assets/Template/Create Prefabs/Fish")]
        public static void CreateFishPrefab() => CreateSpritePrefab("Fish");

        [MenuItem("Assets/Template/Create Prefabs/Enemy")]
        public static void CreateEnemyPrefab() => CreateSpritePrefab("Enemy");

        [MenuItem("Assets/Template/Create Prefabs/Food")]
        public static void CreateFoodPrefab() => CreateSpritePrefab("Food");

        [MenuItem("Assets/Template/Create UI/SkinSettingsView")]
        public static void CreateSkinSettingsView()
        {
            var root = new GameObject("SkinSettingsView");
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            root.AddComponent<CanvasScaler>();
            root.AddComponent<GraphicRaycaster>();

            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(root.transform, false);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(600, 400);

            var title = new GameObject("Title", typeof(RectTransform), typeof(Text));
            title.transform.SetParent(panel.transform, false);
            var titleText = title.GetComponent<Text>();
            titleText.text = "Settings";
            titleText.alignment = TextAnchor.MiddleCenter;
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0, -20);
            titleRect.sizeDelta = new Vector2(400, 60);

            EnsureDirectory("Assets/Template/UI/Settings");
            var path = "Assets/Template/UI/Settings/SkinSettingsView.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            AssetDatabase.Refresh();
        }

        private static void CreateSpritePrefab(string name)
        {
            var go = new GameObject(name);
            go.AddComponent<SpriteRenderer>();

            var dir = $"Assets/Template/Prefabs/{name}";
            EnsureDirectory(dir);
            var path = $"{dir}/{name}.prefab";
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            AssetDatabase.Refresh();
        }

        private static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                AssetDatabase.Refresh();
            }
        }
    }
}


