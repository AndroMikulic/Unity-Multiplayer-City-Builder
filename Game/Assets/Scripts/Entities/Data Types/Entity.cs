using System;

[Serializable]
public class Entity {

	public EntityType entityType;
	public Location location;
	public long tileTimestamp;

	public static Entity ParseToEntity (dynamic obj) {
		Entity entity = new Entity ();
		try {
			entity.entityType = obj.entityType;
			entity.location = new Location ((int) obj.location.x, (int) obj.location.y);
			entity.tileTimestamp = obj.tileTimestamp;
			return entity;
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