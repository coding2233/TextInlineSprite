using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/********
为了图片渲染在最上面
需要将他放砸canvas的最下层
应该可以改shader的渲染顺序  没去试  就这样写吧
*********/

[RequireComponent(typeof(SpriteGraphic))]
public class SpriteGraphicManager : MonoBehaviour {

    /// <summary>
    /// 需要渲染的图片信息列表
    /// </summary>
    private List<InlineSpriteInfor> listSprite;
    #region 动画标签解析
    //最多动态表情数量
    int AnimNum = 8;
    List<InlineSpriteInfor[]> m_AnimSpriteInfor;
    #endregion

    #region 更新图片信息
    public void UpdateSpriteInfor()
    {
        listSprite = new List<InlineSpriteInfor>();
        m_AnimSpriteInfor = new List<InlineSpriteInfor[]>();

        //  inline
        //  InlieSpriteText[] AllInlieSpriteText = GetComponentsInChildren<InlieSpriteText>();
        //  找到所有InlieSpriteText的物体  ----  这里隐藏问题蛮大的  他搜索的所有的InlieSpriteText
        //  包括InlieSpriteText也是全局搜索的SpriteGraphicManager，意思SpriteGraphicManager最好只有一个 
        //  当然 可以自定义根据功能   自己改了  我这里是这么定义的
        InlieSpriteText[] AllInlieSpriteText = GameObject.FindObjectsOfType<InlieSpriteText>();

        for (int i = 0; i < AllInlieSpriteText.Length; i++)
        {
            if (AllInlieSpriteText[i].m_AnimSpriteInfor != null)
            {
                AllInlieSpriteText[i].UpdateSpriteInfor();
                for (int j = 0; j < AllInlieSpriteText[i].m_AnimSpriteInfor.Count; j++)
                {
                    m_AnimSpriteInfor.Add(AllInlieSpriteText[i].m_AnimSpriteInfor[j]);
                    listSprite.Add(AllInlieSpriteText[i].m_AnimSpriteInfor[j][0]);
                }
            }
        }
        DrawSprite();
    }
    #endregion


    #region update刷新动画
    float fTime = 0.0f;
    int iIndex = 0;
    void Update()
    {
        if (m_AnimSpriteInfor == null)
            return;

        fTime += Time.deltaTime;
        if (fTime >= 0.1f)
        {
            //刷新一次 更新绘制图片的相关信息
            UpdateSpriteInfor();

            for (int i = 0; i < m_AnimSpriteInfor.Count; i++)
            {
                if (iIndex >= m_AnimSpriteInfor[i].Length)
                {
                    listSprite[i] = m_AnimSpriteInfor[i][0];
                }
                else
                {
                    listSprite[i] = m_AnimSpriteInfor[i][iIndex];
                }
            }
            DrawSprite();
            iIndex++;
            if (iIndex >= AnimNum)
            {
                iIndex = 0;
            }
            fTime = 0.0f;
        }
    }
    #endregion

    

    #region 绘制图片
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
        
        GetComponent<CanvasRenderer>().SetMesh(m_spriteMesh);
        GetComponent<SpriteGraphic>().UpdateMaterial();
    }
    #endregion

}
