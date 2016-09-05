using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text.RegularExpressions;

//[ExecuteInEditMode]
public class InlieText : Text {

    /// <summary>
    /// 用正则取标签属性 名称-大小-宽度比例
    /// </summary>
    private static readonly Regex m_spriteTagRegex =
          new Regex(@"<quad name=(.+?) size=(\d*\.?\d+%?) width=(\d*\.?\d+%?) />", RegexOptions.Singleline);
    /// <summary>
    /// 需要渲染的图片信息列表
    /// </summary>
    private List<InlineSpriteInfor> listSprite;
    /// <summary>
    /// 图片资源
    /// </summary>
    private SpriteAsset m_spriteAsset;
    /// <summary>
    /// 标签的信息列表
    /// </summary>
    private List<SpriteTagInfor> listTagInfor;
    /// <summary>
    /// 图片渲染组件
    /// </summary>
    private SpriteGraphic m_spriteGraphic;
    /// <summary>
    /// CanvasRenderer
    /// </summary>
    private CanvasRenderer m_spriteCanvasRenderer;

    /// <summary>
    /// 初始化 
    /// </summary>
    protected override void OnEnable()
    {
        //在编辑器中，可能在最开始会出现一张图片，就是因为没有激活文本，在运行中是正常的。可根据需求在编辑器中选择激活...
        base.OnEnable();

        if (m_spriteGraphic == null)
            m_spriteGraphic = GetComponentInChildren<SpriteGraphic>();
        if (m_spriteCanvasRenderer == null)
            m_spriteCanvasRenderer = m_spriteGraphic.GetComponentInChildren<CanvasRenderer>();
        m_spriteAsset = m_spriteGraphic.m_spriteAsset;
    }

