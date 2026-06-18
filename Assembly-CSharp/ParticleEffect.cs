using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Audio;
using UnityEngine;

public class ParticleEffect
{
	public static void Init()
	{
		ParticleEffect.RootT = GameObject.Find("/Particles").transform;
		Origin.Add(ParticleEffect.RootT, 0);
		ParticleEffect.entityParticles.Clear();
	}

	public static IEnumerator LoadResources()
	{
		ParticleEffect.loadedTs.Clear();
		LoadManager.AddressableAssetsRequestTask<GameObject> loadTask = LoadManager.LoadAssetsFromAddressables<GameObject>("particleeffects", delegate(string address)
		{
			if (!address.EndsWith(".prefab"))
			{
				return false;
			}
			StringSpan.CharSplitEnumerator splitEnumerator = address.GetSplitEnumerator('/', StringSplitOptions.None);
			if (!splitEnumerator.MoveNext())
			{
				Log.Error("Particle effect at " + address + " did not have expected folder name");
				return false;
			}
			if (!splitEnumerator.MoveNext())
			{
				Log.Error("Particle effect at " + address + " did not have expected name format");
				return false;
			}
			StringSpan stringSpan = splitEnumerator.Current;
			StringSpan stringSpan2 = ParticleEffect.prefix;
			return stringSpan.IndexOf(stringSpan2) == 0;
		}, null, false, false);
		while (!loadTask.IsDone)
		{
			yield return null;
		}
		List<GameObject> list = new List<GameObject>();
		loadTask.CollectResults(list);
		foreach (GameObject gameObject in list)
		{
			string text = gameObject.name;
			text = text.Substring(ParticleEffect.prefix.Length);
			int num = ParticleEffect.ToId(text);
			if (ParticleEffect.loadedTs.ContainsKey(num))
			{
				Log.Error("Particle Effect " + text + " already exists! Skipping it!");
			}
			else
			{
				ParticleEffect.loadedTs.Add(num, gameObject.transform);
			}
		}
		yield return null;
		yield break;
	}

	public static void LoadAsset(string _path)
	{
		DataLoader.DataPathIdentifier dataPathIdentifier = DataLoader.ParseDataPathIdentifier(_path);
		if (!dataPathIdentifier.IsBundle)
		{
			return;
		}
		int num = ParticleEffect.ToId(_path);
		if (ParticleEffect.loadedTs.ContainsKey(num))
		{
			Log.Warning("Particle Effect {0} already exists! Skipping it!", new object[] { _path });
			return;
		}
		Transform transform = DataLoader.LoadAsset<Transform>(dataPathIdentifier, false);
		ParticleEffect.loadedTs.Add(num, transform);
	}

	public ParticleEffect()
	{
	}

	public ParticleEffect(ParticleType _type, Vector3 _pos, float _lightValue, Color _color)
	{
		this.type = _type;
		this.pos = _pos;
		this.lightValue = _lightValue;
		this.color = _color;
	}

	public ParticleEffect(ParticleType _type, Vector3 _pos, float _lightValue, Color _color, Transform _parentTransform)
		: this(_type, _pos, _lightValue, _color)
	{
		this.SetParent(_parentTransform);
	}

	public ParticleEffect(ParticleType _type, Vector3 _pos, float _lightValue, Color _color, string _soundName, Transform _parentTransform)
		: this(_type, _pos, _lightValue, _color)
	{
		this.soundName = _soundName;
		this.SetParent(_parentTransform);
	}

	public ParticleEffect(string _name, Vector3 _pos, Quaternion _rot, float _lightValue, Color _color)
		: this(_name, _pos, _rot, _lightValue, _color, null, null, 1f, "")
	{
	}

	public ParticleEffect(string _name, Vector3 _pos, Quaternion _rot, float _lightValue, Color _color, string _soundName, Transform _parentTransform, float _volumeScale = 1f, string _additionalHitSound = "")
		: this(_name, _pos, _lightValue, _color, _soundName, _parentTransform, false, 1f, "")
	{
		this.rot = _rot;
		this.volumeScale = _volumeScale;
		this.additionalHitSoundName = _additionalHitSound;
	}

	public ParticleEffect(string _name, Vector3 _pos, float _lightValue, Color _color, string _soundName, Transform _parentTransform, bool _OLDCreateColliders, float _volumeScale = 1f, string _additionalHitSound = "")
		: this(ParticleType.Dynamic, _pos, _lightValue, _color)
	{
		this.ParticleId = ((_name != null) ? ParticleEffect.ToId(_name) : 0);
		this.debugName = _name;
		this.soundName = _soundName;
		this.volumeScale = _volumeScale;
		this.additionalHitSoundName = _additionalHitSound;
		this.SetParent(_parentTransform);
	}

