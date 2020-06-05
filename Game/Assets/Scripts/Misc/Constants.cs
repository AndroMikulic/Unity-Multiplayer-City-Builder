public static class Constants {
	public static class Networking {
		public static int MAX_PACKET_SIZE = 1024; // in bytes

		public static class PacketTypes {
			public static string WORLD_INIT = "WORLD_INIT";
			public static string WORLD_INIT_DONE = "WORLD_INIT_DONE";
			public static string ENTITY_DELETE = "ENTITY_DELETE";
			public static string ENTITY_CREATE = "ENTITY_CREATE";
			public static string OPERATION_FAILED = "OPERATION_FAILED";

			public static class FailReason {
				public static string NO_MONEY = "NO_MONEY";
				public static string ILLEGAL_LOCATION = "ILLEGAL_LOCATION";
			}
		}
	}
	public static class Gameplay {
		public enum BuildingType {
			RESIDENTIAL = 1,
			COMMERCIAL = 2,
			INDUSTRIAL = 3,
			MISC = 4
		}
		public static int BASE_BUILDING_COST = 100;
		public static int ROAD_TILE_COST = 10;
	}
}