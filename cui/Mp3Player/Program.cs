using System;
using System.IO;
using System.Runtime.InteropServices;

class Program
{
    const int MPG123_OK = 0;

    // ==== mpg123 DLL ====
    [DllImport("libmpg123-0.dll", CallingConvention = CallingConvention.Cdecl)]
    static extern int mpg123_init();

    [DllImport("libmpg123-0.dll", CallingConvention = CallingConvention.Cdecl)]
    static extern void mpg123_exit();

    [DllImport("libmpg123-0.dll", CallingConvention = CallingConvention.Cdecl)]
    static extern IntPtr mpg123_new([MarshalAs(UnmanagedType.LPStr)] string? decoder, out int error);

    [DllImport("libmpg123-0.dll", CallingConvention = CallingConvention.Cdecl)]
    static extern void mpg123_delete(IntPtr mh);

    [DllImport("libmpg123-0.dll", CallingConvention = CallingConvention.Cdecl)]
    static extern int mpg123_open(IntPtr mh, [MarshalAs(UnmanagedType.LPStr)] string path);

    [DllImport("libmpg123-0.dll", CallingConvention = CallingConvention.Cdecl)]
    static extern int mpg123_close(IntPtr mh);

    [DllImport("libmpg123-0.dll", CallingConvention = CallingConvention.Cdecl)]
    static extern int mpg123_read(IntPtr mh, byte[] outmem, int outmemsize, out long done);

    [DllImport("libmpg123-0.dll", CallingConvention = CallingConvention.Cdecl)]
    static extern int mpg123_outblock(IntPtr mh);

    [DllImport("libmpg123-0.dll", CallingConvention = CallingConvention.Cdecl)]
    static extern int mpg123_getformat(IntPtr mh, out long rate, out int channels, out int encoding);

    // ==== Windows waveOut API ====
    [DllImport("winmm.dll", SetLastError = true)]
    static extern int waveOutOpen(out IntPtr hWaveOut, int uDeviceID, ref WAVEFORMATEX lpFormat,
        IntPtr dwCallback, IntPtr dwInstance, int dwFlags);

    [DllImport("winmm.dll", SetLastError = true)]
    static extern int waveOutPrepareHeader(IntPtr hWaveOut, ref WAVEHDR lpWaveHdr, int uSize);

    [DllImport("winmm.dll", SetLastError = true)]
    static extern int waveOutWrite(IntPtr hWaveOut, ref WAVEHDR lpWaveHdr, int uSize);

    [DllImport("winmm.dll", SetLastError = true)]
    static extern int waveOutUnprepareHeader(IntPtr hWaveOut, ref WAVEHDR lpWaveHdr, int uSize);

    [DllImport("winmm.dll", SetLastError = true)]
    static extern int waveOutClose(IntPtr hWaveOut);

    [StructLayout(LayoutKind.Sequential)]
    struct WAVEFORMATEX
    {
        public ushort wFormatTag;
        public ushort nChannels;
        public uint nSamplesPerSec;
        public uint nAvgBytesPerSec;
        public ushort nBlockAlign;
        public ushort wBitsPerSample;
        public ushort cbSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct WAVEHDR
    {
        public IntPtr lpData;
        public uint dwBufferLength;
        public uint dwBytesRecorded;
        public IntPtr dwUser;
        public uint dwFlags;
        public uint dwLoops;
        public IntPtr lpNext;
        public IntPtr reserved;
    }

    static void Main()
    {
        Console.Write("Enter music folder path > ");
        string dir = Console.ReadLine() ?? "";

        if (!Directory.Exists(dir))
        {
            Console.WriteLine("Folder not found.");
            return;
        }

        string[] files = Directory.GetFiles(dir, "*.mp3");
        if (files.Length == 0)
        {
            Console.WriteLine("No MP3 files in this folder.");
            return;
        }

        Console.WriteLine("=== MP3 Player ===");
        for (int i = 0; i < files.Length; i++)
            Console.WriteLine($"{i + 1}. {Path.GetFileName(files[i])}");

        Console.Write("Select a number > ");
        if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 1 || choice > files.Length)
            return;

        string path = files[choice - 1];
        PlayMp3(path);
    }

    static void PlayMp3(string path)
    {
        mpg123_init();
        int err;
        IntPtr mh = mpg123_new(null, out err);
        if (mh == IntPtr.Zero || mpg123_open(mh, path) != MPG123_OK)
        {
            Console.WriteLine("Failed to open MP3 file.");
            return;
        }

        // Get actual format from mpg123
        mpg123_getformat(mh, out long rate, out int channels, out int encoding);
        Console.WriteLine($"Format detected: {rate} Hz, {channels} channels, encoding={encoding}");

        // PCM format (force 16-bit PCM)
        WAVEFORMATEX format = new WAVEFORMATEX
        {
            wFormatTag = 1, // PCM
            nChannels = (ushort)channels,
            nSamplesPerSec = (uint)rate,
            wBitsPerSample = 16,
            nBlockAlign = (ushort)(channels * 16 / 8),
            nAvgBytesPerSec = (uint)(rate * channels * 16 / 8),
            cbSize = 0
        };

        waveOutOpen(out IntPtr hWaveOut, -1, ref format, IntPtr.Zero, IntPtr.Zero, 0);

        int outblock = mpg123_outblock(mh);
        byte[] buffer = new byte[outblock];

        Console.WriteLine("Playing: " + path);

        while (mpg123_read(mh, buffer, buffer.Length, out long done) == MPG123_OK && done > 0)
        {
            int size = (int)done;

            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            WAVEHDR header = new WAVEHDR
            {
                lpData = handle.AddrOfPinnedObject(),
                dwBufferLength = (uint)size,
                dwFlags = 0,
                dwLoops = 0
            };

            waveOutPrepareHeader(hWaveOut, ref header, Marshal.SizeOf(header));
            waveOutWrite(hWaveOut, ref header, Marshal.SizeOf(header));

            // crude wait (not ideal, just to let buffer play)
            System.Threading.Thread.Sleep(size * 1000 / (int)format.nAvgBytesPerSec);

            waveOutUnprepareHeader(hWaveOut, ref header, Marshal.SizeOf(header));
            handle.Free();
        }

        waveOutClose(hWaveOut);

        mpg123_close(mh);
        mpg123_delete(mh);
        mpg123_exit();

        Console.WriteLine("Playback finished.");
    }
}
