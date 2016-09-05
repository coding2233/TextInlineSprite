using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public partial class InlineSpriteManager {

    private Text m_text;
    private TextGenerator m_textGenerator;
    private TextGenerationSettings m_textSetting;
    private SpriteGraphic m_spriteGraphic;
    private CanvasRenderer m_spriteCanvasRenderer;


    public SpriteAsset m_spriteAsset;
    // public int index = 0;

    private List<InlineSpriteInfor> listSprite;


    void Awake()
    {
        m_text = GetComponent<Text>();
        m_spriteGraphic = transform.GetComponentInChildren<SpriteGraphic>();
        //  m_spriteCanvasRenderer = m_spriteGraphic.GetComponent<CanvasRenderer>();

        m_textGenerator = m_text.cachedTextGeneratorForLayout;
        m_textSetting = m_text.GetGenerationSettings(Vector2.zero);

        listSprite = new List<InlineSpriteInfor>();
    }

    // Use this for initialization
    void Start()
    {
     //   SaveTextfor();
    }

    string lastText = "";
    string dealText = "";
    public  void SaveTextfor()
    {
        lastText = m_text.text;
        dealText = m_text.text;
        m_text.text = "";
        DealTextInfor(dealText);
    }

    void DealTextInfor(string value)
    {
        if (m_spriteAsset == null)
            return;
        if (m_spriteAsset.listSpriteInfor.Count <= 0)
            return;

        if (value.Contains("<sprite="))
        {
            //获取到<sprite前面的所有内容 并确定最后一个字符的位置信息 再+四个空格 赋值给m_text
            int iNum = value.IndexOf("<sprite=");
            m_text.text += value.Substring(0, iNum);
            Debug.Log("m_Text preferredWidth:" + m_text.preferredWidth + "  m_Text preferredHeight:" + m_text.preferredHeight);
            //Debug.Log("m_Text line count:" + m_TextGenerator.GetLinesArray().Length);
            UILineInfo[] linesInfo = m_textGenerator.GetLinesArray();
            for (int i = 0; i < linesInfo.Length; i++)
            {
                Debug.Log("start char Index:" + linesInfo[i].startCharIdx + "start char:" + m_text.text.ToCharArray()[linesInfo[i].startCharIdx] + "     line height:" + linesInfo[i].height);
            }
            Vector3 textpos = Vector3.zero;
            int startCharIdx = linesInfo[linesInfo.Length - 1].startCharIdx;
            textpos.x = m_textGenerator.GetPreferredWidth(m_text.text.Substring(startCharIdx, m_text.text.Length - startCharIdx), m_textSetting);
            textpos.y = -(m_text.preferredHeight);
            Debug.Log("text pos:" + textpos);

            m_text.text += "    ";
            //获取到精灵索引的ID 并去掉精灵标签
            value = value.Substring(iNum, value.Length - iNum);
            Debug.Log("value:" + value);
            iNum = value.IndexOf("/>");
            int iSpriteIndex = int.Parse(value.Substring(0, iNum).Trim().Replace("<sprite=", ""));
            SaveSpriteInfor(textpos, m_textGenerator.GetPreferredWidth("    ", m_textSetting), iSpriteIndex);
            Debug.Log("sprite index:" + iSpriteIndex);

            value = value.Substring(iNum + 2, value.Length - iNum - 2);
            DealTextInfor(value);
        }
        else
        {
            m_text.text += value;
            DrawSprite();
        }
    }

    void SaveSpriteInfor(Vector3 textpos, float spriteSize, int spriteIndex)
    {
        //if (spriteIndex >= m_spriteAsset.listSpriteInfor.Count)
        //    return;

        // float spriteSize = m_text.fontSize;
        InlineSpriteInfor tempSprite = new InlineSpriteInfor();
        //  tempSprite.textpos = new Vector3(fWidth, -fFontSize, 0.0f);
        tempSprite.textpos = textpos;
        tempSprite.vertices = new Vector3[4];
        tempSprite.vertices[0] = new Vector3(0, 0, 0) + tempSprite.textpos;
        tempSprite.vertices[1] = new Vector3(spriteSize, spriteSize, 0) + tempSprite.textpos;
        tempSprite.vertices[2] = new Vector3(spriteSize, 0, 0) + tempSprite.textpos;
        tempSprite.vertices[3] = new Vector3(0, spriteSize, 0) + tempSprite.textpos;

        Rect spriteRect = m_spriteAsset.listSpriteInfor[0].rect;
        for (int i = 0; i < m_spriteAsset.listSpriteInfor.Count; i++)
        {
            if (spriteIndex == m_spriteAsset.listSpriteInfor[i].ID)
                spriteRect = m_spriteAsset.listSpriteInfor[i].rect;
        }

        Vector2 texSize = new Vector2(m_spriteAsset.texSource.width, m_spriteAsset.texSource.height);

        tempSprite.uv = new Vector2[4];
        tempSprite.uv[0] = new Vector2(spriteRect.x / texSize.x, spriteRect.y / texSize.y);
        tempSprite.uv[1] = new Vector2((spriteRect.x + spriteRect.width) / texSize.x, (spriteRect.y + spriteRect.height) / texSize.y);
        tempSprite.uv[2] = new Vector2((spriteRect.x + spriteRect.width) / texSize.x, spriteRect.y / texSize.y);
        tempSprite.uv[3] = new Vector2(spriteRect.x / texSize.x, (spriteRect.y + spriteRect.height) / texSize.y);

        tempSprite.triangles = new int[6];
        //tempSprite.triangles[0] = 0;
        //tempSprite.triangles[1] = 1;
        //tempSprite.triangles[2] = 2;
        //tempSprite.triangles[3] = 1;
        //tempSprite.triangles[4] = 0;
        //tempSprite.triangles[5] = 3;

        listSprite.Add(tempSprite);
    }



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

        if (m_spriteCanvasRenderer == null)
            m_spriteCanvasRenderer = m_spriteGraphic.GetComponent<CanvasRenderer>();

        m_spriteCanvasRenderer.SetMesh(m_spriteMesh);
        m_spriteGraphic.UpdateMaterial();
    }
}
