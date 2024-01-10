
using System;
using SimpleRemoteExec;

var cliArgs = Environment.GetCommandLineArgs();
if (cliArgs.Length < 3)
{
    Console.WriteLine($"Usage: {cliArgs[0]} server|client <path>");
    return;
}

var path = cliArgs[2];
if (cliArgs[1] == "server")
{
    new Server().Start(path);
}
else if (cliArgs[1] == "client")
{
    await new Client().Start(path, cliArgs[3..].ToArray());
}
else
{
    Console.WriteLine($"Usage: {cliArgs[0]} server|client <path>");
}
