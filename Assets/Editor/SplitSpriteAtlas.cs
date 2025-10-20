using UnityEngine;
using UnityEditor;
using System.IO;

public class SplitSpriteAtlas : EditorWindow
{
    private Texture2D atlasTexture;

    [MenuItem("Tools/Split Sprite Atlas")]
    public static void ShowWindow()
    {
        GetWindow<SplitSpriteAtlas>("Split Sprite Atlas");
    }

    private void OnGUI()
    {
        GUILayout.Label("Split Sprite Atlas to individual PNGs", EditorStyles.boldLabel);
        atlasTexture = (Texture2D)EditorGUILayout.ObjectField("Atlas Texture", atlasTexture, typeof(Texture2D), false);

        if (atlasTexture == null)
        {
            EditorGUILayout.HelpBox("Please assign a texture (with multiple sprites).", MessageType.Info);
            return;
        }

        if (GUILayout.Button("Export Sprites"))
        {
            ExportSprites(atlasTexture);
        }
    }

    private void ExportSprites(Texture2D atlas)
    {
        string path = AssetDatabase.GetAssetPath(atlas);
        string directory = Path.Combine(Path.GetDirectoryName(path), atlas.name + "_Split");

        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);

        foreach (Object obj in assets)
        {
            if (obj is Sprite sprite)
            {
                Texture2D newTex = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
                Color[] pixels = sprite.texture.GetPixels(
                    (int)sprite.rect.x,
                    (int)sprite.rect.y,
                    (int)sprite.rect.width,
                    (int)sprite.rect.height
                );

                newTex.SetPixels(pixels);
                newTex.Apply();

                byte[] pngData = newTex.EncodeToPNG();
                string filePath = Path.Combine(directory, sprite.name + ".png");
                File.WriteAllBytes(filePath, pngData);
            }
        }

        AssetDatabase.Refresh();
        Debug.Log($"✅ Exported sprites to: {directory}");
    }
}
