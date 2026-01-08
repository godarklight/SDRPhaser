using System.Numerics;
using System.Runtime.InteropServices.Marshalling;

interface IAudioDriver
{
    void Start();
    void Stop();
    void Register(Action<Complex[], Complex[]> Register);
}