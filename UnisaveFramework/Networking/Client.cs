using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;
using System.Threading;
using LightJson;
using LightJson.Serialization;
using Unisave.Exceptions;

namespace Unisave.Networking
{
    public class Client
    {
        private readonly Socket socket;

        public Client(Socket socket)
        {
            this.socket = socket;
        }

        /// <summary>
        /// Create a client connected to a given server
        /// </summary>
        public static Client Connect(string ipAddress, int port)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(IPAddress.Parse(ipAddress), port);
            return new Client(socket);
        }

        /// <summary>
        /// Disconnect from the server
        /// </summary>
        public void Disconnect()
        {
            socket.Close();
        }

        public void SendJsonMessage(int messageType, JsonValue payload)
        {
            SendTextMessage(messageType, payload.ToString());
        }

        public void SendJsonMessage(int messageType)
        {
            SendTextMessage(messageType, "null");
        }

        public JsonValue ReceiveJsonMessage(out int messageType)
        {
            return JsonReader.Parse(ReceiveTextMessage(out messageType));
        }

        public JsonValue ReceiveJsonMessageAndExpectType(int expectedMessageType)
        {
            JsonValue payload = ReceiveJsonMessage(out int messageType);

            if (expectedMessageType != messageType)
                throw new NetworkingException(
                    $"Expecting message {expectedMessageType}, "
                    + $"but received {messageType} instead with payload {payload.ToString()}."
                );

            return payload;
        }

        public void SendTextMessage(int messageType, string payload)
        {
            SendMessage(messageType, (new UTF8Encoding()).GetBytes(payload));
        }

        public string ReceiveTextMessage(out int messageType)
        {
            return (new UTF8Encoding()).GetString(ReceiveMessage(out messageType));
        }

        /// <summary>
        /// Sends a message to the client
        /// </summary>
        public void SendMessage(int messageType, byte[] payload)
        {
            byte[] sizeHeader = BitConverter.GetBytes((int)payload.Length);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(sizeHeader);

            byte[] typeHeader = BitConverter.GetBytes((int)messageType);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(typeHeader);

            socket.Send(sizeHeader);
            socket.Send(typeHeader);
            socket.Send(payload);
        }

        /// <summary>
        /// Receive a single message from the input stream
        /// </summary>
        public byte[] ReceiveMessage(out int messageType)
        {
            byte[] sizeHeader = ReadBytes(4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(sizeHeader);
            int size = BitConverter.ToInt32(sizeHeader, 0);

            byte[] typeHeader = ReadBytes(4);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(typeHeader);
            messageType = BitConverter.ToInt32(typeHeader, 0);

            return ReadBytes(size);
        }

        /// <summary>
        /// Reads a given number of bytes from the input stream
        /// (joins individual segments as they arrive to return the requested byte block)
        /// </summary>
        private byte[] ReadBytes(int count)
        {
            if (count <= 0)
                return new byte[0];

            int received = 0;
            byte[] buffer = new byte[count];

            while (true)
            {
                int k = socket.Receive(buffer, received, buffer.Length - received, SocketFlags.None);
                received += k;

                if (k == 0)
                    throw new ConnectionEndedException("Not enough bytes available. Connection ended.");

                if (received == buffer.Length)
                    break;

                if (received > buffer.Length)
                    throw new NetworkingException("Wrong count passed to the method.");
            }

            return buffer;
        }
    }
}
