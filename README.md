# ServerClientSocket
Implementar de uma comunicação via socket seguindo os exemplos da documentação do dotnet
links:<br>
https://docs.microsoft.com/pt-br/dotnet/framework/network-programming/asynchronous-server-socket-example
<br>
https://docs.microsoft.com/pt-br/dotnet/framework/network-programming/using-an-asynchronous-server-socket
<br>
Necessario usar o Hercules SETUP para simular o client e o servidor.<br>
https://www.hw-group.com/software/hercules-setup-utility

## Client:

Um soquete de cliente assíncrono não suspende o aplicativo enquanto aguarda a conclusão das operações de rede. Em vez disso, ele usa o modelo padrão de programação assíncrona do .NET Framework para processar a conexão de rede em um thread, enquanto o aplicativo continua em execução no thread original. Soquetes assíncronos são apropriados para aplicativos que fazem uso intenso da rede ou que não podem aguardar a conclusão das operações de rede antes de continuar.

A classe Socket segue o padrão de nomenclatura do .NET Framework para métodos assíncronos; por exemplo, o método Receive síncrono corresponde aos métodos BeginReceive e EndReceive assíncronos.

As operações assíncronas exigem um método de retorno de chamada para retornar o resultado da operação. Se o aplicativo não precisar saber o resultado, nenhum método de retorno de chamada será necessário. O código de exemplo desta seção demonstra como usar um método para iniciar a conexão com um dispositivo de rede e um método de retorno de chamada para concluir a conexão, um método para iniciar o envio de dados e um método de retorno de chamada para concluir o envio, bem como um método para iniciar o recebimento de dados e um método de retorno de chamada para encerrar o recebimento de dados.

Os soquetes assíncronos usam vários threads do pool de threads do sistema para processar conexões de rede. Um thread é responsável por iniciar o envio ou recebimento de dados; outros threads concluem a conexão com o dispositivo de rede e enviam ou recebem dados. Nos exemplos a seguir, instâncias da classe System.Threading.ManualResetEvent são usadas para suspender a execução do thread principal e sinalizar quando a execução pode continuar.

No exemplo a seguir, para conectar um soquete assíncrono com um dispositivo de rede, o método Connect inicializa um Socket e, em seguida, chama o método Socket.Connect, passando um ponto de extremidade remoto que representa o dispositivo de rede, o método de retorno de chamada de conexão e um objeto de estado, (o Socket de cliente), que é usado para passar informações de estado entre chamadas assíncronas. O exemplo implementa o método Connect para conectar o Socket especificado ao ponto de extremidade especificado. Ele supõe que haja um ManualResetEvent global chamado connectDone.

```bash
public static void Connect(EndPoint remoteEP, Socket client) {  
    client.BeginConnect(remoteEP,
        new AsyncCallback(ConnectCallback), client );  
  
   connectDone.WaitOne();  
}
```  
O método de retorno de chamada de conexão ConnectCallback implementa o representante AsyncCallback. Ele se conecta ao dispositivo remoto quando o dispositivo remoto está disponível e, em seguida, sinaliza ao thread de aplicativo que a conexão foi concluída definindo o ManualResetEvent como connectDone. O código a seguir implementa o método ConnectCallback.

```bash
private static void ConnectCallback(IAsyncResult ar) {  
    try {  
        // Retrieve the socket from the state object.  
        Socket client = (Socket) ar.AsyncState;  
  
        // Complete the connection.  
        client.EndConnect(ar);  
  
        Console.WriteLine("Socket connected to {0}",  
            client.RemoteEndPoint.ToString());  
  
        // Signal that the connection has been made.  
        connectDone.Set();  
    } catch (Exception e) {  
        Console.WriteLine(e.ToString());  
    }  
}
```  
O método de exemplo Send codifica os dados de cadeia de caracteres especificados no formato ASCII e envia-os de forma assíncrona para o dispositivo de rede representado pelo soquete especificado. O exemplo a seguir implementa o método Send.

