using System;
using MemoryPack;

namespace SimpleRemoteExec
{
    [MemoryPackable]
    public partial class RequestMessage
    {
        public string[] Commands { get; set; } = [];
    }
    [MemoryPackable]
    [MemoryPackUnion(0, typeof(StdoutResponseMessage))]
    [MemoryPackUnion(1, typeof(StderrResponseMessage))]
    [MemoryPackUnion(2, typeof(ExitResponseMessage))]
    public abstract partial class ResponseMessage
    {
    }

    [MemoryPackable]
    public partial class StdoutResponseMessage : ResponseMessage
    {
        public string Content { get; set; } = "";
    }

    [MemoryPackable]
    public partial class StderrResponseMessage : ResponseMessage
    {
        public string Content { get; set; } = "";
    }

    [MemoryPackable]
    public partial class ExitResponseMessage : ResponseMessage
    {
        public int ExitCode { get; set; }
    }

}