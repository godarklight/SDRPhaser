using System.Numerics;

class AudioClient
{
    event Action<Complex[], Complex[]>? callbacks;
    Complex[] buffer1 = new Complex[Constants.CHUNK_SIZE];
    Complex[] buffer2 = new Complex[Constants.CHUNK_SIZE];
    Random random = new Random();

    //Debug
    double phase = 0;
    double tuning = Math.Tau * 11500.0 / 48000.0;

    public void Register(Action<Complex[], Complex[]> callback)
    {
        callbacks += callback;
    }

    public void Start()
    {
        while (true)
        {
            Thread.Sleep(21);
            for (int i = 0; i < 1024; i++)
            {
                double noise1 =  0.0001 * random.NextDouble();
                double noise2 = 0.0001 * random.NextDouble();
                buffer1[i] = new Complex(0.01 * Math.Cos(phase), 0.01 *  Math.Sin(phase)) + noise1;
                buffer2[i] = new Complex(0.01 * Math.Sin(phase), 0.01 *  -Math.Cos(phase)) +  noise2;
                //Add artifical noise at -100db
                phase += tuning;
                if (phase > Math.Tau)
                {
                    phase -= Math.Tau;
                }
            }
            callbacks(buffer1, buffer2);
        }
    }
}