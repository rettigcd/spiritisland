using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiritIsland.PowerCards {

	[PowerCard(SongOfSanctity.Name, 1, Speed.Slow,Element.Sun,Element.Water,Element.Plant)]
	public class SongOfSanctity : TargetSpaceAction {

		public const string Name = "Song of Sanctity";

		public SongOfSanctity(Spirit spirit,GameState gs)
			:base(spirit,gs,1,From.Presence){}

		protected override bool FilterSpace( Space space ) {
			return space.Terrain.IsIn(Terrain.Mountain,Terrain.Jungle)
				&& (gameState.HasBlight(space) || gameState.InvadersOn(space).HasExplorer);
		}

		protected override void SelectSpace(Space space){
			var group = gameState.InvadersOn(space);
			int explorerCount = group[Invader.Explorer];
			if(explorerCount>0)
				engine.decisions.Push(new SelectInvadersToPush(group,explorerCount,"Explorer"));
			else if(gameState.HasBlight(space))
				gameState.AddBlight(space,-1);
			else
				throw new ArgumentOutOfRangeException();
		}

	}

}
