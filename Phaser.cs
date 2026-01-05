using System.Diagnostics;
using System.Numerics;

class Phaser
{
    AudioDriver ad;
    Complex antenna1Phase;
    Complex antenna2Phase;
    Action<Complex[]> callback;
    Complex[] processedData = new Complex[Constants.CHUNK_SIZE];
    Complex phase_offset = Complex.One;

    public Phaser(Settings s, AudioDriver ad, Complex antenna1Phase, Complex antenna2Phase)
    {
        phase_offset = Complex.FromPolarCoordinates(1, Single.DegreesToRadians(-s.phase_offset));
        this.ad = ad;
        ad.Register(ProcessSamples);
        this.antenna1Phase = antenna1Phase;
        this.antenna2Phase = antenna2Phase;
    }

    public void Register(Action<Complex[]> callback)
    {
        this.callback = callback;
    }

    private void ProcessSamples(Complex[] data1, Complex[] data2)
    {
        for (int i = 0; i < Constants.CHUNK_SIZE; i++)
        {
            processedData[i] = (data1[i] * antenna1Phase) + (data2[i] * phase_offset * antenna2Phase);
        }
        callback(processedData);
    }
}