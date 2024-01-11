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

## Using as Systemd service
```bash
sudo cp ./bin/Release/publish/SimpleRemoteExec /usr/bin
sudo cp ./simple-remote-exec@.service /etc/systemd/system
sudo systemctl enable --now simple-remote-exec@$(systemd-escape -f ./sock).service
```