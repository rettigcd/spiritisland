namespace SpiritIsland.NatureIncarnate;

public class DahanGainTheEdge : FearCardBase, IFearCard {

	public const string Name = "Dahan Gain the Edge";
	public string Text => Name;

	[FearLevel(1, "Each player chooses a different land with Dahan. In Each: Defend 2" )]
	public Task Level1( GameState gs )
		=> Cmd.Defend(2)
			.In().SpiritPickedLand().Which( Has.Dahan )
			.ForEachSpirit()
			.ActAsync( gs );

	[FearLevel( 2, "Each player chooses a different land with Dahan. In Each: 1 Damage and Defend 3." )]
	public Task Level2( GameState gs )
		=> Cmd.Multiple( Cmd.DamageInvaders(1), Cmd.Defend(2) )
			.In().SpiritPickedLand().Which( Has.Dahan )
			.ForEachSpirit()
			.ActAsync( gs );

	[FearLevel( 3, "Each player chooses a diffferent land with Dahan. In Each: 2 Damage and Defend 4." )]
	public Task Level3( GameState gs )
		=> Cmd.Multiple( Cmd.DamageInvaders(2), Cmd.Defend(4) )
			.In().SpiritPickedLand().Which( Has.Dahan )
			.ForEachSpirit()
			.ActAsync( gs );

}

