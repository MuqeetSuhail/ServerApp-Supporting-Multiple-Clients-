using ServerApp;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            const int bufferSize = 1024 * 1024; // Around 2 Megabytes.
            
            //ipendpoint is like a place you can say where the server is and the client comes to that place to connect/ communicate to the server.
            IPEndPoint ipendpoint = new IPEndPoint(IPAddress.Any, 1234);
            // Listener to listen for Tcp Connection on this end point.
            TcpListener listenclient = new TcpListener(ipendpoint);
            listenclient.Start();

            // Starts Listening for Tcp clients coming on the end point.
            Console.WriteLine("╔════════════════════╗");
            Console.WriteLine("║      Welcome       ║");
            Console.WriteLine("╚════════════════════╝");
            Console.WriteLine();
            Console.WriteLine("╔═════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║ Server Listening at Ip Address ( " + ipendpoint.Address + " ) on Port Number ( " + ipendpoint.Port + " )  ║");
            Console.WriteLine("╚═════════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            Console.WriteLine("Looking for clients: ");
            while (true) // Basically it will be on for an indefinite time
            {
                try
                {
                    // Here I created a client instance so that I can later save it in the dictionary and use it for client-to-client communication
                    TcpClient client = await listenclient.AcceptTcpClientAsync();    // Asynchronous acceptance of clients
                    Task.Run(() => Getinfo(client));                                 // Process each client asynchronously
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error Occurred: " + e.Message);
                }
            }
        }

        private static async Task Getinfo(TcpClient client)
        {
            Person person = new Person();
            while (true)
            {
                try
                {
                    try
                    {
                        if (person.getName() == null && person.getPhoneNumber() == null)
                        {
                            byte[] namebytes = new byte[1024 * 1024];
                            byte[] phonebytes = new byte[1024 * 1024];

                            // Receiving Name:
                            string Name = await Server.ReceiveMessageFromClientAsync(namebytes, client);

                        // Receiving Phone.
                        ReceivePhoneno:
                            string phoneNum = await Server.ReceiveMessageFromClientAsync(phonebytes, client);
                            string returnedCode = await Server.AddClientToDictionaryAsync(phoneNum, client);
                            if (returnedCode == "0")
                            {
                                person.setName(Name);
                                person.setNumber(phoneNum);
                                person.Displayinfo();
                            }
                            else
                            {
                                if (returnedCode == "-1")                // if number already exists then we send error code to user to tell him/her to re-enter number then will receive it again
                                {
                                    goto ReceivePhoneno;
                                }
                            }
                            // Registration Part Ends
                        }
                    }
                    catch
                    {
                            Console.WriteLine("Failed to Register");
                            return;
                    }
                    if (person.getName() != null && person.getPhoneNumber() != null)
                    {
                        try
                        {
                            // Receive message and redirect
                            byte[] messagebytes = new byte[1024 * 1024];
                            string messageType = await Server.ReceiveMessageFromClientAsync(messagebytes, client);
                            Console.WriteLine();
                            if (messageType == "Broadcast")
                            {
                                await Server.SendMessageToAllClientsAsync(client);

                                Console.WriteLine("Broadcasting complete.");
                            }
                            else
                            {
                                if (messageType == "Private")
                                {
                                    byte[] destnumber = new byte[1024 * 1024];
                                    string destinationnumber = await Server.ReceiveMessageFromClientAsync(destnumber, client);
                                    await Task.Delay(1000);
                                    await Server.SendMessageToSpecificClientAsync(destinationnumber, client);
                                }
                            }
                        }
                        catch(Exception e)
                        {
                            Console.WriteLine("Client has Disconnected");
                            return;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Client has Disconnected");
                    Console.WriteLine();
                    Console.WriteLine("Listening: ");
                }
            }
        }

    }
}
