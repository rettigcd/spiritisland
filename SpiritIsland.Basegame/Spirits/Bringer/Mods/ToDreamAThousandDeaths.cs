namespace SpiritIsland.Basegame;


// When doing Damage, Track initial state + Damage
public class ToDreamAThousandDeaths(Spirit spirit) : BaseModEntity, IHandleInvaderDamaged, IModifyRemovingToken {

	public static int[] DreamFear { get; set; } = [0, 2, 5];

	public const string Name = "To Dream a Thousand Deaths";
	const string Description = "Your Powers never cause Damage, nor can they Destroy anything other than your own Presence. When your Powers would Destroy Invaders, instead generate 0/2/5 Fear and Pushes Explorers/Towns";
	static public SpecialRule Rule => new( Name, Description );

	// Track invader damage
	void IHandleInvaderDamaged.HandleDamage(HumanToken before, HumanToken after, Space space) {
		if( spirit.ActionIsMyPower ) {
			var dreamDamageLog = space.OfType<DreamingDamageLog>().FirstOrDefault();
			if( dreamDamageLog is null ) {
				dreamDamageLog = new DreamingDamageLog();
				space.Adjust(dreamDamageLog, 1);
				ActionScope.Current.AtEndOfThisAction(scope => { 
					dreamDamageLog.RollBackAll(space);
					space.Adjust(dreamDamageLog,0); 
				} ); 
			}
			dreamDamageLog.Record(before,after);
		}
	}

	// When an invader is destroyed, roll back the damage and push them instead.
	async Task IModifyRemovingToken.ModifyRemovingAsync(RemovingTokenArgs args) {
		if( args.Reason == RemoveReason.Destroyed && args.From is Space space && args.Token is HumanToken humanToDestroy ) {

			// Verify that the things we are removing, are actually on the space, so that logic below is easier to verify/think about.
			int available = space[args.Token];
			if( available < args.Count )
				throw new InvalidOperationException($"Can't remove {args.Count} when there are only {available} of them.");

			var dreamDamageLog = space.OfType<DreamingDamageLog>().FirstOrDefault();

			// not from damage
			while( 0 < args.Count ) {

				if( dreamDamageLog is not null )
					humanToDestroy = dreamDamageLog.RollbackDamage(humanToDestroy, space);

				// Don't destroy
				--args.Count;

				// push it
				if(humanToDestroy.HasAny(Human.Explorer_Town))
					// Explorers Towns => Push
					await humanToDestroy.On(space).PushAsync(spirit);
				else {
					// City => hide it and restore it at end of round so we can't destroy it twice.
					space.Adjust(humanToDestroy,-1); // remove it for now so it can't be selected again
					ActionScope.Current.AtEndOfThisAction(scope=>space.Adjust(humanToDestroy,1)); // restore it at end of round.
					// !! this prevents actions doing other things to the city like adding strife to it, etc.
				}

				// fear
				int fear = humanToDestroy.HasTag(Human.City) ? DreamFear[2]
					: humanToDestroy.HasTag(Human.Town) ? DreamFear[1]
					: DreamFear[0];
				await space.AddFear(fear);
			}

		}
	}
}


/// <summary>
/// Records damage on one particular space, then rolls it back at end of round.
/// </summary>
class DreamingDamageLog : BaseModEntity, IEndWhenTimePasses {

	public void Record(HumanToken original,HumanToken damaged) {
		Transition? endingOn = _appliedDamage.FirstOrDefault(x=>x.damaged == original);
		if(endingOn is null)
			_appliedDamage.Add(new Transition { original = original, damaged = damaged });
		else
			endingOn.damaged = damaged;
	}

	public HumanToken RollbackDamage(HumanToken orig, Space space) {
		var transition = _appliedDamage.FirstOrDefault(x => x.damaged == orig);
		if( transition is not null ) {
			// remove rollback
			_appliedDamage.Remove(transition);
			transition.RollBack(space);
			orig = transition.original;
		}
		return orig;
	}

	public void RollBackAll(Space space) {
		foreach( var transition in _appliedDamage )
			if( 0 < space[transition.damaged] )
				transition.RollBack(space);
		_appliedDamage.Clear();
	}

	#region private

	class Transition {
		public required HumanToken original;
		public required HumanToken damaged;
		public void RollBack(Space space) {
			space.Adjust(damaged, -1);
			space.Adjust(original, 1);
		}
	}

	readonly List<Transition> _appliedDamage = [];

	#endregion

}
