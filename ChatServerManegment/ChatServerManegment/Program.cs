using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class ChatServer
{
    private TcpListener listener;
    private List<ClientHandler> clients;

    public ChatServer(string enderecoIP, int porta)
    {
        clients = new List<ClientHandler>();
        IPAddress endereco = IPAddress.Parse(enderecoIP);
        listener = new TcpListener(endereco, porta);
    }

    public void Iniciar()
    {
        listener.Start();
        Console.WriteLine("Servidor de chat iniciado.");

        while (true)
        {
            TcpClient cliente = listener.AcceptTcpClient();
            Console.WriteLine("Novo cliente conectado.");

            ClientHandler clientHandler = new ClientHandler(cliente, this);
            clients.Add(clientHandler);

            Thread clientThread = new Thread(clientHandler.HandleClient);
            clientThread.Start();
        }
    }

    public void EnviarMensagemBroadcast(string mensagem, ClientHandler remetente)
    {
        foreach (ClientHandler cliente in clients)
        {
            if (cliente != remetente)
            {
                cliente.EnviarMensagem(mensagem);
            }
        }
    }

    public void RemoverCliente(ClientHandler cliente)
    {
        clients.Remove(cliente);
    }

    public void KickCliente(string nomeUsuario)
    {
        ClientHandler cliente = clients.Find(c => c.NomeUsuario == nomeUsuario);
        if (cliente != null)
        {
            cliente.Desconectar();
        }
    }
}

public class ClientHandler
{
    private TcpClient cliente;
    private ChatServer servidor;
    private NetworkStream stream;
    private string nomeUsuario;

    public string NomeUsuario { get { return nomeUsuario; } }

    public ClientHandler(TcpClient cliente, ChatServer servidor)
    {
        this.cliente = cliente;
        this.servidor = servidor;
        stream = cliente.GetStream();
    }

    public void HandleClient()
    {
        byte[] nomeUsuarioBytes = new byte[1024];
        int bytesLidos = stream.Read(nomeUsuarioBytes, 0, nomeUsuarioBytes.Length);
        nomeUsuario = Encoding.ASCII.GetString(nomeUsuarioBytes, 0, bytesLidos);

        Console.WriteLine("Cliente conectado: " + nomeUsuario);

        servidor.EnviarMensagemBroadcast(nomeUsuario + " entrou no chat.", this);

        while (true)
        {
            byte[] mensagemBytes = new byte[4096];
            bytesLidos = stream.Read(mensagemBytes, 0, mensagemBytes.Length);

            if (bytesLidos == 0)
            {
                break;
            }

            string mensagem = Encoding.ASCII.GetString(mensagemBytes, 0, bytesLidos);
            Console.WriteLine("Mensagem recebida de " + nomeUsuario + ": " + mensagem);

            if (mensagem.StartsWith("/kick "))
            {
                string nomeUsuarioKick = mensagem.Substring(6);
                if (nomeUsuario == nomeUsuarioKick)
                {
                    EnviarMensagem("Você não pode se expulsar.");
                }
                else
                {
                    servidor.KickCliente(nomeUsuarioKick);
                    EnviarMensagem("Você expulsou o usuário: " + nomeUsuarioKick);
                }
            }
            else
            {
                servidor.EnviarMensagemBroadcast(nomeUsuario + ": " + mensagem, this);
            }
        }

        servidor.RemoverCliente(this);
        servidor.EnviarMensagemBroadcast(nomeUsuario + " saiu do chat.", null);

        Desconectar();
    }

    public void EnviarMensagem(string mensagem)
    {
        byte[] mensagemBytes = Encoding.ASCII.GetBytes(mensagem);
        stream.Write(mensagemBytes, 0, mensagemBytes.Length);
        stream.Flush();
    }

    public void Desconectar()
    {
        stream.Close();
        cliente.Close();
    }
}

public class Program
{
    public static void Main(string[] args)
    {

        Console.ForegroundColor = ConsoleColor.Red;
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
        Console.WriteLine("                                                   *Y88P*                                              ServerManegment\n\n");
        Console.ForegroundColor = ConsoleColor.White;

        Console.Write("Digite o endereço IP do servidor:");
        string enderecoIP = Console.ReadLine();

        Console.Write("Digite a porta do servidor:");
        int porta = Convert.ToInt32(Console.ReadLine());

        ChatServer servidor = new ChatServer(enderecoIP, porta);
        servidor.Iniciar();
    }
}
