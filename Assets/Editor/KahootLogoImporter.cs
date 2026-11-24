using UnityEditor;
using UnityEngine;

public sealed class KahootLogoImporter : AssetPostprocessor {
    void OnPostprocessTexture(Texture2D texture) {
        if (assetPath != "Assets/Fonts/KahootLogo.png") return;
        var pixels = texture.GetPixels32();
        for (int i = 0; i < pixels.Length; i++) {
            var p = pixels[i];
            if (p.g > 215 && Mathf.Abs(p.b - p.g) < 12 && Mathf.Abs(p.r - p.g) < 12)
                pixels[i] = new Color32(255, 255, 255, 255);
        }
        texture.SetPixels32(pixels);
        texture.Apply(false, false);
    }
}
