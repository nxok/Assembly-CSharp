using System;
using System.IO;

public class AuthoredText
{
	public string Text
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public PlatformUserIdentifierAbs Author
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public AuthoredText()
	{
		this.Update(string.Empty, null);
	}

	public AuthoredText(string _text, PlatformUserIdentifierAbs _author)
	{
		this.Update(_text, _author);
	}

	public void Update(string _text, PlatformUserIdentifierAbs _author)
	{
		this.Author = _author;
		this.Text = _text;
	}

	public static AuthoredText FromStream(BinaryReader _reader)
	{
		if (_reader == null)
		{
			return null;
		}
		if (!_reader.ReadBoolean())
		{
			return null;
		}
		string text = _reader.ReadString();
		PlatformUserIdentifierAbs platformUserIdentifierAbs = PlatformUserIdentifierAbs.FromStream(_reader, false, false);
		AuthoredText authoredText = new AuthoredText();
		authoredText.Update(text, platformUserIdentifierAbs);
		return authoredText;
	}

	public static void ToStream(AuthoredText _instance, BinaryWriter _writer)
	{
		if (_writer == null)
		{
			return;
		}
		if (_instance == null)
		{
			_writer.Write(0);
			return;
		}
		_writer.Write(1);
		_writer.Write(_instance.Text);
		_instance.Author.ToStream(_writer, false);
	}

	public AuthoredText Clone()
	{
		return new AuthoredText(this.Text, this.Author);
	}
}
