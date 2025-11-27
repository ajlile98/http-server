using System;

namespace HttpServerApp.Models;

public enum ParserState
{
  REQUEST_LINE,
  HEADERS,
  BODY,
}
