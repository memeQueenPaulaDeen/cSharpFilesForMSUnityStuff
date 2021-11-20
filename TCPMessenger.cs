using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DefaultNamespace
{
    public class TCPMessenger
    {
        
        private string serverAdress;
        private Int32 port;
        private TcpClient client;

        public TCPMessenger()
        {
            serverAdress = "localhost";
            port = 10000;
            client = new TcpClient(serverAdress, port);
            Debug.Log("conected???");
        }


        public void sendPicMsg(Byte[] msg)
        {
            
            NetworkStream stream = client.GetStream();
            Byte[] dtype = System.Text.Encoding.ASCII.GetBytes("p "+msg.Length.ToString());
            stream.Write(dtype,0, dtype.Length);
            
            
            Byte[] rdata = new Byte[256];
            String responseData = String.Empty;
            Int32 bytes = stream.Read(rdata, 0, rdata.Length);
            responseData = System.Text.Encoding.ASCII.GetString(rdata, 0, bytes);
            Debug.Log(responseData);
            
            stream.Write(msg,0, msg.Length);
            
        }

        public void sendLocMsg(string msg)
        {
            Byte[] dtype = System.Text.Encoding.ASCII.GetBytes("l");
            NetworkStream stream = client.GetStream();
            stream.Write(dtype,0, dtype.Length);
            
            Byte[] rdata = new Byte[512];
            String responseData = String.Empty;
            Int32 bytes = stream.Read(rdata, 0, rdata.Length);
            responseData = System.Text.Encoding.ASCII.GetString(rdata, 0, bytes);
            Debug.Log(responseData);
            
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(msg);
            stream.Write(data, 0, data.Length);
            
        }


    }
}