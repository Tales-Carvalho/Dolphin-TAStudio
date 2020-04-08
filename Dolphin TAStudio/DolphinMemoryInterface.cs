using System;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

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
            public byte InputActive; // If 1, input from ControllerInput is read
            public byte LazyFixPleaseRemoveMeLater;

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
            m_accessor.Write(position, (byte)1);
        }

        public void PauseEmulation()
        {
            long position = (long)Marshal.OffsetOf(typeof(MInterface), "PlayPause");
            m_accessor.Write(position, (byte)2);
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

        public void SetReadOnly()
        {
            long position = (long)Marshal.OffsetOf(typeof(MInterface), "ReadOnly");
            m_accessor.Write(position, (byte)1);
        }

        public void SetReadWrite()
        {
            long position = (long)Marshal.OffsetOf(typeof(MInterface), "ReadOnly");
            m_accessor.Write(position, (byte)2);
        }

        public void ActivateInputs()
        {
            long position = (long)Marshal.OffsetOf(typeof(MInterface), "InputActive");
            m_accessor.Write(position, (byte)1);
        }

        public void DeactivateInputs()
        {
            long position = (long)Marshal.OffsetOf(typeof(MInterface), "InputActive");
            m_accessor.Write(position, (byte)0);
        }

        public ulong GetFrameCount()
        {
            long position = (long)Marshal.OffsetOf(typeof(MInterface), "FrameCount");
            return m_accessor.ReadUInt64(position);
        }

        public ulong GetInputFrameCount()
        {
            long position = (long)Marshal.OffsetOf(typeof(MInterface), "InputFrameCount");
            return m_accessor.ReadUInt64(position);
        }

        public void SetStateName(string t_name)
        {
            MInterface mi = ReadStructFromMemory();
            mi.StateName = t_name;
            WriteStructToMemory(mi);
        }

        public GCController GetInputs()
        {
            MInterface mi = ReadStructFromMemory();
            int size = Marshal.SizeOf(typeof(byte)) * 8;
            IntPtr p = Marshal.AllocHGlobal(size);

            Marshal.Copy(mi.ReadOnlyControllerInput, 0, p, size);

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

        public string GetInputsAsString()
        {
            GCController gc = GetInputs();
            string inputs = "";

            if ((gc.button & (ushort)PadButton.PAD_BUTTON_A) != 0)
            {
                inputs += "A ";
            }
            if ((gc.button & (ushort)PadButton.PAD_BUTTON_B) != 0)
            {
                inputs += "B ";
            }
            if ((gc.button & (ushort)PadButton.PAD_BUTTON_X) != 0)
            {
                inputs += "X ";
            }
            if ((gc.button & (ushort)PadButton.PAD_BUTTON_Y) != 0)
            {
                inputs += "Y ";
            }
            if ((gc.button & (ushort)PadButton.PAD_BUTTON_START) != 0)
            {
                inputs += "START ";
            }
            if ((gc.button & (ushort)PadButton.PAD_TRIGGER_L) != 0)
            {
                inputs += "L ";
            }
            if ((gc.button & (ushort)PadButton.PAD_TRIGGER_R) != 0)
            {
                inputs += "R ";
            }
            if ((gc.button & (ushort)PadButton.PAD_TRIGGER_Z) != 0)
            {
                inputs += "Z ";
            }
            if ((gc.button & (ushort)PadButton.PAD_BUTTON_DOWN) != 0)
            {
                inputs += "DOWN ";
            }
            if ((gc.button & (ushort)PadButton.PAD_BUTTON_LEFT) != 0)
            {
                inputs += "LEFT ";
            }
            if ((gc.button & (ushort)PadButton.PAD_BUTTON_RIGHT) != 0)
            {
                inputs += "RIGHT ";
            }
            if ((gc.button & (ushort)PadButton.PAD_BUTTON_UP) != 0)
            {
                inputs += "UP ";
            }

            inputs += "ANA: " + gc.stickX.ToString() + ", " + gc.stickY.ToString();

            return inputs;
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
