namespace SpiritIsland.JaggedEarth;

[InnatePower("Epidemics Run Rampant", "This Power's Damage is dealt (separately) to both Invaders and Dahan.")]
[Fast, FromPresence(1,Filter.Disease)]
public class EpidemicsRunRampant {

	// !! add a sub-text desrcription option that dispays
	// "This Power's Damage is dealth (separately) to both Invaders and dahan."

	[InnateTier("1 fire,3 animal","1 Damage per disease.")]
	static public Task Damage1( TargetSpaceCtx ctx ) {
		return DiseaseDamagesInvaders( ctx, 1 );
	}

	[InnateTier("1 water,2 fire,4 animal","+1 Damage per disease.")]
	static public Task Damage2( TargetSpaceCtx ctx ) {
		return DiseaseDamagesInvaders( ctx, 2 );
	}

	[InnateTier("3 water,3 fire,5 animal","+1 Damage per disease. 1 fear per disease (max 5). Remove 1 disease.")]
	static public async Task Damage3( TargetSpaceCtx ctx ) {
		await DiseaseDamagesInvaders( ctx, 2 );
		await ctx.AddFear(System.Math.Min(5, ctx.Disease.Count));
		await ctx.Disease.Remove(1, RemoveReason.Removed);
	}
	
	static Task DiseaseDamagesInvaders( TargetSpaceCtx ctx, int damagePerDisease=1 ) {
		return ctx.DamageInvaders( ctx.Disease.Count * damagePerDisease );
	}

}