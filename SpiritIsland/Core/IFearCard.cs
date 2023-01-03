namespace SpiritIsland;

public interface IFearCard : IOption {

	int? ActivatedTerrorLevel { get; set; }
	bool Flipped { get; set; }

	void Activate( GameState gameState );

	Task Level1( GameCtx ctx );
	Task Level2( GameCtx ctx );
	Task Level3( GameCtx ctx );
}

public class FearCardBase {
	public int? ActivatedTerrorLevel { get; set; }
	public bool Flipped { get; set; }

	public virtual void Activate( GameState gameState ) {
		var topCard = gameState.Fear.Deck.Pop();
		if(topCard != this)
			throw new InvalidOperationException( "Fear card must be on top of deck to activate it." );
		gameState.Fear.ActivatedCards.Push( topCard );
	}
}

static public class IFearOptionsExtension {

	static public string GetDescription( this IFearCard options, int activation ) {
		var memberName = "Level" + activation;

		// This does not find interface methods declared as: void IFearCardOption.Level2(...)
		var member = options.GetType().GetMethod( memberName )
			?? throw new Exception( memberName + " not found on " + options.GetType().Name );

		var attr = (FearLevelAttribute)member.GetCustomAttributes( typeof( FearLevelAttribute ) ).Single();
		string description = attr.Description;
		return description;
	}

}