```bash
private static void Send(Socket client, String data) {  
    // Convert the string data to byte data using ASCII encoding.  
    byte[] byteData = Encoding.ASCII.GetBytes(data);  
  
    // Begin sending the data to the remote device.  
    client.BeginSend(byteData, 0, byteData.Length, SocketFlags.None,  
        new AsyncCallback(SendCallback), client);  
}
```
O método de retorno de chamada de envio SendCallback implementa o representante AsyncCallback. Ele envia os dados quando o dispositivo de rede está pronto para recebê-los. O exemplo a seguir mostra a implementação do método SendCallback. Ele supõe que haja um ManualResetEvent global chamado sendDone.

```bash
private static void SendCallback(IAsyncResult ar) {  
    try {  
        // Retrieve the socket from the state object.  
        Socket client = (Socket) ar.AsyncState;  
  
        // Complete sending the data to the remote device.  
        int bytesSent = client.EndSend(ar);  
        Console.WriteLine("Sent {0} bytes to server.", bytesSent);  
  
        // Signal that all bytes have been sent.  
        sendDone.Set();  
    } catch (Exception e) {  
        Console.WriteLine(e.ToString());  
    }  
}
```  
A leitura de dados de um soquete de cliente exige um objeto de estado que passa valores entre chamadas assíncronas. A classe a seguir é um objeto de estado de exemplo para recebimento dos dados de um soquete de cliente. Ele contém um campo para o soquete de cliente, um buffer para os dados recebidos e um StringBuilder para conter a cadeia de caracteres de dados de entrada. A colocação desses campos no objeto de estado permite que seus valores sejam preservados em várias chamadas para a leitura de dados do soquete de cliente.

```bash
public class StateObject {  
    // Client socket.  
    public Socket workSocket = null;  
    // Size of receive buffer.  
    public const int BufferSize = 256;  
    // Receive buffer.  
    public byte[] buffer = new byte[BufferSize];  
    // Received data string.  
    public StringBuilder sb = new StringBuilder();  
}
```  
O método Receive de exemplo configura o objeto de estado e, em seguida, chama o método BeginReceive para ler os dados do soquete de cliente de forma assíncrona. O exemplo a seguir implementa o método Receive.

```bash
private static void Receive(Socket client) {  
    try {  
        // Create the state object.  
        StateObject state = new StateObject();  
        state.workSocket = client;  
  
        // Begin receiving the data from the remote device.  
        client.BeginReceive( state.buffer, 0, StateObject.BufferSize, 0,  
            new AsyncCallback(ReceiveCallback), state);  
    } catch (Exception e) {  
        Console.WriteLine(e.ToString());  
    }  
}
```  
O método de retorno de chamada de recebimento ReceiveCallback implementa o representante AsyncCallback. Ele recebe os dados do dispositivo de rede e cria uma cadeia de caracteres de mensagem. Ele lê um ou mais bytes de dados da rede no buffer de dados e, em seguida, chama o método BeginReceive novamente até que os dados enviados pelo cliente estejam completos. Depois que todos os dados são lidos do cliente, ReceiveCallback sinaliza ao thread de aplicativo que os dados estão completos definindo o ManualResetEvent como sendDone.

O exemplo de código a seguir implementa o método ReceiveCallback. Ele supõe uma cadeia de caracteres global chamada response que contém a cadeia de caracteres recebida e um ManualResetEvent global chamado receiveDone. O servidor deve desligar o soquete de cliente normalmente para encerrar a sessão de rede.

```bash
private static void ReceiveCallback( IAsyncResult ar ) {  
    try {  
        // Retrieve the state object and the client socket
        // from the asynchronous state object.  
        StateObject state = (StateObject) ar.AsyncState;  
        Socket client = state.workSocket;  
        // Read data from the remote device.  
        int bytesRead = client.EndReceive(ar);  
        if (bytesRead > 0) {  
            // There might be more data, so store the data received so far.  
            state.sb.Append(Encoding.ASCII.GetString(state.buffer,0,bytesRead));  
                //  Get the rest of the data.  
            client.BeginReceive(state.buffer,0,StateObject.BufferSize,0,  
                new AsyncCallback(ReceiveCallback), state);  
        } else {  
            // All the data has arrived; put it in response.  
            if (state.sb.Length > 1) {  
                response = state.sb.ToString();  
            }  
            // Signal that all bytes have been received.  
            receiveDone.Set();  
        }  
    } catch (Exception e) {  
        Console.WriteLine(e.ToString());  
    }  
} 
``` 
## Server:

