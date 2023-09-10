using System;
using Sandbox;
using System.Collections.Generic;
using Sandbox.UI;

namespace SvM.UI.Settings
{
	public class DropDownSettings : BaseSettings
	{
		public DropDown DropDown { get; set; }

		public DropDownSettings( string conVar, string name, string[] optionsName, string[] options ) : base(conVar, name)
		{
			List<Option> optionsList = new();
			DropDown = new DropDown( Panel );

			for (int i = 0; i < options.Length; i++ )
			{
				Option option = new( optionsName[i], options[i] );

				optionsList.Add( option );
			}

			DropDown.Options = optionsList;
			DropDown.ValueChanged = SetValue;
			DropDown.Selected = optionsList[0];

			foreach (var child in DropDown.Children)
			{
				if (child is IconPanel iconPanel)
				{
					iconPanel.Delete();
				}
			}
		}

		public virtual void SetValue( bool value )
		{
			DropDown.Text = DropDown.Selected.Title;
			SetValue( value.ToString() );
		}

		public override void Tick()
		{
			base.Tick();
			DropDown.Tick();
		}
	}
}
