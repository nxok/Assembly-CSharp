using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace mumblelib
{
	[PublicizedFrom(EAccessModifier.Internal)]
	public class UnixLinkFile : ILinkFile, IDisposable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		[DllImport("libc")]
		public static extern int shm_open([MarshalAs(UnmanagedType.LPStr)] string name, int oflag, uint mode);

		[PublicizedFrom(EAccessModifier.Private)]
		[DllImport("libc")]
		public static extern uint getuid();

		[PublicizedFrom(EAccessModifier.Private)]
		[DllImport("libc")]
		public static extern int ftruncate(int fd, long length);

		[PublicizedFrom(EAccessModifier.Private)]
		[DllImport("libc")]
		public unsafe static extern void* mmap(void* addr, long length, int prot, int flags, int fd, long off);

		[PublicizedFrom(EAccessModifier.Private)]
		[DllImport("libc")]
		public unsafe static extern void* munmap(void* addr, long length);

		[PublicizedFrom(EAccessModifier.Private)]
		[DllImport("libc")]
		public static extern int close(int fd);

		public unsafe UnixLinkFile()
		{
			this.fd = UnixLinkFile.shm_open("/MumbleLink." + UnixLinkFile.getuid().ToString(), 66, 384U);
			if (this.fd < 0)
			{
				throw new Exception("[MumbleLF] Failed to open shm");
			}
			Log.Out("[MumbleLF] FD opened");
			int num = Marshal.SizeOf<UnixLinkFile.LinuxLinkMemory>();
			if (UnixLinkFile.ftruncate(this.fd, (long)num) != 0)
			{
				Log.Error("[MumbleLF] Failed resizing shm");
				return;
			}
			Log.Out("[MumbleLF] Resized");
			this.ptr = (UnixLinkFile.LinuxLinkMemory*)UnixLinkFile.mmap(null, (long)num, 3, 1, this.fd, 0L);
			Log.Out("[MumbleLF] MemMapped");
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
				Util.SetString<uint>(&this.ptr->name.FixedElementField, value, 256, Encoding.UTF32);
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
				Util.SetString<uint>(&this.ptr->identity.FixedElementField, value, 256, Encoding.UTF32);
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
				Util.SetString<uint>(&this.ptr->description.FixedElementField, value, 2048, Encoding.UTF32);
			}
		}

		public unsafe void Dispose()
		{
			if (!this.disposed)
			{
				UnixLinkFile.munmap((void*)this.ptr, (long)Marshal.SizeOf<UnixLinkFile.LinuxLinkMemory>());
				UnixLinkFile.close(this.fd);
				this.disposed = true;
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public ~UnixLinkFile()
		{
			this.Dispose();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const int O_RDONLY = 0;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int O_WRONLY = 1;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int O_RDWR = 2;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int O_CREAT = 64;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int O_EXCL = 128;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int O_TRUNC = 512;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int PROT_READ = 1;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int PROT_WRITE = 2;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int PROT_EXEC = 4;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int PROT_NONE = 0;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int MAP_SHARED = 1;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int MAP_PRIVATE = 2;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int MAP_SHARED_VALIDATE = 3;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool disposed;

		[PublicizedFrom(EAccessModifier.Private)]
		public unsafe readonly UnixLinkFile.LinuxLinkMemory* ptr;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly int fd;

		[PublicizedFrom(EAccessModifier.Private)]
		public struct LinuxLinkMemory
		{
			public uint uiVersion;

			public uint uiTick;

			[FixedBuffer(typeof(float), 3)]
			public UnixLinkFile.LinuxLinkMemory.<fAvatarPosition>e__FixedBuffer fAvatarPosition;

			[FixedBuffer(typeof(float), 3)]
			public UnixLinkFile.LinuxLinkMemory.<fAvatarFront>e__FixedBuffer fAvatarFront;

			[FixedBuffer(typeof(float), 3)]
			public UnixLinkFile.LinuxLinkMemory.<fAvatarTop>e__FixedBuffer fAvatarTop;

			[FixedBuffer(typeof(uint), 256)]
			public UnixLinkFile.LinuxLinkMemory.<name>e__FixedBuffer name;

			[FixedBuffer(typeof(float), 3)]
			public UnixLinkFile.LinuxLinkMemory.<fCameraPosition>e__FixedBuffer fCameraPosition;

			[FixedBuffer(typeof(float), 3)]
			public UnixLinkFile.LinuxLinkMemory.<fCameraFront>e__FixedBuffer fCameraFront;

			[FixedBuffer(typeof(float), 3)]
			public UnixLinkFile.LinuxLinkMemory.<fCameraTop>e__FixedBuffer fCameraTop;

			[FixedBuffer(typeof(uint), 256)]
			public UnixLinkFile.LinuxLinkMemory.<identity>e__FixedBuffer identity;

			public uint context_len;

			[FixedBuffer(typeof(byte), 256)]
			public UnixLinkFile.LinuxLinkMemory.<context>e__FixedBuffer context;

			[FixedBuffer(typeof(uint), 2048)]
			public UnixLinkFile.LinuxLinkMemory.<description>e__FixedBuffer description;

			[CompilerGenerated]
			[UnsafeValueType]
			[StructLayout(LayoutKind.Sequential, Size = 256)]
			public struct <context>e__FixedBuffer
			{
				public byte FixedElementField;
			}

			[CompilerGenerated]
			[UnsafeValueType]
			[StructLayout(LayoutKind.Sequential, Size = 8192)]
			public struct <description>e__FixedBuffer
			{
				public uint FixedElementField;
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
			[StructLayout(LayoutKind.Sequential, Size = 1024)]
			public struct <identity>e__FixedBuffer
			{
				public uint FixedElementField;
			}

			[CompilerGenerated]
			[UnsafeValueType]
			[StructLayout(LayoutKind.Sequential, Size = 1024)]
			public struct <name>e__FixedBuffer
			{
				public uint FixedElementField;
			}
		}
	}
}
