using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EmojiUI
{
	public class EmojiRenderGroup : IEmojiRender
	{

		private class ListenerData
		{
			public Vector3 worldpos;

			public Quaternion worldquat;

			public Vector3 scale;

			public void Set(InlineText text)
			{
				if (text != null)
				{
					Transform trans = text.transform;
					this.worldpos = trans.position;
					this.worldquat = trans.rotation;
					this.scale = trans.localScale;
				}
			}

			public bool Dirty(InlineText text)
			{
				if (text != null)
				{
					Transform trans = text.transform;
					return trans.position != worldpos || trans.rotation != worldquat || trans.localScale != scale;
				}
				return false;
			}
		}

		private class CanvasGraphicGroup
		{
			public Canvas canvas;

			public int AtlasID;

			public SpriteGraphic graphic;

			public UnitMeshInfo mesh;

			public bool isDirty;
		}

		public EmojiRenderType renderType { get { return EmojiRenderType.RenderGroup; } }

		public float Speed { get; set; }

		private InlineManager manager;

		private List<SpriteAsset> allatlas;

		private List<InlineText> alltexts;

		/// <summary>
		/// 因为一般一个canvas下就数个spritegrahphic，用list足够，查询追求不高，同时减少内存占用
		/// </summary>
		private List<CanvasGraphicGroup> GraphicTasks;

		private List<InlineText> rebuildqueue;

		private Dictionary<InlineText, ListenerData> listeners;

		private UnitMeshInfo tempMesh;

		private float? _time;

		private static GameObject PoolObj;

		public EmojiRenderGroup(InlineManager target)
		{
			manager = target;
		}

		public List<InlineText> GetAllRenders()
		{
			return alltexts;
		}

		public List<SpriteAsset> GetAllRenderAtlas()
		{
			return allatlas;
		}

		public void Clear()
		{
			if (alltexts != null)
			{
				alltexts.Clear();
			}

			if (allatlas != null)
			{
				allatlas.Clear();
			}

			if (rebuildqueue != null)
			{
				rebuildqueue.Clear();
			}

			if (GraphicTasks != null)
			{
				GraphicTasks.Clear();
			}

			if (listeners != null)
			{
				listeners.Clear();
			}

		}

		public bool isRendingAtlas(SpriteAsset asset)
		{
			if (allatlas != null)
				return allatlas.Contains(asset);
			return false;
		}

		public void PrepareAtlas(SpriteAsset asset)
		{
			if (allatlas == null)
			{
				allatlas = ListPool<SpriteAsset>.Get();
			}

			if (!allatlas.Contains(asset))
			{
				allatlas.Add(asset);
			}
		}

		public bool TryRendering(InlineText text)
		{
			AddInText(text);

			if (rebuildqueue == null)
			{
				rebuildqueue = ListPool<InlineText>.Get();
			}

			if (!rebuildqueue.Contains(text))
			{
				rebuildqueue.Add(text);
				return true;
			}

			return false;
		}

		public void DisRendering(InlineText text)
		{
			RemoveInText(text);

			if (rebuildqueue != null && !rebuildqueue.Contains(text))
			{
				rebuildqueue.Add(text);
			}

			if (listeners != null)
			{
				listeners.Remove(text);
			}
		}

		public Texture getRenderTexture(SpriteGraphic graphic)
		{
			if (graphic != null && GraphicTasks != null)
			{
				CanvasGraphicGroup group = FindGraphicGroup(graphic);
				if (group != null)
				{
					UnitMeshInfo info = group.mesh;
					if (info != null)
					{
						return info.getTexture();
					}
				}
			}
			return null;
		}

		public void Dispose()
		{
			if (allatlas != null)
			{
				ListPool<SpriteAsset>.Release(allatlas);
				allatlas = null;
			}

			if (alltexts != null)
			{
				ListPool<InlineText>.Release(alltexts);
				alltexts = null;
			}

			if (tempMesh != null)
			{
				UnitMeshInfoPool.Release(tempMesh);
				tempMesh = null;
			}

			if (GraphicTasks != null)
			{
				for (int i = 0; i < GraphicTasks.Count; ++i)
				{
					CanvasGraphicGroup group = GraphicTasks[i];
					SpriteGraphic target = group.graphic;
					if (target != null)
					{
						target.Draw(null);
						target.SetDirtyMask();
						target.SetVerticesDirty();
					}

					if (group.mesh != null)
					{
						UnitMeshInfoPool.Release(group.mesh);
					}
				}

				GraphicTasks.Clear();
				GraphicTasks = null;
			}

			if (rebuildqueue != null)
			{
				ListPool<InlineText>.Release(rebuildqueue);
				rebuildqueue = null;
			}


			if (listeners != null)
			{
				listeners.Clear();
				listeners = null;
			}

			if (tempMesh != null)
			{
				UnitMeshInfoPool.Release(tempMesh);
				tempMesh = null;
			}

			_time = null;
		}

		public void Release(Graphic graphic)
		{

			if (graphic is SpriteGraphic && PoolObj != null)
			{
				SpriteGraphic spriteGraphic = graphic as SpriteGraphic;
				spriteGraphic.Draw(null);

				if (GraphicTasks != null)
				{
					for (int i = 0; i < GraphicTasks.Count; ++i)
					{
						CanvasGraphicGroup group = GraphicTasks[i];

						if (group != null && UnityEngine.Object.ReferenceEquals(group.graphic, graphic))
						{
							GraphicTasks.RemoveAt(i);
							break;
						}
					}
				}
			}

		}


		public void LateUpdate()
		{
			if (Application.isPlaying)
			{
				MarkDirtyStaticEmoji();

				RenderRebuild();
				PlayAnimation();

			}

		}

		void MarkDirtyStaticEmoji()
		{
			if (alltexts != null && alltexts.Count > 0)
			{
				if (listeners == null)
					listeners = new Dictionary<InlineText, ListenerData>();

				for (int i = 0; i < alltexts.Count; ++i)
				{
					InlineText text = alltexts[i];
					List<IFillData> emojidata = text.PopEmojiData();
					if (text != null && emojidata != null && emojidata.Count > 0 && allatlas != null && allatlas.Count > 0)
					{
						bool isdirty = false;
						ListenerData data = null;
						if (listeners.TryGetValue(text, out data))
						{
							isdirty = data.Dirty(text);
							if (isdirty)
							{
								data.Set(text);
							}
						}
						else
						{
							data = new ListenerData();
							data.Set(text);

							listeners.Add(text, data);
						}

						int staticcnt = 0;
						for (int j = 0; j < emojidata.Count; ++j)
						{
							IFillData taginfo = emojidata[j];
							SpriteAsset asset = null;
							SpriteInfoGroup groupinfo = manager.FindSpriteGroup(taginfo.Tag, out asset);
							if (groupinfo != null && groupinfo.spritegroups.Count == 1)
							{
								staticcnt++;
							}
						}

						if (staticcnt > 0 && isdirty)
						{
							this.TryRendering(text);
						}
					}
				}
			}
		}

		void RenderRebuild()
		{
			EmojiTools.BeginSample("Emoji_GroupRebuild");
			if (rebuildqueue != null && rebuildqueue.Count > 0)
			{
				List<string> joblist = ListPool<string>.Get();

				this.SetAllGroupDirty(false);

				for (int i = 0; i < alltexts.Count; ++i)
				{
					InlineText text = alltexts[i];

					List<IFillData> emojidata = text.PopEmojiData();
					if (text != null && emojidata != null && emojidata.Count > 0 && allatlas != null && allatlas.Count > 0)
					{
						if (tempMesh == null)
							tempMesh = UnitMeshInfoPool.Get();

						for (int j = 0; j < emojidata.Count; ++j)
						{
							IFillData taginfo = emojidata[j];
							if (taginfo == null || taginfo.ignore)
								continue;
							Graphic job = Parse(text, taginfo, joblist);
							if (job)
							{
								joblist.Add(taginfo.Tag);
							}
						}
					}
				}

				if (GraphicTasks != null)
				{
					for (int i = 0; i < GraphicTasks.Count; ++i)
					{
						CanvasGraphicGroup group = GraphicTasks[i];
						if (group != null && group.graphic != null)
						{
							if (group.isDirty)
								group.graphic.SetVerticesDirty();
							else
							{
								group.graphic.Draw(null);
								group.graphic.SetDirtyMask();
								group.graphic.SetVerticesDirty();
							}
						}
					}
				}

				ListPool<string>.Release(joblist);
				rebuildqueue.Clear();
			}
			EmojiTools.EndSample();
		}

		void PlayAnimation()
		{
			EmojiTools.BeginSample("Emoji_GroupAnimation");
			if (alltexts != null)
			{
				if (_time == null)
				{
					_time = Time.timeSinceLevelLoad;
				}
				else
				{
					float deltatime = Time.timeSinceLevelLoad - _time.Value;

					int framecnt = Mathf.RoundToInt(deltatime * Speed);
					if (framecnt > 0)
					{
						List<string> joblist = ListPool<string>.Get();
						for (int i = 0; i < alltexts.Count; ++i)
						{
							InlineText text = alltexts[i];
							List<IFillData> emojidata = text.PopEmojiData();
							if (emojidata != null && emojidata.Count > 0 && allatlas != null && allatlas.Count > 0)
							{
								for (int j = 0; j < emojidata.Count; ++j)
								{
									IFillData taginfo = emojidata[j];
									if (taginfo == null || taginfo.ignore)
										continue;
									SpriteAsset asset = null;
									SpriteInfoGroup groupinfo = manager.FindSpriteGroup(taginfo.Tag, out asset);
									if (groupinfo != null && groupinfo.spritegroups.Count > 1)
									{
										int index = framecnt % groupinfo.spritegroups.Count;
										SpriteInfo sprInfo = groupinfo.spritegroups[index];
										taginfo.uv = sprInfo.uv;

										//render next
										CanvasGraphicGroup group = FindGraphicGroup(text.canvas, asset.ID);

										if (group != null)
										{
											if (tempMesh == null)
												tempMesh = UnitMeshInfoPool.Get();

											RefreshSubUIMesh(text, group, asset, taginfo.pos, taginfo.uv, joblist);

											if (group.isDirty)
											{
												group.graphic.SetVerticesDirty();
											}
											joblist.Add(taginfo.Tag);
										}
									}
								}
							}
						}
						ListPool<string>.Release(joblist);
					}
				}
			}

			EmojiTools.EndSample();
		}

		Graphic Parse(InlineText text, IFillData taginfo, List<string> joblist)
		{
			if (taginfo != null)
			{
				return ParsePosAndUV(text, taginfo.ID, taginfo.pos, taginfo.uv, joblist);
			}
			return null;
		}

		Graphic ParsePosAndUV(InlineText text, int ID, Vector3[] Pos, Vector2[] UV, List<string> joblist)
		{
			EmojiTools.BeginSample("Emoji_GroupParsePosAndUV");
			SpriteAsset matchAsset = null;
			for (int i = 0; i < allatlas.Count; ++i)
			{
				SpriteAsset asset = allatlas[i];
				if (asset != null && asset.ID == ID)
				{
					matchAsset = asset;
					break;
				}
			}

			if (matchAsset && matchAsset.texSource != null)
			{
				CanvasGraphicGroup group = FindGraphicGroup(text.canvas, matchAsset.ID);
				if (group == null)
				{
					group = new CanvasGraphicGroup();
					group.AtlasID = matchAsset.ID;
					group.canvas = text.canvas;
					group.mesh = UnitMeshInfoPool.Get();
					group.isDirty = false;
					group.graphic = CreateSpriteRender();

					if (GraphicTasks == null)
						GraphicTasks = new List<CanvasGraphicGroup>();

					GraphicTasks.Add(group);
				}

				RefreshSubUIMesh(text, group, matchAsset, Pos, UV, joblist);

				EmojiTools.EndSample();
				return group.graphic;
			}
			else
			{
				Debug.LogErrorFormat("missing {0} atlas", ID);
			}
			EmojiTools.EndSample();
			return null;
		}

		void RefreshSubUIMesh(InlineText text, CanvasGraphicGroup group, SpriteAsset matchAsset, Vector3[] Pos, Vector2[] UV, List<string> joblist)
		{
			// set mesh
			tempMesh.SetAtlas(matchAsset);
			tempMesh.SetUVLen(UV.Length);
			tempMesh.SetVertLen(Pos.Length);

			SpriteGraphic graphic = group.graphic;
			//think about culling and screen coordinate.......
			for (int i = 0; i < Pos.Length; ++i)
			{
				//text object pos
				Vector3 value = Pos[i];
				Vector3 worldpos = text.transform.TransformPoint(value);
				Vector3 localpos = group.graphic.transform.InverseTransformPoint(worldpos);
				tempMesh.SetVert(i, localpos);
			}

			for (int i = 0; i < UV.Length; ++i)
			{
				Vector2 value = UV[i];
				tempMesh.SetUV(i, value);
			}

			//rendermesh
			UnitMeshInfo currentMesh = group.mesh;

			if (!currentMesh.Equals(tempMesh))
			{
				if (joblist != null && joblist.Count > 0)
				{
					currentMesh.AddCopy(tempMesh);
					tempMesh.Clear();
				}
				else
				{
					currentMesh.Copy(tempMesh);
				}
			}

			if (currentMesh.VertCnt() > 3 && currentMesh.UVCnt() > 3)
			{
				graphic.Draw(this);
			}
			else
			{
				graphic.Draw(null);
			}

			group.isDirty = true;
		}

		public void FillMesh(Graphic graphic, VertexHelper vh)
		{
			CanvasGraphicGroup group = FindGraphicGroup(graphic);
			if (group != null)
			{
				UnitMeshInfo rendermesh = group.mesh;
				if (rendermesh != null && rendermesh.getTexture() != null)
				{
					int vertcnt = rendermesh.VertCnt();
					int uvcnt = rendermesh.UVCnt();
					if (vertcnt != uvcnt)
					{
						Debug.LogError("data error");
					}
					else
					{
						for (int i = 0; i < vertcnt; ++i)
						{
							vh.AddVert(rendermesh.GetVert(i), graphic.color, rendermesh.GetUV(i));
						}

						int cnt = vertcnt / 4;
						for (int i = 0; i < cnt; ++i)
						{
							int m = i * 4;

							vh.AddTriangle(m, m + 1, m + 2);
							vh.AddTriangle(m + 2, m + 3, m);
						}

						//vh.AddTriangle(0, 1, 2);
						//vh.AddTriangle(2, 3, 0);
					}
				}
			}
		}

		public void DrawGizmos(Graphic graphic)
		{
			CanvasGraphicGroup group = FindGraphicGroup(graphic);
			if (group != null)
			{
				UnitMeshInfo rendermesh = group.mesh;
				if (rendermesh != null && rendermesh.getTexture() != null)
				{
					Gizmos.color = Color.red;
					int vertcnt = rendermesh.VertCnt();
					int uvcnt = rendermesh.UVCnt();
					if (vertcnt != uvcnt)
					{
						Debug.LogError("data error");
					}
					else
					{
						for (int i = 0; i < vertcnt; i += 4)
						{
							Vector3 p1 = getPoint(graphic, rendermesh.GetVert(i));
							Vector3 p2 = getPoint(graphic, rendermesh.GetVert(i + 1));
							Vector3 p3 = getPoint(graphic, rendermesh.GetVert(i + 2));
							Vector3 p4 = getPoint(graphic, rendermesh.GetVert(i + 3));

							Gizmos.DrawLine(p1, p2);
							Gizmos.DrawLine(p2, p3);
							Gizmos.DrawLine(p3, p4);
							Gizmos.DrawLine(p4, p1);
						}
					}
				}
			}

		}

		Vector3 getPoint(Graphic graphic, Vector3 v)
		{
			Vector3 worldpos = graphic.transform.TransformPoint(v);
			return worldpos;
		}

		#region 考虑到后面测试和调整的时候数据类型的转换
		void SetAllGroupDirty(bool value)
		{
			if (GraphicTasks != null)
			{
				for (int i = 0; i < GraphicTasks.Count; ++i)
				{
					CanvasGraphicGroup group = GraphicTasks[i];
					if (group != null)
					{
						group.isDirty = value;
					}
				}
			}
		}

		CanvasGraphicGroup FindGraphicGroup(Canvas canvas, int atlasId)
		{
			if (GraphicTasks != null)
			{
				for (int i = 0; i < GraphicTasks.Count; ++i)
				{
					CanvasGraphicGroup group = GraphicTasks[i];
					if (group != null && group.canvas == canvas && group.AtlasID == atlasId)
					{
						return group;
					}
				}
			}
			return null;
		}

		CanvasGraphicGroup FindGraphicGroup(Graphic graphic)
		{
			if (GraphicTasks != null)
			{
				for (int i = 0; i < GraphicTasks.Count; ++i)
				{
					CanvasGraphicGroup group = GraphicTasks[i];
					if (group != null && group.graphic == graphic)
					{
						return group;
					}
				}
			}
			return null;
		}

		void AddInText(InlineText text)
		{
			if (alltexts == null)
			{
				alltexts = ListPool<InlineText>.Get();
			}

			if (!alltexts.Contains(text))
			{
				alltexts.Add(text);
			}
		}

		void RemoveInText(InlineText text)
		{
			if (alltexts != null)
			{
				alltexts.Remove(text);
			}
		}

		#endregion

		SpriteGraphic CreateSpriteRender()
		{
			if (PoolObj == null)
			{
				PoolObj = new GameObject("GroupRenderPool");
				SpriteGraphic target = null;
				for (int i = 0; i < 4; i++)
				{
					target = CreateInstance(PoolObj.transform);
				}

				target.transform.SetParent(manager.transform);
				return target;
			}
			else
			{
				int childcnt = PoolObj.transform.childCount;
				if (childcnt > 0)
				{
					Transform trans = PoolObj.transform.GetChild(0);
					trans.SetParent(manager.transform);
					return trans.GetComponent<SpriteGraphic>();
				}

				return CreateInstance(manager.transform);
			}
		}

		SpriteGraphic CreateInstance(Transform targetTrans)
		{
			GameObject newobject = new GameObject("pre_Sprite");
			newobject.transform.SetParent(targetTrans);
			newobject.transform.localPosition = Vector3.zero;
			newobject.transform.localScale = Vector3.one;
			newobject.transform.localRotation = Quaternion.identity;
			newobject.transform.gameObject.layer = targetTrans.gameObject.layer;
			newobject.transform.tag = targetTrans.gameObject.tag;
			//newobject.transform.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;

			SpriteGraphic spritegrahic = newobject.AddComponent<SpriteGraphic>();
			spritegrahic.raycastTarget = false;
			return spritegrahic;
		}
	}

}

