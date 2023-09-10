using System;
using Sandbox;
using System.Collections.Generic;
using Sandbox.UI;

namespace SvM.UI.Settings
{
	public class TickSettings : BaseSettings
	{
		public Checkbox Checkbox { get; set; }

		public TickSettings( string conVar, string name ) : base(conVar, name)
		{
			Checkbox.ValueChanged = SetValue;
			Panel.AddChild( Checkbox );
		}

		public virtual void SetValue( bool value )
		{
			SetValue( value.ToString() );
		}

		public override void Tick()
		{
			base.Tick();
		}
	}
}
