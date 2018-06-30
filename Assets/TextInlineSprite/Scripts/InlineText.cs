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

namespace EmojiUI
{
    public class InlineText : Text, IPointerClickHandler
    {
        // 用正则取  [图集ID#表情Tag] ID值==-1 ,表示为超链接
        private static readonly Regex _InputTagRegex = new Regex(@"\[(\-{0,1}\d{0,})#(.+?)\]", RegexOptions.Singleline);
        private static StringBuilder _textBuilder;
        private static UIVertex[] m_TempVerts = new UIVertex[4];
        private static TextGenerator _UnderlineText;
        private TextGenerator _SpaceGen;
        private List<Vector3> _listVertsPos;
        private InlineManager _InlineManager;
        //文本表情管理器
        private InlineManager Manager
        {
            get
            {
                if (!_InlineManager && canvas != null)
                {
                    _InlineManager =GetComponentInParent<InlineManager>();
                    if (_InlineManager == null)
                    {
                        _InlineManager = canvas.gameObject.AddComponent<InlineManager>();
                    }
                }
                return _InlineManager;
            }
        }

        List<SpriteTagInfo> RenderTagList;

        private bool needupdate;
        private bool updatespace =true;
        private string _OutputText = "";

        #region 超链接
        [System.Serializable]
        public class HrefClickEvent : UnityEvent<string, int> { }
        //点击事件监听
        public HrefClickEvent OnHrefClick = new HrefClickEvent();
        // 超链接信息列表  
        private List<HrefInfo> _ListHrefInfos;
        #endregion

        private float? _pw;

        public override float preferredWidth
        {
            get
            {
                if(_pw == null)
                {
                    //its override from uGUI Code ,but has bug?

                    //var settings = GetGenerationSettings(Vector2.zero);
                    //return cachedTextGeneratorForLayout.GetPreferredWidth(_OutputText, settings) / pixelsPerUnit;

                    //next idea
                    Vector2 extents = rectTransform.rect.size;

                    var settings = GetGenerationSettings(extents);
                    cachedTextGenerator.Populate(_OutputText, settings);
    
                    if(cachedTextGenerator.lineCount > 1)
                    {
                        float? minx = null;
                        float? maxx = null;
                        IList<UIVertex> verts = cachedTextGenerator.verts;
                        int maxIndex = cachedTextGenerator.lines[1].startCharIdx;

                        for (int i = 0, index = 0; i < verts.Count; i += 4, index++)
                        {
                            UIVertex v0 = verts[i];
                            UIVertex v2 = verts[i + 1];
                            float min = v0.position.x;
                            float max = v2.position.x;

                            if (minx.HasValue == false)
                            {
                                minx = min;
                            }
                            else
                            {
                                minx = Mathf.Min(minx.Value, min);
                            }

                            if (maxx.HasValue == false)
                            {
                                maxx = max;
                            }
                            else
                            {
                                maxx = Mathf.Max(maxx.Value, max);
                            }

                            if (index > maxIndex)
                            {
                                break;
                            }
                        }

                        _pw = (maxx - minx);
                    }
                    else
                    {
                        //_pw = cachedTextGeneratorForLayout.GetPreferredWidth(_OutputText, settings) / pixelsPerUnit;
                        float? minx = null;
                        float? maxx = null;
                        IList<UIVertex> verts = cachedTextGenerator.verts;
                        int maxIndex = cachedTextGenerator.characterCount;

                        for (int i = 0, index = 0; i < verts.Count; i += 4, index++)
                        {
                            UIVertex v0 = verts[i];
                            UIVertex v2 = verts[i + 1];
                            float min = v0.position.x;
                            float max = v2.position.x;

                            if (minx.HasValue == false)
                            {
                                minx = min;
                            }
                            else
                            {
                                minx = Mathf.Min(minx.Value, min);
                            }

                            if (maxx.HasValue == false)
                            {
                                maxx = max;
                            }
                            else
                            {
                                maxx = Mathf.Max(maxx.Value, max);
                            }

                            if (index > maxIndex)
                            {
                                break;
                            }
                        }

                        _pw = (maxx - minx);
                    }
                   
                }
                return _pw.Value;
            }
        }

