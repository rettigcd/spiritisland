using SpiritIsland.Log;

namespace SpiritIsland;

public class BlightToken : TokenClassToken
	, IHandleTokenAdded
	, IHandleTokenRemoved
{
	public BlightToken( string label, char k, Img img ) : base( label, k, img, TokenCategory.Blight ) {}

	public async Task HandleTokenAdded( ITokenAddedArgs args ) {

		if(args.Token != this) return; // token-added event handler for blight only

		bool takingFromBlightCard = args.Reason switch {
			AddReason.AsReplacement => false,
			AddReason.MovedTo => false,
			AddReason.Added => true, // Generic add
			AddReason.Ravage => true, // blight from ravage
			AddReason.BlightedIsland => true, // blight from blighted island card
			AddReason.SpecialRule => true, // Heart of wildfire - Blight from add presence
			_ => throw new ArgumentException( nameof( args.Reason ) )
		};
		if(!takingFromBlightCard) return;

		// remove from card.
		var gs = GameState.Current;
		await gs.TakeFromBlightSouce( args.Count, args.AddedTo );

		if(gs.BlightCard != null && gs.blightOnCard <= 0) {
			await gs.Spirits[0].Select( "Island blighted", new IOption[] { gs.BlightCard }, Present.Always );

			gs.Log( new IslandBlighted( gs.BlightCard ) );
			await gs.BlightCard.OnBlightDepleated( gs );
		}

		// Calc side effects
		var effect = new AddBlightEffect {
			DestroyPresence = true,
			Cascade = args.AddedTo.Blight.Count != 1,
			AddedTo = args.AddedTo
		};
		await gs.ModifyBlightAddedEffect.InvokeAsync( effect );

		// Destory presence
		if(effect.DestroyPresence)
			foreach(Spirit spirit in gs.Spirits)
				if(spirit.Presence.IsOn( args.AddedTo ))
					await args.AddedTo.Destroy( spirit.Token, 1 );

		// Cascade blight
		if(effect.Cascade) {
			Space cascadeTo = await gs.Spirits[0].Gateway.Decision( Select.ASpace.ForMoving_SpaceToken(
				$"Cascade blight from {args.AddedTo.Space.Label} to",
				args.AddedTo.Space,
				gs.CascadingBlightOptions( args.AddedTo ),
				Present.Always,
				Token.Blight
			) );
			await gs.Tokens[cascadeTo].Blight.Add( 1, args.Reason ); // Cascading blight shares original blights reason.
		}

	}

	public Task HandleTokenRemoved( ITokenRemovedArgs args ) {
		if(args.Token == Token.Blight
			&& !args.Reason.IsOneOf(
				RemoveReason.MovedFrom, // pushing / gathering blight
				RemoveReason.Replaced   // just in case...
			)
		)	GameState.Current.blightOnCard += args.Count;
		return Task.CompletedTask;
	}
}
