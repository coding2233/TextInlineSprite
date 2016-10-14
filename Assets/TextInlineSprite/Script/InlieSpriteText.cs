using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class InlieSpriteText : Text {

    /// <summary>
    /// 用正则取标签属性 名称-大小-宽度比例
    /// </summary>
    private static readonly Regex m_spriteTagRegex =
          new Regex(@"<quad name=(.+?) size=(\d*\.?\d+%?) width=(\d*\.?\d+%?) />", RegexOptions.Singleline);

    /// <summary>
    /// 图片资源
    /// </summary>
    private SpriteAsset m_spriteAsset;
    /// <summary>
    /// 图片渲染组件
    /// </summary>
    private SpriteGraphic m_spriteGraphic;
    /// <summary>
    /// CanvasRenderer
    /// </summary>
    private CanvasRenderer m_spriteCanvasRenderer;

    /// <summary>
    /// 图片渲染管理
    /// </summary>
    private SpriteGraphicManager m_SGManager;

    #region 动画标签解析
    //最多动态表情数量
    int AnimNum = 8;
  //  List<int> m_AnimIndex;
    List<SpriteTagInfor[]> m_AnimSpiteTag;
    public List<InlineSpriteInfor[]> m_AnimSpriteInfor;
    #endregion

    /// <summary>
    /// 初始化 
    /// </summary>
    protected override void OnEnable()
    {
        //在编辑器中，可能在最开始会出现一张图片，就是因为没有激活文本，在运行中是正常的。可根据需求在编辑器中选择激活...
        base.OnEnable();
        //对齐几何
        alignByGeometry = true;
        
        #region 为了将SpriteGraphicManager显示到最上级，这里的SpriteGraphicManager可能会放在最下面，所以需要从全局去找
        if (m_SGManager == null)
            m_SGManager = GameObject.FindObjectOfType<SpriteGraphicManager>();
        #endregion

        if (m_SGManager != null)
        {
            m_spriteGraphic = m_SGManager.GetComponent<SpriteGraphic>();
            m_spriteCanvasRenderer = m_SGManager.GetComponent<CanvasRenderer>();
            m_spriteAsset = m_spriteGraphic.m_spriteAsset;
        }

        //初始化 调用顶点绘制
        SetVerticesDirty();
    }


   


    /// <summary>
    /// 在设置顶点时调用
    /// </summary>
    public override void SetVerticesDirty()
    {
        base.SetVerticesDirty();
        
      //  m_AnimIndex = new List<int>();
        m_AnimSpiteTag = new List<SpriteTagInfor[]>();

        foreach (Match match in m_spriteTagRegex.Matches(text))
        {
            if (m_spriteAsset == null)
                return;

            #region 解析动画标签
            List<string> tempListName = new List<string>();
            for (int i = 0; i < m_spriteAsset.listSpriteInfor.Count; i++)
            {
               // Debug.Log((m_spriteAsset.listSpriteInfor[i].name));
                if (m_spriteAsset.listSpriteInfor[i].name.Contains(match.Groups[1].Value))
                {
                    tempListName.Add(m_spriteAsset.listSpriteInfor[i].name);
                }
            }
            if (tempListName.Count > 0)
            {
                SpriteTagInfor[] tempArrayTag = new SpriteTagInfor[tempListName.Count];
                for (int i = 0; i < tempArrayTag.Length; i++)
                {
                    tempArrayTag[i] = new SpriteTagInfor();
                    tempArrayTag[i].name = tempListName[i];
                    tempArrayTag[i].index = match.Index;
                    tempArrayTag[i].size = new Vector2(float.Parse(match.Groups[2].Value) * float.Parse(match.Groups[3].Value), float.Parse(match.Groups[2].Value));
                    tempArrayTag[i].Length = match.Length;
                }
                m_AnimSpiteTag.Add(tempArrayTag);
            }
            #endregion
        }
    }

    readonly UIVertex[] m_TempVerts = new UIVertex[4];
    /// <summary>
    /// 绘制模型
    /// </summary>
    /// <param name="toFill"></param>
    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        //  base.OnPopulateMesh(toFill);

        if (font == null)
            return;

        // We don't care if we the font Texture changes while we are doing our Update.
        // The end result of cachedTextGenerator will be valid for this instance.
        // Otherwise we can get issues like Case 619238.
        m_DisableFontTextureRebuiltCallback = true;

        Vector2 extents = rectTransform.rect.size;

        var settings = GetGenerationSettings(extents);
        cachedTextGenerator.Populate(text, settings);

        Rect inputRect = rectTransform.rect;

        // get the text alignment anchor point for the text in local space
        Vector2 textAnchorPivot = GetTextAnchorPivot(alignment);
        Vector2 refPoint = Vector2.zero;
        refPoint.x = (textAnchorPivot.x == 1 ? inputRect.xMax : inputRect.xMin);
        refPoint.y = (textAnchorPivot.y == 0 ? inputRect.yMin : inputRect.yMax);

        // Determine fraction of pixel to offset text mesh.
        Vector2 roundingOffset = PixelAdjustPoint(refPoint) - refPoint;

        // Apply the offset to the vertices
        IList<UIVertex> verts = cachedTextGenerator.verts;
        float unitsPerPixel = 1 / pixelsPerUnit;
        //Last 4 verts are always a new line...
        int vertCount = verts.Count - 4;

        toFill.Clear();

        //清楚乱码
        for (int i = 0; i < m_AnimSpiteTag.Count; i++)
        {
            if (m_AnimSpiteTag[i].Length > 0)
            {
                //UGUIText不支持<quad/>标签，表现为乱码，我这里将他的uv全设置为0,清除乱码
                for (int m = m_AnimSpiteTag[i][0].index * 4; m < m_AnimSpiteTag[i][0].index * 4 + 4; m++)
                {
                    UIVertex tempVertex = verts[m];
                    tempVertex.uv0 = Vector2.zero;
                    verts[m] = tempVertex;
                }
            }
        }
            //计算标签   其实应该计算偏移值后 再计算标签的值    算了 后面再继续改吧
            //  CalcQuadTag(verts);

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

        //计算标签 计算偏移值后 再计算标签的值
        List<UIVertex> vertsTemp = new List<UIVertex>();
        for (int i = 0; i < vertCount; i++)
        {
            UIVertex tempVer=new UIVertex();
            toFill.PopulateUIVertex(ref tempVer,i);
            vertsTemp.Add(tempVer);
        }
        CalcQuadTag(vertsTemp);

        m_DisableFontTextureRebuiltCallback = false;

        //更新绘制图片信息
        if(m_SGManager!=null)
            m_SGManager.UpdateSpriteInfor();
        //DrawSprite();
    }


    private IList<UIVertex> _OldVerts;

    #region 计算标签
    /// <summary>
    /// 解析quad标签  主要清除quad乱码 获取表情的位置
    /// </summary>
    /// <param name="verts"></param>
    void CalcQuadTag(IList<UIVertex> verts)
    {

        m_AnimSpriteInfor = new List<InlineSpriteInfor[]>();

        Vector3 _TempStartPos = Vector3.zero;
        if(m_SGManager!=null)
            _TempStartPos = transform.position - m_SGManager.transform.position;

        for (int i = 0; i < m_AnimSpiteTag.Count; i++)
        {
            SpriteTagInfor[] tempTagInfor = m_AnimSpiteTag[i];
            InlineSpriteInfor[] tempSpriteInfor = new InlineSpriteInfor[tempTagInfor.Length];
            for (int j = 0; j < tempTagInfor.Length; j++)
            {
                tempSpriteInfor[j] = new InlineSpriteInfor();
                tempSpriteInfor[j].textpos = _TempStartPos + verts[((tempTagInfor[j].index + 1) * 4) - 1].position;
                //设置图片的位置
                tempSpriteInfor[j].vertices = new Vector3[4];
                tempSpriteInfor[j].vertices[0] = new Vector3(0, 0, 0) + tempSpriteInfor[j].textpos;
                tempSpriteInfor[j].vertices[1] = new Vector3(tempTagInfor[j].size.x, tempTagInfor[j].size.y, 0) + tempSpriteInfor[j].textpos;
                tempSpriteInfor[j].vertices[2] = new Vector3(tempTagInfor[j].size.x, 0, 0) + tempSpriteInfor[j].textpos;
                tempSpriteInfor[j].vertices[3] = new Vector3(0, tempTagInfor[j].size.y, 0) + tempSpriteInfor[j].textpos;

                //计算其uv
                Rect newSpriteRect = m_spriteAsset.listSpriteInfor[0].rect;
                for (int m = 0; m < m_spriteAsset.listSpriteInfor.Count; m++)
                {
                    //通过标签的名称去索引spriteAsset里所对应的sprite的名称
                    if (tempTagInfor[j].name == m_spriteAsset.listSpriteInfor[m].name)
                        newSpriteRect = m_spriteAsset.listSpriteInfor[m].rect;
                }
                Vector2 newTexSize = new Vector2(m_spriteAsset.texSource.width, m_spriteAsset.texSource.height);

                tempSpriteInfor[j].uv = new Vector2[4];
                tempSpriteInfor[j].uv[0] = new Vector2(newSpriteRect.x / newTexSize.x, newSpriteRect.y / newTexSize.y);
                tempSpriteInfor[j].uv[1] = new Vector2((newSpriteRect.x + newSpriteRect.width) / newTexSize.x, (newSpriteRect.y + newSpriteRect.height) / newTexSize.y);
                tempSpriteInfor[j].uv[2] = new Vector2((newSpriteRect.x + newSpriteRect.width) / newTexSize.x, newSpriteRect.y / newTexSize.y);
                tempSpriteInfor[j].uv[3] = new Vector2(newSpriteRect.x / newTexSize.x, (newSpriteRect.y + newSpriteRect.height) / newTexSize.y);

                //声明三角顶点所需要的数组
                tempSpriteInfor[j].triangles = new int[6];
            }
            m_AnimSpriteInfor.Add(tempSpriteInfor);

            _OldVerts = verts;
        }
    }
    #endregion

    #region 更新图片的信息
    public void UpdateSpriteInfor()
    {
        if (_OldVerts == null)
            return;

        CalcQuadTag(_OldVerts);
    }
    #endregion

}