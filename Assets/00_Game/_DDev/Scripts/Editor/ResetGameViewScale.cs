using System.Reflection;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class ResetGameViewScale
{
    private const float TargetScale = 0.33f;

    static ResetGameViewScale()
    {
        EditorApplication.delayCall += Apply;
    }

    private static void Apply()
    {
        var gameViewType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
        if (gameViewType == null) return;

        var zoomAreaField = gameViewType.GetField("m_ZoomArea", BindingFlags.NonPublic | BindingFlags.Instance);
        if (zoomAreaField == null) return;

        foreach (var window in Resources.FindObjectsOfTypeAll(gameViewType))
        {
            var zoomArea = zoomAreaField.GetValue(window);
            if (zoomArea == null) continue;

            var scaleField = zoomArea.GetType().GetField("m_Scale", BindingFlags.NonPublic | BindingFlags.Instance);
            if (scaleField == null) continue;

            scaleField.SetValue(zoomArea, new Vector2(TargetScale, TargetScale));
            ((EditorWindow)window).Repaint();
        }
    }
}
