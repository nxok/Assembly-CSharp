using System;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace mumblelib
{
	[PublicizedFrom(EAccessModifier.Internal)]
	public class WindowsLinkFile : ILinkFile, IDisposable
	{
		public unsafe WindowsLinkFile()
		{
			this.memoryMappedFile = MemoryMappedFile.CreateOrOpen("MumbleLink", (long)Marshal.SizeOf<WindowsLinkFile.WindowsLinkMemory>());
			byte* ptr = null;
			this.memoryMappedFile.CreateViewAccessor().SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
			this.ptr = (WindowsLinkFile.WindowsLinkMemory*)ptr;
		}

		public unsafe uint UIVersion
		{
			get
			{
				return this.ptr->uiVersion;
			}
			set
			{
				this.ptr->uiVersion = value;
			}
		}

		public unsafe void Tick()
		{
			this.ptr->uiTick += 1U;
		}

		public unsafe Vector3 AvatarPosition
		{
			set
			{
				Util.SetVector3(&this.ptr->fAvatarPosition.FixedElementField, value);
			}
		}

		public unsafe Vector3 AvatarForward
		{
			set
			{
				Util.SetVector3(&this.ptr->fAvatarFront.FixedElementField, value);
			}
		}

		public unsafe Vector3 AvatarTop
		{
			set
			{
				Util.SetVector3(&this.ptr->fAvatarTop.FixedElementField, value);
			}
		}

		public unsafe string Name
		{
			set
			{
				Util.SetString<ushort>(&this.ptr->name.FixedElementField, value, 256, Encoding.Unicode);
			}
		}

		public unsafe Vector3 CameraPosition
		{
			set
			{
				Util.SetVector3(&this.ptr->fCameraPosition.FixedElementField, value);
			}
		}

		public unsafe Vector3 CameraForward
		{
			set
			{
				Util.SetVector3(&this.ptr->fCameraFront.FixedElementField, value);
			}
		}

		public unsafe Vector3 CameraTop
		{
			set
			{
				Util.SetVector3(&this.ptr->fCameraTop.FixedElementField, value);
			}
		}

		public unsafe string Identity
		{
			set
			{
				Util.SetString<ushort>(&this.ptr->identity.FixedElementField, value, 256, Encoding.Unicode);
			}
		}

		public unsafe string Context
		{
			set
			{
				Util.SetContext(&this.ptr->context.FixedElementField, &this.ptr->context_len, value);
			}
		}

		public unsafe string Description
		{
			set
			{
				Util.SetString<ushort>(&this.ptr->description.FixedElementField, value, 2048, Encoding.Unicode);
			}
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public ~WindowsLinkFile()
		{
			this.Dispose();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Dispose(bool _disposing)
		{
			Log.Out("[MumbleLF] Disposing shm");
			if (!this.disposed)
			{
				if (_disposing)
				{
					this.memoryMappedFile.Dispose();
				}
				this.disposed = true;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly MemoryMappedFile memoryMappedFile;

		[PublicizedFrom(EAccessModifier.Private)]
		public unsafe readonly WindowsLinkFile.WindowsLinkMemory* ptr;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool disposed;

		[PublicizedFrom(EAccessModifier.Private)]
		public struct WindowsLinkMemory
		{
			public uint uiVersion;

			public uint uiTick;

			[FixedBuffer(typeof(float), 3)]
			public WindowsLinkFile.WindowsLinkMemory.<fAvatarPosition>e__FixedBuffer fAvatarPosition;

			[FixedBuffer(typeof(float), 3)]
			public WindowsLinkFile.WindowsLinkMemory.<fAvatarFront>e__FixedBuffer fAvatarFront;

			[FixedBuffer(typeof(float), 3)]
			public WindowsLinkFile.WindowsLinkMemory.<fAvatarTop>e__FixedBuffer fAvatarTop;

			[FixedBuffer(typeof(ushort), 256)]
			public WindowsLinkFile.WindowsLinkMemory.<name>e__FixedBuffer name;

			[FixedBuffer(typeof(float), 3)]
			public WindowsLinkFile.WindowsLinkMemory.<fCameraPosition>e__FixedBuffer fCameraPosition;

			[FixedBuffer(typeof(float), 3)]
			public WindowsLinkFile.WindowsLinkMemory.<fCameraFront>e__FixedBuffer fCameraFront;

			[FixedBuffer(typeof(float), 3)]
			public WindowsLinkFile.WindowsLinkMemory.<fCameraTop>e__FixedBuffer fCameraTop;

			[FixedBuffer(typeof(ushort), 256)]
			public WindowsLinkFile.WindowsLinkMemory.<identity>e__FixedBuffer identity;

			public uint context_len;

			[FixedBuffer(typeof(byte), 256)]
			public WindowsLinkFile.WindowsLinkMemory.<context>e__FixedBuffer context;

			[FixedBuffer(typeof(ushort), 2048)]
			public WindowsLinkFile.WindowsLinkMemory.<description>e__FixedBuffer description;

			[CompilerGenerated]
			[UnsafeValueType]
			[StructLayout(LayoutKind.Sequential, Size = 256)]
			public struct <context>e__FixedBuffer
			{
				public byte FixedElementField;
			}

			[CompilerGenerated]
			[UnsafeValueType]
			[StructLayout(LayoutKind.Sequential, Size = 4096)]
			public struct <description>e__FixedBuffer
			{
				public ushort FixedElementField;
			}

			[CompilerGenerated]
			[UnsafeValueType]
			[StructLayout(LayoutKind.Sequential, Size = 12)]
			public struct <fAvatarFront>e__FixedBuffer
			{
				public float FixedElementField;
			}

			[CompilerGenerated]
			[UnsafeValueType]
			[StructLayout(LayoutKind.Sequential, Size = 12)]
			public struct <fAvatarPosition>e__FixedBuffer
			{
				public float FixedElementField;
			}

			[CompilerGenerated]
			[UnsafeValueType]
			[StructLayout(LayoutKind.Sequential, Size = 12)]
			public struct <fAvatarTop>e__FixedBuffer
			{
				public float FixedElementField;
			}

			[CompilerGenerated]
			[UnsafeValueType]
			[StructLayout(LayoutKind.Sequential, Size = 12)]
			public struct <fCameraFront>e__FixedBuffer
			{
				public float FixedElementField;
			}

			[CompilerGenerated]
			[UnsafeValueType]
			[StructLayout(LayoutKind.Sequential, Size = 12)]
			public struct <fCameraPosition>e__FixedBuffer
			{
				public float FixedElementField;
			}

			[CompilerGenerated]
			[UnsafeValueType]
			[StructLayout(LayoutKind.Sequential, Size = 12)]
			public struct <fCameraTop>e__FixedBuffer
			{
				public float FixedElementField;
			}

			[CompilerGenerated]
			[UnsafeValueType]
			[StructLayout(LayoutKind.Sequential, Size = 512)]
			public struct <identity>e__FixedBuffer
			{
				public ushort FixedElementField;
			}

			[CompilerGenerated]
			[UnsafeValueType]
			[StructLayout(LayoutKind.Sequential, Size = 512)]
			public struct <name>e__FixedBuffer
			{
				public ushort FixedElementField;
			}
		}
	}
}
