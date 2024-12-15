
namespace SpiritIsland.Basegame;

public class EarthsVitality(Spirit spirit) : SpiritPresenceToken(spirit)
	, IHandleTokenAdded
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
				if( space[Token.Defend] < 3 ) throw new Exception("Defend sync issue with Earths Vitality");
				space.Defend.Add(-3);
			}
		}
		return base.HandleTokenRemovedAsync(args);
	}

}