        public override float preferredHeight
        {
            get
            {
                var settings = GetGenerationSettings(new Vector2(GetPixelAdjustedRect().size.x, 0.0f));
                return cachedTextGeneratorForLayout.GetPreferredHeight(_OutputText, settings) / pixelsPerUnit;
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;

            Vector3 size = new Vector3(preferredWidth, preferredHeight, 0);
            Vector3 fixsize = transform.TransformDirection(rectTransform.rect.size);
            Vector3 textsize =transform.TransformDirection(size);

            Vector3 fixpos = transform.position ;
            //可简化
            if(this.alignment == TextAnchor.LowerCenter)
            {
                fixpos += new Vector3(0, -0.5f * (fixsize.y - textsize.y) , 0);
            }
            else if(this.alignment == TextAnchor.LowerLeft)
            {
                fixpos += new Vector3(-0.5f * (fixsize.x - textsize.x), -0.5f * (fixsize.y - textsize.y), 0);
            }
            else if (this.alignment == TextAnchor.LowerRight)
            {
                fixpos += new Vector3(0.5f * (fixsize.x - textsize.x), -0.5f * (fixsize.y - textsize.y), 0);
            }
            else if (this.alignment == TextAnchor.MiddleCenter)
            {
                fixpos += new Vector3(0, 0, 0);
            }
            else if (this.alignment == TextAnchor.MiddleLeft)
            {
                fixpos += new Vector3(-0.5f * (fixsize.x - textsize.x),0, 0);
            }
            else if (this.alignment == TextAnchor.MiddleRight)
            {
                fixpos += new Vector3(0.5f * (fixsize.x - textsize.x), 0, 0);
            }
            else if (this.alignment == TextAnchor.UpperCenter)
            {
                fixpos += new Vector3(0, 0.5f * (fixsize.y - textsize.y), 0);
            }
            else if (this.alignment == TextAnchor.UpperLeft)
            {
                fixpos += new Vector3(-0.5f * (fixsize.x - textsize.x), 0.5f * (fixsize.y - textsize.y), 0);
            }
            else if (this.alignment == TextAnchor.UpperRight)
            {
                fixpos += new Vector3(0.5f * (fixsize.x - textsize.x), 0.5f * (fixsize.y - textsize.y), 0);
            }

            Gizmos.DrawWireCube(fixpos, textsize);
        }

        protected override void Start()
        {
            base.Start();

            EmojiTools.AddUnityMemory(this);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

           // supportRichText = true;
            SetVerticesDirty();
        }

        public override void SetVerticesDirty()
        {
            base.SetVerticesDirty();
            if (!Application.isPlaying || !Manager)
            {
                _pw = null;
                _OutputText = m_Text;
                return;
            }

            string outtext = GetOutputText(m_Text);
            if (RenderTagList != null && RenderTagList.Count > 0)
            {
                _OutputText = outtext;
                needupdate = true;
                _pw = null;
            }
            else
            {
                _OutputText = m_Text;
                needupdate = true;
                _pw = null;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if(RenderTagList != null)
            {
                ListPool<SpriteTagInfo>.Release(RenderTagList);
                RenderTagList = null;
            }

            if(_ListHrefInfos != null)
            {
                ListPool<HrefInfo>.Release(_ListHrefInfos);
                _ListHrefInfos = null;
            }

            if(Manager)
            {
                Manager.UnRegister(this);
            }

            EmojiTools.RemoveUnityMemory(this);
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
            cachedTextGenerator.PopulateWithErrors(_OutputText, settings, gameObject);

            // Apply the offset to the vertices
            IList<UIVertex> verts = cachedTextGenerator.verts;
            float unitsPerPixel = 1 / pixelsPerUnit;
            //Last 4 verts are always a new line... (\n)
            int vertCount = verts.Count - 4;
            Vector2 roundingOffset = new Vector2(verts[0].position.x, verts[0].position.y) * unitsPerPixel;
            roundingOffset = PixelAdjustPoint(roundingOffset) - roundingOffset;
            toFill.Clear();

            ClearQuadUVs(verts);

            if(RenderTagList != null && RenderTagList.Count >0)
            {
                if(_listVertsPos == null)
                    _listVertsPos = new List<Vector3>();
                else
                    _listVertsPos.Clear();
            }

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

                    if (RenderTagList != null && RenderTagList.Count > 0)
                        _listVertsPos.Add(m_TempVerts[tempVertsIndex].position);
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

                    if(RenderTagList != null && RenderTagList.Count > 0)
                        _listVertsPos.Add(m_TempVerts[tempVertsIndex].position);

                }
            }

            ////计算quad占位的信息
            CalcQuadInfo();
            //计算包围盒
            CalcBoundsInfo(toFill, settings);

            if (needupdate && Manager != null)
            {
                if((_ListHrefInfos != null && _ListHrefInfos.Count >0 ) || (RenderTagList != null && RenderTagList.Count >0))
                {
                    Manager.Register(this);
                }
                else
                {
                    Manager.UnRegister(this);
                }
            }

            m_DisableFontTextureRebuiltCallback = false;
            needupdate = false;
        }

