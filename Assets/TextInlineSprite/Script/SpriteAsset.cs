using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpriteAsset : ScriptableObject
{
    /// <summary>
    /// 图片资源
    /// </summary>
    public Texture texSource;
    /// <summary>
    /// 所有sprite信息 SpriteAssetInfor类为具体的信息类
    /// </summary>
    public List<SpriteInfor> listSpriteInfor;

    public List<SpriteInforGroup> listSpriteGroup;

}

[System.Serializable]
public class SpriteInfor
{
    /// <summary>
    /// ID
    /// </summary>
    public int  ID;
    /// <summary>
    /// 名称
    /// </summary>
    public string name;
    /// <summary>
    /// 中心点
    /// </summary>
    public Vector2 pivot;
    /// <summary>
    ///坐标&宽高
    /// </summary>
    public Rect rect;
    /// <summary>
    /// 精灵
    /// </summary>
    public Sprite sprite;
    /// <summary>
    /// 表情大小
    /// </summary>
    public float size;
    /// <summary>
    /// 宽度比例0-1
    /// </summary>
    public float width;
    /// <summary>
    /// 标签
    /// </summary>
    public string tag;
}

[System.Serializable]
public class SpriteInforGroup
{
    public string tag;
    public List<SpriteInfor> listSpriteInfor;
}