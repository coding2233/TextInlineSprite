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

        private Vector2 _texScrollView;
        private int _row=0;
        private int _column=0;

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
                GUILayout.Label(_sourceTex.width+"*"+_sourceTex.width);
                GUILayout.Space(5);
                GUILayout.Label("Row:");
                _row = EditorGUILayout.IntField(_row);
                GUILayout.Label("Column:");
                _column = EditorGUILayout.IntField(_column);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }

            Handles.BeginGUI();
            Handles.color = Color.green;

            if (_row > 0)
            {
                float yP = _sourceTex.height / _row;
                float temy = _texScrollView.y % yP;
                int maxRow =(int)(Screen.height / yP);
                
                for (int i = 0; i < maxRow; i++)
                {
                    float h = (yP * i) + (yP- temy);
                    float endx = 0.625f * Screen.width - 10.0f;
                    endx = endx > _sourceTex.width ? _sourceTex.width : endx;
                    Handles.DrawLine(new Vector3(5, h), new Vector3(endx, h));
                }
            }

            if (_column > 0)
            {
                float xP = _sourceTex.height / _row;
                float temx = _texScrollView.x % xP;
                float scrWidth = 0.625f * Screen.width;
                scrWidth = scrWidth > _sourceTex.width ? _sourceTex.width : scrWidth;
                int maxColumn = (int)(scrWidth / xP);

                for (int i = 0; i < maxColumn; i++)
                {
                    float w = (xP * i) + (xP - temx);
                    float endy = Screen.height > _sourceTex.height ? _sourceTex.height : Screen.height;
                    Handles.DrawLine(new Vector3(w, 0), new Vector3(w, endy));
                }
            }

            Handles.EndGUI();
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

	}
}