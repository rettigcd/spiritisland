namespace SpiritIsland.NatureIncarnate;

public class StrugglesOverFarmland : FearCardBase, IFearCard {

	public const string Name = "Struggles Over Farmland";
	public string Text => Name;

	[FearLevel(1, "Each player Adds 1 Strife in a land with Blight." )]
	public override Task Level1( GameState gs )
		=> Cmd.AddStrife(1)
			.In().SpiritPickedLand().Which( Has.Blight )
			.ForEachSpirit()
			.ActAsync( gs );

	[FearLevel( 2, "Each player Adds 1 Strife to a Town or Adds 1 Strife in a land with Blight." )]
	public override Task Level2( GameState gs )
		=> Cmd.Pick1(
			Cmd.AddStrife(1).In().SpiritPickedLand().Which( Has.Blight ),
			Cmd.AddStrifeTo(1,Human.Town).In().SpiritPickedLand()
		)
			.ForEachSpirit()
			.ActAsync( gs );

	[FearLevel( 3, "Each player Adds 1 Strife. In each land with Blight, 1 Invader with Strife does Damage to other Invaders." )]
	public override Task Level3( GameState gs )
		=> Cmd.Multiple(
			Cmd.AddStrife(1).In().SpiritPickedLand().ForEachSpirit(),
			StrifedInvaderDamagesOthers.In().EachActiveLand().Which(Has.Blight)
        )
			.ActAsync( gs );

	static SpaceAction StrifedInvaderDamagesOthers => new SpaceAction("1 Invader with Strife does Damage to other Invaders.", 
		async ctx => {
			var strifedInvader = ctx.Space.HumanOfAnyTag(Human.Invader)
				.Where(h=>0<h.StrifeCount)
				.OrderByDescending(x=>x.Attack)
				.FirstOrDefault();
			if(strifedInvader != null)
				await ctx.StrifedDamageOtherInvaders( 
					strifedInvader.Attack, // total damage from this type.
					strifedInvader, // the source of the damage
					ctx.Space[strifedInvader]==1 // exclude source if there is only 1 - it can't damage itself.
				);
		} );

}

