namespace SpiritIsland;

public interface IFearCard : IOption {

	int? Activation { get; set; }
	bool Flipped { get; set; }

	Task Level1( GameCtx ctx );
	Task Level2( GameCtx ctx );
	Task Level3( GameCtx ctx );
}

static public class IFearOptionsExtension {

	static public string GetDescription( this IFearCard options, int activation ) {
		var memberName = "Level" + activation;

		// This does not find interface methods declared as: void IFearCardOption.Level2(...)
		var member = options.GetType().GetMethod( memberName )
			?? throw new Exception( memberName + " not found on " + options.GetType().Name );

		var attr = (FearLevelAttribute)member.GetCustomAttributes( typeof( FearLevelAttribute ) ).Single();
		string description = attr.Description;
		return description;
	}

}
