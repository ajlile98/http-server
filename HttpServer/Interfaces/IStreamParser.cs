using System;
using System.Threading.Channels;

namespace HttpServerApp.Interfaces;

public interface IStreamParser
{
   public Channel<string> GetLinesChannel();
}
