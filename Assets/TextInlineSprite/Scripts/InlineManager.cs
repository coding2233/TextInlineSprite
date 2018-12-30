/// ========================================================
/// file：InlineManager.cs
/// brief：
/// author： coding2233
/// date：
/// version：v1.0
/// ========================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class InlineManager : MonoBehaviour {

    #region 属性
    //所有的精灵消息
    public Dictionary<int, Dictionary<string, SpriteInforGroup>> IndexSpriteInfo = new Dictionary<int, Dictionary<string, SpriteInforGroup>>();
    //绘制图集的索引
    private readonly Dictionary<int, SpriteGraphicInfo> _indexSpriteGraphic = new Dictionary<int, SpriteGraphicInfo>();
    //绘制的模型数据索引
    private Dictionary<int, Dictionary<InlineText, MeshInfo>> _textMeshInfo = new Dictionary<int, Dictionary<InlineText, MeshInfo>>();
    
    //图集
    [SerializeField]
    private List<SpriteGraphic02> _spriteGraphics = new List<SpriteGraphic02>();

	public readonly Dictionary<int, SpriteTagInfo> GetMeshInfo = new Dictionary<int, SpriteTagInfo>();

	private readonly Dictionary<int, Dictionary<InlineText, MeshInfo>> _graphicMeshInfo = new Dictionary<int, Dictionary<InlineText, MeshInfo>>();

    List<int> _renderIndexs = new List<int>();
   // private Queue<List<int>> _renderIndexs = new Queue<List<int>>();
	#endregion

	// Use this for initialization
	void OnEnable()
    {
        Initialize();
    }
	
    #region 初始化
    void Initialize()
    {
        for (int i = 0; i < _spriteGraphics.Count; i++)
        {
			SpriteAsset mSpriteAsset = _spriteGraphics[i].m_spriteAsset;
            if (!_indexSpriteGraphic.ContainsKey(mSpriteAsset.Id)&&!IndexSpriteInfo.ContainsKey(mSpriteAsset.Id))
            {
                SpriteGraphicInfo spriteGraphicInfo = new SpriteGraphicInfo()
                {
                    SpriteGraphic = _spriteGraphics[i],
                    Mesh = new Mesh(),
                };
                _indexSpriteGraphic.Add(mSpriteAsset.Id, spriteGraphicInfo);

                Dictionary<string, SpriteInforGroup> spriteGroup = new Dictionary<string, SpriteInforGroup>();
                foreach (var item in mSpriteAsset.ListSpriteGroup)
                {
                    if (!spriteGroup.ContainsKey(item.Tag) && item .ListSpriteInfor!=null&& item.ListSpriteInfor.Count > 0)
                        spriteGroup.Add(item.Tag, item);
                }
                IndexSpriteInfo.Add(mSpriteAsset.Id, spriteGroup);
                _textMeshInfo.Add(mSpriteAsset.Id, new Dictionary<InlineText, MeshInfo>());
            }              
        }
    }
    #endregion
    
    private void Update()
    {
        if (_renderIndexs != null && _renderIndexs.Count>0)
        {
            for (int i = 0; i < _renderIndexs.Count; i++)
            {
                int id = _renderIndexs[i];
                SpriteGraphic02 spriteGraphic02 = _spriteGraphics.Find(x => x.m_spriteAsset != null && x.m_spriteAsset.Id == id);
                if (spriteGraphic02 != null)
                {
					if (!_graphicMeshInfo.ContainsKey(id))
					{
						spriteGraphic02.MeshInfo = null;
						continue;
					}

					Dictionary<InlineText, MeshInfo> textMeshInfo = _graphicMeshInfo[id];
					if (textMeshInfo == null || textMeshInfo.Count == 0)
						spriteGraphic02.MeshInfo = null;
					else
					{
						MeshInfo meshInfo = Pool<MeshInfo>.Get();
						meshInfo.Clear();
						foreach (var item in textMeshInfo)
						{
							meshInfo.Vertices.AddRange(item.Value.Vertices);
							meshInfo.UVs.AddRange(item.Value.UVs);
						}
						if (spriteGraphic02.MeshInfo != null)
							Pool<MeshInfo>.Release(spriteGraphic02.MeshInfo);

						spriteGraphic02.MeshInfo = meshInfo;
					}
                }
            }
            //清掉渲染索引
            _renderIndexs.Clear();
        }
    }

    public void UpdateTextInfo(InlineText key, int id, List<SpriteTagInfo> value)
    {
		Dictionary<InlineText, MeshInfo> textMeshInfo;
		if (value == null)
		{
			if (_graphicMeshInfo.TryGetValue(id, out textMeshInfo) && textMeshInfo.ContainsKey(key))
			{
				textMeshInfo[key].Release();
				textMeshInfo.Remove(key);
			}
		}
		else
		{
			if (!_graphicMeshInfo.TryGetValue(id, out textMeshInfo))
			{
				textMeshInfo = new Dictionary<InlineText, MeshInfo>();
				_graphicMeshInfo.Add(id, textMeshInfo);
			}

			MeshInfo meshInfo;
			if (!textMeshInfo.TryGetValue(key, out meshInfo))
			{
				meshInfo = Pool<MeshInfo>.Get();
				textMeshInfo.Add(key, meshInfo);
			}
			meshInfo.Clear();
			for (int i = 0; i < value.Count; i++)
			{
				meshInfo.Vertices.AddRange(value[i].Pos);
				meshInfo.UVs.AddRange(value[i].UVs);
			}
		}

		//添加到渲染列表里面  --  等待下一帧渲染
		if (!_renderIndexs.Contains(id))
		{
			_renderIndexs.Add(id);
		}
	}
    

    #region 精灵组信息
    private class SpriteGraphicInfo
    {
        public SpriteGraphic02 SpriteGraphic;
        public Mesh Mesh;
    }
    #endregion

    
}

#region 模型数据信息
public class MeshInfo
{
    public List<Vector3> Vertices = ListPool<Vector3>.Get();
    public List<Vector2> UVs = ListPool<Vector2>.Get();
    public List<Color> Colors = ListPool<Color>.Get();
    public List<int> Triangles = ListPool<int>.Get();
    
    public void Clear()
    {
        Vertices.Clear();
        UVs.Clear();
        Colors.Clear();
        Triangles.Clear();
    }

    public void Release()
    {
        ListPool<Vector3>.Release(Vertices);
        ListPool<Vector2>.Release(UVs);
        ListPool<Color>.Release(Colors);
        ListPool<int>.Release(Triangles);

		Pool<MeshInfo>.Release(this);
    }
    
    //public string[] Tag;
    //public Vector3[] Vertices;
    //public Vector2[] Uv;
    //public int[] Triangles;

    //比较数据是否一样
    //public bool Equals(MeshInfo value)
    //{
    //	if (Tag.Length != value.Tag.Length || Vertices.Length != value.Vertices.Length)
    //		return false;
    //	for (int i = 0; i < Tag.Length; i++)
    //		if (Tag[i] != value.Tag[i])
    //			return false;
    //	for (int i = 0; i < Vertices.Length; i++)
    //		if (Vertices[i] != value.Vertices[i])
    //			return false;
    //	return true;
    //}
}
#endregion
