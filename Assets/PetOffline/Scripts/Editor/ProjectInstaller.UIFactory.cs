using UnityEngine;
using UnityEngine.UI;

namespace PetOffline.Editor
{
    internal static class ProjectInstallerUiFactory
    {
        public static GameObject CreateRect(Transform parent, string name)
        {
            var gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            return gameObject;
        }

        public static void Stretch(GameObject target)
        {
            RectTransform rect = (RectTransform)target.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        public static void Place(
            GameObject target,
            Vector2 anchor,
            Vector2 position,
            Vector2 size)
        {
            RectTransform rect = (RectTransform)target.transform;
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        public static Image CreateImage(
            Transform parent,
            string name,
            Sprite sprite,
            Vector2 anchor,
            Vector2 position,
            Vector2 size,
            Color color)
        {
            GameObject target = CreateRect(parent, name);
            Place(target, anchor, position, size);
            Image image = target.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.preserveAspect = sprite != null;
            return image;
        }

        public static Image CreateStretchImage(
            Transform parent,
            string name,
            Sprite sprite,
            Color color)
        {
            GameObject target = CreateRect(parent, name);
            Stretch(target);
            Image image = target.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            return image;
        }

        public static Text CreateText(
            Transform parent,
            string name,
            string value,
            Font font,
            int fontSize,
            Vector2 anchor,
            Vector2 position,
            Vector2 size,
            TextAnchor alignment)
        {
            GameObject target = CreateRect(parent, name);
            Place(target, anchor, position, size);
            Text text = target.AddComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.text = value;
            text.alignment = alignment;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        public static Button CreateButton(
            Transform parent,
            string name,
            string label,
            Sprite sprite,
            Font font,
            Vector2 anchor,
            Vector2 position,
            Vector2 size)
        {
            GameObject target = CreateRect(parent, name);
            Place(target, anchor, position, size);
            Image image = target.AddComponent<Image>();
            image.sprite = sprite;
            image.color = sprite == null ? new Color(0.18f, 0.22f, 0.3f, 0.95f) : Color.white;
            Button button = target.AddComponent<Button>();
            button.targetGraphic = image;
            bool useLightText = sprite != null && sprite.name.ToLowerInvariant().Contains("blue");
            AddButtonLabel(target.transform, label, font, useLightText);
            return button;
        }

        public static GameObject CreateModal(Transform parent, string name, Color color)
        {
            GameObject modal = CreateRect(parent, name);
            Stretch(modal);
            Image image = modal.AddComponent<Image>();
            image.color = color;
            return modal;
        }

        private static void AddButtonLabel(
            Transform parent,
            string label,
            Font font,
            bool useLightText)
        {
            GameObject target = CreateRect(parent, "Label");
            Stretch(target);
            RectTransform rect = target.GetComponent<RectTransform>();
            rect.offsetMin = new Vector2(94f, 0f);
            rect.offsetMax = new Vector2(-18f, 0f);
            Text text = target.AddComponent<Text>();
            text.font = font;
            text.fontSize = label.Length >= 14 ? 24 : 30;
            text.fontStyle = FontStyle.Bold;
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.color = useLightText
                ? new Color(0.88f, 0.91f, 0.98f, 1f)
                : new Color(0.12f, 0.14f, 0.2f, 1f);
            text.raycastTarget = false;
        }
    }
}
