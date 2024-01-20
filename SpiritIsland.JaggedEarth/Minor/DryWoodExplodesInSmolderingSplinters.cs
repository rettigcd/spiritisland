namespace SpiritIsland.JaggedEarth;

public class DryWoodExplodesInSmolderingSplinters {

	public const string Name = "Dry Wood Explodes in Smoldering Splinters";

	[MinorCard(DryWoodExplodesInSmolderingSplinters.Name,1,Element.Fire,Element.Air,Element.Plant),FastFor1Energy,FromPresence(0,Filter.NotWetland)]
	[Instructions( "You may spend 1 Energy to make this Power Fast. 2 Fear. 1 Damage." ), Artist( Artists.JorgeRamos )]
	static public async Task ActAsync( TargetSpaceCtx ctx ){
		// You may spend 1 Energy to make this Power Fast.
		if( GameState.Current.Phase == Phase.Fast) {
			if(ctx.Self.Energy < 1 
				|| ! await ctx.Self.UserSelectsFirstText($"Make '{DryWoodExplodesInSmolderingSplinters.Name}' fast?", "Yes, Pay 1 energy", "Nevermind, I'll wait" )
			) {
				var self = ctx.Self.InPlay.Where(x=>x.Name == DryWoodExplodesInSmolderingSplinters.Name).First();
				ctx.Self.AddActionFactory(self);
				return;
			}
				
			--ctx.Self.Energy;
		}

		// 2 Fear.
		ctx.AddFear(2);

		// 1 Damage
		await ctx.DamageInvaders(1);
	}

	[AttributeUsage(AttributeTargets.Method|AttributeTargets.Class)]
	public class FastFor1EnergyAttribute : SpeedAttribute, ISpeedBehavior {
		public FastFor1EnergyAttribute():base(Phase.Slow) {}
//		public Phase DisplaySpeed { get; }

		public override bool CouldBeActiveFor( Phase requestSpeed, Spirit spirit ) {
			return DisplaySpeed.IsOneOf( requestSpeed, Phase.FastOrSlow )
				|| 0 < spirit.Energy && requestSpeed == Phase.Fast;
		}

	}


}