using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using EmojiUI;

public static class CreateSpriteAsset
{
    [MenuItem("Assets/Create/Sprite Asset",false,10)]
    static void CreateSpriteAssets()
    {
        string[] guids =  Selection.assetGUIDs;
        for(int i =0; i < guids.Length;++i)
        {
            string guid = guids[i];
            string filepath = AssetDatabase.GUIDToAssetPath(guid);
            GenerateSpriteInfo(filepath);
        }

        AssetDatabase.Refresh();
    }

    static void GenerateSpriteInfo(string unitypath)
    {
        Texture2D sourceTex = AssetDatabase.LoadAssetAtPath<Texture2D>(unitypath);
        if(sourceTex != null)
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
                SpriteInfo seeked = allSprites.Find(p => p.tag == sprite.name);
                if(seeked == null)
                {
                    SpriteInfo temp = new SpriteInfo();
                    temp.sprite = sprite;
                    temp.tag = sprite.name;
                    temp.uv = GetSpriteUV(texelSize, sprite.rect);
                    allSprites.Add(temp);
                }
            }

        }

        for (int i = 0; i < allSprites.Count; i++)
        {
            SpriteInfo info = allSprites[i];

            SpriteInfoGroup _tempGroup = new SpriteInfoGroup();
            _tempGroup.tag = info.tag;
            _tempGroup.size = Mathf.Max(info.sprite.rect.size.x, info.sprite.rect.size.y);
            _tempGroup.width =1;
            _tempGroup.spritegroups.Add(info);

            _listGroup.Add(_tempGroup);
        }

        return _listGroup;
    }

    private static Vector2[] GetSpriteUV(Vector2 texSize,Rect _sprRect)
    {
        Vector2[] uv = new Vector2[4];
        uv[0] = new Vector2(_sprRect.x / texSize.x, (_sprRect.y+_sprRect.height) / texSize.y);
        uv[1] = new Vector2((_sprRect.x + _sprRect.width) / texSize.x, (_sprRect.y +_sprRect.height) / texSize.y);
        uv[2] = new Vector2((_sprRect.x + _sprRect.width) / texSize.x, _sprRect.y / texSize.y);
        uv[3] = new Vector2(_sprRect.x / texSize.x, _sprRect.y / texSize.y);
        return uv;
    }
    
}
