using UnityEngine;
using UnityEditor;
using System.Xml;
using System.Collections.Generic;
using System.IO;

public class PlistAtlasImporter : EditorWindow
{
    private TextAsset plistFile;
    private Texture2D atlasTexture;

    [MenuItem("Tools/Plist Atlas Importer")]
    public static void ShowWindow()
    {
        GetWindow<PlistAtlasImporter>("Plist Atlas Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Plist Atlas Importer", EditorStyles.boldLabel);

        plistFile = (TextAsset)EditorGUILayout.ObjectField("Plist File", plistFile, typeof(TextAsset), false);
        atlasTexture = (Texture2D)EditorGUILayout.ObjectField("Atlas Texture", atlasTexture, typeof(Texture2D), false);

        if (GUILayout.Button("Import Atlas") && plistFile != null && atlasTexture != null)
        {
            ImportAtlas();
        }
    }

    private void ImportAtlas()
    {
        string path = AssetDatabase.GetAssetPath(atlasTexture);
        TextureImporter textureImporter = (TextureImporter)TextureImporter.GetAtPath(path);

        textureImporter.textureType = TextureImporterType.Sprite;
        textureImporter.spriteImportMode = SpriteImportMode.Multiple;

        List<SpriteMetaData> spriteMetaDataList = new List<SpriteMetaData>();

        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(plistFile.text);

        XmlNode dictNode = xmlDoc.SelectSingleNode("plist/dict/dict");

        foreach (XmlNode keyNode in dictNode.SelectNodes("key"))
        {
            string spriteName = keyNode.InnerText;
            XmlNode frameDict = keyNode.NextSibling;

            var frameStr = frameDict.SelectSingleNode("string").InnerText;
            Rect rect = ParseFrame(frameStr, atlasTexture.height);

            SpriteMetaData metaData = new SpriteMetaData();
            metaData.name = spriteName;
            metaData.rect = rect;
            metaData.pivot = new Vector2(0.5f, 0.5f);
            metaData.alignment = (int)SpriteAlignment.Center;

            spriteMetaDataList.Add(metaData);
        }

        textureImporter.spritesheet = spriteMetaDataList.ToArray();
        EditorUtility.SetDirty(textureImporter);
        textureImporter.SaveAndReimport();

        Debug.Log($"✅ Imported {spriteMetaDataList.Count} sprites from plist.");
    }

    Rect ParseFrame(string frameStr, int textureHeight)
    {
        // Example format: "{{x,y},{w,h}}"
        frameStr = frameStr.Replace("{", "").Replace("}", "");
        string[] parts = frameStr.Split(',');

        float x = float.Parse(parts[0]);
        float y = float.Parse(parts[1]);
        float width = float.Parse(parts[2]);
        float height = float.Parse(parts[3]);

        // Flip Y axis to Unity's coordinate system
        y = textureHeight - y - height;

        return new Rect(x, y, width, height);
    }
}