	public ParticleEffect(string _name, Vector3 _pos, float _lightValue, Color _color, string _soundName, int _parentEntityId, ParticleEffect.Attachment _attachment)
		: this(ParticleType.Dynamic, _pos, _lightValue, _color)
	{
		this.ParticleId = ((_name != null) ? ParticleEffect.ToId(_name) : 0);
		this.debugName = _name;
		this.soundName = _soundName;
		this.SetParent(_parentEntityId, _attachment);
	}

	public static int ToId(string _name)
	{
		return _name.GetHashCode();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform GetParentTransform()
	{
		if (!this.parentTransform && this.parentEntityId != -1)
		{
			Entity entity;
			GameManager.Instance.World.Entities.dict.TryGetValue(this.parentEntityId, out entity);
			if (entity)
			{
				this.parentTransform = entity.transform;
				if (this.attachment != ParticleEffect.Attachment.None)
				{
					ParticleEffect.Attachment attachment = this.attachment;
					Transform transform;
					if (attachment != ParticleEffect.Attachment.Head)
					{
						if (attachment != ParticleEffect.Attachment.Pelvis)
						{
							transform = null;
						}
						else
						{
							transform = entity.emodel.GetPelvisTransform();
						}
					}
					else
					{
						transform = entity.emodel.GetHeadTransform();
					}
					if (transform)
					{
						this.parentTransform = transform;
					}
				}
			}
		}
		return this.parentTransform;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetParent(Transform _parentT)
	{
		this.parentTransform = null;
		this.attachment = ParticleEffect.Attachment.None;
		if (_parentT)
		{
			Entity component = _parentT.GetComponent<Entity>();
			this.parentEntityId = (component ? component.entityId : (-1));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetParent(int _entityId, ParticleEffect.Attachment _attachment = ParticleEffect.Attachment.None)
	{
		this.parentTransform = null;
		this.attachment = _attachment;
		this.parentEntityId = _entityId;
	}

	public static Transform GetDynamicTransform(int _particleId)
	{
		Transform transform;
		if (ParticleEffect.loadedTs.TryGetValue(_particleId, out transform))
		{
			return transform;
		}
		Log.Error(string.Format("Unknown particle effect: {0}", _particleId));
		return null;
	}

	public static bool IsAvailable(string _name)
	{
		return ParticleEffect.loadedTs.ContainsKey(ParticleEffect.ToId(_name));
	}

	public void Read(BinaryReader _br)
	{
		this.ParticleId = _br.ReadInt32();
		this.pos = StreamUtils.ReadVector3(_br);
		this.rot = StreamUtils.ReadQuaterion(_br);
		this.color = StreamUtils.ReadColor32(_br);
		this.soundName = _br.ReadString();
		if (this.soundName == string.Empty)
		{
			this.soundName = null;
		}
		this.additionalHitSoundName = _br.ReadString();
		if (this.additionalHitSoundName == string.Empty)
		{
			this.additionalHitSoundName = null;
		}
		this.volumeScale = _br.ReadSingle();
		this.parentEntityId = _br.ReadInt32();
		this.attachment = (ParticleEffect.Attachment)_br.ReadByte();
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write(this.ParticleId);
		StreamUtils.Write(_bw, this.pos);
		StreamUtils.Write(_bw, this.rot);
		StreamUtils.WriteColor32(_bw, this.color);
		_bw.Write((this.soundName != null) ? this.soundName : string.Empty);
		_bw.Write((this.additionalHitSoundName != null) ? this.additionalHitSoundName : string.Empty);
		_bw.Write(this.volumeScale);
		_bw.Write(this.parentEntityId);
		_bw.Write((byte)this.attachment);
	}

	public static Transform SpawnParticleEffect(ParticleEffect _pe, int _entityThatCausedIt, bool _forceCreation = false, bool _isWorldPos = false)
	{
		ParticleEffect.PlaySoundInServer(_entityThatCausedIt, _pe.pos, _pe.soundName, _pe.volumeScale);
		ParticleEffect.PlaySoundInServer(_entityThatCausedIt, _pe.pos, _pe.additionalHitSoundName, _pe.volumeScale);
		if (GameManager.IsDedicatedServer)
		{
			return null;
		}
		ParticleEffect.PlaySoundInClient(_entityThatCausedIt, _pe.pos, _pe.soundName, _pe.volumeScale);
		ParticleEffect.PlaySoundInClient(_entityThatCausedIt, _pe.pos, _pe.additionalHitSoundName, _pe.volumeScale);
		List<ParticleEffect.EntityData> list = null;
		Transform transform;
		if (_entityThatCausedIt != -1 && !_forceCreation)
		{
			int num = -1;
			if (ParticleEffect.entityParticles.TryGetValue(_entityThatCausedIt, out list))
			{
				int num2 = 0;
				for (int i = list.Count - 1; i >= 0; i--)
				{
					if (!list[i].t)
					{
						list.RemoveAt(i);
						num--;
					}
					else if (list[i].id == _pe.ParticleId && ++num2 >= 3)
					{
						num = i;
					}
				}
			}
			else
			{
				list = new List<ParticleEffect.EntityData>();
				ParticleEffect.entityParticles[_entityThatCausedIt] = list;
			}
			if (num >= 0 && _pe.attachment == ParticleEffect.Attachment.None)
			{
				ParticleEffect.EntityData entityData = list[num];
				list.RemoveAt(num);
				list.Add(entityData);
				transform = entityData.t;
				transform.position = _pe.pos - Origin.position;
				transform.rotation = _pe.rot;
				foreach (ParticleSystem particleSystem in transform.GetComponentsInChildren<ParticleSystem>())
				{
					particleSystem.Clear();
					particleSystem.Play();
				}
				TemporaryObject[] componentsInChildren2 = transform.GetComponentsInChildren<TemporaryObject>();
				for (int j = 0; j < componentsInChildren2.Length; j++)
				{
					componentsInChildren2[j].Restart();
				}
				return null;
			}
		}
		Transform dynamicTransform = ParticleEffect.GetDynamicTransform(_pe.ParticleId);
		if (!dynamicTransform)
		{
			return null;
		}
		if (_isWorldPos)
		{
			transform = Object.Instantiate<Transform>(dynamicTransform, _pe.pos - Origin.position, _pe.rot);
		}
		else
		{
			transform = Object.Instantiate<Transform>(dynamicTransform, _pe.pos, _pe.rot);
		}
		if (!transform)
		{
			return null;
		}
		if (list != null)
		{
			ParticleEffect.EntityData entityData2;
			entityData2.id = _pe.ParticleId;
			entityData2.t = transform;
			list.Add(entityData2);
		}
		foreach (Renderer renderer in transform.GetComponentsInChildren<Renderer>())
		{
			if (renderer.GetComponent<ParticleSystem>() == null)
			{
				renderer.material.SetColor("_Color", _pe.color);
			}
		}
		if (_pe.opqueTextureId != 0)
		{
			Material material = transform.GetComponent<ParticleSystem>().GetComponent<Renderer>().material;
			TextureAtlas textureAtlas = MeshDescription.meshes[0].textureAtlas;
			material.SetTexture("_MainTex", textureAtlas.diffuseTexture);
			material.SetTexture("_BumpMap", textureAtlas.normalTexture);
			material.SetFloat("_TexI", (float)textureAtlas.uvMapping[_pe.opqueTextureId].index);
			if (material.HasProperty("_OffsetUV"))
			{
				Rect uv = textureAtlas.uvMapping[_pe.opqueTextureId].uv;
				material.SetVector("_OffsetUV", new Vector4(uv.x, uv.y, uv.width, uv.height));
			}
		}
		Transform transform2 = _pe.GetParentTransform();
		if (transform2)
		{
			transform.SetParent(transform2, false);
			if (_pe.attachment != ParticleEffect.Attachment.None)
			{
				transform.localPosition = _pe.pos;
			}
			else
			{
				transform.localPosition = Vector3.zero;
			}
			transform.localRotation = Quaternion.identity;
		}
		else
		{
			transform.SetParent(ParticleEffect.RootT, false);
		}
		return transform;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool PlaySoundInServer(int entityId, Vector3 position, string soundName, float volumeScale)
	{
		if (!string.IsNullOrEmpty(soundName) && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			GameManager.Instance.World.aiDirector.OnSoundPlayedAtPosition(entityId, position, soundName, volumeScale);
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool PlaySoundInClient(int entityId, Vector3 position, string soundName, float volumeScale)
	{
		if (!string.IsNullOrEmpty(soundName))
		{
			Manager.Play(position, soundName, entityId, false, volumeScale);
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string prefix = "p_";

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cEntitySameParticleMax = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public ParticleType type;

	[PublicizedFrom(EAccessModifier.Private)]
	public ParticleEffect.Attachment attachment;

	public Vector3 pos;

	public Quaternion rot;

	public Color color;

	public float lightValue;

	public int ParticleId;

	public string soundName;

	public float volumeScale;

	public string additionalHitSoundName;

	public int opqueTextureId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int parentEntityId = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform parentTransform;

	public string debugName;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Transform RootT;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<int, Transform> loadedTs = new Dictionary<int, Transform>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<int, List<ParticleEffect.EntityData>> entityParticles = new Dictionary<int, List<ParticleEffect.EntityData>>();

	public enum Attachment : byte
	{
		None,
		Head,
		Pelvis
	}

	public struct EntityData
	{
		public int id;

		public Transform t;
	}
}
