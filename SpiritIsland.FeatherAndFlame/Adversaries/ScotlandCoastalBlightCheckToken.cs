using System.Linq;

namespace SpiritIsland.FeatherAndFlame;

class ScotlandCoastalBlightCheckToken 
	: BaseModEntity
	, IHandleTokenAddedAsync
	, IModifyRemovingTokenAsync
{

	const string Name = "Runoff and Bilgewater";

	public ScotlandCoastalBlightCheckToken() {
		GameState.Current.LogDebug($"{Name} - To trigger blight removal from the ocean, remove a blight on adjacent coast.");
	}

	public async Task HandleTokenAddedAsync( ITokenAddedArgs args ) {
		// After a Ravage Action adds Blight to a Coastal Land,
		// add 1 Blight to that board's Ocean (without cascading).
		if(args.Added == Token.Blight && args.Reason == AddReason.Ravage) {
			BlightToken.ForThisAction.ShouldCascade = false;
			var space = args.To.Adjacent_Existing // Ocean is not in play here
				.First( adj => adj.Space.IsOcean ); // ignoring rule about ocean being on this board, just using adjacent
			await space.Blight.Add( 1, AddReason.Ravage );
			GameState.Current.Log(new SpiritIsland.Log.Debug( $"{Name} Blight on {args.To.Space.Text} caused additional blight on {space.Space.Text}"));
		}
	}

	public async Task ModifyRemovingAsync( RemovingTokenArgs args ) {
		// Treat the Ocean as a Coastal Wetland for this rule and for Blight removal/movement.

		// Since we can't target a land differently when removing blight, 
		// We are going to do 'good-enough' by allowing targetting blight on coastland
		// will allow them to instead choose blight in the ocean.

		// When: (1) Removing (2) Blight (3) from space adjacent to ocean blight
		if(args.Reason != RemoveReason.Removed) return;
		if(args.Token != Token.Blight) return;
		var blightedOptions = args.From.Adjacent_Existing
			.Where( adj => adj.Space.IsOcean && adj.Blight.Any )
			.ToList();
		if(blightedOptions.Count==0) return;

		// Find spirit
		Spirit spirit = args.From.Space.Boards[0].FindSpirit();

		// Create list of blight they might want to remove.
		blightedOptions.Add( args.From );

		// selection
		var selectionCriteria = new Select.ASpaceToken( 
			$"{Name} Which blight would you like to remove?",
			blightedOptions.Select( s => new SpaceToken( s.Space, Token.Blight ) ),
			Present.Always
		);

		var spaceToken = await spirit.Gateway.Decision( selectionCriteria );
		if(spaceToken.Space == args.From.Space) return; // they are going with the original

		// otherwise, they selected the ocean
		--args.Count;

		// remove the other blight instead.
		await spaceToken.Space.Tokens.Remove(spaceToken.Token, 1);

	}
}
