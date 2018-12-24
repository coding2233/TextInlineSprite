/// ========================================================
/// file：InlineText.cs
/// brief：
/// author： coding2233
/// date：
/// version：v1.0
/// ========================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.Text;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;

public class InlineText : Text, IPointerClickHandler
{
	#region 属性
	// 用正则取  [图集ID#表情Tag] ID值==-1 ,表示为超链接
	private static readonly Regex _inputTagRegex = new Regex(@"\[(\-{0,1}\d{0,})#(.+?)\]", RegexOptions.Singleline);
    //文本表情管理器
    private InlineManager _inlineManager;
    //更新后的文本
    private string _outputText = "";
    //表情位置索引信息
    private Dictionary<int, SpriteTagInfo> _spriteInfo = new Dictionary<int, SpriteTagInfo>();
    //图集ID，相关信息
    private Dictionary<int, List<SpriteTagInfo>> _drawSpriteInfo = new Dictionary<int, List<SpriteTagInfo>>();
	//保留之前的图集ID，相关信息
	private Dictionary<int, List<SpriteTagInfo>> _oldDrawSpriteInfo = new Dictionary<int, List<SpriteTagInfo>>();
	//计算定点信息的缓存数组
	private readonly UIVertex[] m_TempVerts = new UIVertex[4];

	#region 超链接
	[System.Serializable]
    public class HrefClickEvent : UnityEvent<string,int> { }
    //点击事件监听
    public HrefClickEvent OnHrefClick = new HrefClickEvent();
    // 超链接信息列表  
    private readonly List<HrefInfo> _listHrefInfos = new List<HrefInfo>();
	#endregion

	#endregion

	///// <summary>
	///// 初始化 
	///// </summary>
	//protected override void OnEnable()
	//{
	//    //
	//    base.OnEnable();
	//    //支持富文本
	//    supportRichText = true;
	//    //对齐几何
	//    alignByGeometry = true;
	//    if (!_inlineManager)
	//        _inlineManager = GetComponentInParent<InlineManager>();
	//    //启动的是 更新顶点
	//    SetVerticesDirty();
	//}

	protected override void Start()
    {
        ActiveText();
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        ActiveText();
    }
#endif
	
	public void ActiveText()
    {
        //支持富文本
        supportRichText = true;
        //对齐几何
        alignByGeometry = true;
        if (!_inlineManager)
            _inlineManager = GetComponentInParent<InlineManager>();
        //启动的是 更新顶点
        SetVerticesDirty();
    }

    public override void SetVerticesDirty()
    {
        base.SetVerticesDirty();
        if (!_inlineManager)
        {
            _outputText = m_Text;
            return;
        }
        //设置新文本
        _outputText = GetOutputText();
    }

   
    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        if (font == null)
            return;

        // We don't care if we the font Texture changes while we are doing our Update.
        // The end result of cachedTextGenerator will be valid for this instance.
        // Otherwise we can get issues like Case 619238.
        m_DisableFontTextureRebuiltCallback = true;

        Vector2 extents = rectTransform.rect.size;

        var settings = GetGenerationSettings(extents);
        //   cachedTextGenerator.PopulateWithErrors(text, settings, gameObject);
        cachedTextGenerator.Populate(_outputText, settings);

        // Apply the offset to the vertices
        IList<UIVertex> verts = cachedTextGenerator.verts;
        float unitsPerPixel = 1 / pixelsPerUnit;
        //Last 4 verts are always a new line... (\n)
        int vertCount = verts.Count - 4;
        Vector2 roundingOffset = new Vector2(verts[0].position.x, verts[0].position.y) * unitsPerPixel;
        roundingOffset = PixelAdjustPoint(roundingOffset) - roundingOffset;
        toFill.Clear();

        ClearQuadUVs(verts);

        List<Vector3> listVertsPos = new List<Vector3>();
        if (roundingOffset != Vector2.zero)
        {
            for (int i = 0; i < vertCount; ++i)
            {
                int tempVertsIndex = i & 3;
                m_TempVerts[tempVertsIndex] = verts[i];
                m_TempVerts[tempVertsIndex].position *= unitsPerPixel;
                m_TempVerts[tempVertsIndex].position.x += roundingOffset.x;
                m_TempVerts[tempVertsIndex].position.y += roundingOffset.y;
                if (tempVertsIndex == 3)
                    toFill.AddUIVertexQuad(m_TempVerts);
                listVertsPos.Add(m_TempVerts[tempVertsIndex].position);
            }
        }
        else
        {
            for (int i = 0; i < vertCount; ++i)
            {
                int tempVertsIndex = i & 3;
                m_TempVerts[tempVertsIndex] = verts[i];
                m_TempVerts[tempVertsIndex].position *= unitsPerPixel;
                if (tempVertsIndex == 3)
                    toFill.AddUIVertexQuad(m_TempVerts);
                listVertsPos.Add(m_TempVerts[tempVertsIndex].position);
             
            }
        }

