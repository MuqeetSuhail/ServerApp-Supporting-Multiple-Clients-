using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerApp
{
    static class Server
    {
        private static Dictionary<string, TcpClient> ClientLog = new Dictionary<string, TcpClient>();
        const int bufferSize = 1024 * 1024;

        public static async Task<string> AddClientToDictionaryAsync(string tempUserNumber, TcpClient tempClient)
        {
            TcpClient clientConnected = tempClient;
            string enteredNumber = tempUserNumber;
            string errorCode = null;

            if (ClientLog.ContainsKey(enteredNumber))
            {
                string msg = "Number is Already Registered For Another Account, Re-enter Number";
                await SendBackMessageToClientAsync(msg, clientConnected);
                await Task.Delay(1000);
                errorCode = "-1";
                await SendBackMessageToClientAsync(errorCode, clientConnected);
            }
            else
            {
                if (!ClientLog.ContainsKey(enteredNumber))
                {
                    string msg = "Number is Registered Successfully";
                    await SendBackMessageToClientAsync(msg, clientConnected);
                    await Task.Delay(1000);
                    errorCode = "0";
                    await SendBackMessageToClientAsync(errorCode, clientConnected);
                    ClientLog.Add(tempUserNumber, tempClient);
                }
            }
            return errorCode;
        }

        public static async Task SendBackMessageToClientAsync(string message, TcpClient tempClient)
        {
            string messageToSend = message;
            byte[] messageBytes = Encoding.Unicode.GetBytes(messageToSend);
            int lengthOfMessageInBytes = messageBytes.Length;

            if (tempClient.Connected)
            {
                NetworkStream stream = tempClient.GetStream();
                await stream.WriteAsync(messageBytes, 0, lengthOfMessageInBytes);
            }
        }

        public static async Task<string> ReceiveMessageFromClientAsync(byte[] buffer, TcpClient tempClient)
        {
            if (tempClient.Connected)
            {
                NetworkStream stream = tempClient.GetStream();
                int numberOfBytesRead = await stream.ReadAsync(buffer, 0, bufferSize);
                string messageReceived = Encoding.Unicode.GetString(buffer, 0, numberOfBytesRead);
                return messageReceived;
            }
            else
            {
                return "noMessageReceived(connection closed)";
            }
        }

        public static async Task SendMessageToSpecificClientAsync(string numberToSend, TcpClient tempSenderClient)
        {
            TcpClient destinationClient = null;
            string enteredNumber = numberToSend;
            byte[] sendMessageBytes = new byte[bufferSize];
            string msg = null;
            string messageToSend = await ReceiveMessageFromClientAsync(sendMessageBytes, tempSenderClient);
            foreach (var tempNumber in ClientLog)
            {
                if (tempNumber.Key == enteredNumber)
                {
                    destinationClient = tempNumber.Value;
                    break;
                }
            }
            if (destinationClient != null)
            {
                if (destinationClient.Connected)
                {
                    await SendBackMessageToClientAsync("Private", destinationClient);
                    await Task.Delay(1000);
                    await SendBackMessageToClientAsync(messageToSend, destinationClient);
                    Console.WriteLine("Private Message Sent to " + enteredNumber + " completed");
                }
            }
            else
            {
                Console.WriteLine("No Client Found with number ( " +enteredNumber + " )");
            }
        }


        public static async Task SendMessageToAllClientsAsync(TcpClient tempSenderClient)
        {
            TcpClient clientConnected = tempSenderClient;
            byte[] messageReceiveBuffer = new byte[bufferSize];
            string messageToSend = await ReceiveMessageFromClientAsync(messageReceiveBuffer, clientConnected);

            foreach (TcpClient destinationClient in ClientLog.Values)
            {
                if (destinationClient != null && destinationClient.Connected)
                {
                    string message = "Broadcasting Message to All Online Devices";
                    Console.WriteLine("Notice: " + message);
                    Console.WriteLine("Message To Send is: " + messageToSend);
                    await SendBackMessageToClientAsync("Broadcast", destinationClient);
                    await Task.Delay(1000);
                    await SendBackMessageToClientAsync(messageToSend, destinationClient);
                }
            }
        }

    }
}
