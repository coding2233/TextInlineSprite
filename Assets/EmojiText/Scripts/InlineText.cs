using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.Text;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;
namespace EmojiText.Taurus
{
	[ExecuteInEditMode]
	public class InlineText : Text, IPointerClickHandler
	{
		#region 属性
		// 用正则取  [图集ID#表情Tag] ID值==-1 ,表示为超链接
		private static readonly Regex _inputTagRegex = new Regex(@"\[(\-{0,1}\d{0,})#(.+?)(:{1}(.+?))?\]", RegexOptions.Singleline);
		//文本表情管理器
		private InlineManager _inlineManager;

		//表情位置索引信息
		private List<SpriteTagInfo> _spriteInfo = new List<SpriteTagInfo>();
		//计算定点信息的缓存数组
		private readonly UIVertex[] m_TempVerts = new UIVertex[4];

		private StringBuilder _textBuilder = new StringBuilder();

		UIVertex _tempVertex = UIVertex.simpleVert;
		private List<int> _lastRenderIndexs = new List<int>();
		#region 超链接
		[System.Serializable]
		public class HrefClickEvent : UnityEvent<string, int> { }
		//点击事件监听
		public HrefClickEvent OnHrefClick = new HrefClickEvent();
		// 超链接信息列表  
		private readonly List<HrefInfo> _listHrefInfos = new List<HrefInfo>();
		#endregion

		#endregion

		#region 重写函数
		[TextArea(3, 10)]
		[SerializeField]
		protected string _text = string.Empty;

