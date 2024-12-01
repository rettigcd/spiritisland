namespace SpiritIsland.NatureIncarnate;

public class SeekCompany : FearCardBase, IFearCard {

	public const string Name = "Seek Company";
	public string Text => Name;

	[FearLevel(1, "On Each Board: Gather up to 1 Explorer into a land with 2 or more Invaders." )]
	public override Task Level1( GameState gs )
		=> Cmd.GatherUpToNExplorers(1)
			.To().OneLandPerBoard().Which( Has.AtLeastN(2, Human.Invader ) )
			.ForEachBoard()
			.ActAsync( gs );

	[FearLevel( 2, "On Each Board: Gather up to 3 Explorer/Town from a single land into a land with 2 or more Invaders." )]
	public override Task Level2( GameState gs )
		=> new SpaceAction( $"Gather up to 3 Explorer/Town", ctx => ctx.Gatherer.AddGroup(3,Human.Explorer_Town).ConfigSource(SelectFrom.FromASingleLand).DoUpToN() )
			.To().OneLandPerBoard().Which( Has.AtLeastN(2, Human.Invader ) )
			.ForEachBoard()
			.ActAsync( gs );

	[FearLevel( 3, "On Each Board: Gather up to 4 Explorer/Town (total) into lands with 2 or more Invaders." )]
	public override Task Level3( GameState gs )
		=> new SpaceAction( $"Gather up to 4 Explorer/Town", ctx => ctx.Gatherer
			.AddGroup(3,Human.Explorer_Town)
			.ConfigSource(SelectFrom.FromASingleLand)
			.DoUpToN()
		)
			.To().OneLandPerBoard().Which( Has.AtLeastN(2, Human.Invader ) ) // !!! This is supposed to be multiple lands, not 1.  But we don't have "Gather" from multiple working yet.
			.ForEachBoard()
			.ActAsync( gs );


}

