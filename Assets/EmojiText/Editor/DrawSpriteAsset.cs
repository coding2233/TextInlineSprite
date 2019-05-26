using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EmojiText.Taurus
{
    public class DrawSpriteAsset 
    {
        private string _assetPath = "配置文件暂未保存";
        private Vector2 _spritesScrollView = Vector2.zero;
        private int _showIndex = -1;
        private SpriteAsset _spriteAsset;
        
        public DrawSpriteAsset(SpriteAsset spriteAsset)
        {
            _spriteAsset = spriteAsset;
        }

        /// <summary>
        /// 设置信息
        /// </summary>
        /// <param name="spriteAsset"></param>
        public void SetSpriteAsset(SpriteAsset spriteAsset)
        {
            _spriteAsset = spriteAsset;
        }

        /// <summary>
        /// 绘制
        /// </summary>
        public void Draw()
        {
            if (_spriteAsset)
            {
                EditorGUILayout.HelpBox("强烈建议将资源图片上的所有表情，制成规格完全一样的！有规律才支持程序自动分割处理！如果需要实在特殊，请更行更改代码，或者通过github(https://github.com/coding2233/TextInlineSprite)与我沟通。", MessageType.Info);
                //属性
                GUILayout.Label("属性:");
                GUILayout.BeginVertical("HelpBox");
                //id
                GUILayout.BeginHorizontal();
                GUILayout.Label("Id", GUILayout.Width(80));
                _spriteAsset.Id = EditorGUILayout.IntField(_spriteAsset.Id);
                GUILayout.EndHorizontal();
                //是否为静态表情
                GUILayout.BeginHorizontal();
                bool isStatic = GUILayout.Toggle(_spriteAsset.IsStatic, "是否为静态表情?");
                if (isStatic != _spriteAsset.IsStatic)
                {
                    if (EditorUtility.DisplayDialog("提示", "切换表情类型，会导致重新命名Tag,请确认操作", "确认", "取消"))
                    {
                        _spriteAsset.IsStatic = isStatic;
                    }
                }
                GUILayout.FlexibleSpace();
                //动画的速度
                if (!_spriteAsset.IsStatic)
                {
                    GUILayout.Label("动画速度", GUILayout.Width(80));
                    _spriteAsset.Speed = EditorGUILayout.FloatField(_spriteAsset.Speed);
                }
                GUILayout.EndHorizontal();
                //行列速度
                GUILayout.BeginHorizontal();
                GUILayout.Label("Row", GUILayout.Width(80));
                _spriteAsset.Row = EditorGUILayout.IntField(_spriteAsset.Row);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Column", GUILayout.Width(80));
                _spriteAsset.Column = EditorGUILayout.IntField(_spriteAsset.Column);
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();

                //具体的精灵信息
                if (_spriteAsset && _spriteAsset.ListSpriteGroup.Count > 0)
                {
                    List<SpriteInforGroup> inforGroups = _spriteAsset.ListSpriteGroup;
                    GUILayout.Label("精灵信息:");
                    _spritesScrollView = GUILayout.BeginScrollView(_spritesScrollView, "HelpBox");
                    for (int i = 0; i < inforGroups.Count; i++)
                    {
                        GUILayout.BeginVertical("HelpBox");
                        //标题信息..........
                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button(i.ToString(), _showIndex == i ? "OL Minus" : "OL Plus", GUILayout.Width(40), GUILayout.Height(40)))
                        {
                            if (_showIndex == i)
                                _showIndex = -1;
                            else
                                _showIndex = i;

                            //_showSprites.Clear();
                        }
                        //表情预览
                        GUILayout.Label("", GUILayout.Width(40), GUILayout.Height(40));
                        if (inforGroups[i].ListSpriteInfor.Count > 0)
                        {
                            Rect lastRect = GUILayoutUtility.GetLastRect();
                            //渲染精灵图片
                            GUI.DrawTextureWithTexCoords(lastRect, _spriteAsset.TexSource, inforGroups[i].ListSpriteInfor[0].DrawTexCoord);
                        }
                        GUILayout.Label("Tag:");
                        inforGroups[i].Tag = EditorGUILayout.TextField(inforGroups[i].Tag);
                        GUILayout.Label("Size:");
                        inforGroups[i].Size = EditorGUILayout.FloatField(inforGroups[i].Size);
                        GUILayout.Label("Width:");
                        inforGroups[i].Width = EditorGUILayout.FloatField(inforGroups[i].Width);
                        GUILayout.EndHorizontal();
                        //具体信息
                        if (_showIndex == i)
                        {
                            List<SpriteInfor> spriteInfors = inforGroups[i].ListSpriteInfor;
                            for (int m = 0; m < spriteInfors.Count; m++)
                            {
                                GUILayout.BeginHorizontal("HelpBox");
                                //渲染精灵图片
                                GUILayout.Label("", GUILayout.Width(80), GUILayout.Height(80));

                                GUI.DrawTextureWithTexCoords(GUILayoutUtility.GetLastRect(), _spriteAsset.TexSource, spriteInfors[m].DrawTexCoord);

                                //间隔
                                GUILayout.Space(50);

                                //渲染其他信息
                                GUI.enabled = false;
                                GUILayout.BeginVertical();
                                //id
                                GUILayout.BeginHorizontal();
                                GUILayout.Label("Id:", GUILayout.Width(50));
                                GUILayout.Label(spriteInfors[m].Id.ToString());
                                GUILayout.EndHorizontal();
                                //Rect
                                GUILayout.BeginHorizontal();
                                GUILayout.Label("Rect:", GUILayout.Width(50));
                                EditorGUILayout.RectField(spriteInfors[m].Rect);
                                GUILayout.EndHorizontal();
                                //uvs
                                for (int u = 0; u < spriteInfors[m].Uv.Length; u++)
                                {
                                    GUILayout.BeginHorizontal();
                                    GUILayout.Label("UV" + u + ":", GUILayout.Width(50));
                                    EditorGUILayout.Vector2Field("", spriteInfors[m].Uv[u]);
                                    GUILayout.EndHorizontal();
                                }

                                GUILayout.EndVertical();
                                GUI.enabled = true;
                                
                                GUILayout.EndHorizontal();
                            }

                        }
                        GUILayout.EndVertical();

                    }
                    GUILayout.EndScrollView();
                }
                
            }
        }


        /// <summary>
        /// 更新信息
        /// </summary>
        public void UpdateSpriteGroup()
        {
            if (_spriteAsset && _spriteAsset.TexSource && _spriteAsset.Row > 1 && _spriteAsset.Column > 1)
            {
                int count = _spriteAsset.IsStatic ? _spriteAsset.Row * _spriteAsset.Column : _spriteAsset.Row;
                if (_spriteAsset.ListSpriteGroup.Count != count)
                {
                    _spriteAsset.ListSpriteGroup.Clear();
                    //更新
                    //----------------------------------
                    Vector2 texSize = new Vector2(_spriteAsset.TexSource.width, _spriteAsset.TexSource.height);
                    Vector2 size = new Vector2((_spriteAsset.TexSource.width / (float)_spriteAsset.Column)
                        , (_spriteAsset.TexSource.height / (float)_spriteAsset.Row));

                    if (_spriteAsset.IsStatic)
                    {
                        int index = -1;
                        for (int i = 0; i < _spriteAsset.Row; i++)
                        {
                            for (int j = 0; j < _spriteAsset.Column; j++)
                            {
                                index++;
                                SpriteInforGroup inforGroup = Pool<SpriteInforGroup>.Get();
                                SpriteInfor infor = GetSpriteInfo(index, i, j, size, texSize);

                                inforGroup.Tag = "emoji_"+infor.Id;
                                inforGroup.ListSpriteInfor.Add(infor);
                                _spriteAsset.ListSpriteGroup.Add(inforGroup);
                            }
                        }
                    }
                    else
                    {
                        int index = -1;
                        for (int i = 0; i < _spriteAsset.Row; i++)
                        {
                            SpriteInforGroup inforGroup = Pool<SpriteInforGroup>.Get();
                            inforGroup.Tag = "emoji_"+(index + 1);
                            for (int j = 0; j < _spriteAsset.Column; j++)
                            {
                                index++;

                                SpriteInfor infor = GetSpriteInfo(index, i, j, size, texSize);

                                inforGroup.ListSpriteInfor.Add(infor);
                            }
                            _spriteAsset.ListSpriteGroup.Add(inforGroup);
                        }
                    }

                }
            }
        }


        #region 内部函数
        //获取精灵信息
        private SpriteInfor GetSpriteInfo(int index, int row, int column, Vector2 size, Vector2 texSize)
        {
            SpriteInfor infor = Pool<SpriteInfor>.Get();
            infor.Id = index;
            infor.Rect = new Rect(size.y * column, texSize.y - (row + 1) * size.x, size.x, size.y);
            infor.DrawTexCoord = new Rect(infor.Rect.x / texSize.x, infor.Rect.y / texSize.y
                , infor.Rect.width / texSize.x, infor.Rect.height / texSize.y);
            infor.Uv = GetSpriteUV(texSize, infor.Rect);
            return infor;
        }

        //获取uv信息
        private static Vector2[] GetSpriteUV(Vector2 texSize, Rect _sprRect)
        {
            Vector2[] uv = new Vector2[4];
            uv[0] = new Vector2(_sprRect.x / texSize.x, (_sprRect.y + _sprRect.height) / texSize.y);
            uv[1] = new Vector2((_sprRect.x + _sprRect.width) / texSize.x, (_sprRect.y + _sprRect.height) / texSize.y);
            uv[2] = new Vector2((_sprRect.x + _sprRect.width) / texSize.x, _sprRect.y / texSize.y);
            uv[3] = new Vector2(_sprRect.x / texSize.x, _sprRect.y / texSize.y);
            return uv;
        }

        #endregion
    }

}