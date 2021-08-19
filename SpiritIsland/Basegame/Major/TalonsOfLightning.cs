using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	public class TalonsOfLightning {

		[MajorCard( "Talons of Lightning", 6, Speed.Fast, Element.Fire, Element.Air )]
		[FromPresence(1,Target.MountainOrWetland)]
		static public async Task ActionAsync( TargetSpaceCtx ctx ) {

			// if you have: 3 fire, 3 air
			bool bonus = ctx.Self.Elements.Contains( "3 fire, 3 air" );

			// !!! hack/bug! - if spirit wants to reuse card on the same target, they will get to pick a different one
			// Could create an additional [SelectsTarget] attribute that gets loaded by PowerCard.For<...?
			Space target= ctx.Target;
			if(bonus)
				// increase this Power's Range to 3
				target = await ctx.Self.PowerCardApi.TargetSpace(
					ctx.Self,
					ctx.GameState,
					From.Presence,
					null,
					3, 
					Target.MountainOrWetland
				);

			// 3 fear
			ctx.AddFear(3);
			// 5 damage
			await ctx.DamageInvaders(target, 5);

			if(bonus)
				// destory 1 town in each adjacent land
				foreach(var neighbor in target.Adjacent)
					await ctx.InvadersOn(neighbor).Destroy(Invader.Town,1);

		}

	}
}

