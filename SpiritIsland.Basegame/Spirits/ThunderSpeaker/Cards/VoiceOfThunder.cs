namespace SpiritIsland.Basegame;

public class VoiceOfThunder {

	[SpiritCard( "Voice of Thunder", 0, Element.Sun, Element.Air ),Slow,FromPresence(1)]
	[Instructions("Push up to 4 Dahan. -or- If Invaders are Present, 2 Fear." ), Artist( Artists.LoicBelliau )]
	static public Task Act( TargetSpaceCtx ctx ) {

		return ctx.SelectActionOption(
			new SpaceCmd("push up to 4 dahan", ctx => ctx.PushUpToNDahan( 4 ) ).OnlyExecuteIf( x=>x.Dahan.Any ),
			new SpaceCmd("2 fear", ctx => ctx.AddFear(2) ).OnlyExecuteIf( x=>x.Tokens.HasInvaders() )
		);

	}
}