Os soquetes de servidor assíncrono usam o modelo de programação assíncrono do .NET Framework para processar solicitações de serviço da rede. A classe Socket segue o padrão de nomenclatura assíncrona do .NET Framework; por exemplo, o método Accept síncrono corresponde aos métodos BeginAccept e EndAccept assíncronos.

Um soquete de servidor assíncrono exige um método para começar a aceitar solicitações de conexão da rede, um método de retorno de chamada para manipular as solicitações de conexão e começar a receber dados da rede e um método de retorno de chamada para encerrar o recebimento dos dados. Todos esses métodos são abordados mais diante nesta seção.

No exemplo a seguir, para começar a aceitar solicitações de conexão da rede, o método StartListening inicializa o Socket e, em seguida, usa o método BeginAccept para começar a aceitar novas conexões. O método de retorno de chamada de aceitação é chamado quando uma nova solicitação de conexão é recebida no soquete. Ele é responsável por obter a instância Socket que manipulará a conexão e por entregar esse Socket ao thread que processará a solicitação. O método de retorno de chamada de aceitação implementa o representante AsyncCallback; ele retorna um nulo e usa um único parâmetro do tipo IAsyncResult. O exemplo a seguir é o shell de um método de retorno de chamada de aceitação.

```bash
void AcceptCallback(IAsyncResult ar)
{  
    // Add the callback code here.  
}  
```
O método BeginAccept usa dois parâmetros, um representante AsyncCallback, que aponta para o método de retorno de chamada de aceitação e um objeto, que é usado para passar informações de estado para o método de retorno de chamada. No exemplo a seguir, o Socket de escuta é passado para o método de retorno de chamada por meio do parâmetro state. Esse exemplo cria um representante AsyncCallback e começa a aceitar conexões da rede.

```bash
listener.BeginAccept(new AsyncCallback(SocketListener.AcceptCallback), listener);  
```
Os soquetes assíncronos usam threads do pool de threads do sistema para processar conexões de entrada. Um thread é responsável por aceitar conexões, outro thread é usado para manipular cada conexão de entrada e outro thread é responsável por receber dados da conexão. Eles podem ser o mesmo thread, dependendo de qual thread é atribuído pelo pool de threads. No exemplo a seguir, a classe System.Threading.ManualResetEvent suspende a execução do thread principal e sinaliza quando a execução pode continuar.

O exemplo a seguir mostra um método assíncrono que cria um soquete TCP/IP assíncrono no computador local e começa a aceitar conexões. Ele supõe que haja um ManualResetEvent global chamado allDone, que o método seja um membro de uma classe chamada SocketListener e que um método de retorno de chamada chamado AcceptCallback esteja definido.

```bash
public void StartListening()
{  
    IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());  
    IPEndPoint localEP = new IPEndPoint(ipHostInfo.AddressList[0], 11000);  
  
    Console.WriteLine($"Local address and port : {localEP.ToString()}");  
  
    Socket listener = new Socket(localEP.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);  
  
    try
    {  
        listener.Bind(localEP);  
        listener.Listen(10);  
  
        while (true)
        {  
            allDone.Reset();  
  
            Console.WriteLine("Waiting for a connection...");  
            listener.BeginAccept(new AsyncCallback(SocketListener.AcceptCallback), listener);  
  
            allDone.WaitOne();  
        }  
    }
    catch (Exception e)
    {  
        Console.WriteLine(e.ToString());  
    }  
  
    Console.WriteLine("Closing the listener...");  
}  
```
O método de retorno de chamada de aceitação (AcceptCallback no exemplo anterior) é responsável por sinalizar o thread principal do aplicativo para continuar o processamento, estabelecendo a conexão com o cliente e por iniciar a leitura assíncrona de dados do cliente. O exemplo a seguir é a primeira parte de uma implementação do método AcceptCallback. Esta seção do método sinaliza o thread principal do aplicativo para continuar o processamento e estabelece a conexão com o cliente. Ele supõe que haja um ManualResetEvent global chamado allDone.

