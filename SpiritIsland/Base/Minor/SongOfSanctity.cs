using System;
using System.Linq;
using System.Threading.Tasks;
using SpiritIsland.Core;

namespace SpiritIsland.Base {

	[MinorCard(SongOfSanctity.Name, 1, Speed.Slow,Element.Sun,Element.Water,Element.Plant)]
	public class SongOfSanctity : BaseAction{

		public const string Name = "Song of Sanctity";

		public SongOfSanctity(Spirit spirit,GameState gs)
			:base(spirit,gs)
		{
			_ = ActionAsync();
		}

		async Task ActionAsync(){

			bool JungleOrMountain_Plus_InvadersOrBlight( Space space ) {
				return space.Terrain.IsIn(Terrain.Mountain,Terrain.Jungle)
					&& (gameState.HasBlight(space) || gameState.InvadersOn(space).HasExplorer);
			}

			var target = await engine.Api.TargetSpace_Presence(1,JungleOrMountain_Plus_InvadersOrBlight);

			var group = gameState.InvadersOn(target);

			if( group[Invader.Explorer]>0 )
				await PushAllExplorers(group);
			else if(gameState.HasBlight(target))
				gameState.AddBlight(target,-1);

		}

		async Task PushAllExplorers(InvaderGroup group){
			while(group[Invader.Explorer]>0){
				var destination = await engine.SelectSpace("Select destination to push explorer"
					,group.Space.Neighbors,true);
				if(destination==null) break;
				new MoveInvader(Invader.Explorer, group.Space, destination).Apply(gameState);
				--group[Invader.Explorer];
			}
		}


	}

}
