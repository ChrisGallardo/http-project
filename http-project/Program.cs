using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;

namespace http_project
{
    class Program
    {
        static Dictionary<string, string> dictionary = new Dictionary<string, string>()
        {
            { "Hello", "world"}
        };

        static void Main()
        {
            Console.WriteLine("Program Started");
            TcpListener socket;
            try
            {
                // Bind a interface to our socket
                Int32 port = 6789;
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");
                socket = new TcpListener(localAddr, port);

                // Start listening for client requests
                socket.Start(1);
                Console.WriteLine($"Server is listening for requests at 127.0.0.1:6789");

                while (true)
                {
                    // Accept an incoming connection
                    Socket conn = socket.AcceptSocket();
                    Console.WriteLine("Incoming connection accepted.");

                    string next = "Enter a command: \n";
                    while (true)
                    {
                        // Send and receive data from the client
                        string response = SendReceive(conn, next);

                        // if the client says "STOP" close the connection
                        if (response == "STOP")
                        {
                            break;
                        }

                        // the args recieved
                        string[] args = response.Split(' ');

                        try
                        {
                            switch (args[0])
                            {
                                case "GET":
                                    next = GET(args[1]);
                                    break;
                                case "SET":
                                    next = SET(args[1], args[2]);
                                    break;
                                case "CLEAR":
                                    next = CLEAR();
                                    break;
                                default:
                                    throw new IndexOutOfRangeException("command not found");
                            }
                        }
                        catch (IndexOutOfRangeException e)
                        {
                            string errormessage = (e.Message == "command not found" ? $"command '{args[0]}' not found" : "not enough arguments passed");
                            next = $"ERROR {errormessage}\n";
                        }
                    }

                    // Close the connection to the client
                    conn.Close();
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine($"Socket Exception: {e}");
            }
        }

        // send a message to the server and
        // return what's recieved
        static string SendReceive(Socket socket, string message, bool logged = true)
        {
            // The message converted to bytes array
            byte[] msg = Encoding.UTF8.GetBytes(message);
            byte[] bytes = new byte[256];

            // Blocks until server recieves and returns
            int i = socket.Send(msg);
            if (logged)
            {
                Console.WriteLine($"{i} bytes sent.");
            }

            // Get reply from server
            i = socket.Receive(bytes);
            if (logged)
            {
                Console.WriteLine($"{i} bytes recieved.\n");
            }

            // Convert the response to a string
            return Encoding.UTF8.GetString(bytes);
        }

        // Get a word from the dictionary
        static string GET(string word)
        {
            if (dictionary.ContainsKey(word))
            {
                return $"ANSWER {dictionary[word]}";
            }
            else
            {
                return "ERROR undefined";
            }
        }

        // Set a word to a definition
        static string SET(string word, string definition)
        {
            try
            {
                dictionary.Add(word, definition);
            }
            catch (ArgumentException)
            {
                return "That word is already in the dictionary";
            }
            return "The word has been added";
        }

        // Clear the dictionary
        static string CLEAR()
        {
            dictionary.Clear();
            return "The dictionary has been cleared";
        }
    }
}
