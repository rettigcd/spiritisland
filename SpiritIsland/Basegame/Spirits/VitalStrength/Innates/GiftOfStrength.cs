using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace SpiritIsland.Basegame {

	[InnatePower("Gift of Strength",Speed.Fast)]
	[TargetSpirit]
	public class GiftOfStrength {

		#region options

		// * Note * these have a different signature than other Innates, called directly from GiftOfStrength_InnatePower

		[InnateOption("1 sun,2 earth,2 plant")]
		static public Task Option1( TargetSpiritCtx ctx, List<SpaceTargetedArgs> targetedList ) {
			return RepeatPowerCard(ctx,2, targetedList );
		}

		[InnateOption("2 sun,3 earth,2 plant")]
		static public Task Option2( TargetSpiritCtx ctx, List<SpaceTargetedArgs> targetedList ) {
			return RepeatPowerCard(ctx,4, targetedList );
		}

		[InnateOption("2 sun,4 earth,3 plant")]
		static public Task Option3( TargetSpiritCtx ctx, List<SpaceTargetedArgs> targetedList ) {
			return RepeatPowerCard(ctx,6, targetedList );
		}

		static Task RepeatPowerCard( TargetSpiritCtx ctx, int maxCost, List<SpaceTargetedArgs> targetedList ) {
			ctx.Target.AddActionFactory(new ReplaySpaceCardForCost(maxCost,targetedList));
			return Task.CompletedTask;
		}

		#endregion

	}

	public class GiftOfStrength_InnatePower : InnatePower_TargetSpirit {

		public GiftOfStrength_InnatePower() : base( typeof( GiftOfStrength ) ) { }

		public void Initialize( GameState gameState ){
			foreach(var spirit in gameState.Spirits)
				spirit.TargetedSpace += targetedList.Add;
			gameState.TimePasses_ThisRound.Push((_) => { targetedList.Clear(); return Task.CompletedTask; } );
		}

		public override Task ActivateAsync( Spirit self, GameState gameState ) {
			return FindSpiritAndInvoke( self, gameState, HighestMethod( self ) );
		}

		async Task FindSpiritAndInvoke( Spirit self, GameState gameState, MethodBase methodBase ){
			Spirit target = await self.SelectSpirit(gameState.Spirits);
			await (Task)methodBase.Invoke( null, new object[] { target, targetedList } );
		}

		public readonly List<SpaceTargetedArgs> targetedList = new List<SpaceTargetedArgs>();

	}

	public class ReplaySpaceCardForCost : IActionFactory {

		public ReplaySpaceCardForCost(int maxCost, List<SpaceTargetedArgs> targetedList ) {
			this.maxCost = maxCost;
			this.targetedList = targetedList;
		}

		public Speed Speed {
			get { throw new NotImplementedException( "" ); }
			set { throw new NotImplementedException( "" ); }
		}

		public string Name => "Replay Card for cost";
		public string Text => Name;

		public IActionFactory Original => this;

		public Task ActivateAsync( Spirit self, GameState _ ) {
			return self.SelectSpaceCardToReplayForCost( maxCost, targetedList );
		}

		readonly List<SpaceTargetedArgs> targetedList;
		readonly int maxCost;
	}


}
