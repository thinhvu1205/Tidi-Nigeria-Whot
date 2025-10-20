using UnityEngine;
using UnityEditor;
using System.Xml;
using System.Collections.Generic;
using System.IO;

public class PlistAtlasImporterAndSplitter : EditorWindow
{
    private TextAsset plistFile;
    private Texture2D atlasTexture;

    [MenuItem("Tools/Plist Atlas Importer And Splitter")]
    public static void ShowWindow()
    {
        GetWindow<PlistAtlasImporterAndSplitter>("Plist Atlas Importer & Splitter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Plist Atlas Importer & Splitter", EditorStyles.boldLabel);
        plistFile = (TextAsset)EditorGUILayout.ObjectField("Plist File", plistFile, typeof(TextAsset), false);
        atlasTexture = (Texture2D)EditorGUILayout.ObjectField("Atlas Texture", atlasTexture, typeof(Texture2D), false);

        if (GUILayout.Button("Import & Split") && plistFile != null && atlasTexture != null)
        {
            ImportAndSplit();
        }
    }

    private void ImportAndSplit()
    {
        // --- Parse plist ---
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(plistFile.text);

        XmlNode dictNode = xmlDoc.SelectSingleNode("plist/dict/dict");
        if (dictNode == null)
        {
            Debug.LogError("❌ Không tìm thấy node 'plist/dict/dict' trong file plist.");
            return;
        }

        // --- Setup texture importer ---
        string atlasPath = AssetDatabase.GetAssetPath(atlasTexture);
        TextureImporter textureImporter = (TextureImporter)TextureImporter.GetAtPath(atlasPath);
        textureImporter.textureType = TextureImporterType.Sprite;
        textureImporter.spriteImportMode = SpriteImportMode.Multiple;

        List<SpriteMetaData> metaList = new List<SpriteMetaData>();

        foreach (XmlNode keyNode in dictNode.SelectNodes("key"))
        {
            string spriteName = keyNode.InnerText;
            XmlNode frameDict = keyNode.NextSibling;

            XmlNode frameNode = frameDict.SelectSingleNode("string");
            if (frameNode == null) continue;

            Rect rect = ParseFrame(frameNode.InnerText, atlasTexture.height);

            SpriteMetaData meta = new SpriteMetaData
            {
                name = spriteName,
                rect = rect,
                pivot = new Vector2(0.5f, 0.5f),
                alignment = (int)SpriteAlignment.Center
            };
            metaList.Add(meta);
        }

        textureImporter.spritesheet = metaList.ToArray();
        EditorUtility.SetDirty(textureImporter);
        textureImporter.SaveAndReimport();

        Debug.Log($"✅ Imported {metaList.Count} sprites from plist.");

        // --- Split & Export sprites ---
        ExportSprites(atlasTexture, plistFile);
    }

    private Rect ParseFrame(string frameStr, int texHeight)
    {
        // Format: "{{x,y},{w,h}}"
        frameStr = frameStr.Replace("{", "").Replace("}", "");
        string[] parts = frameStr.Split(',');

        float x = float.Parse(parts[0]);
        float y = float.Parse(parts[1]);
        float width = float.Parse(parts[2]);
        float height = float.Parse(parts[3]);

        // Unity Y-axis flip
        y = texHeight - y - height;

        return new Rect(x, y, width, height);
    }

    private void ExportSprites(Texture2D atlas, TextAsset plist)
    {
        string atlasPath = AssetDatabase.GetAssetPath(atlas);
        string exportDir = Path.GetDirectoryName(atlasPath);

        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(atlasPath);

        int count = 0;
        foreach (Object obj in assets)
        {
            if (obj is Sprite sprite)
            {
                Texture2D newTex = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height, TextureFormat.RGBA32, false);
                Color[] pixels = sprite.texture.GetPixels(
                    (int)sprite.rect.x,
                    (int)sprite.rect.y,
                    (int)sprite.rect.width,
                    (int)sprite.rect.height
                );

                newTex.SetPixels(pixels);
                newTex.Apply();

                byte[] pngData = newTex.EncodeToPNG();
                string filePath = Path.Combine(exportDir, sprite.name + ".png");
                File.WriteAllBytes(filePath, pngData);
                count++;
            }
        }

        AssetDatabase.Refresh();
        Debug.Log($"✅ Exported {count} sprites to: {exportDir}");
    }
}
