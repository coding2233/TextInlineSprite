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

[ExecuteInEditMode]
public class InlineText : Text, IPointerClickHandler
{
    // 用正则取  [图集ID#表情Tag] ID值==-1 ,表示为超链接
    private static readonly Regex _InputTagRegex = new Regex(@"\[(\-{0,1}\d{0,})#(.+?)\]", RegexOptions.Singleline);
    //文本表情管理器
    private InlineManager _InlineManager;
    //更新后的文本
    private string _OutputText = "";
    //表情位置索引信息
    Dictionary<int, SpriteTagInfo> _SpriteInfo = new Dictionary<int, SpriteTagInfo>();
    //图集ID，相关信息
    Dictionary<int, List<SpriteTagInfo>> _DrawSpriteInfo = new Dictionary<int, List<SpriteTagInfo>>();

    #region 超链接
    [System.Serializable]
    public class HrefClickEvent : UnityEvent<string> { }
    //点击事件监听
    public HrefClickEvent OnHrefClick = new HrefClickEvent();
    // 超链接信息列表  
    private readonly List<HrefInfo> _ListHrefInfos = new List<HrefInfo>();
    #endregion
    
    /// <summary>
    /// 初始化 
    /// </summary>
    protected override void OnEnable()
    {
        //
        base.OnEnable();
        //支持富文本
        supportRichText = true;
        //对齐几何
        alignByGeometry = true;
        if (!_InlineManager)
            _InlineManager = GetComponentInParent<InlineManager>();
        //启动的是 更新顶点
        SetVerticesDirty();
    }

    public void ActiveText()
    {
        OnEnable();
    }

    public override void SetVerticesDirty()
    {
        base.SetVerticesDirty();
        if (!_InlineManager)
        {
            _OutputText = m_Text;
            return;
        }

        //设置新文本
        _OutputText = GetOutputText();
    }

    readonly UIVertex[] m_TempVerts = new UIVertex[4];
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
        cachedTextGenerator.Populate(_OutputText, settings);

        // Apply the offset to the vertices
        IList<UIVertex> verts = cachedTextGenerator.verts;
        float unitsPerPixel = 1 / pixelsPerUnit;
        //Last 4 verts are always a new line... (\n)
        int vertCount = verts.Count - 4;

        Vector2 roundingOffset = new Vector2(verts[0].position.x, verts[0].position.y) * unitsPerPixel;
        roundingOffset = PixelAdjustPoint(roundingOffset) - roundingOffset;
        toFill.Clear();

