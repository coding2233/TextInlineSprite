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

public class InlineManager : MonoBehaviour {

	#region 属性
	//所有的精灵消息
	public Dictionary<int, Dictionary<string, SpriteInforGroup>> IndexSpriteInfo=new Dictionary<int, Dictionary<string, SpriteInforGroup>>();
    //绘制图集的索引
    private readonly Dictionary<int, SpriteGraphicInfo> _indexSpriteGraphic = new Dictionary<int, SpriteGraphicInfo>();
    //绘制的模型数据索引
    private Dictionary<int, Dictionary<InlineText, MeshInfo>> _textMeshInfo = new Dictionary<int, Dictionary<InlineText, MeshInfo>>();
    //静态表情
    [SerializeField]
    private bool _isStatic=true;
    //动画速度
    [SerializeField]
    [Range(1,10)]
    private float _animationSpeed = 5.0f;
	//动画时间
	float _animationTime = 0.0f;
	//动画索引
	int _animationIndex = 0;
	#endregion

	// Use this for initialization
	void OnEnable()
    {
        Initialize();
    }

    // Update is called once per frame
    void Update () {
        //动态表情
        if(!_isStatic)
            DrawSpriteAnimation();
    }
   
    #region 初始化
    void Initialize()
    {
        SpriteGraphic[] spriteGraphics = GetComponentsInChildren<SpriteGraphic>();
        for (int i = 0; i < spriteGraphics.Length; i++)
        {
            SpriteAsset mSpriteAsset = spriteGraphics[i].m_spriteAsset;
            if (!_indexSpriteGraphic.ContainsKey(mSpriteAsset.Id)&&!IndexSpriteInfo.ContainsKey(mSpriteAsset.Id))
            {
                SpriteGraphicInfo spriteGraphicInfo = new SpriteGraphicInfo()
                {
                    SpriteGraphic = spriteGraphics[i],
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

    public void UpdateTextInfo(int id,InlineText key, List<SpriteTagInfo> value)
    {
        if (!_indexSpriteGraphic.ContainsKey(id)||!_textMeshInfo.ContainsKey(id)|| value.Count<=0)
            return;
        int spriteTagCount = value.Count;
        Vector3 textPos = key.transform.position;
        Vector3 spritePos = _indexSpriteGraphic[id].SpriteGraphic.transform.position;
		Vector3 disPos = (textPos - spritePos)*(1.0f/ key.pixelsPerUnit);
		//新增摄像机模式的位置判断
		if (key.canvas != null)
        {
            if (key.canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                Vector3 scale = key.canvas.transform.localScale;
				disPos = new Vector3(disPos.x / scale.x, disPos.y / scale.y, disPos.z / scale.z);
				disPos /= (1.0f / key.pixelsPerUnit);
			}
        }

        MeshInfo meshInfo = new MeshInfo();
        meshInfo.Tag = new string[spriteTagCount];
        meshInfo.Vertices = new Vector3[spriteTagCount * 4];
        meshInfo.Uv = new Vector2[spriteTagCount * 4];
        meshInfo.Triangles = new int[spriteTagCount * 6];
        for (int i = 0; i < value.Count; i++)
        {
            int m = i * 4;
            //标签
            meshInfo.Tag[i] = value[i].Tag;
            //顶点位置
            meshInfo.Vertices[m + 0] = value[i].Pos[0]+ disPos;
            meshInfo.Vertices[m + 1] = value[i].Pos[1] + disPos;
            meshInfo.Vertices[m + 2] = value[i].Pos[2] + disPos;
            meshInfo.Vertices[m + 3] = value[i].Pos[3] + disPos;
            //uv
            meshInfo.Uv[m + 0] = value[i].Uv[0];
            meshInfo.Uv[m + 1] = value[i].Uv[1];
            meshInfo.Uv[m + 2] = value[i].Uv[2];
            meshInfo.Uv[m + 3] = value[i].Uv[3];
        }
        if (_textMeshInfo[id].ContainsKey(key))
        {
            MeshInfo oldMeshInfo = _textMeshInfo[id][key];
            if (!meshInfo.Equals(oldMeshInfo))
                _textMeshInfo[id][key] = meshInfo;
        }
        else
            _textMeshInfo[id].Add(key, meshInfo);
        
        //更新图片
        DrawSprites(id);
    }

	/// <summary>
	/// 移除文本 
	/// </summary>
	/// <param name="id"></param>
	/// <param name="key"></param>
    public void RemoveTextInfo(int id,InlineText key)
    {
        if (!_textMeshInfo.ContainsKey(id)|| !_textMeshInfo[id].ContainsKey(key))
            return;
	    _textMeshInfo[id].Remove(key);
        //更新图片
        DrawSprites(id);
    }

    #region 播放动态表情
    private void DrawSpriteAnimation()
    {
        _animationTime += Time.deltaTime* _animationSpeed;
        if (_animationTime >= 1.0f)
        {
            _animationIndex++;
            //绘制表情
            foreach (var item in _indexSpriteGraphic)
            {
                if (item.Value.SpriteGraphic.m_spriteAsset.IsStatic)
                    continue;
                if (!_textMeshInfo.ContainsKey(item.Key) || _textMeshInfo[item.Key].Count <= 0)
                    continue;

                //Mesh _mesh = _indexSpriteGraphic[item.Key].Mesh;
                Dictionary<InlineText, MeshInfo> _data = _textMeshInfo[item.Key];
                foreach (var item02 in _data)
                {
                    for (int i = 0; i < item02.Value.Tag.Length; i++)
                    {
                        List<SpriteInfor> _listSpriteInfo = IndexSpriteInfo[item.Key][item02.Value.Tag[i]].ListSpriteInfor;
                        if (_listSpriteInfo.Count <= 1)
                            continue;
                        int _index = _animationIndex % _listSpriteInfo.Count;
                        
                        int m = i * 4;
                        item02.Value.Uv[m + 0] = _listSpriteInfo[_index].Uv[0];
                        item02.Value.Uv[m + 1] = _listSpriteInfo[_index].Uv[1];
                        item02.Value.Uv[m + 2] = _listSpriteInfo[_index].Uv[2];
                        item02.Value.Uv[m + 3] = _listSpriteInfo[_index].Uv[3];
                        
                    }
                }
               // _indexSpriteGraphic[item.Key].Mesh = _mesh;
                DrawSprites(item.Key);
            }

            _animationTime = 0.0f;
        }
     
    }
    #endregion

	/// <summary>
	/// 清除所有的精灵
	/// </summary>
	public void ClearAllSprites()
	{
		Dictionary<int, Dictionary<InlineText, MeshInfo>> _temp = new Dictionary<int, Dictionary<InlineText, MeshInfo>>();
		foreach (var item in _textMeshInfo)
			_temp[item.Key] = new Dictionary<InlineText, MeshInfo>();
		_textMeshInfo = _temp;

		foreach (var item in _indexSpriteGraphic)
			DrawSprites(item.Key);
	}

	#region 绘制图片
    private void DrawSprites(int id)
    {
        if (!_indexSpriteGraphic.ContainsKey(id)
            || !_textMeshInfo.ContainsKey(id))
            return;

		SpriteGraphic spriteGraphic = _indexSpriteGraphic[id].SpriteGraphic;
        Mesh mesh = _indexSpriteGraphic[id].Mesh;
        Dictionary<InlineText, MeshInfo> data = _textMeshInfo[id];
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uv = new List<Vector2>();
        List<int> triangles = new List<int>();
        foreach (var item in data)
        {
			if (item.Key == null)
				continue;

			for (int i = 0; i < item.Value.Vertices.Length; i++)
            {
                //添加顶点
                vertices.Add(item.Value.Vertices[i]);
                //添加uv
                uv.Add(item.Value.Uv[i]);
            }
            //添加顶点索引
            for (int i = 0; i < item.Value.Triangles.Length; i++)
                triangles.Add(item.Value.Triangles[i]);
        }
        //计算顶点绘制顺序
        for (int i = 0; i < triangles.Count; i++)
        {
            if (i % 6 == 0)
            {
                int num = i / 6;
                triangles[i + 0] = 0 + 4 * num;
                triangles[i + 1] = 1 + 4 * num;
                triangles[i + 2] = 2 + 4 * num;

                triangles[i + 3] = 0 + 4 * num;
                triangles[i + 4] = 2 + 4 * num;
                triangles[i + 5] = 3 + 4 * num;
            }
        }
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.uv = uv.ToArray();
        mesh.triangles = triangles.ToArray();

        spriteGraphic.canvasRenderer.SetMesh(mesh);
        spriteGraphic.UpdateMaterial();
    }
    #endregion

    #region 精灵组信息
    private class SpriteGraphicInfo
    {
        public SpriteGraphic SpriteGraphic;
        public Mesh Mesh;
    }
    #endregion

    #region 模型数据信息
    private class MeshInfo
    {
        public string[] Tag;
        public Vector3[] Vertices;
        public Vector2[] Uv;
        public int[] Triangles;

        //比较数据是否一样
        public bool Equals(MeshInfo value)
        {
            if (Tag.Length!= value.Tag.Length|| Vertices.Length!= value.Vertices.Length)
                return false;
            for (int i = 0; i < Tag.Length; i++)
                if (Tag[i] != value.Tag[i])
                    return false;
            for (int i = 0; i < Vertices.Length; i++)
                if (Vertices[i] != value.Vertices[i])
                    return false;
            return true;
        }
    }
    #endregion
}
