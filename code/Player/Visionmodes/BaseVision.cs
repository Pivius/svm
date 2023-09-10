namespace SvM.Player.VisionModes
{
	public class BaseVision
	{
		private bool _enabled = false;
		public bool Enabled
		{
			get { return _enabled; }
			set
			{
				if ( value == _enabled )
					return;

				_enabled = value;

				if ( _enabled )
					OnEnable();
				else
					OnDisable();
			}
		}

		public virtual void OnEnable() { }
		public virtual void OnDisable() { }
		public virtual void OnFrame() { }
	}
}
