using Sandbox;

namespace SvM.Player.Base
{
	public enum STATE : long
	{
		Idle = 0,
		Walk,
		Run,
		Jump,
		InAir,
		Land,
		Duck,
		Roll,
		SprintRoll,
		Dashing,
		Stunned,
		Vault,
		Hang,
		PipeVertical,
		PipeHorizontal,
		Ladder,
		Cover,
		Aim,
		Knocked,
		Dead
	}

	public partial class BasePlayer
	{
		public STATE State;
	}
}
