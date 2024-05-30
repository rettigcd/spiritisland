namespace SpiritIsland.Maui;

public class TokenLocationModel
	: ObservableModel, OptionView {

	#region Control-facing Observable Properties

	// Display the Token
	public int Count                  { get => _count;       private set => SetProp(ref _count, value);      } int _count;
	public bool IsSacredSite          { get => _isSS;        private set => SetProp(ref _isSS, value);       } bool _isSS;

	public int StrifeCount            { get; }
	public ImageSource TokenImage     { get; }
	public string Damage              { get; }

	// Display the Location
	public ImageSource? LocationImage { get; }

	#endregion Control-facing Observable Properties

	#region OptionView imp

	public OptionState State { get => _state; set => SetProp(ref _state, value); } OptionState _state = OptionState.Default;

	public IOption Option => TokenLocation;

	public Action<IOption,bool>? SelectOptionCallback { set; get; }

	public void Select(bool submit=false) => SelectOptionCallback?.Invoke(Option, submit);

	#endregion

	#region constructor

	public TokenLocationModel(TokenLocation tokenOn) {
		ArgumentNullException.ThrowIfNull(tokenOn, nameof(tokenOn));
		TokenLocation = tokenOn;

		// Separate Strife
		if (tokenOn.Token is HumanToken ht){
			StrifeCount = ht.StrifeCount;
			TokenImage = GetTokenImage( ht.HavingStrife(0) );
			Damage = 0 < ht.FullDamage ? ht.FullDamage.ToString() : "";
		} else {
			StrifeCount = 0;
			TokenImage = GetTokenImage(tokenOn.Token);
			Damage = "";
		}

		if(tokenOn.Location is Track track) {
			LocationImage = ImageCache.FromFile( track.Code.ToResourceName() );
		}

		RefreshCountAndSS();
	}

	#endregion constructor

	public void RefreshCountAndSS() {
		Count = TokenLocation.Count;
		IsSacredSite = TokenLocation.IsSacredSite;
	}

	#region private static ImageSource

	static ImageSource GetTokenImage(IToken baseToken) {
		return baseToken is SpiritPresenceToken presenceToken ? GetSpiritImage(presenceToken.Self)
			: baseToken is HumanToken ht ? GetHumanTokenMarker(ht)
			//			: token.GetType().Name == "ManyMindsBeast" ? GetManyMindsBeast( "many-minds-beast.png", token.Img, 60, 40 )
			//			: token.GetType().Name == "MarkedBeast" ? GetManyMindsBeast( "marked-beast.png", token.Img, 240, 20 )
			: GetImgImage(baseToken.Img);
	}

	static ImageSource GetSpiritImage(Spirit spirit) {
		string filename = spirit.SpiritName.ToResourceName(".png");
		return ImageCache.FromFile(filename);
	}

	static ImageSource GetHumanTokenMarker(HumanToken humanToken) {
		return GetImgImage(humanToken.HumanClass.Img);
	}
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

			_ => "vitality.png" // ERROR - see if this ever happens.
		};
		return ImageCache.FromFile(filename);
	}

	#endregion private static ImageSource

	public readonly TokenLocation TokenLocation;
}


