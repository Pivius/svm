using Sandbox;
using SvM.Player.Controller;
using SvM.Player.VisionModes;
using SvM.RenderHooks;
using SvM.Utility;
using System.Collections.Generic;

namespace SvM.Player
{
	public partial class SpyPlayer
	{
		private readonly List<BaseVision> _visionList = new()
		{
			new BaseVision(),
			new NightVision(),
			new ThermalVision(),
		};
		private BaseVision _currentVision;
		private int _currentVisionIndex = 0;

		public BaseVision CurrentVision
		{
			get => _currentVision;
			set
			{
				if ( _currentVision == value )
					return;

				_currentVision = value;
				_currentVisionIndex = value != null ? _visionList.IndexOf( _currentVision ) : 0;
			}
		}
		
		public void EnableVision(int i)
		{
			if ( i < 0 || i >= _visionList.Count )
			{
				DisableVision();
				return;
			}


			if ( CurrentVision != null )
			{
				DisableVision();
			}

			CurrentVision = _visionList[i];
			CurrentVision.Enabled = true;
		}

		public void DisableVision()
		{
			CurrentVision.Enabled = false;
			CurrentVision = null;
		}

		public void CycleVision()
		{
			if (Input.Pressed(InputButton.Score))
			{
				EnableVision( _currentVisionIndex + 1 );
			}
		}
	}
}
