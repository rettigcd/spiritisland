namespace SpiritIsland.NatureIncarnate;

public class WitherBodiesScarStones {
	const string Name = "Wither Bodies, Scar Stones";
	[SpiritCard(Name,1,Element.Sun,Element.Fire,Element.Earth),Slow,FromSacredSite(2)]
	[Instructions( "1 Damage. -or- Add 1 Badlands" ), Artist( Artists.AgnieszkaDabrowiecka )]
	static public async Task ActAsync(TargetSpaceCtx ctx ) {
		await Cmd.Pick1(
			Cmd.DamageInvaders(1), 
			Cmd.AddBadlands(1)
		).ActAsync(ctx);
	}

}