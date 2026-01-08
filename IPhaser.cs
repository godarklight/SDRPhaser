using System.Numerics;

interface IPhaser
{
    void Register(Action<Complex[]> callback);
}