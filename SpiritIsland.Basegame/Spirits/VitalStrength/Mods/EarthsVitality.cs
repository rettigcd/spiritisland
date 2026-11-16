
namespace SpiritIsland.Basegame;

public class EarthsVitality(Spirit spirit) : SpiritPresenceToken(spirit)
	, IHandleTokenAdded
	, ICleanupSpaceWhenTimePasses
// , IHandleTokenRemoved - already implemented in base class
{
	public const string Name = "Earth's Vitality";
	const string Description = "Defend 3 in every land where you have sacred site.";
	static public SpecialRule Rule => new SpecialRule(Name, Description);

	/// <summary> Used for setting up Aspects </summary>
	static public void ReplaceWith( Spirit spirit, SpiritPresenceToken newToken ) {
		var old = spirit.Presence;
		spirit.Presence = new SpiritPresence(spirit, old.Energy, old.CardPlays, newToken );
	}

	Task IHandleTokenAdded.HandleTokenAddedAsync(Space to, ITokenAddedArgs args) {
		if(args.Added == this && args.To is Space space ) {
			int current = space[this];
			int previous = current - args.Count;
			if( previous < 2 && 2<= current ) {
				space.Defend.Add(3);
			}
		}
		return Task.CompletedTask;
	}

	public override Task HandleTokenRemovedAsync(ITokenRemovedArgs args) {
		if( args.Removed == this && args.From is Space space ) {
			int current = space[this];
			int previous = current + args.Count;
			if( current < 2 && 2 <= previous ) {
				// Take away what we gave. (Defend is a shared pool and is cleared every round, so never take more than is there.)
				int defend = Math.Min( 3, space[Token.Defend] );
				if( 0 < defend ) space.Defend.Add( -defend );
			}
		}
		return base.HandleTokenRemovedAsync(args);
	}

	/// <summary> Defend is cleared every round, but this is a standing effect: give it back for the next round. </summary>
	void ICleanupSpaceWhenTimePasses.CleanupSpace( Space space ) {
		if( 2 <= space[this] )
			space.Defend.Add( 3 );
	}

	/// <summary> A sacred site that is there from the start of the game (placed with no 'token added' event) has its Defend too. </summary>
	public void DefendStartingSacredSite( Space space ) {
		if( 2 <= space[this] )
			space.Defend.Add( 3 );
	}

}