		public override string text
		{
			get
			{
				return m_Text;
			}
			set
			{
				if (String.IsNullOrEmpty(value))
				{
					if (String.IsNullOrEmpty(m_Text))
						return;
					//m_Text = "";
					m_Text = GetOutputText(value);
					SetVerticesDirty();
				}
				else if (_text != value)
				{
					m_Text = GetOutputText(value);
					//m_Text = value;
					SetVerticesDirty();
					SetLayoutDirty();
				}
#if UNITY_EDITOR
				//编辑器赋值 如果是一样的 也可以刷新一下
				else
				{
					m_Text = GetOutputText(value);
					SetVerticesDirty();
					SetLayoutDirty();
				}
#endif
				//输入字符备份
				_text = value;
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			supportRichText = true;
			alignByGeometry = true;
			if(_inlineManager==null)
				_inlineManager = GetComponentInParent<InlineManager>();
            UpdateDrawSprite(true);
		}

        protected override void OnDisable()
		{
			base.OnDisable();
            UpdateDrawSprite(false);
         }

		protected override void Start()
		{
			base.Start();

			if (_inlineManager == null)
				_inlineManager = GetComponentInParent<InlineManager>();
			m_Text = GetOutputText(_text);
			SetVerticesDirty();
			SetLayoutDirty();
		}

		protected override void OnPopulateMesh(VertexHelper toFill)
		{
			if (font == null)
				return;
			base.OnPopulateMesh(toFill);

			m_DisableFontTextureRebuiltCallback = true;
			//更新顶点位置&去掉乱码uv
			DealSpriteTagInfo(toFill);
			//处理超链接的信息
			DealHrefInfo(toFill);
			m_DisableFontTextureRebuiltCallback = false;

			//更新表情绘制
			UpdateDrawSprite(true);
		}

		// 重写文本所占的长宽
		//文本的宽度计算好像有bug，超过sizeDelta就取sizeDelta
		public override float preferredWidth
		{
			get
			{
				var settings = GetGenerationSettings(Vector2.zero);
				float width= cachedTextGeneratorForLayout.GetPreferredWidth(m_Text, settings) / pixelsPerUnit;
				return width < rectTransform.sizeDelta.x || horizontalOverflow==HorizontalWrapMode.Overflow ? width : rectTransform.sizeDelta.x;
			}
		}
		public override float preferredHeight
		{
			get
			{
				var settings = GetGenerationSettings(new Vector2(rectTransform.rect.size.x, 0.0f));
				float height= cachedTextGeneratorForLayout.GetPreferredHeight(m_Text, settings) / pixelsPerUnit;
				return height < rectTransform.sizeDelta.y || verticalOverflow == VerticalWrapMode.Overflow ? height : rectTransform.sizeDelta.y;
			}
		}
		#endregion

		#region 事件回调
		//响应点击事件-->检测是否在超链接的范围内
		public void OnPointerClick(PointerEventData eventData)
		{
			Vector2 lp;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(
				rectTransform, eventData.position, eventData.pressEventCamera, out lp);

			foreach (var hrefInfo in _listHrefInfos)
			{
				var boxes = hrefInfo.Boxes;
				for (var i = 0; i < boxes.Count; ++i)
				{
					if (boxes[i].Contains(lp))
					{
						OnHrefClick.Invoke(hrefInfo.HrefValue, hrefInfo.Id);
						return;
					}
				}
			}
		}
		#endregion

		#region  内部函数
		//根据正则规则更新文本
		private string GetOutputText(string inputText)
		{
			//回收各种对象
			ReleaseSpriteTageInfo();
			ReleaseHrefInfos();

			if (string.IsNullOrEmpty(inputText))
				return "";
			
			_textBuilder.Remove(0, _textBuilder.Length);
			int textIndex = 0;

			foreach (Match match in _inputTagRegex.Matches(inputText))
			{
				int tempId = 0;
				if (!string.IsNullOrEmpty(match.Groups[1].Value) && !match.Groups[1].Value.Equals("-"))
					tempId = int.Parse(match.Groups[1].Value);
				string tempTag = match.Groups[2].Value;
				//更新超链接
				if (tempId < 0)
				{
					_textBuilder.Append(inputText.Substring(textIndex, match.Index - textIndex));
					_textBuilder.Append("<color=blue>");
					int startIndex = _textBuilder.Length * 4;
					_textBuilder.Append("[" + match.Groups[2].Value + "]");
					int endIndex = _textBuilder.Length * 4 - 1;
					_textBuilder.Append("</color>");


					var hrefInfo = Pool<HrefInfo>.Get();
					hrefInfo.Id = Mathf.Abs(tempId);
					hrefInfo.StartIndex = startIndex;// 超链接里的文本起始顶点索引
					hrefInfo.EndIndex = endIndex;
					hrefInfo.Name = match.Groups[2].Value;
					hrefInfo.HrefValue = match.Groups[3].Value;
					_listHrefInfos.Add(hrefInfo);

				}
				//更新表情
				else
				{
					if (_inlineManager == null || !_inlineManager.IndexSpriteInfo.ContainsKey(tempId)
						|| !_inlineManager.IndexSpriteInfo[tempId].ContainsKey(tempTag))
						continue;

					SpriteInforGroup tempGroup = _inlineManager.IndexSpriteInfo[tempId][tempTag];

					_textBuilder.Append(inputText.Substring(textIndex, match.Index - textIndex));
					int tempIndex = _textBuilder.Length * 4;
					_textBuilder.Append(@"<quad size=" + tempGroup.Size + " width=" + tempGroup.Width + " />");

					//清理标签
					SpriteTagInfo tempSpriteTag = Pool<SpriteTagInfo>.Get();
					tempSpriteTag.Index = tempIndex;
					tempSpriteTag.Id = tempId;
					tempSpriteTag.Tag = tempTag;
					tempSpriteTag.Size = new Vector2(tempGroup.Size * tempGroup.Width, tempGroup.Size);
					tempSpriteTag.UVs = tempGroup.ListSpriteInfor[0].Uv;

					//添加正则表达式的信息
					_spriteInfo.Add(tempSpriteTag);
				}
				textIndex = match.Index + match.Length;
			}

			_textBuilder.Append(inputText.Substring(textIndex, inputText.Length - textIndex));
			return _textBuilder.ToString();
		}
		//处理表情信息
		private void DealSpriteTagInfo(VertexHelper toFill)
		{
			int index = -1;
			//emoji 
			for (int i = 0; i < _spriteInfo.Count; i++)
			{
				index = _spriteInfo[i].Index;
				if ((index + 4) < toFill.currentVertCount)
				{
					for (int j = index; j < index + 4; j++)
					{
						toFill.PopulateUIVertex(ref _tempVertex, j);
						//清理多余的乱码uv
						_tempVertex.uv0 = Vector2.zero;
						//获取quad的位置 --> 转为世界坐标
						_spriteInfo[i].Pos[j - index] = Utility.TransformPoint2World(transform, _tempVertex.position);
						toFill.SetUIVertex(_tempVertex, j);
					}

				}
			}
		}
		//处理超链接的信息
		private void DealHrefInfo(VertexHelper toFill)
		{
			if (_listHrefInfos.Count > 0)
			{
				// 处理超链接包围框  
				for (int i = 0; i < _listHrefInfos.Count; i++)
				{
					_listHrefInfos[i].Boxes.Clear();
					if (_listHrefInfos[i].StartIndex >= toFill.currentVertCount)
						continue;

					toFill.PopulateUIVertex(ref _tempVertex, _listHrefInfos[i].StartIndex);
					// 将超链接里面的文本顶点索引坐标加入到包围框  
					var pos = _tempVertex.position;
					var bounds = new Bounds(pos, Vector3.zero);
					for (int j = _listHrefInfos[i].StartIndex + 1; j < _listHrefInfos[i].EndIndex; j++)
					{
						if (j >= toFill.currentVertCount)
						{
							break;
						}
						toFill.PopulateUIVertex(ref _tempVertex, j);
						pos = _tempVertex.position;
						if (pos.x < bounds.min.x)
						{
							// 换行重新添加包围框  
							_listHrefInfos[i].Boxes.Add(new Rect(bounds.min, bounds.size));
							bounds = new Bounds(pos, Vector3.zero);
						}
						else
						{
							bounds.Encapsulate(pos); // 扩展包围框  
						}
					}
					//添加包围盒
					_listHrefInfos[i].Boxes.Add(new Rect(bounds.min, bounds.size));
				}

				//添加下划线
				Vector2 extents = rectTransform.rect.size;
				var settings = GetGenerationSettings(extents);
				TextGenerator underlineText = Pool<TextGenerator>.Get();
				underlineText.Populate("_", settings);
				IList<UIVertex> tut = underlineText.verts;
				for (int m = 0; m < _listHrefInfos.Count; m++)
				{
					for (int i = 0; i < _listHrefInfos[m].Boxes.Count; i++)
					{
						//计算下划线的位置
						Vector3[] ulPos = new Vector3[4];
						ulPos[0] = _listHrefInfos[m].Boxes[i].position + new Vector2(0.0f, fontSize * 0.2f);
						ulPos[1] = ulPos[0] + new Vector3(_listHrefInfos[m].Boxes[i].width, 0.0f);
						ulPos[2] = _listHrefInfos[m].Boxes[i].position + new Vector2(_listHrefInfos[m].Boxes[i].width, 0.0f);
						ulPos[3] = _listHrefInfos[m].Boxes[i].position;
						//绘制下划线
						for (int j = 0; j < 4; j++)
						{
							m_TempVerts[j] = tut[j];
							m_TempVerts[j].color = Color.blue;
							m_TempVerts[j].position = ulPos[j];
							if (j == 3)
								toFill.AddUIVertexQuad(m_TempVerts);
						}
					}
				}
				//回收下划线的对象
				Pool<TextGenerator>.Release(underlineText);
			}

		}
		//表情绘制
		private void UpdateDrawSprite(bool visable)
		{
			//记录之前的信息
			if ((_spriteInfo == null || _spriteInfo.Count == 0) && _lastRenderIndexs.Count > 0)
			{
				for (int i = 0; i < _lastRenderIndexs.Count; i++)
				{
					_inlineManager.UpdateTextInfo(this, _lastRenderIndexs[i], null, visable);
				}
				_lastRenderIndexs.Clear();
			}
			else
			{
				_lastRenderIndexs.Clear();
				for (int i = 0; i < _spriteInfo.Count; i++)
				{
					//添加渲染id索引
					if (!_lastRenderIndexs.Contains(_spriteInfo[i].Id))
					{
						_inlineManager.UpdateTextInfo(this, _spriteInfo[i].Id, _spriteInfo.FindAll(x => x.Id == _spriteInfo[i].Id), visable);
						_lastRenderIndexs.Add(_spriteInfo[i].Id);
					}
				}
			}
		}
		//回收SpriteTagInfo
		private void ReleaseSpriteTageInfo()
		{
			//记录之前的信息
			for (int i = 0; i < _spriteInfo.Count; i++)
			{
				//回收信息到对象池
				Pool<SpriteTagInfo>.Release(_spriteInfo[i]);
			}
			_spriteInfo.Clear();
		}
		//回收超链接的信息
		private void ReleaseHrefInfos()
		{
			for (int i = 0; i < _listHrefInfos.Count; i++)
			{
				Pool<HrefInfo>.Release(_listHrefInfos[i]);
			}
			_listHrefInfos.Clear();
		}
		#endregion


		#region UNITY_EDITOR
#if UNITY_EDITOR
		protected override void OnValidate()
		{
			base.OnValidate();
			m_Text = GetOutputText(_text);
			SetVerticesDirty();
			SetLayoutDirty();
		}

		//辅助线框
		Vector3[] _textWolrdVertexs = new Vector3[4];
		private void OnDrawGizmos()
		{
			//text
			rectTransform.GetWorldCorners(_textWolrdVertexs);
			GizmosDrawLine(Color.white,_textWolrdVertexs);

			//preferred size
			Vector2 pivot = GetTextAnchorPivot(alignment);
			Rect rect = new Rect();
			Vector2 size = rectTransform.sizeDelta - new Vector2(preferredWidth, preferredHeight);
            rect.position = new Vector2(pivot.x * size.x, pivot.y * size.y) - new Vector2(rectTransform.sizeDelta.x* rectTransform.pivot.x, rectTransform.sizeDelta.y * rectTransform.pivot.y);
			rect.width = preferredWidth;
			rect.height = preferredHeight;
			_textWolrdVertexs[0] = Utility.TransformPoint2World(transform,new Vector3(rect.x, rect.y));
			_textWolrdVertexs[1] = Utility.TransformPoint2World(transform, new Vector3(rect.x+rect.width, rect.y));
			_textWolrdVertexs[2] = Utility.TransformPoint2World(transform, new Vector3(rect.x + rect.width, rect.y+rect.height));
			_textWolrdVertexs[3] = Utility.TransformPoint2World(transform, new Vector3(rect.x, rect.y+rect.height));
			GizmosDrawLine(Color.blue,_textWolrdVertexs);

			//href
			for (int i = 0; i < _listHrefInfos.Count; i++)
			{
				for (int j = 0; j < _listHrefInfos[i].Boxes.Count; j++)
				{
					rect = _listHrefInfos[i].Boxes[j];
					_textWolrdVertexs[0] = Utility.TransformPoint2World(transform, rect.position);
					_textWolrdVertexs[1] = Utility.TransformPoint2World(transform, new Vector3(rect.x + rect.width, rect.y));
					_textWolrdVertexs[2] = Utility.TransformPoint2World(transform, new Vector3(rect.x + rect.width, rect.y + rect.height));
					_textWolrdVertexs[3] = Utility.TransformPoint2World(transform, new Vector3(rect.x, rect.y + rect.height));
					
					GizmosDrawLine(Color.green,_textWolrdVertexs);
				}
			}

			//sprite
			for (int i = 0; i < _spriteInfo.Count; i++)
			{
				GizmosDrawLine(Color.yellow,_spriteInfo[i].Pos);
			}
		}
		//划线
		private void GizmosDrawLine(Color color,Vector3[] pos)
		{
			Gizmos.color = color;

			Gizmos.DrawLine(pos[0], pos[1]);
			Gizmos.DrawLine(pos[1], pos[2]);
			Gizmos.DrawLine(pos[2], pos[3]);
			Gizmos.DrawLine(pos[3], pos[0]);
		}
#endif
		#endregion
	}

