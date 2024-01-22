using SpiritIsland.FeatherAndFlame;

namespace SpiritIsland.Tests.Spirits.Finder;


public class Finder_Tests : BoardAGame {
    public Finder_Tests():base(new FinderOfPathsUnseen()){
        _gameState.Initialize();
        // Given: 1 Presence on A1
//        _spirit.Presence.Given_Setup(_board[1],1);
    }

    [Fact(Skip="temp")]
    public async Task PlacePresence(){

        // When: Finder places presence
        await new PlacePresence(1).ActAsync(_spirit).AwaitUser(_spirit, u=>{
            u.NextDecision.HasPrompt("Select Presence to place").HasOptions("sun energy,earth energy,FoPU on A3,FoPU on A1").Choose("sun energy");
            u.NextDecision.HasPrompt("Where would you like to place your presence?").HasOptions("A1,A2,A3,A4,A5,A6").Choose("A2");
        }).ShouldComplete();

        // Then: Sun Energy Slot should be revealed
        _spirit.Presence.CoverOptions.Select(s=>s.Text).Join(",").ShouldBe("sun energy,1 cardplay"); // 0 energy,1 cardplay

        //  And: should have Sun Element
        _spirit.Elements.Summary(true).ShouldBe("1 sun");
    }

    [Fact(Skip="temp")]
    public async Task Revealing2WaterSlot_Generates2Energy(){

        // When: Finder Grows: G2,G2 (taking from: Sun then 2-water)
        await Do_Growth2("sun energy", "A1");
        await Do_Growth2("2,water energy", "A1");

        // Then: should have Sun & Water
        _spirit.Elements.Summary(true).ShouldBe("1 sun 1 water");
        //  And: 2 energy
        _spirit.Energy.ShouldBe(2);
    }

    async Task Do_Growth2( string fromTrack, string destination ){
        await _spirit.DoGrowth(_gameState).AwaitUser(_spirit, u=>{
            u.NextDecision.HasPrompt("Select Growth").Choose("PlacePresence(1)");
            u.NextDecision.HasPrompt("Select Presence to place").Choose(fromTrack);
            u.NextDecision.HasPrompt("Where would you like to place your presence?").Choose(destination);
            // Other option is +1 card play - autoselected
        }).ShouldComplete();
    }
}