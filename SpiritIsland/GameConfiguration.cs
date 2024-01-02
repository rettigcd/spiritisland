namespace SpiritIsland;

public class GameConfiguration {

	public string[] Spirits;
	public string[] Boards;
	public int ShuffleNumber;
	public AdversaryConfig Adversary;
	public bool CommandTheBeasts;

	public string AdversarySummary => Adversary == null ? "[none]" : $"{Adversary.Name} {Adversary.Level}";

	#region public helper methods
	public GameConfiguration SetSpirits(params string[] spirits ) { Spirits = spirits; return this; }
	public GameConfiguration SetBoards( params string[] boards ) { Boards = boards; return this; }
	public GameConfiguration SetCommandTheBeasts( bool cmdTheBeasts ) { CommandTheBeasts = cmdTheBeasts; return this; }
	#endregion
}



public record AdversaryConfig( string Name, int Level );
