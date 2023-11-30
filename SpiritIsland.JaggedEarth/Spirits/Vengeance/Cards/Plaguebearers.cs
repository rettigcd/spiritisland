namespace SpiritIsland.JaggedEarth;

public class Plaguebearers {

	[SpiritCard("Plaguebearers",1,Element.Fire,Element.Water,Element.Animal), Slow, FromPresence(2,Target.Disease)]
	[Instructions( "1 Fear if Invaders are present. For each Disease, Push 2 Explorer / Town / Dahan. 1 Disease may move with each Pushed piece." ), Artist( Artists.DamonWestenhofer )]
	static public async Task ActAsync(TargetSpaceCtx ctx ) {

		// 1 fear if invaders are present.
		if(ctx.HasInvaders)
			ctx.AddFear(1);

		// For each disease, Push 2 explorer / town / dahan.
		await ctx.SourceSelector
			.AddGroup( ctx.Disease.Count, Human.Explorer_Town.Plus(Human.Dahan) )
			.Bring( Bring.FromAnywhere(ctx.Self,new Quota().AddGroup(1,Token.Disease) ) )
			.PushN(ctx.Self);

	}

}