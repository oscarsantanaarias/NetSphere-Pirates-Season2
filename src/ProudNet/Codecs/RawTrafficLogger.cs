using System;
using System.Text;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using NLog;

namespace ProudNet.Codecs
{
    internal class RawTrafficLogger : ChannelHandlerAdapter
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            var buffer = message as IByteBuffer;
            if (buffer != null)
                Logger.Info($"[raw] <- {buffer.ReadableBytes} bytes de {context.Channel.RemoteAddress}: {Preview(buffer)}");

            context.FireChannelRead(message);
        }

        public override Task WriteAsync(IChannelHandlerContext context, object message)
        {
            var buffer = message as IByteBuffer;
            if (buffer != null)
                Logger.Info($"[raw] -> {buffer.ReadableBytes} bytes a {context.Channel.RemoteAddress}: {Preview(buffer)}");

            return context.WriteAsync(message);
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            Logger.Error($"[raw] excepcion en {context.Channel.RemoteAddress}: {exception}");
            context.FireExceptionCaught(exception);
        }

        private static string Preview(IByteBuffer buffer)
        {
            var count = Math.Min(buffer.ReadableBytes, 48);
            var text = new StringBuilder(count * 2);
            for (var i = 0; i < count; i++)
                text.Append(buffer.GetByte(buffer.ReaderIndex + i).ToString("x2"));

            if (buffer.ReadableBytes > count)
                text.Append("...");

            return text.ToString();
        }
    }
}
