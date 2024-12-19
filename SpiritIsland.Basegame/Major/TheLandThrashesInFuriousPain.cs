namespace SpiritIsland.Basegame;

public class TheLandThrashesInFuriousPain {

	[MajorCard("The Land Thrashes in Furious Pain",4, Element.Moon, Element.Fire,Element.Earth),Slow,FromPresence(2,Filter.Blight)]
	[Instructions( "2 Damage per Blight in target land. +1 Damage per Blight in adjacent lands. -If you have- 3 Moon, 3 Earth: Repeat on an adjacent land." ), Artist( Artists.NolanNasser )]
	static public async Task ActAsync(TargetSpaceCtx ctx) {

		static Task DamageLandFromBlight( TargetSpaceCtx ctx ) {
			// 2 damage per blight in target land
			int damage = ctx.BlightOnSpace * 2
				// +1 damage per blight in adjacent lands
				+ ctx.Adjacent.Sum( x => x.Blight.Count );
			return ctx.DamageInvaders( damage );
		}

		await DamageLandFromBlight( ctx );

		// if you have 3 moon 3 earth
		if(await ctx.YouHave("3 moon,3 earth")) {
			// repeat on an adjacent land.
			var alsoTarget = await ctx.Self.Select( new A.SpaceDecision( "Select additional land to receive blight damage", ctx.Space.Adjacent, Present.Always));
			if(alsoTarget is null) return; // isolated land has no adjacent
			await DamageLandFromBlight( ctx.Target( alsoTarget ) );
		}
	}

}