    /// <summary>
    /// 在设置顶点时调用
    /// </summary>
    public override void SetVerticesDirty()
    {
        base.SetVerticesDirty();
        //解析标签属性
        listTagInfor = new List<SpriteTagInfor>();
        foreach (Match match in m_spriteTagRegex.Matches(text))
        {
            SpriteTagInfor tempSpriteTag = new SpriteTagInfor();
            tempSpriteTag.name = match.Groups[1].Value;
            tempSpriteTag.index = match.Index;
            tempSpriteTag.size = new Vector2(float.Parse(match.Groups[2].Value)*float.Parse(match.Groups[3].Value), float.Parse(match.Groups[2].Value));
            tempSpriteTag.Length = match.Length;
            listTagInfor.Add(tempSpriteTag);
        }

        Vector2 extents = rectTransform.rect.size;

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
        for (int i = 0; i < listTagInfor.Count; i++)
        {
            //UGUIText不支持<quad/>标签，表现为乱码，我这里将他的uv全设置为0,清除乱码
            for (int m = listTagInfor[i].index * 4; m < listTagInfor[i].index * 4 +4; m++)
            {
                UIVertex tempVertex = verts[m];
                tempVertex.uv0 = Vector2.zero;
                verts[m] = tempVertex;
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
        
        //绘制图片
        DrawSprite();
    }

    /// <summary>
    /// 解析quad标签  主要清除quad乱码 获取表情的位置
    /// </summary>
    /// <param name="verts"></param>
    void CalcQuadTag(IList<UIVertex> verts)
    {
        //通过标签信息来设置需要绘制的图片的信息
        listSprite = new List<InlineSpriteInfor>();
        for (int i = 0; i < listTagInfor.Count; i++)
        {
            ////UGUIText不支持<quad/>标签，表现为乱码，我这里将他的uv全设置为0,清除乱码
            //for (int m = listTagInfor[i].index * 4; m < listTagInfor[i].index * 4 + 18*4; m++)
            //{
            //    UIVertex tempVertex = verts[m];
            //    tempVertex.uv0 = Vector2.zero;
            //    verts[m] = tempVertex;
            //}

            InlineSpriteInfor tempSprite = new InlineSpriteInfor();

            //获取表情的第一个位置,则计算他的位置为quad占位的第四个点   顶点绘制顺序:       
            //                                                                              0    1
            //                                                                              3    2
            tempSprite.textpos = verts[((listTagInfor[i].index+1) * 4) - 1].position;

            //设置图片的位置
            tempSprite.vertices = new Vector3[4];
            tempSprite.vertices[0] = new Vector3(0, 0, 0) + tempSprite.textpos;
            tempSprite.vertices[1] = new Vector3(listTagInfor[i].size.x, listTagInfor[i].size.y, 0) + tempSprite.textpos;
            tempSprite.vertices[2] = new Vector3(listTagInfor[i].size.x, 0, 0) + tempSprite.textpos;
            tempSprite.vertices[3] = new Vector3(0, listTagInfor[i].size.y, 0) + tempSprite.textpos;

            //计算其uv
            Rect spriteRect = m_spriteAsset.listSpriteInfor[0].rect;
            for (int j = 0; j < m_spriteAsset.listSpriteInfor.Count; j++)
            {
                //通过标签的名称去索引spriteAsset里所对应的sprite的名称
                if (listTagInfor[i].name == m_spriteAsset.listSpriteInfor[j].name)
                    spriteRect = m_spriteAsset.listSpriteInfor[j].rect;
            }
            Vector2 texSize = new Vector2(m_spriteAsset.texSource.width, m_spriteAsset.texSource.height);

            tempSprite.uv = new Vector2[4];
            tempSprite.uv[0] = new Vector2(spriteRect.x / texSize.x, spriteRect.y / texSize.y);
            tempSprite.uv[1] = new Vector2((spriteRect.x + spriteRect.width) / texSize.x, (spriteRect.y + spriteRect.height) / texSize.y);
            tempSprite.uv[2] = new Vector2((spriteRect.x + spriteRect.width) / texSize.x, spriteRect.y / texSize.y);
            tempSprite.uv[3] = new Vector2(spriteRect.x / texSize.x, (spriteRect.y + spriteRect.height) / texSize.y);

            //声明三角顶点所需要的数组
            tempSprite.triangles = new int[6];
            listSprite.Add(tempSprite);
        }
    }
    
    /// <summary>
    /// 绘制图片
    /// </summary>
    void DrawSprite()
    {
        Mesh m_spriteMesh = new Mesh();

        List<Vector3> tempVertices = new List<Vector3>();
        List<Vector2> tempUv = new List<Vector2>();
        List<int> tempTriangles = new List<int>();
        
        for (int i = 0; i < listSprite.Count; i++)
        {
            for (int j = 0; j < listSprite[i].vertices.Length; j++)
            {
                tempVertices.Add(listSprite[i].vertices[j]);
            }
            for (int j = 0; j < listSprite[i].uv.Length; j++)
            {
                tempUv.Add(listSprite[i].uv[j]);
            }
            for (int j = 0; j < listSprite[i].triangles.Length; j++)
            {
                tempTriangles.Add(listSprite[i].triangles[j]);
            }
        }
        //计算顶点绘制顺序
        for (int i = 0; i < tempTriangles.Count; i++)
        {
            if (i % 6 == 0)
            {
                int num = i / 6;
                tempTriangles[i] = 0 + 4 * num;
                tempTriangles[i + 1] = 1 + 4 * num;
                tempTriangles[i + 2] = 2 + 4 * num;

                tempTriangles[i + 3] = 1 + 4 * num;
                tempTriangles[i + 4] = 0 + 4 * num;
                tempTriangles[i + 5] = 3 + 4 * num;
            }
        }

        m_spriteMesh.vertices = tempVertices.ToArray();
        m_spriteMesh.uv = tempUv.ToArray();
        m_spriteMesh.triangles = tempTriangles.ToArray();

        if (m_spriteMesh == null)
            return;

        m_spriteCanvasRenderer.SetMesh(m_spriteMesh);
        m_spriteGraphic.UpdateMaterial();
    }

}

[System.Serializable]
public class SpriteTagInfor
{
    /// <summary>
    /// sprite名称
    /// </summary>
    public string name;
    /// <summary>
    /// 对应的字符索引
    /// </summary>
    public int index;
    /// <summary>
    /// 大小
    /// </summary>
    public Vector2 size;

    public int Length;
}


[System.Serializable]
public class InlineSpriteInfor
{
    // 文字的最后的位置
    public Vector3 textpos;
    // 4 顶点 
    public Vector3[] vertices;
    //4 uv
    public Vector2[] uv;
    //6 三角顶点顺序
    public int[] triangles;
}
