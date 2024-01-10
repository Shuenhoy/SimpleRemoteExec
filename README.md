# SimpleRemoteExec

A simple server/client for executing command on host from docker container,
based on Unix Domain Socket.

*Warning:* There may be security issues, use with caution!


## Build
* Requires .NET 8.0+.

```bash
dotnet publish -r linux-x64 -c Release
```
or, 
```bash
dotnet publish -r linux-x64 -c Release /p:StaticExecutable=true
```

## Run server
```bash
SimpleRemoteExec server test.sock
```

## Run client
```bash
SimpleRemoteExec client test.sock whoami
```