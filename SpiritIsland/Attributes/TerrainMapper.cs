﻿using System;

namespace SpiritIsland {

	public class TerrainMapper {

//		public virtual Terrain GetTerrain(Space space) => space.Terrain;
		public virtual bool IsOneOf(Space space, params Terrain[] options) => space.IsOneOf(options);
		public virtual bool IsCoastal(Space space) => space.IsCoastal;

	}

}
