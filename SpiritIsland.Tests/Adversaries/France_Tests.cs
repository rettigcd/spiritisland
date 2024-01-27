using SpiritIsland.BranchAndClaw.Adversaries;

namespace SpiritIsland.Tests.Adversaries;

public class France_Tests {

	[Fact]
	public async Task TimePasses_AdvancesRoundNumber(){
		// Testing this because Round # is how Slave Rebellion tracks when it is activated.
		var gs = new GameState(new Thunderspeaker(),Board.BuildBoardE());

		for(int i=1;i<6;++i){
			gs.RoundNumber.ShouldBe(i);
			await gs.TriggerTimePasses();
		}

	}

	[Fact]
	public async Task Round4_TriggersMinorUprising(){
		// Given: France
		var cfg = Given_FranceLevel(2).SetBoards( "A" ).SetSpirits( Keeper.Name );
		var gs = BuildGame(cfg);

		//   And: Round 4
		for(int i=0;i<3;++i) await gs.TriggerTimePasses();
		gs.RoundNumber.ShouldBe(4);

		// When: start Invader phase
		await InvaderPhase.ActAsync(gs).AwaitUser(gs.Spirits[0],u=>{
			// Then: user should be prompted to add strife to town.
			u.NextDecision.HasPrompt("Add strife to town").Choose("T@2 on A8");
		}).ShouldComplete();

	}

	[Fact]
	public async Task AfterRewind_EscalationStillWorks(){
		// Given: initialized card deck with France
		var cfg = Given_FranceLevel(2).SetBoards( "A" ).SetSpirits( Keeper.Name );
		var gs = BuildGame(cfg);
		gs.Initialize();

		//   And: next card to Reveal has escalation
		var future = gs.InvaderDeck.UnrevealedCards;
		var card = future[0];
		while(!card.TriggersEscalation){
			future.RemoveAt(0);
			card = future[0];
		}
		
		//  And: saved game
		object memento = ((IHaveMemento)gs).Memento;

		//   And: Triggered Escalation
		await ExploreTriggersEscalation(gs);

		//  When: restore game
		((IHaveMemento)gs).Memento = memento;

		//  Then: Triggered Escalation (AGAIN)
		await ExploreTriggersEscalation(gs);

	}
	Task ExploreTriggersEscalation(GameState gs) => InvaderPhase.ActAsync(gs).AwaitUser(gs.Spirits[0],u=>{
		u.NextDecision.HasPrompt("Select space to Add 1 Blight if Town/City. Add Town otherwise.").Choose("A8");
	});

	static GameConfiguration Given_FranceLevel( int level ) => new GameConfiguration { Adversary = new AdversaryConfig( France.Name, level ), ShuffleNumber = 1, };
	static GameState BuildGame( GameConfiguration cfg ) => ConfigurableTestFixture.GameBuilder.BuildGame( cfg );

}
