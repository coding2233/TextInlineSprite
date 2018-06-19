using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace EmojiUI
{
    [CustomEditor(typeof(SpriteAsset))]
    public class SpriteAssetEditor : Editor
    {
        private SpriteAsset spriteAsset;
        private Vector2 ve2ScorllView;
        private Dictionary<string, SpriteInfoGroup> selections = new Dictionary<string, SpriteInfoGroup>();
        private Dictionary<SpriteInfoGroup, float> animations = new Dictionary<SpriteInfoGroup, float>();
        private Dictionary<SpriteInfoGroup, int> animationframes = new Dictionary<SpriteInfoGroup, int>();

        //当前展开的标签索引
        private System.Object showdata;
        //序列帧播放速度
        private int playSpeed;

        //添加的标签的名称
        private string addTagName;

        private string aniTagName;
        private Vector2 batchsize;
        private bool isPackingAnimation;
        private float fw;
        private float lw;

        void OnEnable()
        {
            spriteAsset = (SpriteAsset)target;

            playSpeed = 6;

            Reset();

            EditorApplication.update += UpdateFrame;
        }

        void OnDisable()
        {
            EditorApplication.update -= UpdateFrame;
            EditorUtility.SetDirty(target);
        }

        void UpdateFrame()
        {
            if (animations.Count > 0)
            {
                float current = (float)EditorApplication.timeSinceStartup;
                var en = animations.GetEnumerator();
                while (en.MoveNext())
                {
                    SpriteInfoGroup group = en.Current.Key;
                    float start = en.Current.Value;
                    int shouldframe = Mathf.CeilToInt((current - start) * playSpeed);
                    int maxframe = group.spritegroups.Count;

                    int currentframe = shouldframe % maxframe;
                    animationframes[group] = currentframe;
                }

                this.Repaint();
            }
        }

        private void StartFix(float v1, float v2)
        {
            fw = EditorGUIUtility.fieldWidth;
            lw = EditorGUIUtility.labelWidth;
            EditorGUIUtility.fieldWidth = v1;
            EditorGUIUtility.labelWidth = v2;
        }

        private void EndFix()
        {
            EditorGUIUtility.fieldWidth = fw;
            EditorGUIUtility.labelWidth = lw;
        }

        public override void OnInspectorGUI()
        {

            ve2ScorllView = GUILayout.BeginScrollView(ve2ScorllView);

            ShowBatchTools();

            if (isPackingAnimation)
            {
                ShowAnimations();
                ShowWillPackCells();
            }
            else
            {
                StartFix(50, 50);

                ShowAnimations();
                ShowCells();

                EndFix();
            }


            GUILayout.EndScrollView();
        }

        private void ShowBatchTools()
        {

            GUILayout.BeginVertical("HelpBox");

            spriteAsset.AssetName =EditorGUILayout.TextField("AssetName:" ,spriteAsset.AssetName);

            spriteAsset.ID = EditorGUILayout.IntField("ID:", spriteAsset.ID);

            GUILayout.BeginHorizontal();

            addTagName = EditorGUILayout.TextField("add Tag", addTagName);
            if (GUILayout.Button("Sure"))
            {
                ClickSurebtn();
            }

            GUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            this.batchsize = EditorGUILayout.Vector2Field("batch size", this.batchsize);

            if (GUILayout.Button("Sure"))
            {
                for (int i = 0; i < spriteAsset.listSpriteGroup.Count; i++)
                {
                    SpriteInfoGroup group = spriteAsset.listSpriteGroup[i];
                    group.size = this.batchsize.x;
                    group.width = this.batchsize.y;
                }
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("RestSize", GUILayout.Width(100)))
            {
                for (int i = 0; i < spriteAsset.listSpriteGroup.Count; i++)
                {
                    SpriteInfoGroup group = spriteAsset.listSpriteGroup[i];

                    for (int j = 0; j < group.spritegroups.Count; ++j)
                    {
                        SpriteInfo info = group.spritegroups[j];
                        float value = Mathf.Max(info.sprite.rect.width, info.sprite.rect.height);
                        group.size = Mathf.Max(value, group.size);
                        group.width = 1;
                    }
                }
            }

            if (GUILayout.Button("Clear Tag", GUILayout.Width(100)))
            {
                ClearTag();
            }

            if (GUILayout.Button("batch pack Tags", GUILayout.Width(120)))
            {
                isPackingAnimation = !isPackingAnimation;
                this.animations.Clear();
                this.animationframes.Clear();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

        }

        private void ShowWillPackCells()
        {
            EditorGUILayout.BeginHorizontal("HelpBox");

            GUILayout.Label("Tag:", GUILayout.Width(50));
            if (GUILayout.Button(aniTagName, "MiniPopup", GUILayout.Width(100)))
            {
                GenericMenu gm = new GenericMenu();

                for (int i = 0; i < spriteAsset.listSpriteGroup.Count; i++)
                {
                    SpriteInfoGroup group = spriteAsset.listSpriteGroup[i];
                    gm.AddItem(new GUIContent(group.tag), false, () =>
                    {
                        aniTagName = group.tag;
                    });
                }
                gm.ShowAsContext();
            }

            if (GUILayout.Button("Select All", GUILayout.Width(100)))
            {
                for (int i = 0; i < spriteAsset.listSpriteGroup.Count; i++)
                {
                    SpriteInfoGroup group = spriteAsset.listSpriteGroup[i];
                    selections[group.tag] = group;
                }
            }

            if (GUILayout.Button("DisSelect All", GUILayout.Width(100)))
            {
                selections.Clear();
            }

            if (GUILayout.Button("Pack into Animation", GUILayout.Width(150)))
            {
                if (!string.IsNullOrEmpty(aniTagName))
                {

                    List<SpriteInfoGroup> removelist = new List<SpriteInfoGroup>();

                    SpriteInfoGroup firstGroup = null;
                    var en = selections.GetEnumerator();
                    while (en.MoveNext())
                    {
                        SpriteInfoGroup group = en.Current.Value;
                        if (firstGroup == null)
                        {
                            firstGroup = group;
                            group.tag = aniTagName;
                        }
                        else
                        {
                            firstGroup.size = Mathf.Max(firstGroup.size, group.size);
                            firstGroup.spritegroups.AddRange(group.spritegroups);
                            removelist.Add(group);
                        }
                    }

                    for (int i = 0; i < removelist.Count; ++i)
                    {
                        SpriteInfoGroup group = removelist[i];
                        this.spriteAsset.listSpriteGroup.Remove(group);
                    }

                    selections.Clear();
                }

            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(2);

            int t = 0;
            for (int i = 0; i < spriteAsset.listSpriteGroup.Count; i++)
            {
                SpriteInfoGroup group = spriteAsset.listSpriteGroup[i];

                EditorGUILayout.BeginHorizontal("HelpBox");

                bool exist = selections.ContainsKey(group.tag);
                bool newselect = EditorGUILayout.Toggle(exist, GUILayout.Width(20));
                if (newselect != exist)
                {
                    if (newselect)
                    {
                        selections[group.tag] = group;
                    }
                    else
                    {
                        selections.Remove(group.tag);
                    }
                }

                GUILayout.Label(string.Format("[{0}] : {1}", i, group.tag), GUILayout.Width(100));

                AddFoldOutBtn("", group);
                EditorGUILayout.EndHorizontal();


                for (int j = 0; j < group.spritegroups.Count; j++)
                {
                    SpriteInfo info = group.spritegroups[j];

                    if (ReferenceEquals(showdata, group))
                    {
                        ShowCell(info);
                    }
                    ++t;
                }
            }

        }

        private void ShowAnimations()
        {

            playSpeed = EditorGUILayout.IntSlider("speed", playSpeed, 1, 60);

            GUILayout.Space(5);

            EditorGUILayout.BeginVertical("window");

            int column = 3;
            int m = 0;
            int fixhor = -1;
            for (int i = 0; i < spriteAsset.listSpriteGroup.Count; i++)
            {
                SpriteInfoGroup group = spriteAsset.listSpriteGroup[i];
                if (group.spritegroups.Count > 1)
                {
                    int index;
                    if (!animationframes.TryGetValue(group, out index))
                    {
                        animationframes[group] = 0;
                        index = 0;
                    }

                    SpriteInfo info = group.spritegroups[index];

                    int c = m % column;
                    if (c == 0)
                    {
                        EditorGUILayout.BeginHorizontal();
                        fixhor = 1;

                        ShowAnimationCell(group, info);
                    }
                    else if (c == column - 1)
                    {
                        ShowAnimationCell(group, info);
                        fixhor = 0;
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        ShowAnimationCell(group, info);
                    }

                    m++;
                }
            }

            if (fixhor > 0)
            {
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();

            GUILayout.Space(10);

        }

        private void ShowAnimationCell(SpriteInfoGroup group, SpriteInfo info)
        {
            EditorGUILayout.ObjectField("", info.sprite, typeof(Sprite), false, GUILayout.Width(80));

            GUILayout.BeginVertical();

            GUI.enabled = false;
            GUILayout.Label("Name:" + group.tag, GUILayout.Width(200));
            GUI.enabled = true;

            if (GUILayout.Button("Play", GUILayout.Width(80)))
            {
                animations[group] = (float)EditorApplication.timeSinceStartup;
            }

            if (GUILayout.Button("Stop", GUILayout.Width(80)))
            {
                animations.Remove(group);
            }
            GUILayout.EndVertical();
        }

        private void AddFoldOutBtn(string btnname, System.Object currentdata)
        {
            #region 展开与收缩按钮
            bool foldout = ReferenceEquals(showdata, currentdata);
            if (GUILayout.Button(btnname, foldout ? "OL Minus" : "OL Plus", GUILayout.Width(100)))
            {
                if (foldout)
                {
                    showdata = null;
                }
                else
                {
                    showdata = currentdata;
                }
            }
            #endregion
        }

        private void ShowCells()
        {
            for (int i = 0; i < spriteAsset.listSpriteGroup.Count; i++)
            {
                SpriteInfoGroup group = spriteAsset.listSpriteGroup[i];

                GUILayout.BeginHorizontal("HelpBox");

                this.AddFoldOutBtn(group.tag, group);

                group.size = EditorGUILayout.FloatField("size:", group.size);
                group.width = EditorGUILayout.FloatField("wid:", group.width);
                group.x = EditorGUILayout.FloatField("x:", group.x);
                group.y = EditorGUILayout.FloatField("y:", group.y);

                GUILayout.EndHorizontal();

                #region 展开的sprite组，显示所有sprite属性
                if (showdata == group)
                {
                    for (int j = 0; j < group.spritegroups.Count; j++)
                    {
                        SpriteInfo info = group.spritegroups[j];
                        ShowCell(info);
                    }
                }
                #endregion
            }


        }

        private void ShowCell(SpriteInfo info)
        {
            GUILayout.BeginVertical(info.sprite.name, "window");

            EditorGUILayout.ObjectField("", info.sprite, typeof(Sprite), false, GUILayout.Width(80));

            GUILayout.Space(1);

            GUI.enabled = false;

            EditorGUILayout.TextField("Name:", info.sprite.name);
            EditorGUILayout.Vector2Field("Pivot:", info.sprite.pivot);
            EditorGUILayout.RectField("Rect:", info.sprite.rect);

            for (int m = 0; m < info.uv.Length; m++)
            {
                EditorGUILayout.Vector2Field("UV" + m + ":", info.uv[m]);

            }

            GUI.enabled = true;

            GUILayout.EndVertical();
        }

        private void Reset()
        {
            selections.Clear();
            animations.Clear();
            animationframes.Clear();

            showdata = null;
            isPackingAnimation = false;
            addTagName = "";
            aniTagName = "";

            batchsize = new Vector2(25, 1);
        }

        private void ClickSurebtn()
        {
            if (string.IsNullOrEmpty(addTagName))
            {
                Debug.Log("请输入新建标签的名称！");
            }
            else
            {
                SpriteInfoGroup spriteInforGroup = spriteAsset.listSpriteGroup.Find(sig => sig.tag == addTagName);

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

        /// <summary>
        /// 改变sprite隶属的组
        /// </summary>
        private void ChangeTag(string newTag, SpriteInfo si)
        {
            if (newTag == si.tag)
                return;

            //从旧的组中移除
            SpriteInfoGroup oldSpriteInforGroup = spriteAsset.listSpriteGroup.Find(sig => sig.tag == si.tag);

            if (oldSpriteInforGroup != null && oldSpriteInforGroup.spritegroups.Contains(si))
            {
                oldSpriteInforGroup.spritegroups.Remove(si);
            }

            //如果旧的组为空，则删掉旧的组
            if (oldSpriteInforGroup.spritegroups.Count <= 0)
            {
                spriteAsset.listSpriteGroup.Remove(oldSpriteInforGroup);
            }

            si.tag = newTag;
            //添加到新的组
            SpriteInfoGroup newSpriteInforGroup = spriteAsset.listSpriteGroup.Find(sig => sig.tag == newTag);
            if (newSpriteInforGroup != null)
            {
                newSpriteInforGroup.spritegroups.Add(si);
            }

            EditorUtility.SetDirty(spriteAsset);
        }

        /// <summary>
        /// 新增标签
        /// </summary>
        private void AddTagSure()
        {
            SpriteInfoGroup sig = new SpriteInfoGroup();
            sig.tag = addTagName;

            spriteAsset.listSpriteGroup.Add(sig);
            spriteAsset.listSpriteGroup.Sort((l, r) =>
            {
                return l.tag.CompareTo(r.tag);
            });

            Reset();

            EditorUtility.SetDirty(spriteAsset);
        }

        /// <summary>
        /// 清理空的标签
        /// </summary>
        private void ClearTag()
        {
            for (int i = 0; i < spriteAsset.listSpriteGroup.Count; i++)
            {
                if (spriteAsset.listSpriteGroup[i].spritegroups.Count <= 0)
                {
                    spriteAsset.listSpriteGroup.RemoveAt(i);
                    i -= 1;
                }
            }

            Reset();
        }
    }
}

