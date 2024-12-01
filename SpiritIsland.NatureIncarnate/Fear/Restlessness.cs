namespace SpiritIsland.NatureIncarnate;

public class Restlessness : FearCardBase, IFearCard {

	public const string Name = "Restlessness";
	public string Text => Name;

	[FearLevel(1, "Each player Pushes up to 1 Explorer/Town from a land not matching a Build card." )]
	public override Task Level1( GameState gs )
		=> Cmd.PushUpToNInvaders(1, Human.Explorer_Town)
			.In().SpiritPickedLand().Which( Is.NotBuildCardMatch )
			.ForEachSpirit()
			.ActAsync( gs );

	[FearLevel( 2, "Each player Pushes up to 3 Explorer/Town from a land not matching a Build card." )]
	public override Task Level2( GameState gs )
		=> Cmd.PushUpToNInvaders(3, Human.Explorer_Town)
			.In().SpiritPickedLand().Which( Is.NotBuildCardMatch )
			.ForEachSpirit()
			.ActAsync( gs );

	[FearLevel( 3, "Each player Removes up to 3 Explorer/Town from a land not matching a Build card." )]
	public override Task Level3( GameState gs )
		=> Cmd.RemoveExplorersOrTowns(3)
			.In().SpiritPickedLand().Which( Is.NotBuildCardMatch )
			.ForEachSpirit()
			.ActAsync( gs );

}

