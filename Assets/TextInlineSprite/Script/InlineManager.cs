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
    //
    private Dictionary<int, SpriteGraphic> _IndexSpriteGraphic = new Dictionary<int, SpriteGraphic>();
    private Dictionary<int, Mesh> _IndexSpriteMesh = new Dictionary<int, Mesh>();
    private Dictionary<InlineText_New, Dictionary<int, SpriteTagInfo>> _TextInfo = new Dictionary<InlineText_New, Dictionary<int, SpriteTagInfo>>();

	// Use this for initialization
	void Awake () {
        Initialize();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

#if UNITY_EDITOR
    protected void OnValidate()
    {
        Initialize();
    }
#endif

    #region 初始化
    void Initialize()
    {
        SpriteGraphic[] _tempSpriteGraphic = GetComponentsInChildren<SpriteGraphic>();
        for (int i = 0; i < _tempSpriteGraphic.Length; i++)
        {
            SpriteAsset _tempAsset = _tempSpriteGraphic[i].m_spriteAsset;
            if (!_IndexSpriteGraphic.ContainsKey(_tempAsset.ID)&&!_IndexSpriteInfo.ContainsKey(_tempAsset.ID))
            {
                _IndexSpriteGraphic.Add(_tempAsset.ID,_tempSpriteGraphic[i]);
                _IndexSpriteMesh.Add(_tempAsset.ID, new Mesh());

                Dictionary<string, SpriteInforGroup> _tempSpriteGroup = new Dictionary<string, SpriteInforGroup>();
                foreach (var item in _tempAsset.listSpriteGroup)
                {
                    if (!_tempSpriteGroup.ContainsKey(item.tag) && item .listSpriteInfor!=null&& item.listSpriteInfor.Count > 0)
                        _tempSpriteGroup.Add(item.tag, item);
                }
                _IndexSpriteInfo.Add(_tempAsset.ID, _tempSpriteGroup);
            }

        }
    }
    #endregion

    public void UpdateTextInfo(InlineText_New _key, Dictionary<int,SpriteTagInfo> _value)
    {
        if (_TextInfo.ContainsKey(_key))
        {
            if (_value == _TextInfo[_key])
                return;
            else
                _TextInfo[_key] = _value;
        }
        else
            _TextInfo.Add(_key, _value);

        //更新图片
        
    }

    public void RemoveTextInfo(InlineText_New _key)
    {
        if (!_TextInfo.ContainsKey(_key))
            return;
        _TextInfo.Remove(_key);
        //更新图片

    }

    #region 绘制图片
    private void DrawSprites(int _index)
    {
        //Mesh m_spriteMesh = new Mesh();

        //List<Vector3> tempVertices = new List<Vector3>();
        //List<Vector2> tempUv = new List<Vector2>();
        //List<int> tempTriangles = new List<int>();

        //for (int i = 0; i < listSprite.Count; i++)
        //{
        //    for (int j = 0; j < listSprite[i].vertices.Length; j++)
        //    {
        //        tempVertices.Add(listSprite[i].vertices[j]);
        //    }
        //    for (int j = 0; j < listSprite[i].uv.Length; j++)
        //    {
        //        tempUv.Add(listSprite[i].uv[j]);
        //    }
        //    for (int j = 0; j < listSprite[i].triangles.Length; j++)
        //    {
        //        tempTriangles.Add(listSprite[i].triangles[j]);
        //    }
    //0    }

        ////计算顶点绘制顺序
        //for (int i = 0; i < tempTriangles.Count; i++)
        //{
        //    if (i % 6 == 0)
        //    {
        //        int num = i / 6;
        //        tempTriangles[i] = 0 + 4 * num;
        //        tempTriangles[i + 1] = 1 + 4 * num;
        //        tempTriangles[i + 2] = 2 + 4 * num;

        //        tempTriangles[i + 3] = 1 + 4 * num;
        //        tempTriangles[i + 4] = 0 + 4 * num;
        //        tempTriangles[i + 5] = 3 + 4 * num;
        //    }
        //}

        //m_spriteMesh.vertices = tempVertices.ToArray();
        //m_spriteMesh.uv = tempUv.ToArray();
        //m_spriteMesh.triangles = tempTriangles.ToArray();

        //if (m_spriteMesh == null)
        //    return;

        //m_spriteCanvasRenderer.SetMesh(m_spriteMesh);
        //m_spriteGraphic.UpdateMaterial();

    }
    #endregion
}
