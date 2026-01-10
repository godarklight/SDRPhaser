using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Runtime.InteropServices;

class FloatServer
{
    IPhaser phaser;
    TcpListener listener;
    List<TcpClient> clients = new List<TcpClient>();
    byte[] sendData = new byte[8 * Constants.CHUNK_SIZE];
    //Throwaway data
    byte[] readData = new byte[256];
    public FloatServer(IPhaser phaser, int port)
    {
        this.phaser = phaser;
        phaser.Register(Process);
        listener = new TcpListener(IPAddress.IPv6Any, port);
        listener.Server.DualMode = true;
        listener.Start();
        listener.BeginAcceptTcpClient(AcceptConnection, null);
    }

    private void AcceptConnection(IAsyncResult ar)
    {
        TcpClient newClient = listener.EndAcceptTcpClient(ar);
        Console.WriteLine("Connection from: " + newClient.Client.RemoteEndPoint);
        clients.Add(newClient);
        listener.BeginAcceptTcpClient(AcceptConnection, null);
    }

    public void Process(Complex[] data)
    {
        if (clients.Count == 0)
        {
            return;
        }
        for (int i = 0; i < Constants.CHUNK_SIZE; i++)
        {
            Complex c = data[i];
            float real = (float)(c.Real);
            float imaginary = (float)(c.Imaginary);
            //F32_LE
            BitConverter.GetBytes(real).CopyTo(sendData, i * 8);
            BitConverter.GetBytes(imaginary).CopyTo(sendData, i * 8 + 4);
        }
        TcpClient removeClient = null;
        foreach (TcpClient c in clients)
        {
            try
            {
                Stream stream = c.GetStream();
                //Read any crud first
                int bytesToRead = c.Available;
                while (bytesToRead > 0)
                {
                    int thisRead = bytesToRead;
                    if (thisRead > readData.Length)
                    {
                        thisRead = readData.Length;
                    }
                    bytesToRead -= stream.Read(readData, 0, thisRead);
                }
                //Send data
                stream.Write(sendData, 0, sendData.Length);
            }
            catch
            {
                removeClient = c;
            }
        }
        if (removeClient != null)
        {
            Console.WriteLine("Disconnected from: " + removeClient.Client.RemoteEndPoint);
            clients.Remove(removeClient);
        }
    }
}