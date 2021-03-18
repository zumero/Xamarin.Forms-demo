// ZAGDebugSchemaVersionCheck.cs
//
// This class is intended to be helpful during
// the development process of your app.
//
// Check the "schema info" of a (just sync'd) local
// database against the "schema info" used by ZAG
// to generate this app.
//
// Raise a reminder notice dialog if they are
// different.
//
// Note that this DOES NOT mean that this app is
// invalid.  There are lots of ways/reasons for
// the schema info to change and so this notice
// has a high false-positive rate.
//
// The "schema info" can change for any of the
// following reasons:
//
// [] The initial sync on this device synced with
//    the wrong DBFile on the server.
//
// [] The actual MSSQL database has been altered
//    on the server.
//
// [] The set of "prepared" tables exposed by
//    ZSS Manager has changed.
//
// [] A DBFile "filter" was applied to the
//    credentials used on this device.
//
// [] The set of credentials that you used to
//    sync on this device are different than those
//    give to ZAG when it generated this app.
//
// These types of changes are normal and to be
// expected during the development process.
//
// The actual changes to the local database have
// already been handled by the ZSS Client Library
// and your data is safe.
//
// *BUT* since this app has been generated with a
// hard-coded set of tables and columns based upon
// the original observed schema info, this DEMO app
// may show you incomplete results (in the case of
// added tables and/or columns) or various SQLite
// errors (when it refers to deleted tables and/or
// columns).
//
// This app was written with a hard-coded set of
// tables and columns (rather than dynamically inspecting
// the schema like a spreadsheet app might) because
// I felt that this would better reflect the types of
// code that you might be writing in your app.
//
// The purpose of this alter warning on the device
// is to get you to read this comment and maybe
// consider regenerating this demo app.
//

using System;
using SQLite;
using Xamarin.Forms;

namespace demo.Data
{
	public interface ISHA1Service
    {
        string HashString(string input);
    }
	
	public static class ZagDebugSchemaVersionCheck
	{
		private const string GENERATED_SIG = "45D8F49EECE418BADFF877D5ED79711D8EA22C1C";

		/// <summary>
		///   Return TRUE if the database schema matches the
		///   schema when ZAG generated this app.
		/// </summary>
		public static bool VerifySchema()
			{
				//It would be more efficient to do this in the DataService, 
				// and not open another connection, but
				//I wanted to keep DataService clean of references to nonessential classes.
				SQLiteConnection db = new SQLiteConnection(SharedApp.DatabasePath);
				db.Query<object>("PRAGMA journal_mode=WAL");

				db.Execute("PRAGMA foreign_keys=ON");
				db.Execute("PRAGMA recursive_triggers=ON");
				string current = getCurrentSig(db);
				db.Close();
				return (current == GENERATED_SIG);
			}

		private static string getCurrentSig(SQLiteConnection db)
			{
				string json = db.ExecuteScalar<string>("SELECT v FROM t$v WHERE k=10");
				return DependencyService.Get<ISHA1Service>().HashString(json);
			}
	}
}
