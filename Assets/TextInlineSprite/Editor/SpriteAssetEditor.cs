using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(SpriteAsset))]
public class SpriteAssetEditor : Editor
{
    private SpriteAsset spriteAsset;
    private Vector2 ve2ScorllView;
    //当前的所有标签
    private List<string> tags;
    //当前的所有标签序列动画索引
    private List<int> playIndexs;
    private float playIndex;
    //当前展开的标签索引
    private int showIndex;
    //序列帧播放速度
    private float playSpeed;
    //添加标签
    private bool addTag;
    //添加的标签的名称
    private string addTagName;

    public void OnEnable()
    {
        spriteAsset = (SpriteAsset)target;

        playSpeed = 6;

        Init();

   //     EditorApplication.update += RefreshFrameAnimation;
    }

    public void OnDisable()
    {
    //    EditorApplication.update -= RefreshFrameAnimation;
    }
    
    public override void OnInspectorGUI()
    {

        ve2ScorllView = GUILayout.BeginScrollView(ve2ScorllView);

        #region 标题栏
        EditorGUILayout.HelpBox("Number Of Tags:" + spriteAsset.listSpriteGroup.Count + "     Number Of Group:" + spriteAsset.listSpriteGroup.Count, MessageType.Info);

        GUILayout.BeginVertical("HelpBox");
        GUILayout.BeginHorizontal();
        spriteAsset.ID = EditorGUILayout.IntField("ID:", spriteAsset.ID);
      //  playSpeed = EditorGUILayout.FloatField("FrameSpeed", playSpeed);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        spriteAsset._IsStatic = EditorGUILayout.Toggle("Static:", spriteAsset._IsStatic);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Tag"))
        {
            addTag = !addTag;
        }
        GUILayout.EndHorizontal();
        if (addTag)
        {
            GUILayout.BeginHorizontal();
            addTagName = EditorGUILayout.TextField(addTagName);
            if (GUILayout.Button("sure", "minibutton"))
            {
                if (addTagName == "")
                {
                    Debug.Log("请输入新建标签的名称！");
                }
                else
                {
                    SpriteInforGroup spriteInforGroup = spriteAsset.listSpriteGroup.Find(
                        delegate (SpriteInforGroup sig)
                        {
                            return sig.tag == addTagName;
                        });

                    if (spriteInforGroup != null)
                    {
                        Debug.Log("该标签已存在！");
                    }
                    else
                    {
                        AddTagSure();
                    }
                }
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear Tag"))
        {
            ClearTag();
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        GUILayout.BeginHorizontal();
        GUILayout.Label("");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        #endregion

        for (int i = 0; i < spriteAsset.listSpriteGroup.Count; i++)
        {
            GUILayout.BeginHorizontal("HelpBox");
            #region 展开与收缩按钮
            if (GUILayout.Button(spriteAsset.listSpriteGroup[i].tag, showIndex == i ? "OL Minus" : "OL Plus"))
            {
                if (showIndex == i)
                {
                    showIndex = -1;
                }
                else
                {
                    showIndex = i;
                }
            }
            #endregion

            GUILayout.BeginHorizontal();
            GUILayout.Label("Size:", GUILayout.Width(40));
            spriteAsset.listSpriteGroup[i].size=EditorGUILayout.FloatField("", spriteAsset.listSpriteGroup[i].size, GUILayout.Width(40));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Width:", GUILayout.Width(40));
            spriteAsset.listSpriteGroup[i].width=EditorGUILayout.FloatField("", spriteAsset.listSpriteGroup[i].width, GUILayout.Width(40));
            GUILayout.EndHorizontal();

            #region 未展开的sprite组，播放序列帧动画（帧数大于1的序列帧动画才播放）
            if (showIndex != i && spriteAsset.listSpriteGroup[i].listSpriteInfor.Count > 0)
            {
                if (playIndexs[i] >= spriteAsset.listSpriteGroup[i].listSpriteInfor.Count)
                    playIndexs[i] = 0;

                GUI.enabled = false;
                EditorGUILayout.ObjectField("", spriteAsset.listSpriteGroup[i].listSpriteInfor[playIndexs[i]].sprite, typeof(Sprite), false);
                GUI.enabled = true;
            }
            #endregion
            GUILayout.EndHorizontal();

            #region 展开的sprite组，显示所有sprite属性
            if (showIndex == i)
            {
                for (int j = 0; j < spriteAsset.listSpriteGroup[i].listSpriteInfor.Count; j++)
                {
                    GUILayout.BeginHorizontal("sprite" + j, "window");
                    spriteAsset.listSpriteGroup[i].listSpriteInfor[j].sprite = EditorGUILayout.ObjectField("", spriteAsset.listSpriteGroup[i].listSpriteInfor[j].sprite, typeof(Sprite), false, GUILayout.Width(80)) as Sprite;

                    GUILayout.FlexibleSpace();

                    GUILayout.BeginVertical();

                    GUI.enabled = false;

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("ID:", GUILayout.Width(50));
                    spriteAsset.listSpriteGroup[i].listSpriteInfor[j].ID = EditorGUILayout.IntField(spriteAsset.listSpriteGroup[i].listSpriteInfor[j].ID);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Name:", GUILayout.Width(50));
                    spriteAsset.listSpriteGroup[i].listSpriteInfor[j].name = EditorGUILayout.TextField(spriteAsset.listSpriteGroup[i].listSpriteInfor[j].name);
                    GUILayout.EndHorizontal();

                    GUI.enabled = true;

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Tag:", GUILayout.Width(50));
                    if (GUILayout.Button(spriteAsset.listSpriteGroup[i].listSpriteInfor[j].tag, "MiniPopup"))
                    {
                        GenericMenu gm = new GenericMenu();
                        for (int n = 0; n < tags.Count; n++)
                        {
                            int i2 = i;
                            int j2 = j;
                            int n2 = n;
                            gm.AddItem(new GUIContent(tags[n2]), false, 
                                delegate () 
                                {
                                    ChangeTag(tags[n2], spriteAsset.listSpriteGroup[i2].listSpriteInfor[j2]);
                                });
                        }
                        gm.ShowAsContext();
                    }
                    GUILayout.EndHorizontal();
                    
                    GUI.enabled = false;

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Pivot:", GUILayout.Width(50));
                    EditorGUILayout.Vector2Field("",spriteAsset.listSpriteGroup[i].listSpriteInfor[j].pivot);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Rect:", GUILayout.Width(50));
                    EditorGUILayout.RectField("", spriteAsset.listSpriteGroup[i].listSpriteInfor[j].rect);
                    GUILayout.EndHorizontal();

                    for (int m= 0; m < spriteAsset.listSpriteGroup[i].listSpriteInfor[j].uv.Length; m++)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("UV"+m+":", GUILayout.Width(50));
                        EditorGUILayout.Vector2Field("", spriteAsset.listSpriteGroup[i].listSpriteInfor[j].uv[m]);
                        GUILayout.EndHorizontal();
                    }
                   
                    GUI.enabled = true;

                    GUILayout.EndVertical();

                    GUILayout.EndHorizontal();
                }
            }
            #endregion 
        }

        GUILayout.EndScrollView();
        //unity
        EditorUtility.SetDirty(spriteAsset);
    }

    private void Init()
    {
        tags = new List<string>();
        playIndexs = new List<int>();
        for (int i = 0; i < spriteAsset.listSpriteGroup.Count; i++)
        {
            tags.Add(spriteAsset.listSpriteGroup[i].tag);
            playIndexs.Add(0);
        }
        playIndex = 0;
        showIndex = -1;
        addTag = false;
        addTagName = "";
    }

    /// <summary>
    /// 改变sprite隶属的组
    /// </summary>
    private void ChangeTag(string newTag, SpriteInfor si)
    {
        if (newTag == si.tag)
            return;

        //从旧的组中移除
        SpriteInforGroup oldSpriteInforGroup = spriteAsset.listSpriteGroup.Find(
            delegate (SpriteInforGroup sig)
            {
                return sig.tag == si.tag;
            });
        if (oldSpriteInforGroup != null && oldSpriteInforGroup.listSpriteInfor.Contains(si))
        {
            oldSpriteInforGroup.listSpriteInfor.Remove(si);
        }

        //如果旧的组为空，则删掉旧的组
        if (oldSpriteInforGroup.listSpriteInfor.Count <= 0)
        {
            spriteAsset.listSpriteGroup.Remove(oldSpriteInforGroup);
            Init();
        }

        si.tag = newTag;
        //添加到新的组
        SpriteInforGroup newSpriteInforGroup = spriteAsset.listSpriteGroup.Find(
            delegate (SpriteInforGroup sig)
            {
                return sig.tag == newTag;
            });
        if (newSpriteInforGroup != null)
        {
            newSpriteInforGroup.listSpriteInfor.Add(si);
            newSpriteInforGroup.listSpriteInfor.Sort((a, b) => a.ID.CompareTo(b.ID));
        }

        EditorUtility.SetDirty(spriteAsset);
    }

    /// <summary>
    /// 刷新序列帧
    /// </summary>
    private void RefreshFrameAnimation()
    {
        if (playIndex < 1)
        {
            playIndex += Time.deltaTime * 0.1f * playSpeed;
        }
        if (playIndex >= 1)
        {
            playIndex = 0;
            for (int i = 0; i < playIndexs.Count; i++)
            {
                playIndexs[i] += 1;
                if (playIndexs[i] >= spriteAsset.listSpriteGroup[i].listSpriteInfor.Count)
                    playIndexs[i] = 0;
            }
            Repaint();
        }
    }

    /// <summary>
    /// 新增标签
    /// </summary>
    private void AddTagSure()
    {
        SpriteInforGroup sig = new SpriteInforGroup();
        sig.tag = addTagName;
        sig.listSpriteInfor = new List<SpriteInfor>();

        spriteAsset.listSpriteGroup.Insert(0, sig);

        Init();

        EditorUtility.SetDirty(spriteAsset);
    }

    /// <summary>
    /// 清理空的标签
    /// </summary>
    private void ClearTag()
    {
        for (int i = 0; i < spriteAsset.listSpriteGroup.Count; i++)
        {
            if (spriteAsset.listSpriteGroup[i].listSpriteInfor.Count <= 0)
            {
                spriteAsset.listSpriteGroup.RemoveAt(i);
                i -= 1;
            }
        }

        Init();
    }
}