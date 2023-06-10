using System;
using System.Net.Sockets;
using System.Text;


public class ChatClient
{
    private TcpClient client;
    private NetworkStream? stream;
    private string username;

    public void Connect(string serverIP, int serverPort)
    {
        client = new TcpClient();
        client.Connect(serverIP, serverPort);

        stream = client.GetStream();

        Console.Write("Digite seu nome de usuário: ");
        username = Console.ReadLine();
        byte[] usernameBytes = Encoding.ASCII.GetBytes(username);
        stream.Write(usernameBytes, 0, usernameBytes.Length);
        stream.Flush();

        Thread receiveThread = new Thread(ReceiveMessages);
        receiveThread.Start();

        Console.WriteLine("Conectado ao servidor de chat. Comece a digitar as mensagens.");

        try
        {
            while (true)
            {
                string message = Console.ReadLine();

                if (message.ToLower() == "sair")
                {
                    Console.WriteLine("Encerrando o programa...");
                    break;
                }

                byte[] messageBytes = Encoding.ASCII.GetBytes(message);

                stream.Write(messageBytes, 0, messageBytes.Length);
                stream.Flush();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Conexão com o servidor perdida. Motivo: " + ex.Message);
        }
        finally
        {
            stream.Close();
            client.Close();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\nDesconectado do servidor de chat.");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }

    private void ReceiveMessages()
    {
        byte[] responseBytes = new byte[4096];

        try
        {
            while (true)
            {
                int bytesRead = stream.Read(responseBytes, 0, responseBytes.Length);

                if (bytesRead == 0)
                {
                    break;
                }

                string response = Encoding.ASCII.GetString(responseBytes, 0, bytesRead);
                Console.WriteLine(response);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao receber mensagens. Motivo: " + ex.Message);
        }
        finally
        {
            stream.Close();
            client.Close();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\nServidor fechou a conexão.");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("              .d88888b.                                              .d8888b.  888               888  ");
        Console.WriteLine("             d88P* *Y88b                                            d88P  Y88b 888               888   ");
        Console.WriteLine("             888     888                                            888    888 888               888         ");
        Console.WriteLine("             888     888 888d888 8888b.  88888b.   .d88b.   .d88b.  888        88888b.   8888b.  888888   ");
        Console.WriteLine("             888     888 888P*      *88b 888 *88b d88P*88b d8P  Y8b 888        888 *88b     *88b 888         ");
        Console.WriteLine("             888     888 888    .d888888 888  888 888  888 88888888 888    888 888  888 .d888888 888        ");
        Console.WriteLine("             Y88b. .d88P 888    888  888 888  888 Y88b 888 Y8b.     Y88b  d88P 888  888 888  888 Y88b.      ");
        Console.WriteLine("              *Y88888P*  888    *Y888888 888  888  *Y88888  *Y8888   *Y8888P*  888  888 *Y888888  *Y888      ");
        Console.WriteLine("                                                       888                                                ");
        Console.WriteLine("                                                  Y8b d88P                                                 ");
        Console.WriteLine("                                                   *Y88P*                                              Client v1.0\n\n");
        Console.ForegroundColor = ConsoleColor.White;

        Console.Write("Digite o endereço IP do servidor: ");
        string serverIP = Console.ReadLine();

        Console.Write("Digite a porta do servidor: ");
        int serverPort = Convert.ToInt32(Console.ReadLine());

        ChatClient client = new ChatClient();
        client.Connect(serverIP, serverPort);
        
    }
}