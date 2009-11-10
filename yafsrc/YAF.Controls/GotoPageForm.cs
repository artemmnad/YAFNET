using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Text;

namespace YAF.Controls
{
	public class GotoPageForumEventArgs : EventArgs
	{
		public GotoPageForumEventArgs( int gotoPage )
			: base()
		{
			this.GotoPage = gotoPage;
		}

		private int _gotoPage;

		public int GotoPage
		{
			get { return _gotoPage; }
			set { _gotoPage = value; }
		}
	}

	public class GotoPageForm : BaseControl
	{
		private Panel _mainPanel = new Panel();
		private TextBox _gotoTextBox = new TextBox();
		private Button _gotoButton = new Button();
		private Label _headerText = new Label();
		private HtmlGenericControl _divInner = new HtmlGenericControl();

		public Panel MainPanel
		{
			get
			{
				return _mainPanel;
			}
		}

		public HtmlGenericControl InnerDiv
		{
			get
			{
				return _divInner;
			}
		}

		public GotoPageForm()
			: base()
		{

		}

		protected override void OnInit( EventArgs e )
		{
			base.OnInit( e );

			BuildForm();
		}

		protected override void OnLoad( EventArgs e )
		{
			base.OnLoad( e );

			// localization has to be done in here so as to not attempt
			// to localize before the class has been created
			if ( !String.IsNullOrEmpty( PageContext.Localization.TransPage ) )
			{
				_headerText.Text = PageContext.Localization.GetText( "COMMON", "GOTOPAGE_HEADER" );
				_gotoButton.Text = PageContext.Localization.GetText( "COMMON", "GO" );
			}
			else
			{
				// non-localized for admin pages
				_headerText.Text = "Goto Page...";
				_gotoButton.Text = "Go";
			}
		}

		protected void BuildForm()
		{
			this.Controls.Add( _mainPanel );

			_mainPanel.CssClass = "gotoBase";
			_mainPanel.ID = GetExtendedID( "gotoBase" );

			HtmlGenericControl divHeader = new HtmlGenericControl( "div" );

			divHeader.Attributes.Add( "class", "gotoHeader" );
			divHeader.ID = GetExtendedID( "divHeader" );

			_mainPanel.Controls.Add( divHeader );

			_headerText.ID = GetExtendedID( "headerText" );

			divHeader.Controls.Add( _headerText );

			_divInner.Attributes.Add( "class", "gotoInner" );
			_divInner.ID = GetExtendedID( "gotoInner" );

			_mainPanel.Controls.Add( _divInner );

			_gotoButton.ID = GetExtendedID( "GotoButton" );
			_gotoButton.Style.Add( HtmlTextWriterStyle.Width, "30px" );
			_gotoButton.CausesValidation = false;
			_gotoButton.UseSubmitBehavior = false;
			_gotoButton.Click += new EventHandler( GotoButtonClick );		

			// text box...

			_gotoTextBox.ID = GetExtendedID( "GotoTextBox" );
			_gotoTextBox.Style.Add( HtmlTextWriterStyle.Width, "30px" );

			_divInner.Controls.Add( _gotoTextBox );
			_divInner.Controls.Add( _gotoButton );

			PageContext.PageElements.RegisterJsBlockStartup( String.Format( @"GotoPageFormKeyUp_{0}", this.ClientID ),
																											 String.Format(
																												@"Sys.Application.add_load(function() {{ jQuery('#{0}').bind('keydown', function(e) {{ if(e.keyCode == 13) {{ jQuery('#{1}').click(); return false; }} }}); }});",
																												_gotoTextBox.ClientID, _gotoButton.ClientID ) );

			// add enter key support...
			//_gotoTextBox.Attributes.Add( "onkeydown",
			//                             String.Format(
			//                              @"if( ( event.which || event.keyCode ) && (event.which == 13 || event.keyCode == 13) ) {{ jQuery('#{0}').click(); return false; }} return true;",
			//                              _gotoButton.ClientID ) );
			//document.getElementById('" +
			//                             _gotoButton.ClientID + "').click();return false;}} else {return true}; ") );
		}

		protected override void Render( HtmlTextWriter writer )
		{
			writer.WriteLine( String.Format( @"<div id=""{0}"" style=""display:none"" class=""gotoPageForm"">", this.ClientID ) );

			base.Render( writer );

			writer.WriteLine( "</div>" );
		}

		protected void GotoButtonClick( object sender, EventArgs e )
		{
			if ( GotoPageClick != null )
			{
				// attempt to parse the page value...
				if ( int.TryParse( _gotoTextBox.Text.Trim(), out _gotoPageValue ) )
				{
					// valid, fire the event...
					GotoPageClick( this, new GotoPageForumEventArgs( GotoPageValue ) );
				}
			}
			// clear the old value...
			_gotoTextBox.Text = "";
		}

		public event EventHandler<GotoPageForumEventArgs> GotoPageClick;

		private int _gotoPageValue;

		public int GotoPageValue
		{
			get { return _gotoPageValue; }
			set { _gotoPageValue = value; }
		}

		public string GotoTextBoxClientID
		{
			get { return _gotoTextBox.ClientID; }
		}

		public string GotoButtonClientID
		{
			get { return _gotoButton.ClientID; }
		}

	}
}
