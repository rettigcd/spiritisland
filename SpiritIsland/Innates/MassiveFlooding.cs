using SpiritIsland.PowerCards;
using System.Collections.Generic;
using System.Linq;

namespace SpiritIsland {

	[InnatePower(MassiveFlooding.Name,Speed.Slow)]
	public class MassiveFlooding : BaseAction {
		// Slow, range 1 from SS

		public const string Name = "Massive Flooding";

		public MassiveFlooding(Spirit spirit,GameState gameState):base(gameState){

			var elements = spirit.PurchasedCards
				.SelectMany(c=>c.Elements)
				.GroupBy(c=>c)
				.ToDictionary(grp=>grp.Key,grp=>grp.Count())
				.ToCountDict();

			count = new int[]{
				elements[Element.Sun],
				elements[Element.Water]-1,
				elements[Element.Earth]==0?2:3
			}.Min();

			if(count == 0) return;

			engine.decisions.Push( new TargetSpaceRangeFromSacredSite(spirit,1,
				HasExplorersOrTowns
				,X
			));

		}

		bool HasExplorersOrTowns(Space space){
			var sum = gameState.InvadersOn(space);
			return sum.HasExplorer || sum.HasTown;
		}

		readonly int count;

		void X(Space space,ActionEngine engine){
			var invaders = gameState.InvadersOn(space);
			var d = new NamedDecision[]{
				new NamedDecision{ 
					Text="Push 1 E/T", 
					Item = new SelectInvadersToPush(invaders,1,"Town","Explorer")
				},
				new NamedDecision{ 
					Text="2 damage, Push up to 3 explorers and/or towns", 
					Item = new SelectInvadersToPush(invaders,3,"Town","Explorer") // !
				},
				new NamedDecision{ 
					Text="2 damage to all", 
					Item = new SelectInvadersToPush(invaders,3,"Town","Explorer") // !!
				}
			}.Take(count).ToArray();

			engine.decisions.Push( new SelectInnate(d) );
		}

		class NamedDecision : IOption, IDecision {
			public IDecision Item;
			public string Text { get; set; }
			public string Prompt => Text + "-" + Item.Prompt;
			public IOption[] Options => Item.Options;
			public void Select( IOption option, ActionEngine engine ) => Item.Select(option,engine);
		}


		class SelectInnate : IDecision {
			readonly NamedDecision[] options;
			public SelectInnate(NamedDecision[] options){
				this.options = options;
			}

			public string Prompt => "Select Innate option";

			public IOption[] Options => options;

			public void Select( IOption option, ActionEngine engine ) {
				var decision = (NamedDecision)option;
				engine.decisions.Push(decision.Item);
			}
		}

		//  * 1 sun, 2 water => Push 1 Explorer or Town
		//[InnateOption( Element.Sun, Element.Water, Element.Water )]
		//public class Push1ExplorerOrTown : IDecision {
		//	public string Prompt => throw new System.NotImplementedException();
		//	public IOption[] Options => throw new System.NotImplementedException();
		//	public void Select( IOption option, ActionEngine engine ) {
		//		throw new System.NotImplementedException();
		//	}
		//}

		// * 2 sun, 3 water => Instead, 2 damage, Push up to 3 explorers and/or towns
		[InnateOption(Element.Sun,Element.Sun,Element.Water,Element.Water,Element.Water)]
		public class TwoDamageAndPush3 {

		}

		//` * 3 sun, 4 water, 1 earth => Instead, 2 damage to each invader
		[InnateOption(Element.Sun,Element.Sun,Element.Sun,Element.Water,Element.Water,Element.Water,Element.Water,Element.Earth)]
		public class TwoDamageEach {

		}

	}

	class SelectInnate : IDecision {

		public SelectInnate(){
//			var innate1Elements = new Dictionary<Element,int>{ [Element.Sun] = 1, [Element.Water] = 2 };
//			var innate2Elements = new Dictionary<Element,int>{ [Element.Sun] = 2, [Element.Water] = 3 };
//			var innate3Elements = new Dictionary<Element,int>{ [Element.Sun] = 3, [Element.Water] = 4, [Element.Earth] = 1 };
		}

		public string Prompt => throw new System.NotImplementedException();

		public IOption[] Options => throw new System.NotImplementedException();

		public void Select( IOption option, ActionEngine engine ) {
			throw new System.NotImplementedException();
		}
	}

}