	#region Struct
	/// <summary>
	/// 图片的信息
	/// </summary>
	public class SpriteTagInfo
	{
		/// <summary>
		/// 顶点索引id
		/// </summary>
		public int Index;
		/// <summary>
		/// 图集id
		/// </summary>
		public int Id;
		/// <summary>
		/// 标签标签
		/// </summary>
		public string Tag;
		/// <summary>
		/// 标签大小
		/// </summary>
		public Vector2 Size;
		/// <summary>
		/// 表情位置
		/// </summary>
		public Vector3[] Pos = new Vector3[4];
		/// <summary>
		/// uv
		/// </summary>
		public Vector2[] UVs = new Vector2[4];
	}

	/// <summary>
	/// 超链接信息类
	/// </summary>
	public class HrefInfo
	{
		/// <summary>
		/// 超链接id
		/// </summary>
		public int Id;
		/// <summary>
		/// 顶点开始索引值
		/// </summary>
		public int StartIndex;
		/// <summary>
		/// 顶点结束索引值
		/// </summary>
		public int EndIndex;
		/// <summary>
		/// 名称
		/// </summary>
		public string Name;
		/// <summary>
		/// 超链接的值
		/// </summary>
		public string HrefValue;
		/// <summary>
		/// 碰撞盒范围
		/// </summary>
		public readonly List<Rect> Boxes = new List<Rect>();
	}
	#endregion

}
