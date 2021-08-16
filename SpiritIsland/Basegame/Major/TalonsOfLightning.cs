using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class TalonsOfLightning {

		[MajorCard( "Talons of Lightning", 6, Speed.Fast, Element.Fire, Element.Air )]
		[FromPresence(1,Target.MountainOrWetland)]
		static public async Task ActionAsync( ActionEngine engine, Space target ) {

			// if you have: 3 fire, 3 air
			bool bonus = engine.Self.Elements.Contains( "3 fire, 3 air" );

			// !!! hack/bug! - if spirit wants to reuse card on the same target, they will get to pick a different one
			// Could create an additional [SelectsTarget] attribute that gets loaded by PowerCard.For<...?
			if(bonus)
				// increase this Power's Range to 3
				target = await engine.Self.PowerCardApi.TargetSpace(
					engine,
					engine.Self.Presence.Spaces,
					3, 
					TargetSpaceAttribute.ToLambda(engine,Target.MountainOrWetland)
				);

			// 3 fear
			engine.GameState.AddFear(3);
			// 5 damage
			engine.GameState.DamageInvaders(target,5);

			if(bonus)
				// destory 1 town in each adjacent land
				foreach(var neighbor in target.Adjacent)
					engine.GameState.InvadersOn(neighbor).Destroy(Invader.Town,1);

		}

	}
}

