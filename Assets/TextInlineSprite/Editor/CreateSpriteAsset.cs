using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public static class CreateSpriteAsset
{
    [MenuItem("Assets/Create/UGUI Sprite Asset",false,10)]
    static void main()
    {
        Object target = Selection.activeObject;
        if (target == null || target.GetType() != typeof(Texture2D))
            return;

        Texture2D sourceTex = target as Texture2D;
        //整体路径
        string filePathWithName = AssetDatabase.GetAssetPath(sourceTex);
        //带后缀的文件名
        string fileNameWithExtension = Path.GetFileName(filePathWithName);
        //不带后缀的文件名
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePathWithName);
        //不带文件名的路径
        string filePath = filePathWithName.Replace(fileNameWithExtension, "");

        SpriteAsset spriteAsset = AssetDatabase.LoadAssetAtPath(filePath + fileNameWithoutExtension + ".asset", typeof(SpriteAsset)) as SpriteAsset;
        bool isNewAsset = spriteAsset == null ? true : false;
        if (isNewAsset)
        {
            spriteAsset = ScriptableObject.CreateInstance<SpriteAsset>();
            spriteAsset.texSource = sourceTex;
            spriteAsset.listSpriteInfor = GetSpritesInfor(sourceTex);
            spriteAsset.listSpriteGroup = GetAssetSpriteInfor(sourceTex);
            AssetDatabase.CreateAsset(spriteAsset, filePath + fileNameWithoutExtension + ".asset");
        }
    }
    public static List<SpriteInfor> GetSpritesInfor(Texture2D tex)
    {
        List<SpriteInfor> m_sprites = new List<SpriteInfor>();

        string filePath = UnityEditor.AssetDatabase.GetAssetPath(tex);

        Object[] objects = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(filePath);

        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i].GetType() == typeof(Sprite))
            {
                SpriteInfor temp = new SpriteInfor();
                Sprite sprite = objects[i] as Sprite;
                temp.ID = i;
                temp.name = sprite.name;
                temp.pivot = sprite.pivot;
                temp.rect = sprite.rect;
                temp.sprite = sprite;
                m_sprites.Add(temp);
            }
        }
        return m_sprites;
    }
    public static List<SpriteInforGroup> GetAssetSpriteInfor(Texture2D tex)
    {
        List<SpriteInforGroup> _listGroup = new List<SpriteInforGroup>();
        string filePath = UnityEditor.AssetDatabase.GetAssetPath(tex);

        Object[] objects = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(filePath);

        List<SpriteInfor> _tempSprite = new List<SpriteInfor>();

        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i].GetType() != typeof(Sprite))
                continue;
                SpriteInfor temp = new SpriteInfor();
                Sprite sprite = objects[i] as Sprite;
                temp.ID = i;
                temp.name = sprite.name;
                temp.pivot = sprite.pivot;
                temp.rect = sprite.rect;
                temp.sprite = sprite;
                temp.tag = sprite.name;
                 _tempSprite.Add(temp);
        }

        for (int i = 0; i < _tempSprite.Count; i++)
        {
            SpriteInforGroup _tempGroup = new SpriteInforGroup();
            _tempGroup.tag = _tempSprite[i].tag;
            _tempGroup.listSpriteInfor = new List<SpriteInfor>();
            _tempGroup.listSpriteInfor.Add(_tempSprite[i]);
            for (int j = 0; j < _tempSprite.Count; j++)
            {
                if (j > i && _tempGroup.tag == _tempSprite[j].tag)
                {
                    _tempGroup.listSpriteInfor.Add(_tempSprite[j]);
                    _tempSprite.RemoveAt(j);
                    j--;
                }
            }
            _listGroup.Add(_tempGroup);
            _tempSprite.RemoveAt(i);
            i--;
        }

        return _listGroup;
    }
    
}
