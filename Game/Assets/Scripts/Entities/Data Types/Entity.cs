using System;

public class Entity {
	public EntityType entityType;

	public static Entity ParseToEntity (dynamic obj) {
		Entity entiy = new Entity ();
		try {
			entiy.entityType = obj.entityType;
			return entiy;
		} catch (Exception e) {
			Console.WriteLine ("Error parsing object to Entity");
			return null;
		}
	}
}

public enum EntityType {
	BUILDING = 1,
	ROAD = 2
}