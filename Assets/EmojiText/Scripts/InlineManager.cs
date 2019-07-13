using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmojiText.Taurus
{
	[ExecuteInEditMode]
	public class InlineManager : MonoBehaviour
	{

		#region 属性
		//所有的精灵消息
		public readonly Dictionary<int, Dictionary<string, SpriteInforGroup>> IndexSpriteInfo = new Dictionary<int, Dictionary<string, SpriteInforGroup>>();
	
		//图集
		[SerializeField]
		private List<SpriteGraphic> _spriteGraphics = new List<SpriteGraphic>();
		
		//绘制的模型数据信息
		private readonly Dictionary<int, Dictionary<InlineText, MeshInfo>> _graphicMeshInfo = new Dictionary<int, Dictionary<InlineText, MeshInfo>>();

		//渲染列表
		List<int> _renderIndexs = new List<int>();
		#endregion

		// Use this for initialization
		void OnEnable()
		{
			Initialize();
		}

		#region 初始化
		void Initialize()
		{
			for (int i = 0; i < _spriteGraphics.Count; i++)
			{
				SpriteAsset mSpriteAsset = _spriteGraphics[i].m_spriteAsset;
				if (!IndexSpriteInfo.ContainsKey(mSpriteAsset.Id))
				{
					Dictionary<string, SpriteInforGroup> spriteGroup = new Dictionary<string, SpriteInforGroup>();
					foreach (var item in mSpriteAsset.ListSpriteGroup)
					{
						if (!spriteGroup.ContainsKey(item.Tag) && item.ListSpriteInfor != null && item.ListSpriteInfor.Count > 0)
							spriteGroup.Add(item.Tag, item);
					}
					IndexSpriteInfo.Add(mSpriteAsset.Id, spriteGroup);
				}
			}
		}
		#endregion

		private void Update()
		{
			if (_renderIndexs != null && _renderIndexs.Count > 0)
			{
				for (int i = 0; i < _renderIndexs.Count; i++)
				{
					int id = _renderIndexs[i];
					SpriteGraphic spriteGraphic = _spriteGraphics.Find(x => x.m_spriteAsset != null && x.m_spriteAsset.Id == id);
					if (spriteGraphic != null)
					{
						if (!_graphicMeshInfo.ContainsKey(id))
						{
							spriteGraphic.MeshInfo = null;
							continue;
						}

						Dictionary<InlineText, MeshInfo> textMeshInfo = _graphicMeshInfo[id];
						if (textMeshInfo == null || textMeshInfo.Count == 0)
							spriteGraphic.MeshInfo = null;
						else
						{
							MeshInfo meshInfo = Pool<MeshInfo>.Get();
							meshInfo.Clear();
							foreach (var item in textMeshInfo)
							{
								if (item.Value.visable)
								{
                                    meshInfo.Vertices.AddRange(item.Value.Vertices);
                                    meshInfo.UVs.AddRange(item.Value.UVs);
								}
							}
							if (spriteGraphic.MeshInfo != null)
								Pool<MeshInfo>.Release(spriteGraphic.MeshInfo);

							spriteGraphic.MeshInfo = meshInfo;
						}
					}
				}
				//清掉渲染索引
				_renderIndexs.Clear();
			}
		}

		//更新Text文本信息
		public void UpdateTextInfo(InlineText key, int id, List<SpriteTagInfo> value, bool visable)
		{
			Dictionary<InlineText, MeshInfo> textMeshInfo;
			if (value == null)
			{
				if (_graphicMeshInfo.TryGetValue(id, out textMeshInfo) && textMeshInfo.ContainsKey(key))
				{
					textMeshInfo[key].Release();
					textMeshInfo.Remove(key);
				}
			}
			else
			{
				SpriteGraphic spriteGraphic = _spriteGraphics.Find(x => x.m_spriteAsset != null && x.m_spriteAsset.Id == id);
				if (spriteGraphic != null)
				{
					if (!_graphicMeshInfo.TryGetValue(id, out textMeshInfo))
					{
						textMeshInfo = new Dictionary<InlineText, MeshInfo>();
						_graphicMeshInfo.Add(id, textMeshInfo);
					}

					MeshInfo meshInfo;
					if (!textMeshInfo.TryGetValue(key, out meshInfo))
					{
						meshInfo = Pool<MeshInfo>.Get();
						textMeshInfo.Add(key, meshInfo);
					}
					meshInfo.Clear();
					meshInfo.visable = visable;
					for (int i = 0; i < value.Count; i++)
					{
						for (int j = 0; j < value[i].Pos.Length; j++)
						{
							//世界转本地坐标->避免位置变换的错位
							meshInfo.Vertices.Add(Utility.TransformWorld2Point(spriteGraphic.transform, value[i].Pos[j]));
						}
						meshInfo.UVs.AddRange(value[i].UVs);
					}
				}
			}

			//添加到渲染列表里面  --  等待下一帧渲染
			if (!_renderIndexs.Contains(id))
			{
				_renderIndexs.Add(id);
			}
		}


	}

	#region 模型数据信息
	public class MeshInfo
	{
		public List<Vector3> Vertices = ListPool<Vector3>.Get();
		public List<Vector2> UVs = ListPool<Vector2>.Get();
		public List<Color> Colors = ListPool<Color>.Get();
		public List<int> Triangles = ListPool<int>.Get();
		public bool visable = true;

		public void Clear()
		{
			Vertices.Clear();
			UVs.Clear();
			Colors.Clear();
			Triangles.Clear();
		}

		public void Release()
		{
			ListPool<Vector3>.Release(Vertices);
			ListPool<Vector2>.Release(UVs);
			ListPool<Color>.Release(Colors);
			ListPool<int>.Release(Triangles);

			Pool<MeshInfo>.Release(this);
		}

	}
	#endregion
}
