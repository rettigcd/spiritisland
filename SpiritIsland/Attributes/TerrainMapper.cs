using System;

namespace SpiritIsland {

	public class TerrainMapper {

		static public TerrainMapper For(Cause cause) => cause switch {
			Cause.Power  => TerrainMapper.ForPowerAndBlight,
			Cause.Blight => TerrainMapper.ForPowerAndBlight,
			_            => TerrainMapper.Normal
		};

		static readonly TerrainMapper Normal = new TerrainMapper();
		static readonly TerrainMapper ForPowerAndBlight = new TerrainForPowerAndBlight();

		public virtual Terrain GetTerrain(Space space) => space.Terrain;
		public virtual bool IsCoastal(Space space) => space.IsCoastal;

		class TerrainForPowerAndBlight : TerrainMapper {
			public override Terrain GetTerrain( Space space ) => space.TerrainForPower;
			public override bool IsCoastal( Space space ) => space.IsCostalForPower;

		}

	}

}
