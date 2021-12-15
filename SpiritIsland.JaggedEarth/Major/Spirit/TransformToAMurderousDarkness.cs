using System.Threading.Tasks;

namespace SpiritIsland.JaggedEarth {

	public class TransformToAMurderousDarkness {

		[MajorCard("Transform to a Murderous Darkness",6,Element.Moon,Element.Fire,Element.Air,Element.Water,Element.Plant), Slow, AnySpirit]
		public static async Task ActAsync(TargetSpiritCtx ctx ) {
			// Target Spirt may choose one of their Sacred Sites.
			var space = await ctx.OtherCtx.Presence.SelectSacredSite( "Replace presence with badlands" );

			await TargetSpiritActions( ctx.OtherCtx.Target( space ) );

			// if you have 3 moon,2 fire,2 air: 
			if(await ctx.YouHave("3 moon,2 fire,2 air" )){
				var sCtx = ctx.Target(space);
				// 1 damage in an adjactnt land.
				await DamageInAdjacentLand( sCtx );
				// 1 damage in an adjactnt land.
				await DamageInAdjacentLand( sCtx );
			}

		}

		static async Task DamageInAdjacentLand( TargetSpaceCtx ctx ) {
			var adjCtx = await ctx.SelectAdjacentLand("1 damage");
			if(adjCtx != null)
				await adjCtx.DamageInvaders(1);
		}


		static async Task TargetSpiritActions( TargetSpaceCtx otherCtx ) {
			// In that land: Replace each of their presence with badlands;  The replaced presence leaves the game.
			int count = otherCtx.Self.Presence.CountOn( otherCtx.Space );
			for(int i = 0; i < count; ++i)
				await otherCtx.Presence.RemoveFrom( otherCtx.Space );
			await otherCtx.Badlands.Add( count, AddReason.AsReplacement );

			// Push any number of those Badlands.
			await otherCtx.PushUpTo( count, TokenType.Badlands );

			// 3 fear.
			otherCtx.AddFear( 3 );

			// 3 damage per presence replaced
			await otherCtx.DamageInvaders( count * 3 );
		}

	}


}
