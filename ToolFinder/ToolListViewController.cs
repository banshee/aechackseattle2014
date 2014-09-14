using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace ToolFinder
{
	partial class ToolListViewController : UITableViewController
	{
		public ToolListViewController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);

			var tfs = ToolFinderServer.getData ();
			tfs.ToObservable<string> ()
				.ObserveOn (System.Threading.SynchronizationContext.Current)
				.Subscribe (items => {
					TableView.Source = new BasicReadsTableViewSource (ToolFinderServer.expectedItems(items));
					TableView.ReloadData();
				});
		}
	}
}
