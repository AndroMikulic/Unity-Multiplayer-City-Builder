using System;

[Serializable]
public class Building : Entity {
	public int type;
	public int size;
	public string name;
	public int population;
	public int resource;

	public static Building ParseToBuilding (dynamic obj) {
		Building building = new Building ();
		try {
			building.entityType = obj.entityType;
			Console.WriteLine (obj.location.x + " " + obj.location.y);
			building.location = new Location ((int) obj.location.x, (int) obj.location.y);
			building.type = obj.type;
			building.size = obj.size;
			building.name = obj.name;
			building.population = obj.population;
			building.resource = obj.resource;
			return building;
		} catch {
			Console.WriteLine ("Error parsing object to Building");
			return null;
		}
	}
}