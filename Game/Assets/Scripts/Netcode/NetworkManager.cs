using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using TMPro;

public class NetworkManager : MonoBehaviour {

	public Managers managers;

	public TMP_InputField ip;

	[SerializeField]
	int port;

	public bool connected = false;

	TcpClient client;
	NetworkStream stream;

	Thread listenThread;
	Thread sendThread;

	public BlockingCollection<Packet> outboundPackets = new BlockingCollection<Packet> ();

	public void Connect () {

		client = new TcpClient (ip.text, port);
		stream = client.GetStream ();

		connected = true;

		listenThread = new Thread (new ThreadStart (Listen));
		listenThread.Start ();

		sendThread = new Thread (new ThreadStart (Send));
		sendThread.Start ();
	}

	void Listen () {
		while (true) {
			byte[] rawData = new byte[Constants.Networking.MAX_PACKET_SIZE];
			int bytes = 0;
			try {
				bytes = stream.Read (rawData, 0, rawData.Length);
			} catch (Exception e) {
				Console.WriteLine (e.StackTrace);
			}

			//Check if bytes == 0, if so, connection was dropped
			if (bytes == 0) {
				return;
			}

			String data = String.Empty;
			data = Encoding.UTF8.GetString (rawData, 0, bytes);
			HandlePacket (Packet.Parse (data));
		}
	}

	void Send () {
		while (true) {
			Packet packet = outboundPackets.Take ();
			byte[] rawData = Encoding.UTF8.GetBytes (packet.json);
			stream.Write (rawData, 0, rawData.Length);
		}
	}

	void HandlePacket (Packet packet) {
		Debug.Log (packet.type);
		if (packet.type.Equals (Constants.Networking.PacketTypes.ENTITY_CREATE)) {
			managers.entityManager.spawner.AddEntity (packet.payload);
		}

		if (packet.type.Equals (Constants.Networking.PacketTypes.ENTITY_DELETE)) {

		}
	}
}