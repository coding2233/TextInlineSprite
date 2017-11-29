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

    //所有的精灵消息
    public Dictionary<int, Dictionary<string, SpriteInforGroup>> _IndexSpriteInfo=new Dictionary<int, Dictionary<string, SpriteInforGroup>>();
    //绘制图集的索引
    private Dictionary<int, SpriteGraphicInfo> _IndexSpriteGraphic = new Dictionary<int, SpriteGraphicInfo>();
    //绘制的模型数据索引
    private Dictionary<int, Dictionary<InlineText, MeshInfo>> _TextMeshInfo = new Dictionary<int, Dictionary<InlineText, MeshInfo>>();
    //静态表情
    [SerializeField]
    private bool _IsStatic;
    //动画速度
    [SerializeField]
    [Range(1,10)]
    private float _AnimationSpeed = 5.0f;
    
    // Use this for initialization
    void OnEnable()
    {
        Initialize();
    }

    // Update is called once per frame
    void Update () {
        //动态表情
        if(!_IsStatic)
            DrawSpriteAnimation();
    }
   
    #region 初始化
    void Initialize()
    {
        SpriteGraphic[] _spriteGraphic = GetComponentsInChildren<SpriteGraphic>();
        for (int i = 0; i < _spriteGraphic.Length; i++)
        {
            SpriteAsset _spriteAsset = _spriteGraphic[i].m_spriteAsset;
            if (!_IndexSpriteGraphic.ContainsKey(_spriteAsset.ID)&&!_IndexSpriteInfo.ContainsKey(_spriteAsset.ID))
            {
                SpriteGraphicInfo _spriteGraphicInfo = new SpriteGraphicInfo()
                {
                    _SpriteGraphic = _spriteGraphic[i],
                    _Mesh = new Mesh(),
                };
                _IndexSpriteGraphic.Add(_spriteAsset.ID, _spriteGraphicInfo);

                Dictionary<string, SpriteInforGroup> _spriteGroup = new Dictionary<string, SpriteInforGroup>();
                foreach (var item in _spriteAsset.listSpriteGroup)
                {
                    if (!_spriteGroup.ContainsKey(item.tag) && item .listSpriteInfor!=null&& item.listSpriteInfor.Count > 0)
                        _spriteGroup.Add(item.tag, item);
                }
                _IndexSpriteInfo.Add(_spriteAsset.ID, _spriteGroup);
                _TextMeshInfo.Add(_spriteAsset.ID, new Dictionary<InlineText, MeshInfo>());
            }              
        }
    }
    #endregion

    public void UpdateTextInfo(int _id,InlineText _key, List<SpriteTagInfo> _value)
    {
        if (!_IndexSpriteGraphic.ContainsKey(_id)||!_TextMeshInfo.ContainsKey(_id)|| _value.Count<=0)
            return;
        int _spriteTagCount = _value.Count;
        Vector3 _textPos = _key.transform.position;
        Vector3 _spritePos = _IndexSpriteGraphic[_id]._SpriteGraphic.transform.position;
        Vector3 _disPos = (_textPos - _spritePos)*(1.0f/ _key.pixelsPerUnit);
        //新增摄像机模式的位置判断
        if (_key.canvas != null)
        {
            if (_key.canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                Vector3 _scale = _key.canvas.transform.localScale;
                _disPos = new Vector3(_disPos.x / _scale.x, _disPos.y / _scale.y, _disPos.z / _scale.z);
            }
        }

        MeshInfo _meshInfo = new MeshInfo();
        _meshInfo._Tag = new string[_spriteTagCount];
        _meshInfo._Vertices = new Vector3[_spriteTagCount * 4];
        _meshInfo._UV = new Vector2[_spriteTagCount * 4];
        _meshInfo._Triangles = new int[_spriteTagCount * 6];
        for (int i = 0; i < _value.Count; i++)
        {
            int m = i * 4;
            //标签
            _meshInfo._Tag[i] = _value[i]._Tag;
            //顶点位置
            _meshInfo._Vertices[m + 0] = _value[i]._Pos[0]+ _disPos;
            _meshInfo._Vertices[m + 1] = _value[i]._Pos[1] + _disPos;
            _meshInfo._Vertices[m + 2] = _value[i]._Pos[2] + _disPos;
            _meshInfo._Vertices[m + 3] = _value[i]._Pos[3] + _disPos;
            //uv
            _meshInfo._UV[m + 0] = _value[i]._UV[0];
            _meshInfo._UV[m + 1] = _value[i]._UV[1];
            _meshInfo._UV[m + 2] = _value[i]._UV[2];
            _meshInfo._UV[m + 3] = _value[i]._UV[3];
        }
        if (_TextMeshInfo[_id].ContainsKey(_key))
        {
            MeshInfo _oldMeshInfo = _TextMeshInfo[_id][_key];
            if (_meshInfo.Equals(_oldMeshInfo))
                return;
            else
                _TextMeshInfo[_id][_key] = _meshInfo;
        }
        else
            _TextMeshInfo[_id].Add(_key, _meshInfo);
        
        //更新图片
        DrawSprites(_id);
    }

    public void RemoveTextInfo(int _id,InlineText _key)
    {
        if (!_TextMeshInfo.ContainsKey(_id)|| _TextMeshInfo[_id].ContainsKey(_key))
            return;
        _TextMeshInfo.Remove(_id);
        //更新图片
        DrawSprites(_id);
    }

    #region 播放动态表情
    float _animationTime = 0.0f;
    int _AnimationIndex = 0;
    private void DrawSpriteAnimation()
    {
        _animationTime += Time.deltaTime* _AnimationSpeed;
        if (_animationTime >= 1.0f)
        {
            _AnimationIndex++;
            //绘制表情
            foreach (var item in _IndexSpriteGraphic)
            {
                if (item.Value._SpriteGraphic.m_spriteAsset._IsStatic)
                    continue;
                if (!_TextMeshInfo.ContainsKey(item.Key) || _TextMeshInfo[item.Key].Count <= 0)
                    continue;

                //Mesh _mesh = _IndexSpriteGraphic[item.Key]._Mesh;
                Dictionary<InlineText, MeshInfo> _data = _TextMeshInfo[item.Key];
                foreach (var item02 in _data)
                {
                    for (int i = 0; i < item02.Value._Tag.Length; i++)
                    {
                        List<SpriteInfor> _listSpriteInfo = _IndexSpriteInfo[item.Key][item02.Value._Tag[i]].listSpriteInfor;
                        if (_listSpriteInfo.Count <= 1)
                            continue;
                        int _index = _AnimationIndex % _listSpriteInfo.Count;
                        
                        int m = i * 4;
                        item02.Value._UV[m + 0] = _listSpriteInfo[_index].uv[0];
                        item02.Value._UV[m + 1] = _listSpriteInfo[_index].uv[1];
                        item02.Value._UV[m + 2] = _listSpriteInfo[_index].uv[2];
                        item02.Value._UV[m + 3] = _listSpriteInfo[_index].uv[3];
                        
                    }
                }
               // _IndexSpriteGraphic[item.Key]._Mesh = _mesh;
                DrawSprites(item.Key);
            }

            _animationTime = 0.0f;
        }
     
    }
    #endregion

    #region 绘制图片
    private void DrawSprites(int _id)
    {
        if (!_IndexSpriteGraphic.ContainsKey(_id)
            || !_TextMeshInfo.ContainsKey(_id))
            return;
        SpriteGraphic _spriteGraphic = _IndexSpriteGraphic[_id]._SpriteGraphic;
        Mesh _mesh = _IndexSpriteGraphic[_id]._Mesh;
        Dictionary<InlineText, MeshInfo> _data = _TextMeshInfo[_id];
        List<Vector3> _vertices = new List<Vector3>();
        List<Vector2> _uv = new List<Vector2>();
        List<int> _triangles = new List<int>();
        foreach (var item in _data)
        {
            for (int i = 0; i < item.Value._Vertices.Length; i++)
            {
                //添加顶点
                _vertices.Add(item.Value._Vertices[i]);
                //添加uv
                _uv.Add(item.Value._UV[i]);
            }
            //添加顶点索引
            for (int i = 0; i < item.Value._Triangles.Length; i++)
                _triangles.Add(item.Value._Triangles[i]);
        }
        //计算顶点绘制顺序
        for (int i = 0; i < _triangles.Count; i++)
        {
            if (i % 6 == 0)
            {
                int num = i / 6;
                _triangles[i + 0] = 0 + 4 * num;
                _triangles[i + 1] = 1 + 4 * num;
                _triangles[i + 2] = 2 + 4 * num;

                _triangles[i + 3] = 0 + 4 * num;
                _triangles[i + 4] = 2 + 4 * num;
                _triangles[i + 5] = 3 + 4 * num;
            }
        }
        _mesh.Clear();
        _mesh.vertices = _vertices.ToArray();
        _mesh.uv = _uv.ToArray();
        _mesh.triangles = _triangles.ToArray();

        _spriteGraphic.canvasRenderer.SetMesh(_mesh);
        _spriteGraphic.UpdateMaterial();
    }
    #endregion

    #region 精灵组信息
    private class SpriteGraphicInfo
    {
        public SpriteGraphic _SpriteGraphic;
        public Mesh _Mesh;
    }
    #endregion

    #region 模型数据信息
    private class MeshInfo
    {
        public string[] _Tag;
        public Vector3[] _Vertices;
        public Vector2[] _UV;
        public int[] _Triangles;

        //比较数据是否一样
        public bool Equals(MeshInfo _value)
        {
            if (_Tag.Length!= _value._Tag.Length|| _Vertices.Length!= _value._Vertices.Length)
                return false;
            for (int i = 0; i < _Tag.Length; i++)
                if (_Tag[i] != _value._Tag[i])
                    return false;
            for (int i = 0; i < _Vertices.Length; i++)
                if (_Vertices[i] != _value._Vertices[i])
                    return false;
            return true;
        }
    }
    #endregion
}
