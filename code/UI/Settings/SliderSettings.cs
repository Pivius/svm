using System;
using Sandbox;
using System.Collections.Generic;
using Sandbox.UI;

namespace SvM.UI.Settings
{
	public class SliderSettings : BaseSettings
	{
		public SliderEntry Slider { get; set; } = new SliderEntry();
		public float Steps {
			get => Slider.Step;
			set => Slider.Step = value;
		}

		public SliderSettings( string conVar, string name, float steps, float min, float max ) : base(conVar, name)
		{
			Slider.ValueChanged = SetValue;
			Steps = steps;
			Slider.MinValue = min;
			Slider.MaxValue = max;
			Slider.Value = float.Parse( ConsoleSystem.GetValue( conVar ) );
			Panel.AddChild( Slider );
		}

		public virtual void SetValue( float value )
		{
			SetValue( value.ToString() );
		}

		public override void Tick()
		{
			base.Tick();
		}
	}
}
