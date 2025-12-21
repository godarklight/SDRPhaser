using System.Numerics;

namespace SDRPhaser;

class Program
{
    static void Main(string[] args)
    {
        AudioClient ac = new AudioClient();
        //Ant 1
        Phaser p1 = new Phaser(ac, new Complex(1, 0), new Complex(0, 0));
        //Ant 2
        Phaser p2 = new Phaser(ac, new Complex(0, 0), new Complex(1, 0));
        //Ant 1 + Ant 2 90deg
        Phaser p3 = new Phaser(ac, new Complex(1, 0), new Complex(0, 1));
        //Ant 1 + Ant 2 270deg
        Phaser p4 = new Phaser(ac, new Complex(1, 0), new Complex(0, -1));
        //Ant 1 + 2
        Phaser p5 = new Phaser(ac, new Complex(1, 0), new Complex(1, 0));
        //Ant 1 - 2
        Phaser p6 = new Phaser(ac, new Complex(1, 0), new Complex(-1, 0));
        MiriServer tcp1 = new MiriServer(p1, 5991);
        MiriServer tcp2 = new MiriServer(p2, 5992);
        MiriServer tcp3 = new MiriServer(p3, 5993);
        MiriServer tcp4 = new MiriServer(p4, 5994);
        MiriServer tcp5 = new MiriServer(p5, 5995);
        MiriServer tcp6 = new MiriServer(p6, 5996);
        ac.Start();
    }
}
