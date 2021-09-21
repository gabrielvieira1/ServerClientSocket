using System;
using static ServerClientSocket.AsynchronousSocketListener;
using static ServerClientSocket.AsynchronousClient;

namespace ServerClientSocket
{
  class Program
  {
    static void Main(string[] args)
    {
      //StartClient();
      StartListening();
    }
  }
}
