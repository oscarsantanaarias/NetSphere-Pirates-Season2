using System.Collections.Generic;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;

namespace ProudNet.Codecs
{
    internal class ProudFrameEncoder : MessageToMessageEncoder<IByteBuffer>
    {
        protected override void Encode(IChannelHandlerContext context, IByteBuffer message, List<object> output)
        {
            // Header and payload go in one buffer so the frame leaves as a single
            // write. Split across two writes it reaches the client as two TCP
            // segments, and a client that does not reassemble them stalls.
            var buffer = context.Allocator
                .Buffer(2 + 5 + message.ReadableBytes)
                .WriteShortLE(Constants.NetMagic)
                .WriteScalar(message.ReadableBytes);

            buffer.WriteBytes(message, message.ReaderIndex, message.ReadableBytes);
            output.Add(buffer);
        }
    }
}
