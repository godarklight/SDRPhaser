using System.Numerics;

class FileDriver : IAudioDriver
{

    bool running = true;
    byte[] fileBytes;
    event Action<Complex[], Complex[]>? callbacks;
    Complex[] antenna1 = new Complex[1024];
    Complex[] antenna2 = new Complex[1024];
    int readPos = 0;
    long nextSendTime = 0;
    long sendInterval = (1024L * TimeSpan.TicksPerSecond) / 96000L;
    Settings settings;

    public FileDriver(Settings settings)
    {
        this.settings = settings;
    }

    public void Register(Action<Complex[], Complex[]> callback)
    {
        callbacks += callback;
    }

    public void Start()
    {
        fileBytes = File.ReadAllBytes("test.raw");
        Console.WriteLine("Send Hz: " + TimeSpan.TicksPerSecond / sendInterval);
        nextSendTime = DateTime.UtcNow.Ticks;
        while (running)
        {
            long currentTime = DateTime.UtcNow.Ticks;
            while (currentTime > nextSendTime)
            {
                nextSendTime += sendInterval;
                for (int i = 0; i < 1024; i++)
                {
                    int sample1 = BitConverter.ToInt32(fileBytes, readPos);
                    int sample2 = BitConverter.ToInt32(fileBytes, readPos + 4);
                    int sample3 = BitConverter.ToInt32(fileBytes, readPos + 8);
                    int sample4 = BitConverter.ToInt32(fileBytes, readPos + 12);
                    double real1 = sample1 / (double)int.MaxValue;
                    double imaginary1 = sample2 / (double)int.MaxValue;
                    double real2 = sample3 / (double)int.MaxValue;
                    double imaginary2 = sample4 / (double)int.MaxValue;
                    antenna1[i] = new Complex(real1 * settings.channelA, imaginary1 * settings.channelB);
                    antenna2[i] = new Complex(real2 * settings.channelC, imaginary2 * settings.channelD);
                    readPos += 16;
                    //if ((readPos + 16) >= fileBytes.Length)
                    if ((readPos + 16) >= 46080000)
                    {
                        Console.WriteLine("Replay");
                        readPos = 0;
                    }
                }
                callbacks(antenna1, antenna2);
            }
            Thread.Sleep(1);
        }
    }

    public void Stop()
    {
        running = false;
    }
}