using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Poemem.Versus
{
	class VersusCommand : Command
	{
		const int HostPort = 11000;
		const string EndOfMessage = "\n";

		public VersusCommand() : base("versus")
		{
			var nameArgument = new Argument<string>("name");
			var ipArgument = new Argument<string>("ip");

			var hostCommand = new Command("host") { };
			var joinCommand = new Command("join") { ipArgument };

			hostCommand.SetHandler(HandleHost);
			joinCommand.SetHandler(HandleJoin, ipArgument);

			AddCommand(hostCommand);
			AddCommand(joinCommand);
		}

		static async Task HandleHost()
		{
			//IPHostEntry host = Dns.GetHostEntry("localhost");
			IPAddress ipAddress = IPAddress.Any;
			IPEndPoint localEndPoint = new IPEndPoint(ipAddress, HostPort);
			
			var listener = new TcpListener(localEndPoint);

			try
			{

				listener.Start();

				Console.WriteLine($"Open on {listener.Server.LocalEndPoint}");
				Console.WriteLine("Waiting for connection...");
				using var handler = await listener.AcceptTcpClientAsync();

				Console.WriteLine($"Accepted {handler.Client.LocalEndPoint}");

				await using NetworkStream stream = handler.GetStream();
				
				while (true)
				{
					var buffer = new byte[1_024];
					int received = await stream.ReadAsync(buffer);

					var message = Encoding.UTF8.GetString(buffer, 0, received);
					Console.WriteLine($"Message received: \"{message}\"");
				}
			}
			finally
			{
				listener.Stop();
			}
		}

		static async Task HandleJoin(string ipString)
		{
			var ipAddress = IPAddress.Parse(ipString);
			var remoteEndPoint = new IPEndPoint(ipAddress, HostPort);

			var client = new TcpClient();

			Console.WriteLine("Connecting...");

			await client.ConnectAsync(remoteEndPoint);

			Console.WriteLine($"Connected to {client.Client.RemoteEndPoint}");

			await using NetworkStream stream = client.GetStream();

			while (true)
			{
				Console.Write("message: ");
				var message = Console.ReadLine() ?? "";
				await stream.WriteAsync(Encoding.UTF8.GetBytes(message));

				Console.WriteLine("Message sent!");
			}
		}
	}
}