```bash
public void AcceptCallback(IAsyncResult ar)
{  
    allDone.Set();  
  
    Socket listener = (Socket) ar.AsyncState;  
    Socket handler = listener.EndAccept(ar);  
  
    // Additional code to read data goes here.
}  
```
A leitura de dados de um soquete de cliente exige um objeto de estado que passa valores entre chamadas assíncronas. O exemplo a seguir implementa um objeto de estado para o recebimento de uma cadeia de caracteres do cliente remoto. Ele contém campos para o soquete de cliente, um buffer de dados para o recebimento de dados e um StringBuilder para a criação da cadeia de caracteres de dados enviada pelo cliente. A colocação desses campos no objeto de estado permite que seus valores sejam preservados em várias chamadas para a leitura de dados do soquete de cliente.

```bash
public class StateObject
{  
    public Socket workSocket = null;  
    public const int BufferSize = 1024;  
    public byte[] buffer = new byte[BufferSize];  
    public StringBuilder sb = new StringBuilder();  
} 
``` 
A seção do método AcceptCallback que começa a receber os dados do soquete de cliente primeiro inicializa uma instância da classe StateObject e, em seguida, chama o método BeginReceive para começar a ler os dados do soquete de cliente de forma assíncrona.

O exemplo a seguir mostra todo o método AcceptCallback. Ele supõe que haja um ManualResetEvent global chamado allDone,, que a classe StateObject esteja definida e que o método ReadCallback esteja definido em uma classe chamada SocketListener.

```bash
public static void AcceptCallback(IAsyncResult ar)
{  
    // Get the socket that handles the client request.  
    Socket listener = (Socket) ar.AsyncState;  
    Socket handler = listener.EndAccept(ar);  
  
    // Signal the main thread to continue.  
    allDone.Set();  
  
    // Create the state object.  
    StateObject state = new StateObject();  
    state.workSocket = handler;  
    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,  
        new AsyncCallback(AsynchronousSocketListener.ReadCallback), state);  
}  
```
O método final que precisa ser implementado para o servidor de soquete assíncrono é o método de retorno de chamada de leitura que retorna os dados enviados pelo cliente. Assim como o método de retorno de chamada de aceitação, o método de retorno de chamada de leitura é um representante AsyncCallback. Esse método lê um ou mais bytes do soquete do cliente no buffer de dados e, em seguida, chama o método BeginReceive novamente até que os dados enviados pelo cliente estejam completos. Depois que a mensagem inteira for lida do cliente, a cadeia de caracteres será exibida no console e o soquete de servidor que manipula a conexão com o cliente será fechado.

A amostra a seguir implementa o método ReadCallback. Ele supõe que a classe StateObject esteja definida.

```bash
public static void ReadCallback(IAsyncResult ar)
{  
    StateObject state = (StateObject) ar.AsyncState;  
    Socket handler = state.WorkSocket;  
  
    // Read data from the client socket.  
    int read = handler.EndReceive(ar);  
  
    // Data was read from the client socket.  
    if (read > 0)
    {  
        state.sb.Append(Encoding.ASCII.GetString(state.buffer,0,read));  
        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,  
            new AsyncCallback(ReadCallback), state);  
    }
    else
    {  
        if (state.sb.Length > 1)
        {  
            // All the data has been read from the client;  
            // display it on the console.  
            string content = state.sb.ToString();  
            Console.WriteLine($"Read {content.Length} bytes from socket.\n Data : {content}");
        }  
        handler.Close();  
    }  
} 
``` 
