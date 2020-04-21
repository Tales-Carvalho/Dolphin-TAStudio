using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Dolphin_TAStudio
{
    /// <summary>
    /// Creates and manages the Shared Memory communication with a Dolphin instance.
    /// Class written by Tales Carvalho (THC98).
    /// </summary>
    class DolphinMemoryInterface
    {
        /// <summary>
        /// Name of Memory Map Object defined in Dolphin.
        /// </summary>
        private const string MEMORY_MAP_OBJECT = "DolphinMappingObject";

        /// <summary>
        /// Size of Memory Map Object defined in Dolphin, in bytes.
        /// </summary>
        private const long MEMORY_MAP_SIZE = 256;

        /// <summary>
        /// Timer interval for polling the update, in miliseconds.
        /// </summary>
        private const int TIMER_INTERVAL_MS = 10;

        /// <summary>
        /// Masks each button press in GCController.button.
        /// </summary>
        public enum PadButton
        {
            PAD_BUTTON_LEFT = 0x0001,
            PAD_BUTTON_RIGHT = 0x0002,
            PAD_BUTTON_DOWN = 0x0004,
            PAD_BUTTON_UP = 0x0008,
            PAD_BUTTON_Z = 0x0010,
            PAD_TRIGGER_R = 0x0020,
            PAD_TRIGGER_L = 0x0040,
            PAD_BUTTON_A = 0x0100,
            PAD_BUTTON_B = 0x0200,
            PAD_BUTTON_X = 0x0400,
            PAD_BUTTON_Y = 0x0800,
            PAD_BUTTON_START = 0x1000,
        };

        /// <summary>
        /// Represents a GameCube Controller input.
        /// </summary>
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

        /// <summary>
        /// Internal struct used to communicate to Dolphin's Shared Memory Interface.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct MInterface
        {
            public byte FrameAdvance; // Event for frame advance = 1
            public byte PlayPause; // Play = 1; Pause = 2
            public byte LoadState; // If 1, load StateName.sav
            public byte SaveState; // If 1, save StateName.sav
            public byte IsMoviePlayingBack; // If 1, Movie is playing back
            public byte ReadOnly; // ReadOnly = 1; ReadWrite = 2
            public byte InputActive; // If 1, Dolphin reads inputs from ControllerInputQueue
            public byte InputsInQueue; // Count of elements in ControllerInputQueue

            [MarshalAs(UnmanagedType.U8)]
            public ulong FrameCount; // Dolphin's frame count (VI) (read-only)

            [MarshalAs(UnmanagedType.U8)]
            public ulong InputFrameCount; // Dolphin's input frame count (read-only)

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 80)]
            public byte[] ControllerInputQueue; // Array used to send inputs to Dolphin

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] ReadOnlyControllerInput; // Array used to read inputs from Dolphin

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string StateName; // Filename used by LoadState and SaveState functions
        };

        /// <summary>
        /// Event thrown when Dolphin's Frame Count (VI) changes.
        /// </summary>
        public event EventHandler FrameCountChanged = null;

        /// <summary>
        /// Event thrown when Dolphin's Input Frame Count (Input) changes.
        /// </summary>
        public event EventHandler InputFrameCountChanged = null;

        /// <summary>
        /// Event thrown when Dolphin's movie playback state (Read-Only or Read-and-Write) is toggled.
        /// </summary>
        public event EventHandler MoviePlaybackStateChanged = null;

        private MemoryMappedFile m_file;
        private MemoryMappedViewAccessor m_accessor;
        
        private MInterface m_mInterface;
        private Queue<GCController> m_inputsToSend;

        private ulong _frameCount;
        private ulong _inputFrameCount;
        private byte _moviePlayingBack;

        public DolphinMemoryInterface()
        {
            // Initializing Shared Memory Communication
            m_file = MemoryMappedFile.OpenExisting(MEMORY_MAP_OBJECT);
            m_accessor = m_file.CreateViewAccessor(0, MEMORY_MAP_SIZE, MemoryMappedFileAccess.ReadWrite);

            // Initializing internal variables that handle event activation
            m_mInterface = ReadStructFromMemory();
            _frameCount = m_mInterface.FrameCount;
            _inputFrameCount = m_mInterface.InputFrameCount;
            _moviePlayingBack = m_mInterface.IsMoviePlayingBack;

            // Initializing queue of inputs
            m_inputsToSend = new Queue<GCController>();

            // Initializing timer that calls Update in every inverval of TIMER_INTERVAL_MS
            Timer m_timer = new Timer();
            m_timer.Tick += new EventHandler(Update);
            m_timer.Interval = TIMER_INTERVAL_MS;
            m_timer.Start();
        }

        /// <summary>
        /// Triggers a frame step event in Dolphin.
        /// </summary>
        public void FrameAdvance() { WriteByteToMemory("FrameAdvance", 1); }
        /// <summary>
        /// Makes Dolphin play/resume the current emulation.
        /// </summary>
        public void PlayEmulation() { WriteByteToMemory("PlayPause", 1); }
        /// <summary>
        /// Makes Dolphin pause the current emulation.
        /// </summary>
        public void PauseEmulation() { WriteByteToMemory("PlayPause", 2); }
        /// <summary>
        /// Makes Dolphin save a state with the name specified by the SetStateName method.
        /// </summary>
        public void SaveState() { WriteByteToMemory("SaveState", 1); }
        /// <summary>
        /// Makes Dolphin load a state with the name specified by the SetStateName method.
        /// </summary>
        public void LoadState() { WriteByteToMemory("LoadState", 1); }
        /// <summary>
        /// Sets Dolphin's movie playback state to Read-Only.
        /// </summary>
        public void SetReadOnly() { WriteByteToMemory("ReadOnly", 1); }
        /// <summary>
        /// Sets Dolphin's movie playback state to Read-and-Write.
        /// </summary>
        public void SetReadWrite() { WriteByteToMemory("ReadOnly", 2); }
        /// <summary>
        /// Makes Dolphin start reading inputs from the input queue.
        /// </summary>
        public void ActivateInputs() { WriteByteToMemory("InputActive", 1); }
        /// <summary>
        /// Makes Dolphin stop reading inputs from the input queue.
        /// </summary>
        public void DeactivateInputs() { WriteByteToMemory("InputActive", 0); }

        /// <summary>
        /// Returns the state of Dolphin's movie playback.
        /// </summary>
        /// <returns>True when a movie is playing back in read-only mode, false otherwise.</returns>
        public bool IsMoviePlayingBack() { return (m_mInterface.IsMoviePlayingBack == 1); }
        /// <summary>
        /// Returns Dolphin's frame count (VI). Note: this only works when recording/playing back a DTM.
        /// </summary>
        /// <returns>Dolphin's frame count as a ulong variable.</returns>
        public ulong GetFrameCount() { return m_mInterface.FrameCount; }
        /// <summary>
        /// Returns Dolphin's input frame count. Note: this only works when recording/playing back a DTM.
        /// </summary>
        /// <returns>Dolphin's input frame count as a ulong variable.</returns>
        public ulong GetInputFrameCount() { return m_mInterface.InputFrameCount; }

        /// <summary>
        /// Sets the state name used by the LoadState and SaveState functions.
        /// The extension (.sav) does not need to be included in the argument.
        /// </summary>
        /// <param name="t_name">The state name as a string.</param>
        public void SetStateName(string t_name)
        {
            // TODO: Although this method works, it would be preferrable to use the WriteArray function to write the StateName
            //       alone for performance improvement. But when I tried to implement this, only the first character of the 
            //       string was being written, thus the function did not work at all.

            MInterface mi = ReadStructFromMemory();
            mi.StateName = t_name;
            WriteStructToMemory(mi);
        }

        /// <summary>
        /// Gets the current GameCube Controller input that is being executed in Dolphin.
        /// </summary>
        /// <returns>The input specified as a GCController object.</returns>
        public GCController GetCurrentInput()
        {
            int size = Marshal.SizeOf(typeof(byte)) * 8;
            IntPtr p = Marshal.AllocHGlobal(size);

            Marshal.Copy(m_mInterface.ReadOnlyControllerInput, 0, p, size);

            return (GCController)Marshal.PtrToStructure(p, typeof(GCController));
        }

        /// <summary>
        /// Adds a GameCube Controller input object to the queue of inputs that will be sent to Dolphin.
        /// </summary>
        /// <param name="t_gcCont">The input specified as a GCController object.</param>
        public void AddInputInQueue(GCController t_gcCont)
        {
            m_inputsToSend.Enqueue(t_gcCont);
        }

        /// <summary>
        /// Clears the input queue that's being run in Dolphin.
        /// </summary>
        public void ClearQueue()
        {
            // Clearing internal queue
            m_inputsToSend.Clear();

            // Clearing Dolphin's queue
            WriteByteToMemory("InputsInQueue", 0);
        }

        /// <summary>
        /// Updates the MInterface object with Dolphin's Shared Memory and activates the events caused by Dolphin.
        /// </summary>
        private void Update(object sender, EventArgs myEventArgs)
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

            SendInputsToDolphin();
        }

        /// <summary>
        /// Updates Dolphin's input queue with the class internal queue, if there is enough space.
        /// </summary>
        private void SendInputsToDolphin()
        {
            // Number of inputs to send to Dolphin
            int countInputsToSend = m_inputsToSend.Count;

            // countInputsToSend is limited to (10 - InputsInQueue) because Dolphin's queue is limited to 10 inputs
            if (countInputsToSend > 10 - m_mInterface.InputsInQueue)
            {
                countInputsToSend = 10 - m_mInterface.InputsInQueue;
            }

            // If there's no input to send, return
            if (countInputsToSend == 0)
            {
                return;
            }

            // Each input is represented by an array of 8 bytes
            int size = Marshal.SizeOf(typeof(byte)) * 8 * countInputsToSend;
            byte[] data = new byte[size];

            // Build data array with the byte values of each input
            for (int i = 0; i < countInputsToSend; i++)
            {
                GCController input = m_inputsToSend.Dequeue();
                IntPtr p = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(byte)) * 8);
                Marshal.StructureToPtr(input, p, false);
                Marshal.Copy(p, data, i * 8, Marshal.SizeOf(typeof(byte)) * 8);
            }

            // The position to write the array is offset by (8 * InputsInQueue) because there are "InputsInQueue" inputs in queue already
            long position = (long)Marshal.OffsetOf(typeof(MInterface), "ControllerInputQueue") + m_mInterface.InputsInQueue * 8;
            m_accessor.WriteArray<byte>(position, data, 0, data.Length);

            // Finally, InputsInQueue is updated with the value that was written
            WriteByteToMemory("InputsInQueue", (byte)(m_mInterface.InputsInQueue + countInputsToSend));
        }

        /// <summary>
        /// Reads the whole struct from Dolphin's Shared Memory.
        /// </summary>
        /// <returns>A MInterface object with Dolphin's data.</returns>
        private MInterface ReadStructFromMemory()
        {
            int size = Marshal.SizeOf(typeof(MInterface));
            byte[] data = new byte[size];

            // Using an IntPtr to convert the byte array to an MInterface object
            IntPtr p = Marshal.AllocHGlobal(size);
            m_accessor.ReadArray<byte>(0, data, 0, size);
            Marshal.Copy(data, 0, p, size);

            return (MInterface)Marshal.PtrToStructure(p, typeof(MInterface));
        }

        /// <summary>
        /// Writes the whole struct to Dolphin's Shared Memory.
        /// </summary>
        /// <param name="t_mInterface">The MInterface object to be written.</param>
        private void WriteStructToMemory(MInterface t_mInterface)
        {
            int size = Marshal.SizeOf(typeof(MInterface));
            byte[] data = new byte[size];
            
            // Using an IntPtr to convert the MInterface to a byte array
            IntPtr p = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(t_mInterface, p, false);
            Marshal.Copy(p, data, 0, data.Length);
            
            m_accessor.WriteArray<byte>(0, data, 0, data.Length);
        }

        /// <summary>
        /// Writes a single byte to Dolphin's Shared Memory.
        /// </summary>
        /// <param name="t_member">The MInterface member name as a text.</param>
        /// <param name="t_value">The value to write to the MInterface member.</param>
        private void WriteByteToMemory(string t_member, byte t_value)
        {
            long position = (long)Marshal.OffsetOf(typeof(MInterface), t_member);
            m_accessor.Write(position, t_value);
        }
    }
}
