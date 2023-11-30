namespace SpiritIsland.BranchAndClaw;

public class ConfoundingMists {

	public const string Name = "Confounding Mists";

	[MinorCard( Name, 1, Element.Air, Element.Water ),Fast,FromPresence( 1 )]
	[Instructions( "Defend 4. -or- Each Invader added to target land this turn may be immediately Pushed to any adjacent land." ), Artist( Artists.LoicBelliau )]
	static public Task ActAsync( TargetSpaceCtx ctx ) {
		return ctx.SelectActionOption(
			new SpaceCmd("Defend 4", ctx => ctx.Defend(4) ),
			new SpaceCmd(
				"Invaders added to target are immediately pushed",
				ctx => ctx.Tokens.Adjust( new MistPusher( ctx.Self ), 1 )
			)
		);
	}

	class MistPusher : BaseModEntity, IHandleTokenAddedAsync, IEndWhenTimePasses {
		readonly Spirit _spirit;
		public MistPusher(Spirit spirit ) { _spirit=spirit; }
		public async Task HandleTokenAddedAsync( ITokenAddedArgs args ) {
			// each invader added to target land this turn may be immediatley pushed to any adjacent land
			if(	args.Added.Class.IsOneOf(Human.Invader) 
				&& args.Reason.IsOneOf( AddReason.Added, AddReason.MovedTo, AddReason.Explore, AddReason.Build )
			)
				await args.To.SourceSelector
					.AddGroup(1,args.Added.Class)
					.PushN( _spirit );
		}
	}

}