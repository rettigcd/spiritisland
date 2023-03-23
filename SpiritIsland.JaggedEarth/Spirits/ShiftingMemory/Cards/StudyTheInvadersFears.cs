namespace SpiritIsland.JaggedEarth;

public class StudyTheInvadersFears {

	[SpiritCard("Study the Invaders' Fears",0,Element.Moon,Element.Air,Element.Animal), Fast, FromPresence(0,Target.Town,Target.City)]
	[Instructions( "2 Fear. Turn the top card of the Fear Deck face-up." ), Artist( Artists.JoshuaWright )]
	static public async Task ActAsync(TargetSpaceCtx ctx ) { 
		// 2 fear.
		ctx.AddFear(2);

		// Turn the top card of the Fear Deck face-up.
		await ctx.FlipFearCard( ctx.GameState.Fear.Deck.Peek() );
	}

}