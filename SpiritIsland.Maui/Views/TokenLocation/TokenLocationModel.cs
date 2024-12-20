namespace SpiritIsland.Maui;

/// <summary>
/// Only shows the Token, but must know its location to generate the correct input command.
/// </summary>
public class TokenLocationModel : ObservableModel, OptionView {

	#region Control-facing Observable Properties

	// Display the Token
	public int Count                  { get => _count;       private set => SetProp(ref _count, value);      } int _count;
	public bool IsSacredSite          { get => _isSS;        private set => SetProp(ref _isSS, value);       } bool _isSS;

	public int StrifeCount            { get; }
	public ImageSource TokenImage     { get; }
	public string Damage              { get; }

	// NOT used by TokenLocationView but IS USED by collection in SpiritPanel
	public ImageSource? TrackImage { get; }

	#endregion Control-facing Observable Properties

	#region OptionView imp

	public OptionState State { 
		get => _state; 
		set { SetProp(ref _state, value); InputTransparent = value == OptionState.Default; }
		} OptionState _state = OptionState.Default;

	public bool InputTransparent { 
		get=>_inputTransparent; 
		private set => SetProp(ref _inputTransparent, value);
	} bool _inputTransparent = true;

	public IOption Option => TokenLocation;

	public Action<IOption,bool>? SelectOptionCallback { set; get; }

	public void Select(bool submit=false) => SelectOptionCallback?.Invoke(Option, submit);

	#endregion

	#region constructor

	public TokenLocationModel(ITokenLocation tokenOn) {
		ArgumentNullException.ThrowIfNull(tokenOn, nameof(tokenOn));
		TokenLocation = tokenOn;

		// Separate Strife
		var token = tokenOn.Token;
		if ( token is HumanToken ht ) {
			StrifeCount = ht.StrifeCount;
			TokenImage = GetImgImage(ht.Img);
		} else
			TokenImage = token is SpiritPresenceToken presenceToken
				? ImageCache.FromFile(presenceToken.Self.SpiritName.ToResourceName(".png"))
				: GetImgImage(token.Img);
		Damage = tokenOn.Token.Badge;

		// Not used by the TokenLocationView, but IS used by collection in the SpiritPanel
		if(tokenOn.Location is Track track)
			TrackImage = ImageCache.FromFile(track.Code.ToResourceName());

		RefreshCountAndSS();
	}

	#endregion constructor

	public void RefreshCountAndSS() {
		Count = TokenLocation.Count;
		IsSacredSite = TokenLocation.IsSacredSite;
	}

	#region private static ImageSource

	static ImageSource GetImgImage(Img img) {
		string filename = img switch {
			Img.Dahan => "dahan.png",
			Img.Town => "town.png",
			Img.City => "city.png",
			Img.Explorer => "humans/explorer.png",
			Img.Blight => "blight.png",
			Img.Disease => "disease.png",
			Img.Beast => "beast.png",
			Img.Vitality => "vitality.png",
			Img.Deeps => "deeps.png",
			Img.Badlands => "badlands.png",
			Img.Isolate => "isolate.png",
			Img.Wilds => "wilds.png",
			Img.Defend => "defend.png",
			Img.Token_Any => "any_token.png",

			// incarna
			Img.BoDDYS_Incarna           => "boddys.png",
			Img.BoDDYS_Incarna_Empowered => "boddys_e.png",
			Img.EEB_Incarna              => "eeb.png",
			Img.EEB_Incarna_Empowered    => "eeb_e.png",
			Img.L_Incarna                => "ll.png",
			Img.S_Incarna                => "ss.png",
			Img.S_Incarna_Empowered      => "ss_e.png",
			Img.TRotJ_Incarna            => "trotj.png",
			Img.TRotJ_Incarna_Empowered  => "trotj_e.png",
			Img.T_Incarna                => "tt.png",
			Img.WVKD_Incarna             => "wvkd.png",
			Img.WVKD_Incarna_Empowered   => "wvkd_e.png",

			Img.Dahan_Solid              => "dahan_solid.png",

			_ => "vitality.png" // ERROR - see if this ever happens.
		};
		return ImageCache.FromFile(filename);
	}

	#endregion private static ImageSource

	public readonly ITokenLocation TokenLocation;
}


