using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace EmojiText.Taurus
{
	public class CreateSpriteAsset:EditorWindow
	{
        static Texture2D _sourceTex;

		[MenuItem("Assets/Create/Sprite Asset", false, 10)]
		static void main()
		{
            GetWindow<CreateSpriteAsset>("Asset Window");
           // return;

			Object target = Selection.activeObject;
			if (target == null || target.GetType() != typeof(Texture2D))
				return;

			Texture2D sourceTex = target as Texture2D;
            _sourceTex = sourceTex;
            return;
            //整体路径
            string filePathWithName = AssetDatabase.GetAssetPath(sourceTex);
			//带后缀的文件名
			string fileNameWithExtension = Path.GetFileName(filePathWithName);
			//不带后缀的文件名
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePathWithName);
			//不带文件名的路径
			string filePath = filePathWithName.Replace(fileNameWithExtension, "");

			SpriteAsset spriteAsset = AssetDatabase.LoadAssetAtPath(filePath + fileNameWithoutExtension + ".asset", typeof(SpriteAsset)) as SpriteAsset;
			bool isNewAsset = spriteAsset == null ? true : false;
			if (isNewAsset)
			{
				spriteAsset = ScriptableObject.CreateInstance<SpriteAsset>();
				spriteAsset.TexSource = sourceTex;
				spriteAsset.ListSpriteGroup = GetAssetSpriteInfor(sourceTex);
				AssetDatabase.CreateAsset(spriteAsset, filePath + fileNameWithoutExtension + ".asset");
			}
		}

        private Vector2 _texScrollView = Vector2.zero;
        private string _assetPath = "配置文件暂未保存";
        private Vector2 _spritesScrollView = Vector2.zero;
        private SpriteAsset _spriteAsset;
        private int _showIndex = -1;
        private int _row=0;
        private int _column = 0;
      

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
                GUILayout.BeginVertical("HelpBox");
                GUILayout.Label(_sourceTex.name);
                GUILayout.Label(_sourceTex.width+"*"+_sourceTex.height);
                //保存
                GUILayout.BeginHorizontal();
                GUILayout.Label(_assetPath);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Save"))
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
                    }
                    
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(5);
                if (_spriteAsset)
                {
                    //行列
                    GUILayout.BeginVertical("HelpBox");
                    _spriteAsset.IsStatic = GUILayout.Toggle(_spriteAsset.IsStatic, "是否为静态表情?");
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Row:", GUILayout.Width(100));
                    _spriteAsset.Row = EditorGUILayout.IntField(_spriteAsset.Row);
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Column:", GUILayout.Width(100));
                    _spriteAsset.Column = EditorGUILayout.IntField(_spriteAsset.Column);
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                    //具体的精灵信息
                    if (_spriteAsset&&_spriteAsset.ListSpriteGroup.Count>0)
                    {
                        List<SpriteInforGroup> inforGroups = _spriteAsset.ListSpriteGroup;
                        GUILayout.Label("精灵信息:");
                        _spritesScrollView = GUILayout.BeginScrollView(_spritesScrollView, "HelpBox");
                        for (int i = 0; i < inforGroups.Count; i++)
                        {
                            GUILayout.BeginVertical("HelpBox");
                            //标题信息..........
                            GUILayout.BeginHorizontal();
                            if (GUILayout.Button(i.ToString(), _showIndex == i ? "OL Minus" : "OL Plus"))
                            {
                                if (_showIndex == i)
                                    _showIndex = -1;
                                else
                                    _showIndex = i;

                                //_showSprites.Clear();
                            }
                            GUILayout.Label("Tag:");
                            inforGroups[i].Tag=EditorGUILayout.TextField(inforGroups[i].Tag);
                            GUILayout.Label("Size:");
                            inforGroups[i].Size = EditorGUILayout.FloatField(inforGroups[i].Size);
                            GUILayout.Label("Width:");
                            inforGroups[i].Width = EditorGUILayout.FloatField(inforGroups[i].Width);
                            GUILayout.EndHorizontal();
                            //具体信息
                            if (_showIndex == i)
                            {
                                List<SpriteInfor> spriteInfors = inforGroups[i].ListSpriteInfor;
                              //  bool createSprite = _showSprites.Count > 0 ? false : true;
                                for (int m = 0; m < spriteInfors.Count; m++)
                                {
                                    //if (createSprite)
                                    //{
                                    //    Sprite sprite = Sprite.Create((Texture2D)_spriteAsset.TexSource, spriteInfors[m].Rect, new Vector2(0.5f,0.5f));
                                    //    _showSprites.Add(sprite);
                                    //}

                                    GUILayout.BeginHorizontal("Box");
                                    //渲染精灵图片
                                  //  EditorGUILayout.ObjectField(_showSprites[m],typeof(Sprite),false,GUILayout.Height(80.0f), GUILayout.Width(80));
                                    //渲染其他信息
                                    GUILayout.BeginVertical("HelpBox");
                                    GUILayout.Label(spriteInfors[m].Id.ToString());
                                    GUILayout.Label(spriteInfors[m].Name);
                                    GUILayout.Label(spriteInfors[m].Tag);
                                    GUILayout.Label(spriteInfors[m].Rect.ToString());
                                    GUILayout.Label(spriteInfors[m].Uv.ToString());
                                    GUILayout.EndVertical();
                                    GUILayout.EndHorizontal();
                                }
                               
                            }
                            GUILayout.EndVertical();

                        }
                        GUILayout.EndScrollView();
                    }

                   
                }

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();

                //非自动布局绘制------------------
                //绘制线
                DrawTextureLines();

            }

            //更新信息
            UpdateSpriteGroup();

            //保存序列化文件
            if (_spriteAsset)
                EditorUtility.SetDirty(_spriteAsset);

        }

     

        public static List<SpriteInforGroup> GetAssetSpriteInfor(Texture2D tex)
		{
			List<SpriteInforGroup> _listGroup = new List<SpriteInforGroup>();
			string filePath = UnityEditor.AssetDatabase.GetAssetPath(tex);

			Object[] objects = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(filePath);

			List<SpriteInfor> _tempSprite = new List<SpriteInfor>();

			Vector2 _texSize = new Vector2(tex.width, tex.height);
			for (int i = 0; i < objects.Length; i++)
			{
				if (objects[i].GetType() != typeof(Sprite))
					continue;
				SpriteInfor temp = new SpriteInfor();
				Sprite sprite = objects[i] as Sprite;
				temp.Id = i;
				temp.Name = sprite.name;
				temp.Pivot = sprite.pivot;
				temp.Rect = sprite.rect;
				temp.Sprite = sprite;
				temp.Tag = sprite.name;
				temp.Uv = GetSpriteUV(_texSize, sprite.rect);
				_tempSprite.Add(temp);
			}

			for (int i = 0; i < _tempSprite.Count; i++)
			{
				SpriteInforGroup _tempGroup = new SpriteInforGroup();
				_tempGroup.Tag = _tempSprite[i].Tag;
				//_tempGroup.Size = 24.0f;
				//_tempGroup.Width = 1.0f;
				_tempGroup.ListSpriteInfor = new List<SpriteInfor>();
				_tempGroup.ListSpriteInfor.Add(_tempSprite[i]);
				for (int j = i + 1; j < _tempSprite.Count; j++)
				{
					if (_tempGroup.Tag == _tempSprite[j].Tag)
					{
						_tempGroup.ListSpriteInfor.Add(_tempSprite[j]);
						_tempSprite.RemoveAt(j);
						j--;
					}
				}
				_listGroup.Add(_tempGroup);
				_tempSprite.RemoveAt(i);
				i--;
			}

			return _listGroup;
		}

		private static Vector2[] GetSpriteUV(Vector2 texSize, Rect _sprRect)
		{
			Vector2[] uv = new Vector2[4];
			uv[0] = new Vector2(_sprRect.x / texSize.x, (_sprRect.y + _sprRect.height) / texSize.y);
			uv[1] = new Vector2((_sprRect.x + _sprRect.width) / texSize.x, (_sprRect.y + _sprRect.height) / texSize.y);
			uv[2] = new Vector2((_sprRect.x + _sprRect.width) / texSize.x, _sprRect.y / texSize.y);
			uv[3] = new Vector2(_sprRect.x / texSize.x, _sprRect.y / texSize.y);
			return uv;
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


        //更新精灵组的信息
        private void UpdateSpriteGroup()
        {
            if (_spriteAsset&& _spriteAsset.TexSource&& _spriteAsset.Row>1&&_spriteAsset.Column>1)
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

                                inforGroup.Tag = infor.Name;
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
                            inforGroup.Tag = (index + 1).ToString();
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


        //获取精灵信息
        private SpriteInfor GetSpriteInfo(int index,int row, int column,Vector2 size,Vector2 texSize)
        {
            SpriteInfor infor = Pool<SpriteInfor>.Get();
            infor.Id = index;
            infor.Name = index.ToString();
            infor.Rect = new Rect(size.y * column, texSize.y - (row + 1) * size.x, size.x, size.y);
            infor.Uv = GetSpriteUV(texSize, infor.Rect);
            return infor;
        }




    }
}