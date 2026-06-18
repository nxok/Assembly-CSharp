using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class WaveCleanUp : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		base.StartCoroutine(this.FormatHeader());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator FormatHeader()
	{
		yield return new WaitUntil(() => this.FilePath != null);
		UnityWebRequest getAudioFile = UnityWebRequestMultimedia.GetAudioClip("file://" + this.FilePath, 20);
		getAudioFile.disposeDownloadHandlerOnDispose = true;
		yield return getAudioFile.SendWebRequest();
		if (getAudioFile.result == 2)
		{
			Debug.Log(getAudioFile.error);
		}
		else
		{
			AudioClip content = DownloadHandlerAudioClip.GetContent(getAudioFile);
			float[] array = new float[content.samples * content.channels];
			content.GetData(array, 0);
			byte[] array2 = WaveCleanUp.PCMDataToByteArray(array);
			int num = content.samples * (int)WaveCleanUp.Channels * (int)WaveCleanUp.BitsPerSample / 8;
			int num2 = 36 + num;
			using (Stream stream = SdFile.Open(this.FilePath, FileMode.Create, FileAccess.Write, FileShare.Read))
			{
				stream.Write(WaveCleanUp.ChunkID, 0, 4);
				stream.Write(BitConverter.GetBytes(num2), 0, 4);
				stream.Write(WaveCleanUp.Format, 0, 4);
				stream.Write(WaveCleanUp.Subchunk1ID, 0, 4);
				stream.Write(BitConverter.GetBytes(WaveCleanUp.Subchunk1Size), 0, 4);
				stream.Write(BitConverter.GetBytes(WaveCleanUp.AudioFormat), 0, 2);
				stream.Write(BitConverter.GetBytes(WaveCleanUp.Channels), 0, 2);
				stream.Write(BitConverter.GetBytes(WaveCleanUp.SampleRate), 0, 4);
				stream.Write(BitConverter.GetBytes(WaveCleanUp.ByteRate), 0, 4);
				stream.Write(BitConverter.GetBytes(WaveCleanUp.BlockAlign), 0, 2);
				stream.Write(BitConverter.GetBytes(WaveCleanUp.BitsPerSample), 0, 2);
				stream.Write(WaveCleanUp.Subchunk2ID, 0, 4);
				stream.Write(BitConverter.GetBytes(num), 0, 4);
				stream.Write(array2, 0, array2.Length);
			}
		}
		getAudioFile.Dispose();
		Log.Out("Cleaned up: " + this.FilePath);
		Object.Destroy(base.gameObject);
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static byte[] PCMDataToByteArray(float[] _pcmData)
	{
		byte[] array = new byte[2 * _pcmData.Length];
		for (int i = 0; i < _pcmData.Length; i++)
		{
			byte[] bytes = BitConverter.GetBytes((short)(_pcmData[i] * 32767f));
			for (int j = 0; j < 2; j++)
			{
				array[2 * i + j] = bytes[j];
			}
		}
		return array;
	}

	public static GameObject Create()
	{
		if (WaveCleanUp.PrefabWaveCleanUp == null)
		{
			WaveCleanUp.PrefabWaveCleanUp = Resources.Load<GameObject>("Prefabs/prefabDMSWaveCleanup");
		}
		GameObject gameObject = Object.Instantiate<GameObject>(WaveCleanUp.PrefabWaveCleanUp);
		gameObject.name = "WaveCleanUp";
		return gameObject;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static GameObject PrefabWaveCleanUp;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static byte[] ChunkID = Encoding.ASCII.GetBytes("RIFF");

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static byte[] Format = Encoding.ASCII.GetBytes("WAVE");

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static byte[] Subchunk1ID = Encoding.ASCII.GetBytes("fmt ");

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static byte[] Subchunk2ID = Encoding.ASCII.GetBytes("data");

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static short AudioFormat = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static int SampleRate = 44100;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static short Channels = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static short BitsPerSample = 16;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static int ByteRate = WaveCleanUp.SampleRate * (int)WaveCleanUp.Channels * (int)WaveCleanUp.BitsPerSample / 8;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static short BlockAlign = WaveCleanUp.Channels * WaveCleanUp.BitsPerSample / 8;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static int Subchunk1Size = 16;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool IsFinished;

	public string FilePath;
}
