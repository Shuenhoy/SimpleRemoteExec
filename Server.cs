using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using MemoryPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;


namespace SimpleRemoteExec
{
    public class Server
    {
        public Server()
        {
            using ILoggerFactory factory = LoggerFactory.Create(builder =>
                builder.AddSimpleConsole(options =>
                    {
                        options.IncludeScopes = true;
                        options.SingleLine = true;
                    })
                );
            logger = factory.CreateLogger<Server>();
        }
        private readonly ILogger logger;
        SocketHelper socketHelper = new SocketHelper();



        private async Task Exec(string[] commands, Socket clientSocket, CancellationToken cancellationToken = default)
        {
            var process = new Process();
            process.StartInfo.FileName = commands[0];
            foreach (var command in commands.Skip(1))
            {
                process.StartInfo.ArgumentList.Add(command);
            }
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            logger.LogInformation($"Starting {process.StartInfo.FileName} {string.Join(" ", process.StartInfo.ArgumentList)}");
            process.Start();
            cancellationToken.Register(() =>
            {
                try
                {
                    process.Kill();
                }
                catch { }
            });

            var stdoutHandler = new TaskCompletionSource();
            var stderrHandler = new TaskCompletionSource();

            process.OutputDataReceived += (sender, args) =>
            {
                if (args.Data == null)
                {
                    stdoutHandler.SetResult();
                }
                else
                {
                    using (logger.BeginScope($"\x1B[1m\x1B[32m[stdout {process.Id}]\x1B[0m"))

                    {
                        logger.LogInformation(args.Data);
                    }
                    try
                    {
                        socketHelper.Send<ResponseMessage>(clientSocket, new StdoutResponseMessage { Content = args.Data }).Wait();
                    }
                    catch
                    {
                        process.Kill();
                    }
                }
            };
            process.ErrorDataReceived += (sender, args) =>
            {
                if (args.Data == null)
                {
                    stderrHandler.SetResult();
                }
                else
                {
                    using (logger.BeginScope($"\x1B[1m\x1B[31m[stderr {process.Id}]\x1B[0m"))
                    {
                        logger.LogInformation(args.Data);
                    }
                    try
                    {
                        socketHelper.Send<ResponseMessage>(clientSocket, new StderrResponseMessage { Content = args.Data }).Wait();
                    }
                    catch
                    {
                        process.Kill();
                    }
                }
            };
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await Task.WhenAll(stdoutHandler.Task, stderrHandler.Task, process.WaitForExitAsync());

            logger.LogInformation($"Process {process.StartInfo.FileName} {string.Join(" ", process.StartInfo.ArgumentList)} exited with code {process.ExitCode}");
            try
            {
                await socketHelper.Send<ResponseMessage>(clientSocket, new ExitResponseMessage { ExitCode = process.ExitCode });

            }
            catch { }

        }
        public void Start(string path)
        {
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
            using var mainSocket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
            var endPoint = new UnixDomainSocketEndPoint(path);
            mainSocket.Bind(endPoint);
            mainSocket.Listen(10);
            logger.LogInformation($"Listening on {path}");

            try
            {
                while (true)
                {
                    var clientSocket = mainSocket.Accept();
                    logger.LogInformation($"Accepted {clientSocket.RemoteEndPoint}");

                    new Thread(async () =>
                    {
                        try
                        {
                            var request = socketHelper.Receive<RequestMessage>(clientSocket);
                            if (request == null)
                                return;
                            logger.LogInformation($"Received {request.Commands.Length} commands");
                            var cancellationTokenSource = new CancellationTokenSource();
                            var exec = Exec(request.Commands, clientSocket, cancellationTokenSource.Token);

                            var buffer = new byte[1];
                            if (clientSocket.Receive(buffer) == 0)
                            {
                                logger.LogInformation($"Client {clientSocket.RemoteEndPoint} disconnected");
                                clientSocket.Close();
                                cancellationTokenSource.Cancel();
                                return;
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex.ToString());
                        }
                    }).Start();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
            }
        }
    }
}