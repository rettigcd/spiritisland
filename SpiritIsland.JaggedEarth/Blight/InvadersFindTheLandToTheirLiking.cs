namespace SpiritIsland.JaggedEarth;

public class InvadersFindTheLandToTheirLiking : StillHealthyBlightCard {

	public InvadersFindTheLandToTheirLiking():base("Invaders Find the Land to Their Liking",2) {}

	public override ActionOption<GameState> Immediately

		// If the Terror Level is 1/2/3, add 1/1.5/2 Fear Markers per player to the Fear Pool (round down at TL2)
		=> new ActionOption<GameState>("For Terror Level 1/2/3, add 1/1.5/2 Fear Markers per player to the Fear Pool.", gs => {
			int pc = gs.Spirits.Length;
			int fearCount = gs.Fear.TerrorLevel switch {
				1 => pc*1, // 1
				2 => pc*3/2, // 1.5 round down
				3 => pc*2, // 2
				_ => throw new System.IndexOutOfRangeException( nameof(gs.Fear.TerrorLevel) ),
			};
			gs.Fear.AddDirect(new FearArgs { count = fearCount });
		});

}