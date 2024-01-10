using System;
using System.Net.Sockets;
using System.Text;

namespace SimpleRemoteExec
{
    public class Client
    {
        public async Task Start(string path, string[] commands)
        {
            using var clientSocket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
            var endPoint = new UnixDomainSocketEndPoint(path);
            clientSocket.Connect(endPoint);
            SocketHelper socketHelper = new SocketHelper();


            await socketHelper.Send(clientSocket, new RequestMessage { Commands = commands });
            while (true)
            {
                if (clientSocket.Poll(80, SelectMode.SelectRead))
                {

                    var response = socketHelper.Receive<ResponseMessage>(clientSocket);
                    if (response == null)
                        break;
                    switch (response)
                    {
                        case StdoutResponseMessage stdout:
                            Console.WriteLine(stdout.Content);
                            break;
                        case StderrResponseMessage stderr:
                            Console.Error.WriteLine(stderr.Content);
                            break;
                        case ExitResponseMessage exit:
                            Environment.Exit(exit.ExitCode);
                            break;
                    }
                }
            }

        }
    }
}