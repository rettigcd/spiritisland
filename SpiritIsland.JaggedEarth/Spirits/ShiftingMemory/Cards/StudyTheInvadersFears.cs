namespace SpiritIsland.JaggedEarth;

public class StudyTheInvadersFears {

	[SpiritCard("Study the Invaders' Fears",0,Element.Moon,Element.Air,Element.Animal), Fast, FromPresence(0,Target.TownOrCity)]
	static public async Task ActAsync(TargetSpaceCtx ctx ) { 
		// 2 fear.
		ctx.AddFear(2);

		// Turn the top card of the Fear Deck face-up.
		await ctx.FlipFearCard( ctx.GameState.Fear.Deck.Peek() );
	}

}