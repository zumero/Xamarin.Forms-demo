using demo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Zumero;

namespace demo.Data
{
	//The sync operation is abstracted out as a service,
	//in order for Android to wrap the sync in a background
	//service.  That ensures that a sync operation will not
	//be terminated by the OS if the user leaves the sync activity
	public interface ISyncService
	{
		bool IsSyncRunning();
		Task RevertLocalChanges();
		Task Sync(SyncParams syncParams);
		void Cancel();
		void Cancel(int cancelToken);
	}

	public class BaseSyncService : ISyncService
	{
		int _cancellationToken;
		bool _isSyncRunning = false;
		public bool IsSyncRunning()
		{
			return _isSyncRunning;
		}

		public void Cancel()
		{
			ZumeroClient.Cancel(_cancellationToken);
		}

		public void Cancel(int cancelToken)
		{
			ZumeroClient.Cancel(cancelToken);
		}

		public async Task RevertLocalChanges()
		{
			if (await DependencyService.Get<IDataService>().IsEmpty())
				return;
			ZumeroClient.QuarantineSinceLastSync(SharedApp.DatabasePath, null);
		}

		public async Task Sync(SyncParams syncParams)
		{
			ZumeroClient.callback_progress_handler callback = (cancellationToken, phase, bytesSoFar, bytesTotal) =>
			{
				_cancellationToken = cancellationToken;
				SharedApp.CancelToken = cancellationToken;
				string syncProgressString = "";
				if (phase == (int)ZumeroPhase.Preparing)
					syncProgressString = "Preparing";
				else if (phase == (int)ZumeroPhase.Uploading)
					syncProgressString = "Uploading " + bytesSoFar + " of " + bytesTotal;
				else if (phase == (int)ZumeroPhase.WaitingForResponse)
					syncProgressString = "Waiting for response";
				else if (phase == (int)ZumeroPhase.Downloading)
					syncProgressString = "Downloading " + bytesSoFar + " of " + bytesTotal;
				else if (phase == (int)ZumeroPhase.Applying)
					syncProgressString = "Applying";
				((demo.SharedApp)Xamarin.Forms.Application.Current).NotifySyncProgress(syncProgressString);
			};
			try
			{
				//Sending the Sync request through the DataService means that it will wait for any outstanding write operations to 
				//complete, and that any future write operations will wait for the sync to complete.
				var syncid = await DependencyService.Get<IDataService>().Sync(syncParams, callback);

				string syncDescription = await DependencyService.Get<IDataService>().DescribeSync(syncid);
				syncParams.SyncDescription = syncDescription;
				((demo.SharedApp)Xamarin.Forms.Application.Current).NotifySyncCompleted(syncParams);
			}
			catch (Exception e)
			{
				if (e is ZumeroException)
				{
					//if you need to react to a certain result, you can check
					//the result of the sync like this:

					//ZumeroException ze = (ZumeroException)e;
					//
					//if (ze.ErrorCode == (int)ZumeroResult.AuthenticationFailed)
					//...

					//The error code could also be a SQLite error code
					//
					//if (ze.ErrorCode == (int)SQLite3.Result.Error)
					//...
				}
				_isSyncRunning = false;
				((demo.SharedApp)Xamarin.Forms.Application.Current).NotifySyncFailed(e);
			}
		}
	}
}