using System;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace Dolphin_TAStudio
{
    class DolphinMemoryInterface
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct MInterface
        {
            public byte FrameAdvance; // Event for frame advance = 1
            public byte PlayPause; // Play = 1; Pause = 2
            public byte LoadState; // If 1, load StateName.sav
            public byte SaveState; // If 1, save StateName.sav
            public byte IsMoviePlayingBack; // If 1, Movie is playing back
            public byte ReadOnly; // ReadOnly = 1; ReadWrite = 2
            public byte InputActive; // If 1, input from ControllerInput is read

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] ControllerInput; // Array used to send inputs to Dolphin

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] ReadOnlyControllerInput; // Array used to read inputs from Dolphin

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
            public string StateName; // Filename used by LoadState and SaveState functions
        };

        private const string MEMORY_MAP_OBJECT = "DolphinMappingObject";

        private MemoryMappedFile m_file;
        private MemoryMappedViewAccessor m_accessor;

        public DolphinMemoryInterface()
        {
            m_file = MemoryMappedFile.OpenExisting(MEMORY_MAP_OBJECT);
            m_accessor = m_file.CreateViewAccessor(0, 256, MemoryMappedFileAccess.ReadWrite);
        }

        public void FrameAdvance()
        {
            long position = (long)Marshal.OffsetOf(typeof(MInterface), "FrameAdvance");
            m_accessor.Write(position, (byte)1);
        }

        public void PlayEmulation()
        {
            long position = (long)Marshal.OffsetOf(typeof(MInterface), "PlayPause");
            m_accessor.Write(position, (byte)2);
        }

        public void PauseEmulation()
        {
            long position = (long)Marshal.OffsetOf(typeof(MInterface), "PlayPause");
            m_accessor.Write(position, (byte)1);
        }

        public void SaveState()
        {
            long position = (long)Marshal.OffsetOf(typeof(MInterface), "SaveState");
            m_accessor.Write(position, (byte)1);
        }

        public void LoadState()
        {
            long position = (long)Marshal.OffsetOf(typeof(MInterface), "LoadState");
            m_accessor.Write(position, (byte)1);
        }

        public void SetStateName(string t_name)
        {
            MInterface mi = ReadStructFromMemory();
            mi.StateName = t_name;
            WriteStructToMemory(mi);
        }

        private MInterface ReadStructFromMemory()
        {
            int size = Marshal.SizeOf(typeof(MInterface));
            byte[] data = new byte[size];
            IntPtr p = Marshal.AllocHGlobal(size);

            m_accessor.ReadArray<byte>(0, data, 0, size);
            Marshal.Copy(data, 0, p, size);
            return (MInterface)Marshal.PtrToStructure(p, typeof(MInterface));
        }

        private void WriteStructToMemory(MInterface t_mInterface)
        {
            int size = Marshal.SizeOf(typeof(MInterface));
            byte[] data = new byte[size];
            IntPtr p = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(t_mInterface, p, false);
            Marshal.Copy(p, data, 0, data.Length);
            m_accessor.WriteArray<byte>(0, data, 0, data.Length);
        }
    }
}
