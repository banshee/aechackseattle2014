using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace ToolFinder
{
	public partial class MissingListTableViewController : UITableViewController
	{
		public MissingListTableViewController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);

			var tfs = ToolFinderServer.getData ("reads.json");
			tfs.ToObservable<ToolFinderServer.ToolEntry[]> ()
				.ObserveOn (System.Threading.SynchronizationContext.Current)
				.Subscribe (items => {
					TableView.Source = new BasicReadsTableViewSource (items);
					TableView.ReloadData();
				});
		}
	}
}
