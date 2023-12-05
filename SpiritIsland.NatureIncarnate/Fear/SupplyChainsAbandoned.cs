namespace SpiritIsland.NatureIncarnate;

public class SupplyChainsAbandoned : FearCardBase, IFearCard {

	public const string Name = "Supply Chains Abandoned";
	public string Text => Name;

	[FearLevel(1, "On Each Board: Isolate one land." )]
	public Task Level1( GameState gs )
		=> Cmd.Isolate
			.In().OneLandPerBoard()
			.ForEachBoard()
			.ActAsync( gs );

	[FearLevel( 2, "On Each Board: Isolate one land. If Town/City are present, skip all Build Actions (in that land)." )]
	public Task Level2( GameState gs )
		=> Cmd.Multiple(
			Cmd.DamageInvaders(1),
			Cmd.SkipAllBuilds(Name).OnlyExecuteIf(ctx=>ctx.Tokens.HasAny(Human.Town_City))
		)
			.In().OneLandPerBoard()
			.ForEachBoard()
			.ActAsync( gs );

	[FearLevel( 3, "On Each Board: Isolate two lands. In each of those lands, if Town/City are present, skip all Build Actions (in that land)." )]
	public Task Level3( GameState gs )
		=> Cmd.Multiple(
			Cmd.DamageInvaders(1),
			Cmd.SkipAllBuilds(Name).OnlyExecuteIf(ctx=>ctx.Tokens.HasAny(Human.Town_City))
		)
			.In().NDifferentLands(2)
			.ForEachBoard()
			.ActAsync( gs );


}

