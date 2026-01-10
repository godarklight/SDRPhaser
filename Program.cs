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

        FloatServer tcp1 = new FloatServer(p1, 5991);
        FloatServer tcp2 = new FloatServer(p2, 5992);
        FloatServer tcp3 = new FloatServer(p3, 5993);
        FloatServer tcp4 = new FloatServer(p4, 5994);
        FloatServer tcp5 = new FloatServer(p5, 5995);
        FloatServer tcp6 = new FloatServer(p6, 5996);
        FloatServer tcp7 = new FloatServer(arb7, 5997);
        FloatServer tcp8 = new FloatServer(p8, 5998);
        
        Console.CancelKeyPress += (_, _) => { ad.Stop(); };
        ad.Start();
    }
}
