﻿using System;


public class Location {
	public int x;
	public int y;

	public Location (int x, int y) {
		this.x = x;
		this.y = y;
	}

	public static Location ParseToLocation (object obj) {
		try {
			Location location = (Location) obj;
			return location;
		} catch (Exception e) {
			return null;
		}
	}

	public override bool Equals (object obj) {
		Location location = Location.ParseToLocation (obj);
		if (location == null) {
			return false;
		}
		return (location.x == this.x && location.y == this.y);
	}

	public override int GetHashCode () {
		return this.x.GetHashCode () ^ this.y.GetHashCode ();
	}
}