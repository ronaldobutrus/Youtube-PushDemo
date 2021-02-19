// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace PushDemoAppXN.iOS
{
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		UIKit.UIButton deregisterButton { get; set; }

		[Outlet]
		UIKit.UIButton registerButton { get; set; }

		[Outlet]
		UIKit.UITextField tagTextField { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (tagTextField != null) {
				tagTextField.Dispose ();
				tagTextField = null;
			}

			if (registerButton != null) {
				registerButton.Dispose ();
				registerButton = null;
			}

			if (deregisterButton != null) {
				deregisterButton.Dispose ();
				deregisterButton = null;
			}
		}
	}
}
