namespace SpiritIsland;

public interface IFearCard : IOption {

	int? ActivatedTerrorLevel { get; set; }
	bool Flipped { get; set; }

	void Activate( GameState gameState );

	Task Level1( GameState gameState );
	Task Level2( GameState gameState );
	Task Level3( GameState gameState );
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

/// <summary>
/// When all Spirits have to coordinate on something. (pay Energy, ACK card turned up, etc)
/// </summary>
static class AllSpirits {

	static public async Task Acknowledge( string prompt, string text, object item ) {
		var options = new Acknowledgement[] { new Acknowledgement( text, item ) };
		IEnumerable<Task> spiritsAck = GameState.Current.Spirits.Select( async spirit => { 
			await spirit.Select( prompt, options, Present.Always );
		} );
		await Task.WhenAll( spiritsAck );
	}

}

/// <summary>
/// Generates the Acknowledgment Pop-Up in the UI
/// </summary>
/// <remarks>
/// This is required because some items implement their own IOption and we do not want them appearing as the ACK-popup.
/// </remarks>
public class Acknowledgement : IOption {

	public Acknowledgement(string text, object item ) {
		Text = text;
		Item = item;
	}
	public string Text { get; }
	public object Item { get; }
}