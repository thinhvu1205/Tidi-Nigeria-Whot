using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.TextCore;

using TMPro;
using litefeelcustom;

namespace Fnt2TMPro.EditorUtilities
{
    public class Fnt2TMPro : EditorWindow
    {
        [MenuItem("Window/Bitmap Font Converter")]

        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(Fnt2TMPro), false, "Bitmap Font Converter");
        }
        private Texture2D m_Texture2D;
        private TextAsset m_SourceFontFile;
        private TMP_FontAsset m_DestinationFontFile;
        
        void PatchGlyph(RawCharacterInfo character, int textureHeight, int textureWidth, ref Glyph g)
        {
            int x = character.X;
            int y = textureHeight - character.Y - character.Height; // Lật trục Y đúng với Unity
            int width = character.Width;
            int height = character.Height;

            g.glyphRect = new GlyphRect(x, y, width, height);
            g.metrics = new GlyphMetrics(
                width, 
                height, 
                character.Xoffset, 
                -character.Yoffset, 
                character.Xadvance
            );
        }
        
        
        void UpdateFont(TMP_FontAsset fontFile)
        {
            var fontText = m_SourceFontFile.text;
            var fnt = FntParse.GetFntParse(ref fontText);

            foreach (var character in fontFile.characterTable)
            {
                var unicode = character.unicode;
                RawCharacterInfo charInfo = default;
                bool found = false;

                foreach (var info in fnt.rawCharInfos)
                {
                    if (info.ID == unicode)
                    {
                        if (info.ID == 32)
                        {
                            Debug.Log("Found space character");
                        }
                        charInfo = info;
                        found = true;
                        break;
                    }
                }

                if (found)
                {
                    if (fontFile.glyphLookupTable.TryGetValue(character.glyphIndex, out var glyph))
                    {
                        PatchGlyph(charInfo, fnt.textureHeight, fnt.textureWidth, ref glyph);
                        fontFile.glyphLookupTable[character.glyphIndex] = glyph;
                    }
                }
            }

            
            fontFile.faceInfo = new FaceInfo
            {
                baseline = fnt.lineBaseHeight,
                lineHeight = fnt.lineHeight,
                ascentLine = fnt.lineHeight,
                pointSize = fnt.fontSize
            };

            fontFile.material.SetTexture("_MainTex", m_Texture2D);
            fontFile.atlasTextures[0] = m_Texture2D;
            fontFile.ReadFontAssetDefinition();
        }
        
        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            m_Texture2D = EditorGUILayout.ObjectField("Font Texture",
                m_Texture2D, typeof(Texture2D), false) as Texture2D;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            m_SourceFontFile = EditorGUILayout.ObjectField("Source Font File",
                m_SourceFontFile, typeof(TextAsset), false) as TextAsset;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            m_DestinationFontFile = EditorGUILayout.ObjectField("Destination Font File",
                m_DestinationFontFile, typeof(TMP_FontAsset), false) as TMP_FontAsset;
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Convert"))
            {
                UpdateFont(m_DestinationFontFile);
            }
        }
    }
}