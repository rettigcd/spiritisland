using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using SpiritIsland;

namespace SpiritIsland.Basegame {

	[InnatePower("Gift of Strength",Speed.Fast)]
	[TargetSpirit]
	class GiftOfStrength {

		#region options

		// * Note * these have a different signature than other Innates, called directly from GiftOfStrength_InnatePower

		[InnateOption("1 sun,2 earth,2 plant")]
		static public Task Option1( Spirit target, List<SpaceTargetedArgs> targetedList ) {
			return RepeatPowerCard(target,2, targetedList );
		}

		[InnateOption("2 sun,3 earth,2 plant")]
		static public Task Option2( Spirit target, List<SpaceTargetedArgs> targetedList ) {
			return RepeatPowerCard(target,4, targetedList );
		}

		[InnateOption("2 sun,4 earth,3 plant")]
		static public Task Option3( Spirit target, List<SpaceTargetedArgs> targetedList ) {
			return RepeatPowerCard(target,6, targetedList );
		}

		static Task RepeatPowerCard( Spirit target,  int maxCost, List<SpaceTargetedArgs> targetedList ) {
			target.AddActionFactory(new ReplaySpaceCardForCost(maxCost,targetedList));
			return Task.CompletedTask;
		}

		#endregion

	}

	public class GiftOfStrength_InnatePower : InnatePower_TargetSpirit {

		public GiftOfStrength_InnatePower() : base( typeof( GiftOfStrength ) ) { }

		public void Initialize( GameState gameState ){
			foreach(var spirit in gameState.Spirits)
				spirit.TargetedSpace += targetedList.Add;
			gameState.TimePassed += (_) => targetedList.Clear();
		}

		public override Task Activate( Spirit self, GameState gameState ) {
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

		public Speed Speed => throw new System.NotImplementedException();

		public string Name => "Replay Card for cost";
		public string Text => Name;

		public IActionFactory Original => this;

		public Task Activate( Spirit self, GameState _ ) {
			return self.SelectSpaceCardToReplayForCost( maxCost, targetedList );
		}

		readonly List<SpaceTargetedArgs> targetedList;
		readonly int maxCost;
	}


}
