using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace EmojiUI
{
	public class UnitMeshInfo : IEquatable<UnitMeshInfo>
	{
		private SpriteAsset atlas;
		private List<Vector3> _Vertices = new List<Vector3>();
		private List<Vector2> _UV = new List<Vector2>();

		public Texture getTexture()
		{
			if (atlas != null)
				return atlas.texSource;
			return null;
		}

		public SpriteAsset GetAtlas()
		{
			return atlas;
		}

		public void SetAtlas(SpriteAsset data)
		{
			atlas = data;
		}

		public void Clear()
		{
			atlas = null;
			_Vertices.Clear();
			_UV.Clear();
		}

		public void AddCopy(UnitMeshInfo mesh)
		{
			if (atlas != null && atlas != mesh.atlas)
			{
				throw new ArgumentException();
			}
			atlas = mesh.atlas;

			for (int i = 0; i < mesh._Vertices.Count; ++i)
				_Vertices.Add(mesh._Vertices[i]);

			for (int i = 0; i < mesh._UV.Count; ++i)
				_UV.Add(mesh._UV[i]);
		}

		public void Copy(UnitMeshInfo mesh)
		{
			atlas = mesh.atlas;

			if (_Vertices.Count < mesh._Vertices.Count)
			{
				for (int i = 0; i < _Vertices.Count; ++i)
					_Vertices[i] = mesh._Vertices[i];

				for (int i = _Vertices.Count; i < mesh._Vertices.Count; ++i)
					_Vertices.Add(mesh._Vertices[i]);
			}
			else
			{
				for (int i = 0; i < mesh._Vertices.Count; ++i)
					_Vertices[i] = mesh._Vertices[i];

				for (int i = _Vertices.Count - 1; i >= mesh._Vertices.Count; --i)
					_Vertices.RemoveAt(i);
			}


			if (_UV.Count < mesh._UV.Count)
			{
				for (int i = 0; i < _UV.Count; ++i)
					_UV[i] = mesh._UV[i];

				for (int i = _UV.Count; i < mesh._UV.Count; ++i)
					_UV.Add(mesh._UV[i]);
			}
			else
			{
				for (int i = 0; i < mesh._UV.Count; ++i)
					_UV[i] = mesh._UV[i];

				for (int i = _UV.Count - 1; i >= mesh._UV.Count; --i)
					_UV.RemoveAt(i);
			}
		}

		public int VertCnt()
		{
			return _Vertices.Count;
		}

		public int UVCnt()
		{
			return _UV.Count;
		}

		public void AddVert(Vector3 v)
		{
			_Vertices.Add(v);
		}

		public void AddUV(Vector2 uv)
		{
			_UV.Add(uv);
		}

		public void SetVertLen(int l)
		{
			if (l > _Vertices.Count)
			{
				for (int i = _Vertices.Count; i < l; ++i)
				{
					_Vertices.Add(Vector3.zero);
				}
			}
			else
			{
				for (int i = _Vertices.Count - 1; i >= l; --i)
				{
					_Vertices.RemoveAt(i);
				}
			}
		}

		public void SetUVLen(int l)
		{
			if (l > _UV.Count)
			{
				for (int i = _UV.Count; i < l; ++i)
				{
					_UV.Add(Vector2.zero);
				}
			}
			else
			{
				for (int i = _UV.Count - 1; i >= l; --i)
				{
					_UV.RemoveAt(i);
				}
			}
		}

		public void SetVert(int index, Vector3 v)
		{
			if (index < _Vertices.Count)
			{
				_Vertices[index] = v;
			}
			else
				throw new System.IndexOutOfRangeException();
		}

		public void SetUV(int index, Vector2 v)
		{
			if (index < _UV.Count)
			{
				_UV[index] = v;
			}
			else
				throw new System.IndexOutOfRangeException();
		}

		public Vector3 GetVert(int index)
		{
			if (index < _Vertices.Count)
			{
				return _Vertices[index];
			}
			throw new System.IndexOutOfRangeException();
		}

		public Vector2 GetUV(int index)
		{
			if (index < _UV.Count)
			{
				return _UV[index];
			}
			throw new System.IndexOutOfRangeException();
		}

		public bool Equals(UnitMeshInfo other)
		{
			if (atlas != other.atlas || _Vertices.Count != other._Vertices.Count)
				return false;

			for (int i = 0; i < _Vertices.Count; i++)
				if (_Vertices[i] != other._Vertices[i])
					return false;

			for (int i = 0; i < _UV.Count; i++)
				if (_UV[i] != other._UV[i])
					return false;
			return true;
		}
	}

	public class MeshInfo
	{
		public List<string> _Tag = new List<string>();
		public List<Vector3> _Vertices = new List<Vector3>();
		public List<Vector2> _UV = new List<Vector2>();

		public void Clear()
		{
			_Tag.Clear();
			_Vertices.Clear();
			_UV.Clear();
		}

		public void Copy(MeshInfo other)
		{
			Clear();
			_Tag.AddRange(other._Tag);
			_Vertices.AddRange(other._Vertices);
			_UV.AddRange(other._UV);
		}

		//比较数据是否一样
		public bool Equals(MeshInfo _value)
		{
			if (_Tag.Count != _value._Tag.Count || _Vertices.Count != _value._Vertices.Count)
				return false;

			for (int i = 0; i < _Tag.Count; i++)
				if (_Tag[i] != _value._Tag[i])
					return false;
			for (int i = 0; i < _Vertices.Count; i++)
				if (_Vertices[i] != _value._Vertices[i])
					return false;
			return true;
		}
	}

}