        private void ClearQuadUVs(IList<UIVertex> verts)
        {
            if (RenderTagList != null)
            {
                for (int i = 0; i < RenderTagList.Count; ++i)
                {
                    SpriteTagInfo info = RenderTagList[i];
                    int pos = info._Position;
                    if ((pos + 4) > verts.Count)
                        continue;
                    for (int m = pos; m < pos + 4; m++)
                    {
                        //清除乱码
                        UIVertex tempVertex = verts[m];
                        tempVertex.uv0 = Vector2.zero;
                        verts[m] = tempVertex;
                    }
                }
            }

        }

        void CalcQuadInfo()
        {
            if(RenderTagList != null)
            {
                float height = this.preferredHeight /2;
                for(int i =0; i < RenderTagList.Count;++i)
                {
                    SpriteTagInfo info = RenderTagList[i];
                    int pos = info._Position;
                    if ((pos + 4) > _listVertsPos.Count)
                        continue;

                    Vector3 p1 = _listVertsPos[pos];

                    //info._Pos[0] = p1 ;
                    //info._Pos[1] = _listVertsPos[pos+1];
                    //info._Pos[2] = _listVertsPos[pos+2];
                    //info._Pos[3] = _listVertsPos[pos+3];
                    //int cid = ((int)alignment) /3;
                    //int rid = ((int)alignment) % 3;

                    info._Pos[0] = p1 + new Vector3(0, info._Size.y/2 + height, 0);
                    info._Pos[1] = p1 + new Vector3(info._Size.x,info._Size.y/ 2 + height , 0);
                    info._Pos[2] = p1 + new Vector3(info._Size.x, height - info._Size.y / 2, 0);
                    info._Pos[3] = p1 + new Vector3(0, height - info._Size.y / 2, 0);
                }
            }

        }