        //计算quad占位的信息
        CalcQuadInfo(listVertsPos);
        //计算包围盒
        CalcBoundsInfo(listVertsPos, toFill, settings);

        m_DisableFontTextureRebuiltCallback = false;

    }

    #region 文本所占的长宽
    public override float preferredWidth
    {
        get
        {
            var settings = GetGenerationSettings(Vector2.zero);
            return cachedTextGeneratorForLayout.GetPreferredWidth(_outputText, settings) / pixelsPerUnit;
        }
    }
    public override float preferredHeight
    {
        get
        {
            var settings = GetGenerationSettings(new Vector2(rectTransform.rect.size.x, 0.0f));
            return cachedTextGeneratorForLayout.GetPreferredHeight(_outputText, settings) / pixelsPerUnit;
        }
    }
    #endregion


    #region 清除乱码
    private void ClearQuadUVs(IList<UIVertex> verts)
    {
        foreach (var item in _spriteInfo)
        {
            if ((item.Key + 4) > verts.Count)
                continue;
            for (int i = item.Key; i < item.Key + 4; i++)
            {
                //清除乱码
                UIVertex tempVertex = verts[i];
                tempVertex.uv0 = Vector2.zero;
                verts[i] = tempVertex;
            }
        }
    }
#endregion

    #region 计算Quad占位信息
    void CalcQuadInfo(List<Vector3> _listVertsPos)
    {
        foreach (var item in _spriteInfo)
        {
            if ((item.Key + 4) > _listVertsPos.Count)
                continue;
            for (int i = item.Key; i < item.Key + 4; i++)
            {
                item.Value.Pos[i - item.Key] = _listVertsPos[i];
            }
        }
        //绘制表情
        UpdateDrawnSprite();
    }
    #endregion

    #region 绘制表情
    void UpdateDrawnSprite()
    {
		//记录之前的信息
	    _oldDrawSpriteInfo = _drawSpriteInfo;

		_drawSpriteInfo = new Dictionary<int, List<SpriteTagInfo>>();
        foreach (var item in _spriteInfo)
        {
            int _id = item.Value.Id;

            //更新绘制表情的信息
            List<SpriteTagInfo> listSpriteInfo = null;
            if (_drawSpriteInfo.ContainsKey(_id))
                listSpriteInfo = _drawSpriteInfo[_id];
            else
            {
                listSpriteInfo = new List<SpriteTagInfo>();
                _drawSpriteInfo.Add(_id, listSpriteInfo);
            }
            listSpriteInfo.Add(item.Value);
        }

		//没有表情时也要提醒manager删除之前的信息
	    foreach (var item in _oldDrawSpriteInfo)
	    {
		    if(!_drawSpriteInfo.ContainsKey(item.Key))
			    _inlineManager.RemoveTextInfo(item.Key,this);
		}

	    foreach (var item in _drawSpriteInfo)
        {
            _inlineManager.UpdateTextInfo(item.Key, this, item.Value);
        }
    }

    #endregion

    #region 处理超链接的包围盒
    void CalcBoundsInfo(List<Vector3> listVertsPos, VertexHelper toFill,TextGenerationSettings settings)
    {
        #region 包围框
        // 处理超链接包围框  
        foreach (var hrefInfo in _listHrefInfos)
        {
            hrefInfo.Boxes.Clear();
            if (hrefInfo.StartIndex >= listVertsPos.Count)
            {
                continue;
            }

            // 将超链接里面的文本顶点索引坐标加入到包围框  
            var pos = listVertsPos[hrefInfo.StartIndex];
            var bounds = new Bounds(pos, Vector3.zero);
            for (int i = hrefInfo.StartIndex, m = hrefInfo.EndIndex; i < m; i++)
            {
                if (i >= listVertsPos.Count)
                {
                    break;
                }

                pos = listVertsPos[i];
                if (pos.x < bounds.min.x)
                {
                    // 换行重新添加包围框  
                    hrefInfo.Boxes.Add(new Rect(bounds.min, bounds.size));
                    bounds = new Bounds(pos, Vector3.zero);
                }
                else
                {
                    bounds.Encapsulate(pos); // 扩展包围框  
                }
            }
            //添加包围盒
            hrefInfo.Boxes.Add(new Rect(bounds.min, bounds.size));
        }
        #endregion

        #region 添加下划线
        TextGenerator underlineText = new TextGenerator();
        underlineText.Populate("_", settings);
        IList<UIVertex> tut = underlineText.verts;
        foreach (var item in _listHrefInfos)
        {
            for (int i = 0; i < item.Boxes.Count; i++)
            {
                //计算下划线的位置
                Vector3[] ulPos = new Vector3[4];
                ulPos[0] = item.Boxes[i].position + new Vector2(0.0f, fontSize * 0.2f);
                ulPos[1] = ulPos[0]+new Vector3(item.Boxes[i].width,0.0f);
                ulPos[2] = item.Boxes[i].position + new Vector2(item.Boxes[i].width, 0.0f);
                ulPos[3] =item.Boxes[i].position;
                //绘制下划线
                for (int j = 0; j < 4; j++)
                {
                    m_TempVerts[j] = tut[j];
                    m_TempVerts[j].color = Color.blue;
                    m_TempVerts[j].position = ulPos[j];
                    if (j == 3)
                        toFill.AddUIVertexQuad(m_TempVerts);
                }

            }
        }

        #endregion

    }
    #endregion

    #region 根据正则规则更新文本
    private string GetOutputText()
    {
        _spriteInfo = new Dictionary<int, SpriteTagInfo>();
        StringBuilder textBuilder = new StringBuilder();
        int textIndex = 0;

        foreach (Match match in _inputTagRegex.Matches(text))
        {
            int tempId = 0;
            if (!string.IsNullOrEmpty(match.Groups[1].Value)&& !match.Groups[1].Value.Equals("-"))
                tempId = int.Parse(match.Groups[1].Value);
            string tempTag = match.Groups[2].Value;
            //更新超链接
            if (tempId <0 )
            {
                textBuilder.Append(text.Substring(textIndex, match.Index - textIndex));
                textBuilder.Append("<color=blue>");
                int startIndex = textBuilder.Length * 4;
                textBuilder.Append("[" + match.Groups[2].Value + "]");
                int endIndex = textBuilder.Length * 4 - 2;
                textBuilder.Append("</color>");

                var hrefInfo = new HrefInfo
                {
                    Id = Mathf.Abs(tempId),
                    StartIndex = startIndex, // 超链接里的文本起始顶点索引
                    EndIndex = endIndex,
                    Name = match.Groups[2].Value
                };
                _listHrefInfos.Add(hrefInfo);

            }
            //更新表情
            else
            {
                if (!_inlineManager.IndexSpriteInfo.ContainsKey(tempId)
                    || !_inlineManager.IndexSpriteInfo[tempId].ContainsKey(tempTag))
                    continue;
                SpriteInforGroup tempGroup = _inlineManager.IndexSpriteInfo[tempId][tempTag];

                textBuilder.Append(text.Substring(textIndex, match.Index - textIndex));
                int tempIndex = textBuilder.Length * 4;
                textBuilder.Append(@"<quad size=" + tempGroup.Size + " width=" + tempGroup.Width + " />");

                SpriteTagInfo tempSpriteTag = new SpriteTagInfo
                {
                    Id = tempId,
                    Tag = tempTag,
                    Size = new Vector2(tempGroup.Size * tempGroup.Width, tempGroup.Size),
                    Pos = new Vector3[4],
                    Uv = tempGroup.ListSpriteInfor[0].Uv
                };
                if (!_spriteInfo.ContainsKey(tempIndex))
                    _spriteInfo.Add(tempIndex, tempSpriteTag);
            }

            textIndex = match.Index + match.Length;
        }

        textBuilder.Append(text.Substring(textIndex, text.Length - textIndex));
        return textBuilder.ToString();
    }
    #endregion

    #region  超链接信息类
    private class HrefInfo
    {
        public int Id;

        public int StartIndex;

        public int EndIndex;

        public string Name;

        public readonly List<Rect> Boxes = new List<Rect>();
    }
    #endregion
    
    #region 点击事件检测是否点击到超链接文本  
    public void OnPointerClick(PointerEventData eventData)
    {
        Vector2 lp;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, eventData.position, eventData.pressEventCamera, out lp);

        foreach (var hrefInfo in _listHrefInfos)
        {
            var boxes = hrefInfo.Boxes;
            for (var i = 0; i < boxes.Count; ++i)
            {
                if (boxes[i].Contains(lp))
                {
                    OnHrefClick.Invoke(hrefInfo.Name, hrefInfo.Id);
                    return;
                }
            }
        }
    }
    #endregion

}

public class SpriteTagInfo
{
    //图集ID
    public int Id;
    //标签标签
    public string Tag;
    //标签大小
    public Vector2 Size;
    //表情位置
    public Vector3[] Pos;
    //uv
    public Vector2[] Uv;
}


