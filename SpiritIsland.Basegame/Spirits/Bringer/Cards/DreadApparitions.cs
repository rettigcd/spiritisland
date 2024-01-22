namespace SpiritIsland.Basegame;

public class DreadApparitions {
	public const string Name = "Dread Apparitions";

	[SpiritCard(Name,2,Element.Moon,Element.Air), Fast, FromPresence(1,Filter.Invaders)]
	[Instructions("When Powers generate Fear in target land, Defend 1 per Fear. 1 Fear. (Fear from To Dream a Thousand Deaths counts. Fear from destroying Town / City does not.)"),Artist( Artists.ShaneTyree)]
	static public Task ActAsync(TargetSpaceCtx ctx ) {

		ctx.Tokens.Adjust( new ConvertFearToDefense(Name),1 );

		// 1 fear
		ctx.AddFear(1);

		return Task.CompletedTask;
	}


	// When powers generate fear in target land, defend 1 per fear.
	class ConvertFearToDefense( string powerName ) : IReactToLandFear, IEndWhenTimePasses {
		void IReactToLandFear.HandleFearAdded( SpaceState tokens, int fearAdded, FearType fearType ) {
			// (Fear from destroying town/cities does not.)
			if(fearType == FearType.FromInvaderDestruction) return;

			tokens.Defend.Add( fearAdded );
			ActionScope.Current.Log( new Log.Debug( $"{fearAdded} Fear => +{fearAdded} Defend ({_powerName})" ) );
		}
		readonly string _powerName = powerName;
	}

}