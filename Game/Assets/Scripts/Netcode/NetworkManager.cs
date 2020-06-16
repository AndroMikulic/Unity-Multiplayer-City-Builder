using System;
using System.Collections.Concurrent;
using System.Dynamic;
using System.Globalization;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;

public class NetworkManager : MonoBehaviour {

	public Managers managers;

	public TMP_InputField ip;

	public int port;

	public bool connected = false;
	public bool initialized = false;

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

		RequestWorldInit ();

		sendThread = new Thread (new ThreadStart (Send));
		sendThread.Start ();
	}

	void RequestWorldInit () {
		dynamic payload = new ExpandoObject ();
		payload.message = "Hello!";
		Packet p = new Packet (Constants.Networking.PacketTypes.WORLD_INIT, payload);
		outboundPackets.Add (p);
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

			String data = Encoding.UTF8.GetString (rawData, 0, bytes);
			HandlePacket (Packet.Parse (data));
		}
	}

	void Send () {
		while (true) {
			Packet packet = outboundPackets.Take ();
			byte[] rawData = Encoding.UTF8.GetBytes (packet.json.PadRight (Constants.Networking.MAX_PACKET_SIZE, ' '));
			stream.Write (rawData, 0, rawData.Length);
		}
	}

	void HandlePacket (Packet packet) {

		if (packet.type.Equals (Constants.Networking.PacketTypes.ENTITY_CREATE)) {
			managers.entityManager.spawner.AddEntity (packet.payload);
			return;
		}

		if (packet.type.Equals (Constants.Networking.PacketTypes.ENTITY_DELETE)) {
			managers.entityManager.remover.RemoveEntity (packet.payload);
			return;
		}
	}
}