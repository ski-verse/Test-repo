using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class GradientHudRuntimeUpdater : MonoBehaviour
{
    private static readonly Vector2 GradientTextPosition = new Vector2(28f, -178f);
    private bool configured;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallGradientHudUpdater()
    {
        if (Object.FindFirstObjectByType<GradientHudRuntimeUpdater>() != null)
        {
            return;
        }

        var updater = new GameObject("Gradient HUD Runtime Updater");
        updater.AddComponent<GradientHudRuntimeUpdater>();
    }

    private void Start()
    {
        configured = ConfigureIfAvailable();
    }

    private void Update()
    {
        if (!configured)
        {
            configured = ConfigureIfAvailable();
        }
    }

    public static bool EnsureGradientText(SpeedDistanceDisplay display)
    {
        if (display == null)
        {
            return false;
        }

        if (display.gradientText != null)
        {
            return true;
        }

        display.gradientText = CreateGradientText(display.transform);
        return true;
    }

    private static bool ConfigureIfAvailable()
    {
        return EnsureGradientText(Object.FindFirstObjectByType<SpeedDistanceDisplay>());
    }

    private static TextMeshProUGUI CreateGradientText(Transform parent)
    {
        var textObject = new GameObject("Gradient Text");
        textObject.transform.SetParent(parent, false);

        var text = textObject.AddComponent<TextMeshProUGUI>();
        text.fontSize = 36f;
        text.fontStyle = FontStyles.Bold;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.TopLeft;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;
        text.text = string.Empty;

        var rectTransform = text.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.anchoredPosition = GradientTextPosition;
        rectTransform.sizeDelta = new Vector2(520f, 48f);

        return text;
    }
}
