using System.Xml.Linq;

namespace SpiritIsland.JaggedEarth;

public class ScreamDiseaseIntoTheWind{

	public const string Name = "Scream Disease Into the Wind";

	[MinorCard(Name,1,Element.Air,Element.Water,Element.Animal),Fast,AnotherSpirit]
	[Instructions( "Target Spirit gets +1 Range with all their Powers. Once this turn, after target Spirit uses a Power targeting a land, they may add 1 Disease to that land. (Hand them a Disease token as a reminder.)" ), Artist( Artists.MoroRogers )]
	static public Task ActAsync(TargetSpiritCtx ctx){
		// Target Spirit gets +1 range with all their Powers.
		RangeCalcRestorer.Save(ctx.Other);
		RangeExtender.Extend( ctx.Other, 1 );

		ActionScope.StartOfActionHandlers.Add( new ScreamingDiseaseActionHandlers(ctx.Other) );

		// (Hand them a disease token as a reminder.)
		return Task.CompletedTask;
	}

}

class ScreamingDiseaseActionHandlers( Spirit _targetSpirit ) : IRunAtStartOfAction {

	public Task Start( ActionScope startingScope ) {
		// Add to the end of EVERY Action this round.
		if(_round == GameState.Current.RoundNumber)
			startingScope.AtEndOfThisAction( EndOfRoundCheck );
		else
			ActionScope.StartOfActionHandlers.Remove( this );
		return Task.CompletedTask;
	}

	async Task EndOfRoundCheck( ActionScope endScope ) {
		Space ss=null;
		bool add = !_used
			&& endScope.Category == ActionCategory.Spirit_Power
			&& endScope.Owner == _targetSpirit
			&& (ss = TargetSpaceAttribute.TargettedSpace) != null
			&& await _targetSpirit.UserSelectsFirstText( ScreamDiseaseIntoTheWind.Name + " (" + ss.SpaceSpec.Label + ")", "Yes, add 1 disease", "No thank you" );
		if( add ) {
			_used = true;
			await ss.Disease.AddAsync( 1 );
		}
	}

	#region private 
	readonly int _round = GameState.Current.RoundNumber;
	bool _used = false;
	#endregion

}