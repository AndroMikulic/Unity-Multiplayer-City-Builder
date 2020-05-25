using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class NetworkManager : MonoBehaviour {
	[SerializeField]
	string ipAddress;

	[SerializeField]
	int port;

	TcpClient client;
	Thread networkThread;
	NetworkStream stream;
	byte[] rawData;

	void Start()
	{
		networkThread = new Thread(new ThreadStart(Connect));
		networkThread.Start();
	}

	void Connect()
	{
		client = new TcpClient(ipAddress, port);
		stream = client.GetStream();

		//debug
		string data = "Hello, I want to play on this city :)";
		rawData = System.Text.Encoding.ASCII.GetBytes(data);
		stream.Write(rawData, 0, rawData.Length);
	}
}