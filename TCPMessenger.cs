using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using UnityEngine;

namespace DefaultNamespace
{
    public class TCPMessenger
    {
        
        private string serverAdress;
        private Int32 port;
        private TcpClient client;
        public static action act;

        public class action
        {
            private float heading;
            private float force;
            private bool reset;

            public action()
            {
                heading = 0;
                force = 0;
                reset = false;
            }

            void setForce(float force)
            {
                this.force = force;
            }
            
            void setHeading(float heading)
            {
                this.heading = heading;
            }

            public float getHeading()
            {
                return this.heading;
            }

            public float getForce()
            {
                return this.force;
            }

            public bool getReset()
            {
                return this.reset;
            }

            void setReset(bool r)
            {
                this.reset = r;
            }

            

            public void setAction(string Raw)
            {
                if (Raw == "reset")
                {
                    setReset(true);
                }
                else if(Raw == "done")
                {
                    setForce(0);
                    /// maybe just show a done screen here?
                }
                else
                {
                    setReset(false);
                    float[] myArray = JsonConvert.DeserializeObject<float[]>(Raw);
                    setHeading(myArray[0]);
                    setForce(myArray[1]);
                }
            }
            

        }

        public TCPMessenger()
        {
            serverAdress = "localhost";
            port = 10000;
            client = new TcpClient(serverAdress, port);
            Debug.Log("conected???");
            act = new action();
        }

        // public action getAction()
        // {
        //     return act;
        // }

        private void sendPic(byte[] msg, NetworkStream stream)
        {
            //recieve action to take given the previous send state [picture]
            Byte[] rdata = new Byte[256];
            String responseData = String.Empty;
            Int32 bytes = stream.Read(rdata, 0, rdata.Length);
            responseData = System.Text.Encoding.ASCII.GetString(rdata, 0, bytes);
            Debug.Log("action to take: " + responseData);

            //set the action field which is read by other classes
            act.setAction(responseData);
            //send the captured UAV image
            stream.Write(msg,0, msg.Length);
            
            //Receive image Ack ? probablly not needed for tcp
            rdata = new Byte[512];
            bytes = stream.Read(rdata, 0, rdata.Length);
            responseData = System.Text.Encoding.ASCII.GetString(rdata, 0, bytes);
            Debug.Log(responseData);
        }

        public void sendPicMsg(Byte[] msg)
        {
            //Send pic data type
            NetworkStream stream = client.GetStream();
            Byte[] dtype = System.Text.Encoding.ASCII.GetBytes("p "+msg.Length.ToString());
            stream.Write(dtype,0, dtype.Length);
            
            sendPic(msg,stream);
            
        }
        
        public void sendPicStitchMsg(Byte[] msg)
        {
            //Send pic data type
            NetworkStream stream = client.GetStream();
            Byte[] dtype = System.Text.Encoding.ASCII.GetBytes("s "+msg.Length.ToString());
            stream.Write(dtype,0, dtype.Length);
            
            sendPic(msg,stream);
            
        }

        public void sendLocMsg(string msg)
        {
            //Send location data type
            Byte[] dtype = System.Text.Encoding.ASCII.GetBytes("l");
            NetworkStream stream = client.GetStream();
            stream.Write(dtype,0, dtype.Length);
            
            //recieve ready for loc from python
            Byte[] rdata = new Byte[512];
            String responseData = String.Empty;
            Int32 bytes = stream.Read(rdata, 0, rdata.Length);
            responseData = System.Text.Encoding.ASCII.GetString(rdata, 0, bytes);
            Debug.Log(responseData);
            
            //send the loc to pyton
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(msg);
            stream.Write(data, 0, data.Length);
            
            //receive loc ack ? probablly not needed for tcp
            rdata = new Byte[512];
            bytes = stream.Read(rdata, 0, rdata.Length);
            responseData = System.Text.Encoding.ASCII.GetString(rdata, 0, bytes);
            Debug.Log(responseData);
            
        }


    }
}