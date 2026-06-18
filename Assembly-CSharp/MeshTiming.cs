using System;

public class MeshTiming
{
	public string Details()
	{
		double num = this.CopyVerts + this.CopyUv + this.CopyUv2 + this.CopyUv3 + this.CopyUv4 + this.CopyColours + this.CopyTriangles + this.CopyNormals + this.CopyTangents + this.UploadMesh + this.NormalRecalc;
		return string.Format("\r\nCopyVerts: {0}\r\nCopyUv:{1}\r\nCopyUv2:{2}\r\nCopyUv3:{3}\r\nCopyUv4:{4}\r\nCopyColours:{5}\r\nCopyTriangles:{6}\r\nCopyNormals:{7}\r\nCopyTangents:{8}\r\nUploadMesh:{9}\r\nNormalRecalc:{10}\r\nTotal: {11}\r\n", new object[]
		{
			this.CopyVerts, this.CopyUv, this.CopyUv2, this.CopyUv3, this.CopyUv4, this.CopyColours, this.CopyTriangles, this.CopyNormals, this.CopyTangents, this.UploadMesh,
			this.NormalRecalc, num
		});
	}

	public double time
	{
		get
		{
			return this.GetTime();
		}
	}

	public double GetTime()
	{
		return (DateTime.Now - this.Start).TotalMilliseconds;
	}

	public void Reset()
	{
		this.Start = DateTime.Now;
	}

	public double CopyVerts;

	public double CopyUv;

	public double CopyUv2;

	public double CopyUv3;

	public double CopyUv4;

	public double CopyColours;

	public double CopyTriangles;

	public double CopyNormals;

	public double CopyTangents;

	public double UploadMesh;

	public double NormalRecalc;

	public DateTime Start;
}
