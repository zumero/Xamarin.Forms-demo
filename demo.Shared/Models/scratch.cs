using System;
using System.Text;
using SQLite;

namespace demo.Models
{

	/**
	 * This class presents a simple ORM over the Zumero-synced SQLite 
	 * table.  Some properties may require conversion from C# objects
	 * to the representation that Zumero requires in SQLite.  For more
	 * information on why some data types are converted when stored in 
	 * a Zumero-synced SQLite database, see: 
	 * http://zumero.com/docs/zumero_for_sql_server_manager.html#data_type_conversion_and_limitations
	 */
	[Table("scratch")]
	public class scratch : BaseModel
	{
		public scratch()
		{
			//Don't fire notifications by default, since
			//they make editing the properties difficult.
			this.NotifyIfPropertiesChange = false;
		}



		

		[PrimaryKey]
        [NotNull]
		[AutoIncrement, Column("id")]
		public int id 
		{ 
			get { return id_private; }
			set { SetProperty(id_private, value, (val) => { id_private = val; }, id_PropertyName); }
		}
		public static string id_PropertyName = "id";
		private int id_private;
		
		



		[Column("string1")]
		public string string1 
		{ 
			get { return string1_private; }
			set { SetProperty(string1_private, value, (val) => { string1_private = val; }, string1_PropertyName); }
		}
		public static string string1_PropertyName = "string1";
		private string string1_private;
		
		



		[Column("string2")]
		public string string2 
		{ 
			get { return string2_private; }
			set { SetProperty(string2_private, value, (val) => { string2_private = val; }, string2_PropertyName); }
		}
		public static string string2_PropertyName = "string2";
		private string string2_private;
		
		



		[Column("int1")]
		public Nullable<int> int1 
		{ 
			get { return int1_private; }
			set { SetProperty(int1_private, value, (val) => { int1_private = val; }, int1_PropertyName); }
		}
		public static string int1_PropertyName = "int1";
		private Nullable<int> int1_private;
		
		



		[Column("int2")]
		public Nullable<int> int2 
		{ 
			get { return int2_private; }
			set { SetProperty(int2_private, value, (val) => { int2_private = val; }, int2_PropertyName); }
		}
		public static string int2_PropertyName = "int2";
		private Nullable<int> int2_private;
		
		



		[Column("float1")]
		public Nullable<double> float1 
		{ 
			get { return float1_private; }
			set { SetProperty(float1_private, value, (val) => { float1_private = val; }, float1_PropertyName); }
		}
		public static string float1_PropertyName = "float1";
		private Nullable<double> float1_private;
		
		



		[Column("float2")]
		public Nullable<double> float2 
		{ 
			get { return float2_private; }
			set { SetProperty(float2_private, value, (val) => { float2_private = val; }, float2_PropertyName); }
		}
		public static string float2_PropertyName = "float2";
		private Nullable<double> float2_private;
		
		



		[Column("datetime_col")]

		// The actual column definition, as seen in SQLite
		public string datetime_col_raw { get; set; }

		public static string datetime_col_PropertyName = "datetime_col";
		
		// A helper definition that will not be saved to SQLite directly.
		// This property reads and writes to the _raw property.
		[Ignore]
		public Nullable<DateTime> datetime_col { 
			// Watch out for time zones, as they are not encoded into
			// the database. Here, I make no assumptions about time
			// zones.
			get { return datetime_col_raw != null ? (Nullable<DateTime>)DateTime.Parse(datetime_col_raw) : (Nullable<DateTime>)null; }
			set { SetProperty(datetime_col_raw, datetime_col_ConvertToString(value), (val) => { datetime_col_raw = val; }, datetime_col_PropertyName); }
		}

		// This static method is helpful when you need to query
		// on the raw value.
		public static string datetime_col_ConvertToString(Nullable<DateTime> date)
		{
			if (!date.HasValue)
				return null;
			else
	
			return date.Value.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
		
		}

		


		public override string ToString() 
		{
			StringBuilder sb = new StringBuilder();


			sb.Append(id.ToString());

			sb.Append("|");

			if (string1 != null)
			{
				sb.Append(string1.ToString());
			}
			sb.Append("|");

			if (string2 != null)
			{
				sb.Append(string2.ToString());
			}
			sb.Append("|");

			if (int1.HasValue)
			{
				sb.Append(int1.ToString());
			}
			sb.Append("|");

			if (int2.HasValue)
			{
				sb.Append(int2.ToString());
			}
			sb.Append("|");

			if (float1.HasValue)
			{
				sb.Append(float1.ToString());
			}
			sb.Append("|");

			if (float2.HasValue)
			{
				sb.Append(float2.ToString());
			}
			sb.Append("|");

			if (datetime_col != null)
			{
				sb.Append(datetime_col_ConvertToString(datetime_col));
			}
			sb.Append("|");

			return sb.ToString();
		}
	}
}
