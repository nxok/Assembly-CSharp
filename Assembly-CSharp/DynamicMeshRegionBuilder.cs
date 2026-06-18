using System;
using System.Threading;

public class DynamicMeshRegionBuilder
{
	public static World world
	{
		get
		{
			return GameManager.Instance.World;
		}
	}

	public bool AddNewItem(DynamicMeshRegion region)
	{
		if (this.Status != DynamicMeshBuilderStatus.Ready)
		{
			Log.Warning("Builder thread tried to start when not ready. Current Status: " + this.Status.ToString());
			return false;
		}
		this.Region = region;
		this.Status = DynamicMeshBuilderStatus.StartingExport;
		return true;
	}

	public void RequestStop(bool forceStop = false)
	{
		this.StopRequested = true;
		if (forceStop)
		{
			try
			{
				Thread thread = this.thread;
				if (thread != null)
				{
					thread.Abort();
				}
			}
			catch (Exception)
			{
			}
			this.Status = DynamicMeshBuilderStatus.Stopped;
		}
	}

	public void StartThread()
	{
		this.thread = new Thread(delegate
		{
			try
			{
				while (!this.StopRequested)
				{
					if (GameManager.Instance == null)
					{
						return;
					}
					if (GameManager.Instance.World == null)
					{
						return;
					}
					if (this.Status == DynamicMeshBuilderStatus.Ready || this.Status == DynamicMeshBuilderStatus.Complete)
					{
						Thread.Sleep(100);
					}
					else
					{
						if (this.Status != DynamicMeshBuilderStatus.StartingExport)
						{
							Log.Error("Builder thread and wrong state: " + this.Status.ToString());
							this.Status = DynamicMeshBuilderStatus.Error;
							return;
						}
						throw new NotImplementedException("No build method");
					}
				}
			}
			catch (Exception ex)
			{
				this.Error = "Builder error: " + ex.Message;
				Log.Error(this.Error);
			}
			this.Status = DynamicMeshBuilderStatus.Stopped;
		});
		this.thread.Start();
	}

	public string Error;

	public bool StopRequested;

	public DynamicMeshBuilderStatus Status;

	public ExportMeshResult Result = ExportMeshResult.Missing;

	public DynamicMeshRegion Region;

	[PublicizedFrom(EAccessModifier.Private)]
	public Thread thread;
}
