namespace SpiritIsland.JaggedEarth;

public class StudyTheInvadersFears {

	[SpiritCard("Study the Invaders' Fears",0,Element.Moon,Element.Air,Element.Animal), Fast, FromPresence(0,Filter.Town,Filter.City)]
	[Instructions( "2 Fear. Turn the top card of the Fear Deck face-up." ), Artist( Artists.JoshuaWright )]
	static public void ActAsync(TargetSpaceCtx ctx ) { 
		// 2 fear.
		ctx.AddFear(2);

		// Turn the top card of the Fear Deck face-up.
		var fear = GameState.Current.Fear;
		fear.FlipFearCard( fear.Deck.Peek() );
	}

}