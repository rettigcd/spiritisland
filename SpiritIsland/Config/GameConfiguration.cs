#nullable enable
namespace SpiritIsland;

public class GameConfiguration {

	public string[]? Spirits;
	public AspectConfigKey[]? Aspects;
	public string[]? Boards;
	public int ShuffleNumber;
	public AdversaryConfig? Adversary;
	public bool CommandTheBeasts;

	public string AdversarySummary => Adversary == null ? "[none]" : $"{Adversary.Name} {Adversary.Level}";

	#region public helper methods
	public GameConfiguration ConfigSpirits(params string[] spirits ) { Spirits = spirits; return this; }
	public GameConfiguration ConfigAspects(params AspectConfigKey[] aspects) { Aspects = aspects; return this; }
	public GameConfiguration ConfigBoards( params string[] boards ) { Boards = boards; return this; }
	public GameConfiguration ConfigCommandBeasts( bool cmdBeasts ) { CommandTheBeasts = cmdBeasts; return this; }
	public GameConfiguration ConfigAdversary(string adversaryName, int level) { return ConfigAdversary(new AdversaryConfig(adversaryName, level)); }
	public GameConfiguration ConfigAdversary( AdversaryConfig adversary ) {  Adversary = adversary; return this; }
	#endregion
}
