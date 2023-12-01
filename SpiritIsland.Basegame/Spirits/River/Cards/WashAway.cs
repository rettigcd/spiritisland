namespace SpiritIsland.Basegame;

public class WashAway {

	public const string Name = "Wash Away";

	[SpiritCard(WashAway.Name, 1, Element.Water, Element.Earth)]
	[Slow, FromPresence(1,Filter.Any)]
	[Preselect("Push up to (3)", "Explorer,Town", Present.Done )] // !!! Implements as Present.Always, need Present.Done option
	[Instructions( "Push up to 3 Explorer / Town" ), Artist( Artists.NolanNasser )]
	static public async Task ActionAsync(TargetSpaceCtx ctx){
		await ctx.PushUpTo( 3, Human.Explorer_Town );
	}

}