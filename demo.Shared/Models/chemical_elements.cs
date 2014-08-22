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
	[Table("chemical_elements")]
	public class chemical_elements : BaseModel
	{
		public chemical_elements()
		{
			//Don't fire notfications by default, since
			//they make editing the properties difficult.
			this.NotifyIfPropertiesChange = false;
		}


		[PrimaryKey]
		[NotNull]
		[AutoIncrement, Column("atomic_number")]
		public int atomic_number 
		{ 
			get { return atomic_number_private; }
			set { SetProperty(atomic_number_private, value, (val) => { atomic_number_private = val; }, atomic_number_PropertyName); }
		}
		public static string atomic_number_PropertyName = "atomic_number";
		private int atomic_number_private;
		
		



		[Column("symbol")]
		public string symbol 
		{ 
			get { return symbol_private; }
			set { SetProperty(symbol_private, value, (val) => { symbol_private = val; }, symbol_PropertyName); }
		}
		public static string symbol_PropertyName = "symbol";
		private string symbol_private;
		
		



		[Column("name")]
		public string name 
		{ 
			get { return name_private; }
			set { SetProperty(name_private, value, (val) => { name_private = val; }, name_PropertyName); }
		}
		public static string name_PropertyName = "name";
		private string name_private;
		
		



		[Column("atomic_mass")]
		public string atomic_mass 
		{ 
			get { return atomic_mass_private; }
			set { SetProperty(atomic_mass_private, value, (val) => { atomic_mass_private = val; }, atomic_mass_PropertyName); }
		}
		public static string atomic_mass_PropertyName = "atomic_mass";
		private string atomic_mass_private;
		
		



		[Column("electron_configuration")]
		public string electron_configuration 
		{ 
			get { return electron_configuration_private; }
			set { SetProperty(electron_configuration_private, value, (val) => { electron_configuration_private = val; }, electron_configuration_PropertyName); }
		}
		public static string electron_configuration_PropertyName = "electron_configuration";
		private string electron_configuration_private;
		
		



		[Column("electronegativity")]

		// The actual column definition, as seen in SQLite
		public Nullable<long> electronegativity_raw { get; set; }

		// This is the static scaling factor that will be applied to convert
		// from long to decimal. 
		private static long _electronegativity_scale = (long)Math.Pow(10, 2);

		public static string electronegativity_PropertyName = "electronegativity";
		
		// A helper definition that will not be saved to SQLite directly.
		// This property reads and writes to the _raw property.
		[Ignore]
		public Nullable<decimal> electronegativity { 
			get { return electronegativity_raw.HasValue ? (Nullable<decimal>)((decimal)electronegativity_raw / (decimal)_electronegativity_scale) : null; }
			set { SetProperty(electronegativity_raw, electronegativity_ConvertToInt(value), (val) => { electronegativity_raw = val; }, electronegativity_PropertyName); }
		}

		// This static method is helpful when you need to query
		// on the raw value.
		public static Nullable<long> electronegativity_ConvertToInt(Nullable<decimal> arg_electronegativity)
		{
			if (!arg_electronegativity.HasValue)
				return null;
			else
				return (long)Math.Floor((double)(arg_electronegativity.Value * (decimal)_electronegativity_scale));
		}

		



		[Column("atomic_radius")]
		public Nullable<int> atomic_radius 
		{ 
			get { return atomic_radius_private; }
			set { SetProperty(atomic_radius_private, value, (val) => { atomic_radius_private = val; }, atomic_radius_PropertyName); }
		}
		public static string atomic_radius_PropertyName = "atomic_radius";
		private Nullable<int> atomic_radius_private;
		
		



		[Column("ionic_energy")]
		public Nullable<int> ionic_energy 
		{ 
			get { return ionic_energy_private; }
			set { SetProperty(ionic_energy_private, value, (val) => { ionic_energy_private = val; }, ionic_energy_PropertyName); }
		}
		public static string ionic_energy_PropertyName = "ionic_energy";
		private Nullable<int> ionic_energy_private;
		
		



		[Column("standard_state")]
		public string standard_state 
		{ 
			get { return standard_state_private; }
			set { SetProperty(standard_state_private, value, (val) => { standard_state_private = val; }, standard_state_PropertyName); }
		}
		public static string standard_state_PropertyName = "standard_state";
		private string standard_state_private;
		
		



		[Column("melting_point")]
		public Nullable<int> melting_point 
		{ 
			get { return melting_point_private; }
			set { SetProperty(melting_point_private, value, (val) => { melting_point_private = val; }, melting_point_PropertyName); }
		}
		public static string melting_point_PropertyName = "melting_point";
		private Nullable<int> melting_point_private;
		
		



		[Column("boiling_point")]
		public Nullable<int> boiling_point 
		{ 
			get { return boiling_point_private; }
			set { SetProperty(boiling_point_private, value, (val) => { boiling_point_private = val; }, boiling_point_PropertyName); }
		}
		public static string boiling_point_PropertyName = "boiling_point";
		private Nullable<int> boiling_point_private;
		
		



		[Column("density")]
		public Nullable<double> density 
		{ 
			get { return density_private; }
			set { SetProperty(density_private, value, (val) => { density_private = val; }, density_PropertyName); }
		}
		public static string density_PropertyName = "density";
		private Nullable<double> density_private;
		
		



		[Column("periodic_group")]
		public string periodic_group 
		{ 
			get { return periodic_group_private; }
			set { SetProperty(periodic_group_private, value, (val) => { periodic_group_private = val; }, periodic_group_PropertyName); }
		}
		public static string periodic_group_PropertyName = "periodic_group";
		private string periodic_group_private;
		
		



		[Column("year_discovered")]
		public Nullable<int> year_discovered 
		{ 
			get { return year_discovered_private; }
			set { SetProperty(year_discovered_private, value, (val) => { year_discovered_private = val; }, year_discovered_PropertyName); }
		}
		public static string year_discovered_PropertyName = "year_discovered";
		private Nullable<int> year_discovered_private;
		
		



		[Column("discoverer")]
		public string discoverer 
		{ 
			get { return discoverer_private; }
			set { SetProperty(discoverer_private, value, (val) => { discoverer_private = val; }, discoverer_PropertyName); }
		}
		public static string discoverer_PropertyName = "discoverer";
		private string discoverer_private;
		
		



		[Column("history")]
		public string history 
		{ 
			get { return history_private; }
			set { SetProperty(history_private, value, (val) => { history_private = val; }, history_PropertyName); }
		}
		public static string history_PropertyName = "history";
		private string history_private;
		
		


		public override string ToString() 
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(atomic_number.ToString());

			sb.Append("|");

			if (symbol != null)
			{
				sb.Append(symbol.ToString());
			}
			sb.Append("|");

			if (name != null)
			{
				sb.Append(name.ToString());
			}
			sb.Append("|");

			if (atomic_mass != null)
			{
				sb.Append(atomic_mass.ToString());
			}
			sb.Append("|");

			if (electron_configuration != null)
			{
				sb.Append(electron_configuration.ToString());
			}
			sb.Append("|");

			if (electronegativity.HasValue)
			{
				sb.Append(electronegativity.ToString());
			}
			sb.Append("|");

			if (atomic_radius.HasValue)
			{
				sb.Append(atomic_radius.ToString());
			}
			sb.Append("|");

			if (ionic_energy.HasValue)
			{
				sb.Append(ionic_energy.ToString());
			}
			sb.Append("|");

			if (standard_state != null)
			{
				sb.Append(standard_state.ToString());
			}
			sb.Append("|");

			if (melting_point.HasValue)
			{
				sb.Append(melting_point.ToString());
			}
			sb.Append("|");

			if (boiling_point.HasValue)
			{
				sb.Append(boiling_point.ToString());
			}
			sb.Append("|");

			if (density.HasValue)
			{
				sb.Append(density.ToString());
			}
			sb.Append("|");

			if (periodic_group != null)
			{
				sb.Append(periodic_group.ToString());
			}
			sb.Append("|");

			if (year_discovered.HasValue)
			{
				sb.Append(year_discovered.ToString());
			}
			sb.Append("|");

			if (discoverer != null)
			{
				sb.Append(discoverer.ToString());
			}
			sb.Append("|");

			if (history != null)
			{
				sb.Append(history.ToString());
			}
			sb.Append("|");

			return sb.ToString();
		}
	}
}