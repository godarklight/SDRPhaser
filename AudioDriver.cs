using System.Numerics;
using PortAudioSharp;
using Stream = PortAudioSharp.Stream;

class AudioDriver : IAudioDriver
{
    event Action<Complex[], Complex[]>? callbacks;
    Complex[] antennaA = new Complex[Constants.CHUNK_SIZE];
    Complex[] antennaB = new Complex[Constants.CHUNK_SIZE];
    Complex[] antennaANext = new Complex[Constants.CHUNK_SIZE];
    Complex[] antennaBNext = new Complex[Constants.CHUNK_SIZE];
    AutoResetEvent are = new AutoResetEvent(false);
    Random random = new Random();
    int writePos = 0;
    Thread? processThread;
    readonly Lock lockObject = new Lock();

    //Debug
    bool running = true;
    Settings settings;
    Complex antennaBAdjust;

    public AudioDriver(Settings settings)
    {
        this.settings = settings;
        antennaBAdjust = Complex.FromPolarCoordinates(settings.voltage_offset, double.RadiansToDegrees(settings.phase_offset));
    }

    public void Register(Action<Complex[], Complex[]> callback)
    {
        callbacks += callback;
    }

    public void Start()
    {
        processThread = new Thread(new ThreadStart(ProcessThread));
        processThread.Start();
        PortAudio.Initialize();
        int selectedDevice = -1;
        for (int i = 0; i < PortAudio.DeviceCount; i++)
        {
            DeviceInfo di = PortAudio.GetDeviceInfo(i);
            if (di.name.Contains("hw:") && di.name.Contains(settings.audioDevice))
            {
                Console.WriteLine("Found: " + di.name);
                selectedDevice = i;
                break;
            }
        }
        if (selectedDevice == -1)
        {
            Console.WriteLine("Audio device not found");
            running = false;
            return;
        }
        StreamParameters inParam = new StreamParameters();
        inParam.channelCount = 4;
        inParam.device = selectedDevice;
        inParam.sampleFormat = SampleFormat.Float32;
        inParam.suggestedLatency = 0.02;
        Stream audioStream = new Stream(inParam, null, 96000, 0, StreamFlags.NoFlag, AudioCallback, null);
        audioStream.Start();
        while (running)
        {
            Thread.Sleep(10);
        }
        audioStream.Stop();
        PortAudio.Terminate();
    }

    public StreamCallbackResult AudioCallback(IntPtr input, IntPtr output, uint frameCount, ref StreamCallbackTimeInfo timeInfo, StreamCallbackFlags statusFlags, IntPtr userDataPtr)
    {
        unsafe
        {

            float* floatIn = (float*)input.ToPointer();
            for (int i = 0; i < frameCount; i++)
            {
                antennaANext[writePos] = new Complex(*floatIn++ * settings.channelA, *floatIn++ * settings.channelB);
                antennaBNext[writePos] = new Complex(*floatIn++ * settings.channelC, *floatIn++ * settings.channelD) * antennaBAdjust;
                writePos++;
                if (writePos == Constants.CHUNK_SIZE)
                {
                    lock (lockObject)
                    {
                        //Swap and process
                        Complex[] tempA = antennaA;
                        Complex[] tempB = antennaB;
                        antennaA = antennaANext;
                        antennaB = antennaBNext;
                        antennaANext = tempA;
                        antennaBNext = tempB;
                        writePos = 0;                        
                    }
                    are.Set();
                }
            }
            if (!running)
            {
                return StreamCallbackResult.Complete;
            }
            return StreamCallbackResult.Continue;
        }
    }

    public void ProcessThread()
    {
        while (running)
        {
            if (are.WaitOne(10))
            {
                lock (lockObject)
                {
                    callbacks?.Invoke(antennaA, antennaB);
                }
            }
        }
    }

    public void Stop()
    {
        running = false;
    }
}