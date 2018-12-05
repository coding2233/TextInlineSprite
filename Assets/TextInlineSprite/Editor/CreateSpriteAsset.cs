using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using EmojiUI;
using System.Linq;

public static class CreateSpriteAsset
{
    public const string assetPath = "Assets/Resources/Emoji/Emoji.png";

    [MenuItem("Assets/Create/Sprite Asset",false,10)]
    static void CreateSpriteAssets()
    {
        var textures = Selection.GetFiltered<Texture2D>(SelectionMode.Assets);
        var list = new List<Texture2D>(textures);
        list.Sort((lt, rt) => lt.name.CompareTo(rt.name));
        textures = list.ToArray();
        var atlas = CreateSprite(textures);
        var names = list.Select(t => t.name).ToArray();
        var path = AutoSpriteSlicer.ProcessTexture(atlas, names);

        GenerateSpriteInfo(path);
        AssetDatabase.Refresh();
    }

    static Texture2D CreateSprite(Texture2D[] textures)
    {
        // 计算所有图片的总面积
        int size = 0;
        for (int i = 0; i < textures.Length; i++)
        {
            size += textures[i].width * textures[i].height;
        }

        // 计算最终生成的整张图的最小长宽，默认图集为最小2的N次方正方形
        int power = Mathf.NextPowerOfTwo(size);
        int w = Mathf.CeilToInt(Mathf.Sqrt(power));
        w = Mathf.NextPowerOfTwo(w);

        // make your new texture
        var atlas = new Texture2D(w, w, TextureFormat.RGBA32, false);
        // clear pixel
        Color32[] fillColor = atlas.GetPixels32();
        for (int i = 0; i < fillColor.Length; ++i)
            fillColor[i] = Color.clear;
        atlas.SetPixels32(fillColor);

        int textureWidthCounter = 0;
        int textureHeightCounter = 0;
        for (int i = 0; i < textures.Length; i++)
        {
            // 填充单个图片的像素到 Atlas 中
            for (int j = 0; j < textures[i].width; j++)
            {
                for (int k = 0; k < textures[i].height; k++)
                {
                    atlas.SetPixel(j + textureWidthCounter, k + textureHeightCounter, textures[i].GetPixel(j, k));
                }
            }

            textureWidthCounter += textures[i].width;
            if (textureWidthCounter >= atlas.width)
            {
                textureWidthCounter = 0;
                textureHeightCounter += textures[i].height;
            }
        }

        var path = assetPath;
        var tex = SaveSpriteToEditorPath(atlas, path);

        return tex;
    }

    static Texture2D SaveSpriteToEditorPath(Texture2D sp, string path)
    {
        string dir = Path.GetDirectoryName(path);

        Directory.CreateDirectory(dir);

        File.WriteAllBytes(path, sp.EncodeToPNG());
        AssetDatabase.Refresh();
        AssetDatabase.AddObjectToAsset(sp, path);
        AssetDatabase.SaveAssets();

        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;

        ti.textureType = TextureImporterType.Default;
        //ti.spritePixelsPerUnit = sp.pixelsPerUnit;
        ti.spritePackingTag = "Emoji";
        ti.npotScale = TextureImporterNPOTScale.None;
        ti.alphaIsTransparency = true;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.mipmapEnabled = false;
        EditorUtility.SetDirty(ti);
        ti.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
    }

    static void GenerateSpriteInfo(string unitypath)
    {
        Texture2D sourceTex = AssetDatabase.LoadAssetAtPath<Texture2D>(unitypath);
        if (sourceTex != null)
        {
            string Extensionname = Path.GetExtension(unitypath);
            string filePath = unitypath.Replace(Extensionname, ".asset");

            SpriteAsset spriteAsset = AssetDatabase.LoadAssetAtPath(filePath, typeof(SpriteAsset)) as SpriteAsset;
            int tag = 0;
            if (spriteAsset != null)
            {
                tag = spriteAsset.ID;
            }

            //replace
            spriteAsset = ScriptableObject.CreateInstance<SpriteAsset>();
            spriteAsset.ID = tag;
            spriteAsset.AssetName = Path.GetFileNameWithoutExtension(filePath);
            spriteAsset.texSource = sourceTex;
            spriteAsset.listSpriteGroup = GetAssetSpriteInfor(sourceTex);
            AssetDatabase.CreateAsset(spriteAsset, filePath);
        }
    }

    static List<SpriteInfoGroup> GetAssetSpriteInfor(Texture2D tex)
    {
        string filePath = AssetDatabase.GetAssetPath(tex);
        Object[] objects = AssetDatabase.LoadAllAssetsAtPath(filePath);

        List<SpriteInfoGroup> _listGroup = new List<SpriteInfoGroup>();
        List<SpriteInfo> allSprites = new List<SpriteInfo>();

        Vector2 texelSize = new Vector2(tex.width, tex.height);
        for (int i = 0; i < objects.Length; i++)
        {
            Sprite sprite = objects[i] as Sprite;

            if (sprite != null)
            {
                SpriteInfo temp = new SpriteInfo
                {
                    sprite = sprite,
                    tag = sprite.name.Split('_')[0],
                    uv = GetSpriteUV(texelSize, sprite.rect)
                };
                allSprites.Add(temp);
            }
        }

        for (int i = 0; i < allSprites.Count; i++)
        {
            SpriteInfo info = allSprites[i];

            var _tempGroup = _listGroup.Find(p => p.tag == info.tag);
            if (_tempGroup == null)
            {
                _tempGroup = new SpriteInfoGroup
                {
                    tag = info.tag,
                    size = Mathf.Max(info.sprite.rect.size.x, info.sprite.rect.size.y),
                    width = 1
                };
                _listGroup.Add(_tempGroup);
            }

            _tempGroup.spritegroups.Add(info);
        }

        return _listGroup;
    }

    private static Vector2[] GetSpriteUV(Vector2 texSize, Rect _sprRect)
    {
        Vector2[] uv = new Vector2[4];
        uv[0] = new Vector2(_sprRect.x / texSize.x, (_sprRect.y + _sprRect.height) / texSize.y);
        uv[1] = new Vector2((_sprRect.x + _sprRect.width) / texSize.x, (_sprRect.y + _sprRect.height) / texSize.y);
        uv[2] = new Vector2((_sprRect.x + _sprRect.width) / texSize.x, _sprRect.y / texSize.y);
        uv[3] = new Vector2(_sprRect.x / texSize.x, _sprRect.y / texSize.y);
        return uv;
    }
}
