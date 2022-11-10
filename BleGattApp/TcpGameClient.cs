using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace BleGattApp
{

    /// <summary>
    /// TcpGameClient
    /// </summary>
    public class TcpGameClient
    {
        /// <summary>
        /// socketConnection
        /// </summary>
        private TcpClient socketConnection;

        public TcpGameClient()
        {

        }

        /// <summary>
        /// Connect
        /// </summary>
        /// <returns></returns>
        public bool Connect()
        {
            socketConnection = null;
            try
            {
                socketConnection = new TcpClient("localhost", 9999);

            }
            catch (SocketException socketException)
            {
                Console.WriteLine("TcpGameClient - Connect() --> Error " + socketException);
                return false;

            }
            return true;
        }

        /// <summary>
        /// SendMessage
        /// </summary>
        public void SendMessage(string command)
        {
            // TODO: Check if socketConnection is null then return, do not try to send message to unity server
            if (socketConnection == null && !Connect())
            {
                return;
            }
            try
            {
                // Get a stream object for writing. 			
                NetworkStream stream = socketConnection.GetStream();
                if (stream.CanWrite)
                {
                    // Write byte array to socketConnection stream.                 
                    Byte[] data = Encoding.Default.GetBytes(command);
                    stream.Write(data, 0, data.Length);
                }
            }
            catch (SocketException socketException)
            {
                Console.WriteLine("TcpGameClient - SendMessage() --> Error " + socketException);
            }
            catch (System.IO.IOException)
            {
                socketConnection.Close();
                Connect();
            }
        }
    }
}