using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class CameraPerspectives
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetFullFilePath()
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return GameIO.GetSaveGameDir() + "/camerapositions.xml";
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			return GameIO.GetSaveGameLocalDir() + "/camerapositions.xml";
		}
		return null;
	}

	public CameraPerspectives(bool _load = true)
	{
		if (_load)
		{
			this.Load();
		}
	}

	public bool Load()
	{
		this.Perspectives.Clear();
		string fullFilePath = CameraPerspectives.GetFullFilePath();
		if (!SdFile.Exists(fullFilePath))
		{
			return false;
		}
		XmlDocument xmlDocument = new XmlDocument();
		try
		{
			xmlDocument.SdLoad(fullFilePath);
		}
		catch (XmlException ex)
		{
			Log.Error("Failed loading camera file: " + ex.Message);
			return false;
		}
		if (xmlDocument.DocumentElement == null)
		{
			Log.Warning("Camera file has no root XML element.");
			return false;
		}
		foreach (object obj in xmlDocument.DocumentElement.ChildNodes)
		{
			XmlNode xmlNode = (XmlNode)obj;
			if (xmlNode.NodeType == XmlNodeType.Element && !(xmlNode.Name != "position"))
			{
				CameraPerspectives.Perspective perspective = CameraPerspectives.Perspective.FromXml((XmlElement)xmlNode);
				if (perspective != null && !this.Perspectives.TryAdd(perspective.Name, perspective))
				{
					Log.Warning("Duplicate camera perspective entry '" + perspective.Name + "'");
				}
			}
		}
		return true;
	}

	public void Save()
	{
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.CreateXmlDeclaration();
		XmlElement xmlElement = xmlDocument.AddXmlElement("camerapositions");
		foreach (KeyValuePair<string, CameraPerspectives.Perspective> keyValuePair in this.Perspectives)
		{
			keyValuePair.Value.ToXml(xmlElement);
		}
		xmlDocument.SdSave(CameraPerspectives.GetFullFilePath());
	}

	public readonly SortedDictionary<string, CameraPerspectives.Perspective> Perspectives = new SortedDictionary<string, CameraPerspectives.Perspective>(StringComparer.OrdinalIgnoreCase);

	public class Perspective : IEquatable<CameraPerspectives.Perspective>
	{
		public Perspective(string _name, Vector3 _position, Vector3 _direction, string _comment)
		{
			this.Name = _name;
			this.Position = _position;
			this.Direction = _direction;
			this.Comment = _comment;
		}

		public Perspective(string _name, EntityPlayerLocal _player, string _comment = null)
		{
			this.Name = _name;
			if (_player.movementInput.bDetachedCameraMove)
			{
				this.Position = _player.cameraTransform.position - Constants.cDefaultCameraPlayerOffset;
				this.Direction = _player.cameraTransform.localEulerAngles;
				this.Direction.x = -this.Direction.x;
			}
			else
			{
				this.Position = _player.GetPosition();
				this.Direction = _player.rotation;
			}
			this.Comment = _comment;
		}

		public void ToPlayer(EntityPlayerLocal _player)
		{
			if (_player.movementInput.bDetachedCameraMove)
			{
				_player.cameraTransform.position = this.Position + Constants.cDefaultCameraPlayerOffset;
				Vector3 direction = this.Direction;
				direction.x = -direction.x;
				_player.cameraTransform.localEulerAngles = direction;
				return;
			}
			_player.TeleportToPosition(this.Position, false, new Vector3?(this.Direction));
		}

		public bool Equals(CameraPerspectives.Perspective _other)
		{
			return _other != null && (this == _other || (this.Position.Equals(_other.Position) && this.Direction.Equals(_other.Direction)));
		}

		public override bool Equals(object _obj)
		{
			return _obj != null && (this == _obj || (!(_obj.GetType() != base.GetType()) && this.Equals((CameraPerspectives.Perspective)_obj)));
		}

		public override int GetHashCode()
		{
			return (this.Position.GetHashCode() * 397) ^ this.Direction.GetHashCode();
		}

		public void ToXml(XmlElement _parent)
		{
			XmlElement xmlElement = _parent.AddXmlElement("position").SetAttrib("name", this.Name).SetAttrib("position", this.Position.ToString())
				.SetAttrib("direction", this.Direction.ToString());
			if (!string.IsNullOrEmpty(this.Comment))
			{
				xmlElement.SetAttrib("comment", this.Comment);
			}
		}

		public static CameraPerspectives.Perspective FromXml(XmlElement _lineItem)
		{
			if (!_lineItem.HasAttribute("name"))
			{
				Log.Warning("Ignoring camera-entry because of missing 'name' attribute: " + _lineItem.OuterXml);
				return null;
			}
			if (!_lineItem.HasAttribute("position"))
			{
				Log.Warning("Ignoring camera-entry because of missing 'position' attribute: " + _lineItem.OuterXml);
				return null;
			}
			if (!_lineItem.HasAttribute("direction"))
			{
				Log.Warning("Ignoring camera-entry because of missing 'direction' attribute: " + _lineItem.OuterXml);
				return null;
			}
			string attribute = _lineItem.GetAttribute("name");
			Vector3 vector = StringParsers.ParseVector3(_lineItem.GetAttribute("position"), 0, -1);
			Vector3 vector2 = StringParsers.ParseVector3(_lineItem.GetAttribute("direction"), 0, -1);
			string text = null;
			if (_lineItem.HasAttribute("comment"))
			{
				text = _lineItem.GetAttribute("comment");
			}
			return new CameraPerspectives.Perspective(attribute, vector, vector2, text);
		}

		public readonly string Name;

		public readonly Vector3 Position;

		public readonly Vector3 Direction;

		public readonly string Comment;
	}
}
