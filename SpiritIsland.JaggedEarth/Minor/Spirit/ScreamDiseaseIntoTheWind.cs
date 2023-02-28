using System.Xml.Linq;

namespace SpiritIsland.JaggedEarth;

public class ScreamDiseaseIntoTheWind{

	public const string Name = "Scream Disease Into the Wind";

	[MinorCard(Name,1,Element.Air,Element.Water,Element.Animal),Fast,AnotherSpirit]
	static public Task ActAsync(TargetSpiritCtx ctx){
		// Target Spirit gets +1 range with all their Powers.
		RangeCalcRestorer.Save(ctx.Other,ctx.GameState);
		RangeExtender.Extend( ctx.Other, 1 );

		ActionScope.StartOfActionHandlers.Add( new ScreamingDiseaseActionHandlers(ctx.Other) );

		// (Hand them a disease token as a reminder.)
		return Task.CompletedTask;
	}

}

class ScreamingDiseaseActionHandlers : IRunAtStartOfAction {

	#region constructor

	public ScreamingDiseaseActionHandlers(Spirit targetSpirit) {
		_round = GameState.Current.RoundNumber;
		_targetSpirit = targetSpirit;
	}

	#endregion

	public Task Start( ActionScope startingScope ) {
		// Add to the end of EVERY Action this round.
		if(_round == GameState.Current.RoundNumber)
			startingScope.AtEndOfThisAction( EndOfRoundCheck );
		else
			ActionScope.StartOfActionHandlers.Remove( this );
		return Task.CompletedTask;
	}

	async Task EndOfRoundCheck( ActionScope endScope ) {
		SpaceState ss=null;
		bool add = !_used
			&& endScope.Category == ActionCategory.Spirit_Power
			&& endScope.Owner == _targetSpirit
			&& (ss = TargetSpaceAttribute.TargettedSpace) != null
			&& await _targetSpirit.UserSelectsFirstText( ScreamDiseaseIntoTheWind.Name + " (" + ss.Space.Label + ")", "Yes, add 1 disease", "No thank you" );
		if( add ) {
			_used = true;
			await ss.Disease.Add( 1 );
		}
	}

	#region private 
	readonly int _round;
	readonly Spirit _targetSpirit;

	bool _used = false;
	#endregion

}