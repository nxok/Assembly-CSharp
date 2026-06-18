using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class AllyStore
{
	public event AllyStore.AllyChangeEvent OnAllyChangeEvent;

	public void AllyUpdateRequest(PlatformUserIdentifierAbs _target, bool _addAlly)
	{
		PlatformUserIdentifierAbs internalLocalUserIdentifier = PlatformManager.InternalLocalUserIdentifier;
		if (internalLocalUserIdentifier == null || _target == null)
		{
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageAllyRequest>().Setup(internalLocalUserIdentifier, _target, _addAlly), false);
			return;
		}
		this.ProcessAllyRequest(internalLocalUserIdentifier, _target, _addAlly);
	}

	public void ProcessAllyRequest(PlatformUserIdentifierAbs _source, PlatformUserIdentifierAbs _target, bool _addAlly)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return;
		}
		if (_source == null || _target == null)
		{
			return;
		}
		AllyStore.AllyStatus allyStatus;
		AllyStore.AllyEvent allyEvent;
		AllyStore.AllyEvent allyEvent2;
		AllyStore.ComputeTransition(this.GetStatus(_source, _target), _addAlly, out allyStatus, out allyEvent, out allyEvent2);
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageAllyResponse>().Setup(_source, _target, allyStatus, allyEvent, allyEvent2), false, -1, -1, -1, null, 192, false);
		this.AllyUpdateResponse(_source, _target, allyStatus, allyEvent, allyEvent2);
	}

	public void ApplyTransition(PlatformUserIdentifierAbs _source, PlatformUserIdentifierAbs _target, bool _addAlly)
	{
		if (_source == null || _target == null)
		{
			return;
		}
		AllyStore.AllyStatus allyStatus;
		AllyStore.AllyEvent allyEvent;
		AllyStore.AllyEvent allyEvent2;
		AllyStore.ComputeTransition(this.GetStatus(_source, _target), _addAlly, out allyStatus, out allyEvent, out allyEvent2);
		this.SetStatus(_source, _target, allyStatus);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ComputeTransition(AllyStore.AllyStatus _oldStatus, bool addAlly, out AllyStore.AllyStatus _newStatus, out AllyStore.AllyEvent _eventSource, out AllyStore.AllyEvent _eventTarget)
	{
		_newStatus = _oldStatus;
		_eventSource = AllyStore.AllyEvent.None;
		_eventTarget = AllyStore.AllyEvent.None;
		switch (_oldStatus)
		{
		case AllyStore.AllyStatus.NotAllied:
			if (addAlly)
			{
				_newStatus = AllyStore.AllyStatus.OutgoingInvite;
				_eventSource = AllyStore.AllyEvent.OutgoingSent;
				_eventTarget = AllyStore.AllyEvent.IncomingReceived;
				return;
			}
			break;
		case AllyStore.AllyStatus.Allies:
			if (!addAlly)
			{
				_newStatus = AllyStore.AllyStatus.NotAllied;
				_eventSource = AllyStore.AllyEvent.AllyRemoved;
				_eventTarget = AllyStore.AllyEvent.RemovedByAlly;
			}
			break;
		case AllyStore.AllyStatus.OutgoingInvite:
			if (!addAlly)
			{
				_newStatus = AllyStore.AllyStatus.NotAllied;
				_eventSource = AllyStore.AllyEvent.OutgoingCanceled;
				_eventTarget = AllyStore.AllyEvent.IncomingCanceled;
				return;
			}
			break;
		case AllyStore.AllyStatus.IncomingInvite:
			if (addAlly)
			{
				_newStatus = AllyStore.AllyStatus.Allies;
				_eventSource = AllyStore.AllyEvent.IncomingAccepted;
				_eventTarget = AllyStore.AllyEvent.OutgoingAccepted;
			}
			if (!addAlly)
			{
				_newStatus = AllyStore.AllyStatus.NotAllied;
				_eventSource = AllyStore.AllyEvent.IncomingDeclined;
				_eventTarget = AllyStore.AllyEvent.OutgoingDeclined;
				return;
			}
			break;
		default:
			return;
		}
	}

	public void AllyUpdateResponse(PlatformUserIdentifierAbs _source, PlatformUserIdentifierAbs _target, AllyStore.AllyStatus _newStatus, AllyStore.AllyEvent _allyEventSource, AllyStore.AllyEvent _allyEventTarget)
	{
		this.SetStatus(_source, _target, _newStatus);
		AllyStore.AllyChangeEvent onAllyChangeEvent = this.OnAllyChangeEvent;
		if (onAllyChangeEvent != null)
		{
			onAllyChangeEvent(_source, _target, _allyEventSource);
		}
		AllyStore.AllyChangeEvent onAllyChangeEvent2 = this.OnAllyChangeEvent;
		if (onAllyChangeEvent2 == null)
		{
			return;
		}
		onAllyChangeEvent2(_target, _source, _allyEventTarget);
	}

	public bool IsAlly(PlatformUserIdentifierAbs _source, PlatformUserIdentifierAbs _target)
	{
		return this.GetStatus(_source, _target) == AllyStore.AllyStatus.Allies;
	}

	public IEnumerable<PlatformUserIdentifierAbs> EnumerateAllies(PlatformUserIdentifierAbs _id)
	{
		Dictionary<PlatformUserIdentifierAbs, AllyStore.AllyStatus> dictionary;
		if (_id == null || !this.relationships.TryGetValue(_id, out dictionary))
		{
			yield break;
		}
		foreach (KeyValuePair<PlatformUserIdentifierAbs, AllyStore.AllyStatus> keyValuePair in dictionary)
		{
			if (keyValuePair.Value == AllyStore.AllyStatus.Allies)
			{
				yield return keyValuePair.Key;
			}
		}
		Dictionary<PlatformUserIdentifierAbs, AllyStore.AllyStatus>.Enumerator enumerator = default(Dictionary<PlatformUserIdentifierAbs, AllyStore.AllyStatus>.Enumerator);
		yield break;
		yield break;
	}

	public bool HasAllies(PlatformUserIdentifierAbs _id)
	{
		Dictionary<PlatformUserIdentifierAbs, AllyStore.AllyStatus> dictionary;
		if (_id == null || !this.relationships.TryGetValue(_id, out dictionary))
		{
			return false;
		}
		foreach (KeyValuePair<PlatformUserIdentifierAbs, AllyStore.AllyStatus> keyValuePair in dictionary)
		{
			if (keyValuePair.Value == AllyStore.AllyStatus.Allies)
			{
				return true;
			}
		}
		return false;
	}

	public AllyStore.AllyStatus GetStatus(PlatformUserIdentifierAbs _source, PlatformUserIdentifierAbs _target)
	{
		if (_source == null || _target == null)
		{
			return AllyStore.AllyStatus.NotAllied;
		}
		Dictionary<PlatformUserIdentifierAbs, AllyStore.AllyStatus> dictionary;
		if (!this.relationships.TryGetValue(_source, out dictionary))
		{
			return AllyStore.AllyStatus.NotAllied;
		}
		AllyStore.AllyStatus allyStatus;
		if (!dictionary.TryGetValue(_target, out allyStatus))
		{
			return AllyStore.AllyStatus.NotAllied;
		}
		return allyStatus;
	}

	public void SetStatus(PlatformUserIdentifierAbs _source, PlatformUserIdentifierAbs _target, AllyStore.AllyStatus _status)
	{
		if (_source == null || _target == null)
		{
			return;
		}
		if (_status == AllyStore.AllyStatus.NotAllied)
		{
			this.ClearStatus(_source, _target);
			return;
		}
		if (!this.relationships.ContainsKey(_source))
		{
			this.relationships.Add(_source, new Dictionary<PlatformUserIdentifierAbs, AllyStore.AllyStatus>());
		}
		if (!this.relationships.ContainsKey(_target))
		{
			this.relationships.Add(_target, new Dictionary<PlatformUserIdentifierAbs, AllyStore.AllyStatus>());
		}
		if (_status == AllyStore.AllyStatus.Allies)
		{
			this.relationships[_source][_target] = AllyStore.AllyStatus.Allies;
			this.relationships[_target][_source] = AllyStore.AllyStatus.Allies;
			return;
		}
		if (_status == AllyStore.AllyStatus.OutgoingInvite)
		{
			this.relationships[_source][_target] = AllyStore.AllyStatus.OutgoingInvite;
			this.relationships[_target][_source] = AllyStore.AllyStatus.IncomingInvite;
			return;
		}
		if (_status == AllyStore.AllyStatus.IncomingInvite)
		{
			this.relationships[_source][_target] = AllyStore.AllyStatus.IncomingInvite;
			this.relationships[_target][_source] = AllyStore.AllyStatus.OutgoingInvite;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClearStatus(PlatformUserIdentifierAbs _source, PlatformUserIdentifierAbs _target)
	{
		Dictionary<PlatformUserIdentifierAbs, AllyStore.AllyStatus> dictionary;
		if (this.relationships.TryGetValue(_source, out dictionary))
		{
			dictionary.Remove(_target);
		}
		Dictionary<PlatformUserIdentifierAbs, AllyStore.AllyStatus> dictionary2;
		if (this.relationships.TryGetValue(_target, out dictionary2))
		{
			dictionary2.Remove(_source);
		}
	}

	public void ClearAll()
	{
		this.relationships.Clear();
	}

	public void CopyFrom(AllyStore _other)
	{
		this.relationships.Clear();
		if (_other == null)
		{
			return;
		}
		foreach (KeyValuePair<PlatformUserIdentifierAbs, Dictionary<PlatformUserIdentifierAbs, AllyStore.AllyStatus>> keyValuePair in _other.relationships)
		{
			foreach (KeyValuePair<PlatformUserIdentifierAbs, AllyStore.AllyStatus> keyValuePair2 in keyValuePair.Value)
			{
				this.SetStatus(keyValuePair.Key, keyValuePair2.Key, keyValuePair2.Value);
			}
		}
	}

	public void ReadXml(XmlElement _alliesElement, int _readVersion)
	{
		if (_readVersion == 0)
		{
			return;
		}
		foreach (object obj in _alliesElement.ChildNodes)
		{
			XmlElement xmlElement = ((XmlNode)obj) as XmlElement;
			if (xmlElement != null && xmlElement.Name == "ally")
			{
				PlatformUserIdentifierAbs platformUserIdentifierAbs = PlatformUserIdentifierAbs.FromXml(xmlElement, true, "a");
				PlatformUserIdentifierAbs platformUserIdentifierAbs2 = PlatformUserIdentifierAbs.FromXml(xmlElement, true, "b");
				if (platformUserIdentifierAbs != null && platformUserIdentifierAbs2 != null)
				{
					if (xmlElement.GetAttribute("status") == "allies")
					{
						this.SetStatus(platformUserIdentifierAbs, platformUserIdentifierAbs2, AllyStore.AllyStatus.Allies);
					}
					else if (xmlElement.GetAttribute("status") == "pending")
					{
						this.SetStatus(platformUserIdentifierAbs, platformUserIdentifierAbs2, AllyStore.AllyStatus.OutgoingInvite);
					}
				}
			}
		}
	}

	public void WriteXml(XmlElement _root)
	{
		bool flag = false;
		foreach (KeyValuePair<PlatformUserIdentifierAbs, Dictionary<PlatformUserIdentifierAbs, AllyStore.AllyStatus>> keyValuePair in this.relationships)
		{
			if (keyValuePair.Value.Count > 0)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return;
		}
		XmlElement xmlElement = _root.AddXmlElement("allies");
		foreach (KeyValuePair<PlatformUserIdentifierAbs, Dictionary<PlatformUserIdentifierAbs, AllyStore.AllyStatus>> keyValuePair2 in this.relationships)
		{
			foreach (KeyValuePair<PlatformUserIdentifierAbs, AllyStore.AllyStatus> keyValuePair3 in keyValuePair2.Value)
			{
				PlatformUserIdentifierAbs key = keyValuePair2.Key;
				PlatformUserIdentifierAbs key2 = keyValuePair3.Key;
				AllyStore.AllyStatus value = keyValuePair3.Value;
				if (value != AllyStore.AllyStatus.NotAllied && value != AllyStore.AllyStatus.IncomingInvite && (value != AllyStore.AllyStatus.Allies || string.CompareOrdinal(key.CombinedString, key2.CombinedString) <= 0))
				{
					XmlElement xmlElement2 = xmlElement.AddXmlElement("ally");
					key.ToXml(xmlElement2, "a");
					key2.ToXml(xmlElement2, "b");
					if (value == AllyStore.AllyStatus.Allies)
					{
						xmlElement2.SetAttribute("status", "allies");
					}
					else
					{
						xmlElement2.SetAttribute("status", "pending");
					}
				}
			}
		}
	}

	public void Read(BinaryReader _br)
	{
		this.relationships.Clear();
		int num = _br.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			PlatformUserIdentifierAbs platformUserIdentifierAbs = PlatformUserIdentifierAbs.FromStream(_br, false, false);
			PlatformUserIdentifierAbs platformUserIdentifierAbs2 = PlatformUserIdentifierAbs.FromStream(_br, false, false);
			AllyStore.AllyStatus allyStatus = (AllyStore.AllyStatus)_br.ReadByte();
			this.SetStatus(platformUserIdentifierAbs, platformUserIdentifierAbs2, allyStatus);
		}
	}

	public void Write(BinaryWriter _bw)
	{
		int num = 0;
		foreach (KeyValuePair<PlatformUserIdentifierAbs, Dictionary<PlatformUserIdentifierAbs, AllyStore.AllyStatus>> keyValuePair in this.relationships)
		{
			foreach (KeyValuePair<PlatformUserIdentifierAbs, AllyStore.AllyStatus> keyValuePair2 in keyValuePair.Value)
			{
				num++;
			}
		}
		num /= 2;
		_bw.Write(num);
		foreach (KeyValuePair<PlatformUserIdentifierAbs, Dictionary<PlatformUserIdentifierAbs, AllyStore.AllyStatus>> keyValuePair3 in this.relationships)
		{
			foreach (KeyValuePair<PlatformUserIdentifierAbs, AllyStore.AllyStatus> keyValuePair4 in keyValuePair3.Value)
			{
				if (string.CompareOrdinal(keyValuePair3.Key.CombinedString, keyValuePair4.Key.CombinedString) <= 0)
				{
					keyValuePair3.Key.ToStream(_bw, false);
					keyValuePair4.Key.ToStream(_bw, false);
					_bw.Write((byte)keyValuePair4.Value);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<PlatformUserIdentifierAbs, Dictionary<PlatformUserIdentifierAbs, AllyStore.AllyStatus>> relationships = new Dictionary<PlatformUserIdentifierAbs, Dictionary<PlatformUserIdentifierAbs, AllyStore.AllyStatus>>();

	public enum AllyStatus : byte
	{
		NotAllied,
		Allies,
		OutgoingInvite,
		IncomingInvite
	}

	public enum AllyEvent : byte
	{
		None,
		OutgoingSent,
		OutgoingCanceled,
		IncomingAccepted,
		IncomingDeclined,
		AllyRemoved,
		OutgoingAccepted,
		OutgoingDeclined,
		IncomingReceived,
		IncomingCanceled,
		RemovedByAlly
	}

	public delegate void AllyChangeEvent(PlatformUserIdentifierAbs _source, PlatformUserIdentifierAbs _target, AllyStore.AllyEvent _allyEventSource);
}
