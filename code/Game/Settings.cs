using Sandbox;

namespace SvM
{
	public partial class SvMGame
	{
		public readonly static string[] Settings =
		{
			"svm_spy_life",
			"svm_merc_life",
			"svm_game_length",
			"svm_respawn_time",
			"svm_game_mode"
		};

		public readonly static string[] GameModes =
		{
			"normal",
			"hack",
			"deathmatch",
			"extract"
		};

		[ConVar.Replicated( "svm_spy_life" )]
		public static int SpyLife { get; set; } = 3;
		[ConVar.Replicated( "svm_merc_life" )]
		public static int MercLife { get; set; } = 3;
		[ConVar.Replicated( "svm_game_length" )]
		public static int GameLength { get; set; } = 10;
		[ConVar.Replicated( "svm_respawn_time" )]
		public static int RespawnTime { get; set; } = 10;
		[ConVar.Replicated( "svm_game_mode" )]
		public static string GameMode { get; set; } = "normal";
	}
}
