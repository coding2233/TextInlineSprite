using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(SpriteAsset))]
public class SpriteAssetEditor : Editor
{
    private SpriteAsset _spriteAsset;
    private Vector2 _ve2ScorllView;
    //当前的所有标签
    private List<string> _tags;
    //当前的所有标签序列动画索引
    private List<int> _playIndexs;
    private float _playIndex;
    //当前展开的标签索引
    private int _showIndex;
    //序列帧播放速度
    private float _playSpeed;
    //添加标签
    private bool _addTag;
    //添加的标签的名称
    private string _addTagName;

    public void OnEnable()
    {
        _spriteAsset = (SpriteAsset)target;

        _playSpeed = 6;

        Init();

   //     EditorApplication.update += RefreshFrameAnimation;
    }

    public void OnDisable()
    {
    //    EditorApplication.update -= RefreshFrameAnimation;
    }
    
    public override void OnInspectorGUI()
    {

        _ve2ScorllView = GUILayout.BeginScrollView(_ve2ScorllView);

        #region 标题栏
        EditorGUILayout.HelpBox("Number Of Tags:" + _spriteAsset.ListSpriteGroup.Count + "     Number Of Group:" + _spriteAsset.ListSpriteGroup.Count, MessageType.Info);

        GUILayout.BeginVertical("HelpBox");
        GUILayout.BeginHorizontal();
        _spriteAsset.Id = EditorGUILayout.IntField("ID:", _spriteAsset.Id);
      //  _playSpeed = EditorGUILayout.FloatField("FrameSpeed", _playSpeed);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        _spriteAsset.IsStatic = EditorGUILayout.Toggle("Static:", _spriteAsset.IsStatic);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Tag"))
        {
            _addTag = !_addTag;
        }
        GUILayout.EndHorizontal();
        if (_addTag)
        {
            GUILayout.BeginHorizontal();
            _addTagName = EditorGUILayout.TextField(_addTagName);
            if (GUILayout.Button("sure", "minibutton"))
            {
                if (_addTagName == "")
                {
                    Debug.Log("请输入新建标签的名称！");
                }
                else
                {
                    SpriteInforGroup spriteInforGroup = _spriteAsset.ListSpriteGroup.Find(
                        delegate (SpriteInforGroup sig)
                        {
                            return sig.Tag == _addTagName;
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

        for (int i = 0; i < _spriteAsset.ListSpriteGroup.Count; i++)
        {
            GUILayout.BeginHorizontal("HelpBox");
            #region 展开与收缩按钮
            if (GUILayout.Button(_spriteAsset.ListSpriteGroup[i].Tag, _showIndex == i ? "OL Minus" : "OL Plus"))
            {
                if (_showIndex == i)
                {
                    _showIndex = -1;
                }
                else
                {
                    _showIndex = i;
                }
            }
            #endregion

            GUILayout.BeginHorizontal();
            GUILayout.Label("Size:", GUILayout.Width(40));
            _spriteAsset.ListSpriteGroup[i].Size=EditorGUILayout.FloatField("", _spriteAsset.ListSpriteGroup[i].Size, GUILayout.Width(40));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Width:", GUILayout.Width(40));
            _spriteAsset.ListSpriteGroup[i].Width=EditorGUILayout.FloatField("", _spriteAsset.ListSpriteGroup[i].Width, GUILayout.Width(40));
            GUILayout.EndHorizontal();

            #region 未展开的sprite组，播放序列帧动画（帧数大于1的序列帧动画才播放）
            if (_showIndex != i && _spriteAsset.ListSpriteGroup[i].ListSpriteInfor.Count > 0)
            {
                if (_playIndexs[i] >= _spriteAsset.ListSpriteGroup[i].ListSpriteInfor.Count)
                    _playIndexs[i] = 0;

                GUI.enabled = false;
                EditorGUILayout.ObjectField("", _spriteAsset.ListSpriteGroup[i].ListSpriteInfor[_playIndexs[i]].Sprite, typeof(Sprite), false);
                GUI.enabled = true;
            }
            #endregion
            GUILayout.EndHorizontal();

            #region 展开的sprite组，显示所有sprite属性
            if (_showIndex == i)
            {
                for (int j = 0; j < _spriteAsset.ListSpriteGroup[i].ListSpriteInfor.Count; j++)
                {
                    GUILayout.BeginHorizontal("sprite" + j, "window");
                    _spriteAsset.ListSpriteGroup[i].ListSpriteInfor[j].Sprite = EditorGUILayout.ObjectField("", _spriteAsset.ListSpriteGroup[i].ListSpriteInfor[j].Sprite, typeof(Sprite), false, GUILayout.Width(80)) as Sprite;

                    GUILayout.FlexibleSpace();

                    GUILayout.BeginVertical();

                    GUI.enabled = false;

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("ID:", GUILayout.Width(50));
                    _spriteAsset.ListSpriteGroup[i].ListSpriteInfor[j].Id = EditorGUILayout.IntField(_spriteAsset.ListSpriteGroup[i].ListSpriteInfor[j].Id);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Name:", GUILayout.Width(50));
                    _spriteAsset.ListSpriteGroup[i].ListSpriteInfor[j].Name = EditorGUILayout.TextField(_spriteAsset.ListSpriteGroup[i].ListSpriteInfor[j].Name);
                    GUILayout.EndHorizontal();

                    GUI.enabled = true;

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Tag:", GUILayout.Width(50));
                    if (GUILayout.Button(_spriteAsset.ListSpriteGroup[i].ListSpriteInfor[j].Tag, "MiniPopup"))
                    {
                        GenericMenu gm = new GenericMenu();
                        for (int n = 0; n < _tags.Count; n++)
                        {
                            int i2 = i;
                            int j2 = j;
                            int n2 = n;
                            gm.AddItem(new GUIContent(_tags[n2]), false, 
                                delegate () 
                                {
                                    ChangeTag(_tags[n2], _spriteAsset.ListSpriteGroup[i2].ListSpriteInfor[j2]);
                                });
                        }
                        gm.ShowAsContext();
                    }
                    GUILayout.EndHorizontal();
                    
                    GUI.enabled = false;

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Pivot:", GUILayout.Width(50));
                    EditorGUILayout.Vector2Field("",_spriteAsset.ListSpriteGroup[i].ListSpriteInfor[j].Pivot);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Rect:", GUILayout.Width(50));
                    EditorGUILayout.RectField("", _spriteAsset.ListSpriteGroup[i].ListSpriteInfor[j].Rect);
                    GUILayout.EndHorizontal();

                    for (int m= 0; m < _spriteAsset.ListSpriteGroup[i].ListSpriteInfor[j].Uv.Length; m++)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("UV"+m+":", GUILayout.Width(50));
                        EditorGUILayout.Vector2Field("", _spriteAsset.ListSpriteGroup[i].ListSpriteInfor[j].Uv[m]);
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
        EditorUtility.SetDirty(_spriteAsset);
    }

    private void Init()
    {
        _tags = new List<string>();
        _playIndexs = new List<int>();
        for (int i = 0; i < _spriteAsset.ListSpriteGroup.Count; i++)
        {
            _tags.Add(_spriteAsset.ListSpriteGroup[i].Tag);
            _playIndexs.Add(0);
        }
        _playIndex = 0;
        _showIndex = -1;
        _addTag = false;
        _addTagName = "";
    }

    /// <summary>
    /// 改变sprite隶属的组
    /// </summary>
    private void ChangeTag(string newTag, SpriteInfor si)
    {
        if (newTag == si.Tag)
            return;

        //从旧的组中移除
        SpriteInforGroup oldSpriteInforGroup = _spriteAsset.ListSpriteGroup.Find(
            delegate (SpriteInforGroup sig)
            {
                return sig.Tag == si.Tag;
            });
        if (oldSpriteInforGroup != null && oldSpriteInforGroup.ListSpriteInfor.Contains(si))
        {
            oldSpriteInforGroup.ListSpriteInfor.Remove(si);
        }

        //如果旧的组为空，则删掉旧的组
        if (oldSpriteInforGroup.ListSpriteInfor.Count <= 0)
        {
            _spriteAsset.ListSpriteGroup.Remove(oldSpriteInforGroup);
            Init();
        }

        si.Tag = newTag;
        //添加到新的组
        SpriteInforGroup newSpriteInforGroup = _spriteAsset.ListSpriteGroup.Find(
            delegate (SpriteInforGroup sig)
            {
                return sig.Tag == newTag;
            });
        if (newSpriteInforGroup != null)
        {
            newSpriteInforGroup.ListSpriteInfor.Add(si);
            newSpriteInforGroup.ListSpriteInfor.Sort((a, b) => a.Id.CompareTo(b.Id));
        }

        EditorUtility.SetDirty(_spriteAsset);
    }

    /// <summary>
    /// 刷新序列帧
    /// </summary>
    private void RefreshFrameAnimation()
    {
        if (_playIndex < 1)
        {
            _playIndex += Time.deltaTime * 0.1f * _playSpeed;
        }
        if (_playIndex >= 1)
        {
            _playIndex = 0;
            for (int i = 0; i < _playIndexs.Count; i++)
            {
                _playIndexs[i] += 1;
                if (_playIndexs[i] >= _spriteAsset.ListSpriteGroup[i].ListSpriteInfor.Count)
                    _playIndexs[i] = 0;
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
        sig.Tag = _addTagName;
        sig.ListSpriteInfor = new List<SpriteInfor>();

        _spriteAsset.ListSpriteGroup.Insert(0, sig);

        Init();

        EditorUtility.SetDirty(_spriteAsset);
    }

    /// <summary>
    /// 清理空的标签
    /// </summary>
    private void ClearTag()
    {
        for (int i = 0; i < _spriteAsset.ListSpriteGroup.Count; i++)
        {
            if (_spriteAsset.ListSpriteGroup[i].ListSpriteInfor.Count <= 0)
            {
                _spriteAsset.ListSpriteGroup.RemoveAt(i);
                i -= 1;
            }
        }

        Init();
    }
}