        //计算quad占位的信息
        CalcQuadInfo(verts);
        //计算包围盒
        CalcBoundsInfo(verts,toFill,settings);

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
            }
        }

        m_DisableFontTextureRebuiltCallback = false;

    }

    #region 文本所占的长宽
    public override float preferredWidth
    {
        get
        {
            var settings = GetGenerationSettings(Vector2.zero);
            return cachedTextGeneratorForLayout.GetPreferredWidth(_OutputText, settings) / pixelsPerUnit;
        }
    }
    public override float preferredHeight
    {
        get
        {
            var settings = GetGenerationSettings(new Vector2(rectTransform.rect.size.x, 0.0f));
            return cachedTextGeneratorForLayout.GetPreferredHeight(_OutputText, settings) / pixelsPerUnit;
        }
    }
    #endregion


    #region 计算Quad占位信息
    void CalcQuadInfo(IList<UIVertex> verts)
    {
        foreach (var item in _SpriteInfo)
        {
            if ((item.Key + 4) > verts.Count)
                continue;
            for (int i = item.Key; i < item.Key + 4; i++)
            {
                //清除乱码
                UIVertex tempVertex = verts[i];
                tempVertex.uv0 = Vector2.zero;
                verts[i] = tempVertex;
                //计算位置
                item.Value._Pos[i - item.Key] = tempVertex.position;
            }
        }
        //绘制表情
        UpdateDrawnSprite();
    }
    #endregion

    #region 绘制表情
    void UpdateDrawnSprite()
    {
        _DrawSpriteInfo = new Dictionary<int, List<SpriteTagInfo>>();
        foreach (var item in _SpriteInfo)
        {
            int _id = item.Value._ID;
            string _tag = item.Value._Tag;

            //更新绘制表情的信息
            List<SpriteTagInfo> _listSpriteInfo = null;
            if (_DrawSpriteInfo.ContainsKey(_id))
                _listSpriteInfo = _DrawSpriteInfo[_id];
            else
            {
                _listSpriteInfo = new List<SpriteTagInfo>();
                _DrawSpriteInfo.Add(_id, _listSpriteInfo);
            }
            _listSpriteInfo.Add(item.Value);
        }

        foreach (var item in _DrawSpriteInfo)
        {
            _InlineManager.UpdateTextInfo(item.Key, this, item.Value);
        }
    }

    #endregion

    #region 处理超链接的包围盒
    void CalcBoundsInfo(IList<UIVertex> verts, VertexHelper toFill,TextGenerationSettings settings)
    {
        #region 包围框
        // 处理超链接包围框  
        UIVertex vert = new UIVertex();
        foreach (var hrefInfo in _ListHrefInfos)
        {
            hrefInfo.boxes.Clear();
            if (hrefInfo.startIndex >= verts.Count)
            {
                continue;
            }

            // 将超链接里面的文本顶点索引坐标加入到包围框  
            vert = verts[hrefInfo.startIndex];
            var pos = vert.position;
            var bounds = new Bounds(pos, Vector3.zero);
            for (int i = hrefInfo.startIndex, m = hrefInfo.endIndex; i < m; i++)
            {
                if (i >= verts.Count)
                {
                    break;
                }

                vert = verts[i];
                pos = vert.position;
                if (pos.x < bounds.min.x)
                {
                    // 换行重新添加包围框  
                    hrefInfo.boxes.Add(new Rect(bounds.min, bounds.size));
                    bounds = new Bounds(pos, Vector3.zero);
                }
                else
                {
                    bounds.Encapsulate(pos); // 扩展包围框  
                }
            }
            //添加包围盒
            hrefInfo.boxes.Add(new Rect(bounds.min, bounds.size));
        }
        #endregion

        #region 添加下划线
        TextGenerator _UnderlineText = new TextGenerator();
        _UnderlineText.Populate("_", settings);
        IList<UIVertex> _TUT = _UnderlineText.verts;
        foreach (var item in _ListHrefInfos)
        {
            for (int i = 0; i < item.boxes.Count; i++)
            {
                //计算下划线的位置
                Vector3[] _ulPos = new Vector3[4];
                _ulPos[0] = item.boxes[i].position + new Vector2(0.0f, fontSize * 0.2f);
                _ulPos[1] = _ulPos[0]+new Vector3(item.boxes[i].width,0.0f);
                _ulPos[2] = item.boxes[i].position + new Vector2(item.boxes[i].width, 0.0f);
                _ulPos[3] =item.boxes[i].position;
                //绘制下划线
                for (int j = 0; j < 4; j++)
                {
                    m_TempVerts[j] = _TUT[j];
                    m_TempVerts[j].color = Color.blue;
                    m_TempVerts[j].position = _ulPos[j];
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
        _SpriteInfo = new Dictionary<int, SpriteTagInfo>();
        StringBuilder _textBuilder = new StringBuilder();
        int _textIndex = 0;

        foreach (Match match in _InputTagRegex.Matches(text))
        {
            int _tempID = 0;
            if (!string.IsNullOrEmpty(match.Groups[1].Value))
                _tempID = int.Parse(match.Groups[1].Value);
            string _tempTag = match.Groups[2].Value;
            //更新超链接
            if (_tempID == -1)
            {
                _textBuilder.Append(text.Substring(_textIndex, match.Index - _textIndex));
                _textBuilder.Append("<color=blue>");
                int _startIndex = _textBuilder.Length * 4;
                _textBuilder.Append("[" + match.Groups[2].Value + "]");
                int _endIndex = _textBuilder.Length * 4 - 2;
                _textBuilder.Append("</color>");

                var hrefInfo = new HrefInfo
                {
                    startIndex = _startIndex, // 超链接里的文本起始顶点索引
                    endIndex = _endIndex,
                    name = match.Groups[2].Value
                };
                _ListHrefInfos.Add(hrefInfo);

            }
            //更新表情
            else
            {
                if (!_InlineManager._IndexSpriteInfo.ContainsKey(_tempID)
                    || !_InlineManager._IndexSpriteInfo[_tempID].ContainsKey(_tempTag))
                    continue;
                SpriteInforGroup _tempGroup = _InlineManager._IndexSpriteInfo[_tempID][_tempTag];

                _textBuilder.Append(text.Substring(_textIndex, match.Index - _textIndex));
                int _tempIndex = _textBuilder.Length * 4;
                _textBuilder.Append(@"<quad size=" + _tempGroup.size + " width=" + _tempGroup.width + " />");

                SpriteTagInfo _tempSpriteTag = new SpriteTagInfo
                {
                    _ID = _tempID,
                    _Tag = _tempTag,
                    _Size = new Vector2(_tempGroup.size * _tempGroup.width, _tempGroup.size),
                    _Pos = new Vector3[4],
                    _UV = _tempGroup.listSpriteInfor[0].uv
                };
                if (!_SpriteInfo.ContainsKey(_tempIndex))
                    _SpriteInfo.Add(_tempIndex, _tempSpriteTag);
            }

            _textIndex = match.Index + match.Length;
        }

        _textBuilder.Append(text.Substring(_textIndex, text.Length - _textIndex));
        return _textBuilder.ToString();
    }
    #endregion

    #region  超链接信息类
    private class HrefInfo
    {
        public int startIndex;

        public int endIndex;

        public string name;

        public readonly List<Rect> boxes = new List<Rect>();
    }
    #endregion



    #region 点击事件检测是否点击到超链接文本  
    public void OnPointerClick(PointerEventData eventData)
    {
        Vector2 lp;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, eventData.position, eventData.pressEventCamera, out lp);

        foreach (var hrefInfo in _ListHrefInfos)
        {
            var boxes = hrefInfo.boxes;
            for (var i = 0; i < boxes.Count; ++i)
            {
                if (boxes[i].Contains(lp))
                {
                    OnHrefClick.Invoke(hrefInfo.name);
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
    public int _ID;
    //标签标签
    public string _Tag;
    //标签大小
    public Vector2 _Size;
    //表情位置
    public Vector3[] _Pos;
    //uv
    public Vector2[] _UV;
}


