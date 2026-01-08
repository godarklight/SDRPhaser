using System.Diagnostics;
using System.Numerics;
using System.Net.Sockets;
using System.Net;
using System.Text.Json;

class ArbPhaser : IPhaser
{
    IAudioDriver ad;
    Action<Complex[]>? callback;
    Complex[] processedData = new Complex[Constants.CHUNK_SIZE];
    Complex[] separateData = new Complex[Constants.CHUNK_SIZE * 2];
    TcpListener listener;
    TcpListener listenerOut;
    List<TcpClient> clients = new List<TcpClient>();
    //Short (2) * IQ * 2 Antenna
    byte[] writeBuffer = new byte[Constants.CHUNK_SIZE * 2 * 2 * 2];
    byte[] throwAway = new byte[1024];
    byte[] u4 = new byte[4];
    float vfo = 7132000;
    Complex phaseGain = Complex.One;
    int skip = 0;



    public ArbPhaser(Settings s, IAudioDriver ad, int port)
    {
        this.ad = ad;
        ad.Register(ProcessSamples);
        listener = new TcpListener(new IPEndPoint(IPAddress.IPv6Any, port));
        listener.Server.DualMode = true; ;
        listener.Start();
        listener.BeginAcceptTcpClient(AcceptConnectionIn, null);
    }

    private void AcceptConnectionIn(IAsyncResult ar)
    {
        TcpClient newClient = listener.EndAcceptTcpClient(ar);
        Console.WriteLine("ArbPhase connected: " + newClient.Client.RemoteEndPoint);
        clients.Add(newClient);
        listener.BeginAcceptTcpClient(AcceptConnectionIn, null);
    }

    public void Register(Action<Complex[]> callback)
    {
        this.callback = callback;
    }

    private void ProcessSamples(Complex[] data1, Complex[] data2)
    {
        //Miri
        for (int i = 0; i < Constants.CHUNK_SIZE; i++)
        {
            processedData[i] = data1[i] + (data2[i] * phaseGain);
            separateData[i * 2] = data1[i];
            separateData[i * 2 + 1] = data2[i] * phaseGain;
        }
        callback(processedData);

        TcpClient? removeClient = null;
        foreach (TcpClient client in clients)
        {
            try
            {
                Stream s = client.GetStream();
                //IN
                while (client.Available >= 8)
                {
                    int bytesRead = s.Read(throwAway, 0, 8);
                    if (bytesRead == 0)
                    {
                        removeClient = client;
                        break;
                    }
                    float gain = BitConverter.ToSingle(throwAway, 0);
                    float phase = BitConverter.ToSingle(throwAway, 4);
                    Console.WriteLine("Update Gain/Phase " + gain + ":" + phase);
                    //Convert DB to value
                    gain = (float)Math.Pow(10.0, gain / 20.0);
                    Console.WriteLine("Gain Value: " + gain);
                    phaseGain = Complex.FromPolarCoordinates(gain, float.DegreesToRadians(phase));
                }
                //OUT
                if (skip == 0)
                {
                    for (int i = 0; i < separateData.Length; i++)
                    {
                        int writePos = i * 4;
                        short real = (short)(separateData[i].Real * short.MaxValue);
                        short imaginary = (short)(separateData[i].Imaginary * short.MaxValue);
                        BitConverter.GetBytes(real).CopyTo(writeBuffer, writePos);
                        BitConverter.GetBytes(imaginary).CopyTo(writeBuffer, writePos + 2);
                    }
                    s.Write(writeBuffer, 0, writeBuffer.Length);
                    skip = 10;
                }
                skip--;
            }
            catch
            {
                removeClient = client;
            }
        }
        if (removeClient != null)
        {
            Console.WriteLine("ArbPhase disconnect: " + removeClient.Client.RemoteEndPoint);
            clients.Remove(removeClient);
        }
    }
}