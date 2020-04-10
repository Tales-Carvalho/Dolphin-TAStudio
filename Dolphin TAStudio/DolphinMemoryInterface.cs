using System;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Threading;

namespace Dolphin_TAStudio
{
    class DolphinMemoryInterface
    {
        public enum PadButton
        {
            PAD_BUTTON_LEFT = 0x0001,
            PAD_BUTTON_RIGHT = 0x0002,
            PAD_BUTTON_DOWN = 0x0004,
            PAD_BUTTON_UP = 0x0008,
            PAD_TRIGGER_Z = 0x0010,
            PAD_TRIGGER_R = 0x0020,
            PAD_TRIGGER_L = 0x0040,
            PAD_BUTTON_A = 0x0100,
            PAD_BUTTON_B = 0x0200,
            PAD_BUTTON_X = 0x0400,
            PAD_BUTTON_Y = 0x0800,
            PAD_BUTTON_START = 0x1000,
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct GCController
        {
            public ushort button;
            public byte stickX;
            public byte stickY;
            public byte substickX;
            public byte substickY;
            public byte triggerLeft;
            public byte triggerRight;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct MInterface
        {
            public byte FrameAdvance; // Event for frame advance = 1
            public byte PlayPause; // Play = 1; Pause = 2
            public byte LoadState; // If 1, load StateName.sav
            public byte SaveState; // If 1, save StateName.sav
            public byte IsMoviePlayingBack; // If 1, Movie is playing back
            public byte ReadOnly; // ReadOnly = 1; ReadWrite = 2
            public byte InputActive; // If 1, Dolphin reads inputs from ControllerInput
            public byte NotImplemented; // Extra byte to align next variables in memory

            [MarshalAs(UnmanagedType.U8)]
            public ulong FrameCount;

            [MarshalAs(UnmanagedType.U8)]
            public ulong InputFrameCount;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] ControllerInput; // Array used to send inputs to Dolphin

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] ReadOnlyControllerInput; // Array used to read inputs from Dolphin

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string StateName; // Filename used by LoadState and SaveState functions
        };

        public event EventHandler FrameCountChanged = null;
        public event EventHandler InputFrameCountChanged = null;
        public event EventHandler MoviePlaybackStateChanged = null;

        private const string MEMORY_MAP_OBJECT = "DolphinMappingObject";

        private MemoryMappedFile m_file;
        private MemoryMappedViewAccessor m_accessor;
        private MInterface m_mInterface;

        private ulong _frameCount;
        private ulong _inputFrameCount;
        private byte _moviePlayingBack;

        public DolphinMemoryInterface()
        {
            m_file = MemoryMappedFile.OpenExisting(MEMORY_MAP_OBJECT);
            m_accessor = m_file.CreateViewAccessor(0, 256, MemoryMappedFileAccess.ReadWrite);
            m_mInterface = ReadStructFromMemory();
            _frameCount = m_mInterface.FrameCount;
            _inputFrameCount = m_mInterface.InputFrameCount;
            _moviePlayingBack = m_mInterface.IsMoviePlayingBack;
            
            new Timer(Update, null, 0, 10);
        }

        public void Update(object stateInfo)
        {
            m_mInterface = ReadStructFromMemory();
            if (m_mInterface.FrameCount != _frameCount && FrameCountChanged != null)
            {
                FrameCountChanged(this, EventArgs.Empty);
            }
            _frameCount = m_mInterface.FrameCount;
            if (m_mInterface.InputFrameCount != _inputFrameCount && InputFrameCountChanged != null)
            {
                InputFrameCountChanged(this, EventArgs.Empty);
            }
            _inputFrameCount = m_mInterface.InputFrameCount;
            if (m_mInterface.IsMoviePlayingBack != _moviePlayingBack && MoviePlaybackStateChanged != null)
            {
                MoviePlaybackStateChanged(this, EventArgs.Empty);
            }
            _moviePlayingBack = m_mInterface.IsMoviePlayingBack;
        }

        public void FrameAdvance() { WriteByteToMemory("FrameAdvance", 1); }
        public void PlayEmulation() { WriteByteToMemory("PlayPause", 1); }
        public void PauseEmulation() { WriteByteToMemory("PlayPause", 2); }
        public void SaveState() { WriteByteToMemory("SaveState", 1); }
        public void LoadState() { WriteByteToMemory("LoadState", 1); }
        public void SetReadOnly() { WriteByteToMemory("ReadOnly", 1); }
        public void SetReadWrite() { WriteByteToMemory("ReadOnly", 2); }
        public void ActivateInputs() { WriteByteToMemory("InputActive", 1); }
        public void DeactivateInputs() { WriteByteToMemory("InputActive", 0); }

        public bool IsMoviePlayingBack() { return (m_mInterface.IsMoviePlayingBack == 1); }
        public ulong GetFrameCount() { return m_mInterface.FrameCount; }
        public ulong GetInputFrameCount() { return m_mInterface.InputFrameCount; }

        public void SetStateName(string t_name)
        {
            //long position = (long)Marshal.OffsetOf(typeof(MInterface), "StateName");
            //byte[] data = new byte[sizeof(char) * 32];
            //
            //IntPtr p = Marshal.StringToHGlobalAuto(t_name);
            //Marshal.Copy(p, data, 0, data.Length);
            //
            //m_accessor.WriteArray<byte>(position, data, 0, data.Length);

            MInterface mi = ReadStructFromMemory();
            mi.StateName = t_name;
            WriteStructToMemory(mi);
        }

        public GCController GetInputs()
        {
            int size = Marshal.SizeOf(typeof(byte)) * 8;
            IntPtr p = Marshal.AllocHGlobal(size);

            Marshal.Copy(m_mInterface.ReadOnlyControllerInput, 0, p, size);

            return (GCController)Marshal.PtrToStructure(p, typeof(GCController));
        }

        public void SendInputs(GCController t_gcCont)
        {
            long position = (long)Marshal.OffsetOf(typeof(MInterface), "ControllerInput");
            int size = Marshal.SizeOf(typeof(byte)) * 8;
            byte[] data = new byte[size];
            IntPtr p = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(t_gcCont, p, false);
            Marshal.Copy(p, data, 0, data.Length);
            m_accessor.WriteArray<byte>(position, data, 0, data.Length);
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

        private void WriteByteToMemory(string t_member, byte t_value)
        {
            long position = (long)Marshal.OffsetOf(typeof(MInterface), t_member);
            m_accessor.Write(position, t_value);
        }
    }
}
