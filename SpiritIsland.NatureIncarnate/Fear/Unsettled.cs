namespace SpiritIsland.NatureIncarnate;

public class Unsettled : FearCardBase, IFearCard {

	public const string Name = "Unsettled";
	public string Text => Name;

	[FearLevel(1, "On Each Board: Choose a land with Beast/Strife/Dahan. Downgrade 1 Town/City." )]
	public Task Level1( GameState gs )
		=> DowngradeTownOrCity
			.In().OneLandPerBoard().Which( Has.BeastStrifeOrDahan )
			.ForEachBoard()
			.ActAsync( gs );

	[FearLevel( 2, "On Each Board: Choose a land with Beast/Strife/Dahan. Downgrade 1 Town/City there or skip the next Build Action there (this turn)." )]
	public Task Level2( GameState gs )
		=> Cmd.Pick1(
			DowngradeTownOrCity,
			Cmd.Skip1Build(Name)
        )
			.In().OneLandPerBoard().Which( Has.BeastStrifeOrDahan )
			.ForEachBoard()
			.ActAsync( gs );

	[FearLevel( 3, "On Each Board: Choose a land with Beast/Strife/Dahan. Remove 1 Invader there or skip the next Build Action there (this turn)." )]
	public Task Level3( GameState gs )
		=> Cmd.Pick1(
			Cmd.RemoveInvaders(1),
			Cmd.Skip1Build(Name)
        )
			.In().OneLandPerBoard().Which( Has.BeastStrifeOrDahan )
			.ForEachBoard()
			.ActAsync( gs );

	static SpaceAction DowngradeTownOrCity => new SpaceAction(
		"Downgrade 1 Town/City",
		ctx => ReplaceInvader.Downgrade1(ctx.Self, ctx.Tokens,Present.Always,Human.Town_City)
	);

}

