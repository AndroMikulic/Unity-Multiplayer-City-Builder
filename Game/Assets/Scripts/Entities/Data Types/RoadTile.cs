using System;

[Serializable]
public class RoadTile : Entity {

	public static RoadTile ParseToRoadTile (dynamic obj) {
		RoadTile tile = new RoadTile ();
		try {
			tile.entityType = obj.entityType;
			tile.location = new Location (obj.location.x, obj.location.y);
			return tile;
		} catch {
			Console.WriteLine ("Error parsing object to RoadTile");
			return null;
		}
	}
}