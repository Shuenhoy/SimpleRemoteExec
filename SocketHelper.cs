using System;
using System.Net.Sockets;
using MemoryPack;

namespace SimpleRemoteExec
{
    public class SocketHelper
    {
        public SocketHelper()
        {
            if (!MemoryPackFormatterProvider.IsRegistered<RequestMessage>())
                MemoryPackFormatterProvider.Register<RequestMessage>();
            if (!MemoryPackFormatterProvider.IsRegistered<ResponseMessage>())
                MemoryPackFormatterProvider.Register<ResponseMessage>();
        }
        public T? Receive<T>(Socket socket) where T : class
        {
            Span<byte> sizeBuffer = stackalloc byte[4];
            var sizeByteRead = socket.Receive(sizeBuffer);
            if (sizeByteRead == 0)
                return null;
            var size = BitConverter.ToInt32(sizeBuffer);
            Span<byte> buffer = stackalloc byte[size];
            var byteRead = socket.Receive(buffer);
            if (byteRead == 0)
                return null;
            return MemoryPackSerializer.Deserialize<T>(buffer);
        }
        public async Task Send<T>(Socket socket, T message) where T : class
        {
            var buffer = MemoryPackSerializer.Serialize(message);
            var sizeBuffer = BitConverter.GetBytes(buffer.Length);
            await socket.SendAsync(sizeBuffer);
            await socket.SendAsync(buffer);
        }
    }
}