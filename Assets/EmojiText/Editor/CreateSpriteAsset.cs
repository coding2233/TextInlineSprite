using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace EmojiText.Taurus
{
	public class CreateSpriteAsset:EditorWindow
	{
        private static Texture2D _sourceTex;

        private Vector2 _texScrollView = Vector2.zero;
        private static string _assetPath = "";
        private static SpriteAsset _spriteAsset;
        private static DrawSpriteAsset _drawSpriteAsset;

        [MenuItem("Assets/Create/Sprite Asset", false, 10)]
		static void main()
		{
			Object target = Selection.activeObject;
			if (target == null || target.GetType() != typeof(Texture2D))
				return;

            _sourceTex = target as Texture2D;

            GetWindow<CreateSpriteAsset>("Asset Window");
        }

        /// <summary>
        /// 打开资源窗口
        /// </summary>
        /// <param name="spriteAsset"></param>
        public static void OpenAsset(SpriteAsset spriteAsset)
        {
            if (spriteAsset != null && spriteAsset.TexSource != null)
            {
                _spriteAsset = spriteAsset;
                _sourceTex = (Texture2D)_spriteAsset.TexSource;
                SetDrawSpriteAsset(_spriteAsset);
                _assetPath = AssetDatabase.GetAssetPath(_spriteAsset);
                GetWindow<CreateSpriteAsset>("Asset Window");
            }
        }

      

        private void OnGUI()
        {
            if (_sourceTex != null)
            {
                GUILayout.BeginHorizontal();
                //纹理渲染--------------
                _texScrollView = GUILayout.BeginScrollView(_texScrollView, "",GUILayout.Width(0.625f*Screen.width));
                GUILayout.Label(_sourceTex);
                GUILayout.EndScrollView();
                //参数设置---------------
                GUILayout.BeginVertical();
                GUILayout.BeginVertical("HelpBox");
                GUILayout.BeginHorizontal();
                GUILayout.Label("纹理名称",GUILayout.Width(80));
                GUILayout.Label(_sourceTex.name);
                GUILayout.FlexibleSpace();
                //加载 图片
                if (GUILayout.Button("Load"))
                {
                    string filePath = EditorUtility.OpenFilePanel("加载图集文件", "", "png");
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        //绝对路径->相对路径
                        filePath = "Assets" + filePath.Replace(Application.dataPath, "");
                        Texture2D tex2d = AssetDatabase.LoadAssetAtPath<Texture2D>(filePath);
                        if (tex2d != null)
                        {
                            _sourceTex = tex2d;
                            if (_spriteAsset)
                                _spriteAsset.TexSource = _sourceTex;
                        }
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("纹理分辨率", GUILayout.Width(80));
                GUILayout.Label(_sourceTex.width+" * "+_sourceTex.height);
                GUILayout.EndHorizontal();
                
                //保存
                GUILayout.BeginHorizontal();
                GUILayout.Label("配置文件路径", GUILayout.Width(80));
                GUILayout.Label(_assetPath);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(_spriteAsset==null?"Save":"Save As"))
                {
                    string filePath = EditorUtility.SaveFilePanelInProject("保存表情的序列化文件", _sourceTex.name, "asset", "保存表情的序列化文件");
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        _assetPath = filePath;
                        //创建序列化文件
                        _spriteAsset = ScriptableObject.CreateInstance<SpriteAsset>();
                        _spriteAsset.TexSource = _sourceTex;
                        _spriteAsset.ListSpriteGroup = new List<SpriteInforGroup>();
                       // spriteAsset.ListSpriteGroup = GetAssetSpriteInfor(sourceTex);
                        AssetDatabase.CreateAsset(_spriteAsset, _assetPath);

                        //设置精灵信息的绘制类
                        SetDrawSpriteAsset(_spriteAsset);
                    }
                    
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();

                GUILayout.Space(5);

                //绘制属性
                if(_drawSpriteAsset!=null)
                    _drawSpriteAsset.Draw();
              
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();

                //非自动布局绘制------------------
                //绘制线
                DrawTextureLines();
            }

            //更新信息
            if (_drawSpriteAsset != null)
                _drawSpriteAsset.UpdateSpriteGroup();

            //保存序列化文件
            if (_spriteAsset)
                EditorUtility.SetDirty(_spriteAsset);

        }
        
	
        //绘制纹理上的线
        private void DrawTextureLines()
        {
            if (_sourceTex && _spriteAsset)
            {
                Handles.BeginGUI();

                //行 - line 
                if (_spriteAsset.Row > 0)
                {
                    Handles.color = _spriteAsset.IsStatic ? Color.green : Color.red;
                    float interval = _sourceTex.height / _spriteAsset.Row;
                    float remain = _texScrollView.y % interval;
                    int max = (int)(Screen.height / interval);

                    for (int i = 0; i < max; i++)
                    {
                        float h = (interval * i) + (interval - remain);
                        float endx = 0.625f * Screen.width - 15.0f;
                        endx = endx > _sourceTex.width ? _sourceTex.width : endx;
                        Handles.DrawLine(new Vector3(5, h), new Vector3(endx, h));
                    }
                }
                //列 - line
                if (_spriteAsset.Column > 0)
                {
                    Handles.color = Color.green;
                    float interval = _sourceTex.width / _spriteAsset.Column;
                    float remain = _texScrollView.x % interval;
                    float scrollViewWidth = 0.625f * Screen.width;
                    scrollViewWidth = scrollViewWidth > _sourceTex.width ? _sourceTex.width : scrollViewWidth;
                    int max = (int)(scrollViewWidth / interval);

                    for (int i = 0; i < max; i++)
                    {
                        float w = (interval * i) + (interval - remain);
                        float endy = Screen.height > _sourceTex.height ? _sourceTex.height : (Screen.height);
                        Handles.DrawLine(new Vector3(w, 5), new Vector3(w, endy));
                    }
                }

                Handles.EndGUI();
            }
        }


        //绘制信息的类
        private static void SetDrawSpriteAsset(SpriteAsset spriteAsset)
        {
            //添加
            if (_drawSpriteAsset == null)
                _drawSpriteAsset = new DrawSpriteAsset(spriteAsset);
            else
                _drawSpriteAsset.SetSpriteAsset(spriteAsset);
        }
    }
}