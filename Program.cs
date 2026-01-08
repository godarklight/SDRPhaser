using System.ComponentModel;
using System.Numerics;

namespace SDRPhaser;

class Program
{
    static void Main(string[] args)
    {
        Settings s = new Settings();
        IAudioDriver ad = null;
        if (args.Length == 0)
        {
            ad = new AudioDriver(s);
        }
        else
        {
            //ad = new DebugAudioDriver();
            ad = new FileDriver(s);
        }
        
        //Ant 1
        Phaser p1 = new Phaser(s, ad, Complex.One, Complex.Zero);
        //Ant 2
        Phaser p2 = new Phaser(s, ad, Complex.Zero, Complex.One);
        //Ant 1 + Ant 2 90deg
        Phaser p3 = new Phaser(s, ad, Complex.One, Complex.FromPolarCoordinates(1.0, float.DegreesToRadians(90.0f)));
        //Ant 1 + Ant 2 270deg
        Phaser p4 = new Phaser(s, ad, Complex.One, Complex.FromPolarCoordinates(1.0, float.DegreesToRadians(270.0f)));
        //Ant 1 + 2
        Phaser p5 = new Phaser(s, ad, Complex.One, Complex.One);
        //Ant 1 - 2
        Phaser p6 = new Phaser(s, ad, Complex.One, -Complex.One);
        ArbPhaser arb7 = new ArbPhaser(s, ad, 5797);
        Phaser p8 = new Phaser(s, ad, Complex.One, Complex.FromPolarCoordinates(0.5, float.DegreesToRadians(46.0f)));

        MiriServer tcp1 = new MiriServer(p1, 5991);
        MiriServer tcp2 = new MiriServer(p2, 5992);
        MiriServer tcp3 = new MiriServer(p3, 5993);
        MiriServer tcp4 = new MiriServer(p4, 5994);
        MiriServer tcp5 = new MiriServer(p5, 5995);
        MiriServer tcp6 = new MiriServer(p6, 5996);
        MiriServer tcp7 = new MiriServer(arb7, 5997);
        MiriServer tcp8 = new MiriServer(p8, 5998);
        
        Console.CancelKeyPress += (_, _) => { ad.Stop(); };
        ad.Start();
    }
}
