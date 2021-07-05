﻿using System;
using System.Linq;

namespace SpiritIsland.Base {
	static public class ContainerExtensions{
		// shorter syntax:
		// space.Terrain.IsIn(Terrain.Wetland,Terrain.Sand)
		// vs.
		// new Terraion[]{Terrain.Wetland,Terrain.Sand}.Contains(space.Terrain);
		static public bool IsIn<T>(this T needle, params T[] haystack )
			=> haystack.Contains(needle);
	}

}