        void CalcBoundsInfo(VertexHelper toFill, TextGenerationSettings settings)
        {

            if(_ListHrefInfos != null && _ListHrefInfos.Count >0)
            {

                if (_listVertsPos == null)
                    _listVertsPos = new List<Vector3>();
                // 处理超链接包围框  
                for (int k = 0; k< _ListHrefInfos.Count;++k)
                {
                    var hrefInfo = _ListHrefInfos[k];
                    hrefInfo.boxes.Clear();
                    if (hrefInfo.startIndex >= _listVertsPos.Count)
                    {
                        continue;
                    }

                    // 将超链接里面的文本顶点索引坐标加入到包围框  
                    var pos = _listVertsPos[hrefInfo.startIndex];
                    var bounds = new Bounds(pos, Vector3.zero);
                    for (int i = hrefInfo.startIndex, m = hrefInfo.endIndex; i < m; i++)
                    {
                        if (i >= _listVertsPos.Count)
                        {
                            break;
                        }

                        pos = _listVertsPos[i];
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


                if(_UnderlineText == null)
                {
                    _UnderlineText = new TextGenerator();
                    _UnderlineText.Populate("_", settings);
                }

                IList<UIVertex> _TUT = _UnderlineText.verts;

                for (int m = 0; m < _ListHrefInfos.Count; ++m)
                {
                    var item = _ListHrefInfos[m];
                    for (int i = 0; i < item.boxes.Count; i++)
                    {
    
                        //绘制下划线
                        for (int j = 0; j < 4; j++)
                        {
                            m_TempVerts[j] = _TUT[j];
                            m_TempVerts[j].color = Color.blue;

                            if(j ==0)
                            {
                                m_TempVerts[j].position =item.boxes[i].position + new Vector2(0.0f, fontSize * 0.2f);
                            }
                            else if(j == 1)
                            {
                                m_TempVerts[j].position =item.boxes[i].position + new Vector2(item.boxes[i].width, fontSize * 0.2f);
                            }
                            else if(j == 2)
                            {
                                m_TempVerts[j].position = item.boxes[i].position + new Vector2(item.boxes[i].width, 0.0f);
                            }
                            else
                            {
                                m_TempVerts[j].position = item.boxes[i].position;
                            }

                            if (j == 3)
                                toFill.AddUIVertexQuad(m_TempVerts);
                        }

                    }
                }
            }
        }

        public List<SpriteTagInfo> PopEmojiData()
        {
            return RenderTagList;
        }

        bool ParseHref(string newInfo,int hrefCnt, int Id,string TagName, Match match,ref int _textIndex)
        {
            if(Id <0)
            {
                _textBuilder.Append(newInfo.Substring(_textIndex, match.Index - _textIndex));
                _textBuilder.Append("<color=blue>");
                int _startIndex = _textBuilder.Length * 4;
                _textBuilder.Append("[" + TagName + "]");
                int _endIndex = _textBuilder.Length * 4 - 2;
                _textBuilder.Append("</color>");

                if(_ListHrefInfos.Count > hrefCnt)
                {
                    HrefInfo hrefInfo = _ListHrefInfos[hrefCnt];
                    hrefInfo.id = Mathf.Abs(Id);
                    hrefInfo.startIndex = _startIndex;// 超链接里的文本起始顶点索引
                    hrefInfo.endIndex = _endIndex;
                    hrefInfo.name = TagName;
                }
                else
                {
                    HrefInfo hrefInfo = new HrefInfo
                    {
                        id = Mathf.Abs(Id),
                        startIndex = _startIndex, // 超链接里的文本起始顶点索引
                        endIndex = _endIndex,
                        name = TagName
                    };
                    _ListHrefInfos.Add(hrefInfo);
                }


                return true;
            }
            return false;
        }

        bool ParseEmoji(string newInfo,int Index, int Id, string TagName, Match match, ref int _textIndex)
        {
            if (Manager != null && Manager.CanRendering(Id) && Manager.CanRendering(TagName))
            {
                SpriteAsset sprAsset;
                SpriteInfoGroup tagSprites = Manager.FindSpriteGroup(TagName, out sprAsset);
                if (tagSprites != null && tagSprites.spritegroups.Count > 0)
                {
                    if (!Manager.isRendering(sprAsset))
                    {
                        Manager.PushRenderAtlas(sprAsset);
                    }

                    _textBuilder.Append(newInfo.Substring(_textIndex, match.Index - _textIndex));
                    int _tempIndex = _textBuilder.Length * 4;

                    float h = Mathf.Max(1, this.rectTransform.rect.height - 8);
                    float autosize = Mathf.Min(h, tagSprites.size);
                    if (_SpaceGen == null)
                    { 
                        _SpaceGen = new TextGenerator();
   
                    }

                    if(updatespace)
                    {
                        Vector2 extents = rectTransform.rect.size;
                        TextGenerationSettings settings = GetGenerationSettings(extents);
                        _SpaceGen.Populate(" ", settings);
                        updatespace = false;
                    }

                    IList<UIVertex> spaceverts = _SpaceGen.verts;
                    float spacewid = spaceverts[1].position.x - spaceverts[0].position.x;
                    float spaceheight = spaceverts[0].position.y - spaceverts[3].position.y;
                    float spacesize = Mathf.Max(spacewid, spaceheight);
     
                    int fillspacecnt = Mathf.RoundToInt(autosize / spacesize);
                    if (autosize > spacesize)
                    {
                        fillspacecnt = Mathf.RoundToInt((autosize ) / (spacesize ));
                    }
                    else
                    {
                        fillspacecnt = Mathf.RoundToInt(autosize / spacesize);
                    }
                    for (int i = 0; i < fillspacecnt; i++)
                    {
                        _textBuilder.Append(" ");
                    }

                    //_textBuilder.AppendFormat("<quad material=0 x={0} y={1} size={2} width={3} />", tagSprites.x, tagSprites.y, autosize, tagSprites.width);

                    if (RenderTagList.Count > Index)
                    {
                        SpriteTagInfo _tempSpriteTag = RenderTagList[Index];
                        if(Id != _tempSpriteTag._ID || TagName != _tempSpriteTag._Tag || _tempIndex != _tempSpriteTag._Position)
                        {
                            _tempSpriteTag._ID = Id;
                            _tempSpriteTag._Tag = TagName;
                            _tempSpriteTag._Size = new Vector2(autosize * tagSprites.width, autosize);
                            _tempSpriteTag._Position = _tempIndex;
                            _tempSpriteTag._UV = tagSprites.spritegroups[0].uv;
                        }
                    }
                    else
                    {
                        SpriteTagInfo _tempSpriteTag = new SpriteTagInfo
                        {
                            _ID = Id,
                            _Tag = TagName,
                            _Size = new Vector2(autosize * tagSprites.width, autosize),
                            _Pos = new Vector3[4],
                            _Position = _tempIndex,
                            _UV = tagSprites.spritegroups[0].uv
                        };

                        RenderTagList.Add(_tempSpriteTag);
                    }

                    return true;
                }
                else
                {
                    Debug.LogErrorFormat("missing {0} or lenght too low :{1}", TagName, tagSprites != null ? tagSprites.spritegroups.Count : 0);
                }
                
            }
            else if(Manager)
            {
                Debug.LogErrorFormat("not found that atlas:{0} or Tag:{1}", Id, TagName);
            }
            return false;
        }

        private string GetOutputText(string newinfo)
        {
            if (RenderTagList == null)
                RenderTagList = ListPool<SpriteTagInfo>.Get();

            if (_ListHrefInfos == null)
                _ListHrefInfos = ListPool<HrefInfo>.Get();

            if (_textBuilder == null)
                _textBuilder = new StringBuilder(newinfo.Length);
            else
                _textBuilder.Length = 0;

            int _textIndex = 0;
            int hrefCnt = 0;
            int emojicnt = 0;

            MatchCollection en = _InputTagRegex.Matches(newinfo);
            for(int i =0; i < en.Count;++i)
            {
                Match match = en[i];

                int tempId = 0;
                if(match.Groups.Count>0 )
                {
                    string firstgpval = match.Groups[1].Value;
                    if (!string.IsNullOrEmpty(firstgpval) && !firstgpval.Equals("-"))
                    {
                        tempId = int.Parse(firstgpval);
                    }
                }

                string tempTag = null;
                if (match.Groups.Count > 1)
                {
                    tempTag = match.Groups[2].Value;
                }

                if(!ParseHref(newinfo, hrefCnt, tempId, tempTag,match,ref _textIndex))
                {
                    if(ParseEmoji(newinfo, emojicnt, tempId, tempTag, match, ref _textIndex))
                    {
                        emojicnt++;
                    }
                }
                else
                {
                    hrefCnt++;
                }

                _textIndex = match.Index + match.Length;
            }

            //remove unused
            for(int i = RenderTagList.Count -1; i >= emojicnt;--i)
            {
                SpriteTagInfo info = RenderTagList[i];
                RenderTagList.RemoveAt(i);
            }

            for (int i = _ListHrefInfos.Count - 1; i >= hrefCnt; --i)
            {
                HrefInfo info = _ListHrefInfos[i];
                _ListHrefInfos.RemoveAt(i);
            }

            _textBuilder.Append(newinfo.Substring(_textIndex, newinfo.Length - _textIndex));

            updatespace = true;
            return _textBuilder.ToString();
        }


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
                        OnHrefClick.Invoke(hrefInfo.name, hrefInfo.id);
                        return;
                    }
                }
            }
        }
        #endregion

    }
}





