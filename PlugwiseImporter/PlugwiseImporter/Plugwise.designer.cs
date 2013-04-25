﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.296
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PlugwiseImporter
{
	using System.Data.Linq;
	using System.Data.Linq.Mapping;
	using System.Data;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Linq;
	using System.Linq.Expressions;
	using System.ComponentModel;
	using System;
	
	
	public partial class PlugwiseDataContext : System.Data.Linq.DataContext
	{
		
		private static System.Data.Linq.Mapping.MappingSource mappingSource = new AttributeMappingSource();
		
    #region Extensibility Method Definitions
    partial void OnCreated();
    #endregion
		
		public PlugwiseDataContext(string connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public PlugwiseDataContext(System.Data.IDbConnection connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public PlugwiseDataContext(string connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public PlugwiseDataContext(System.Data.IDbConnection connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public System.Data.Linq.Table<Appliance_Log> Appliance_Logs
		{
			get
			{
				return this.GetTable<Appliance_Log>();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="")]
	public partial class Appliance_Log
	{
		
		private System.DateTime _LogDate;
		
		private int _ApplianceID;
		
		private double _Usage_peak;
		
		private double _Usage_offpeak;
		
		public Appliance_Log()
		{
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_LogDate")]
		public System.DateTime LogDate
		{
			get
			{
				return this._LogDate;
			}
			set
			{
				if ((this._LogDate != value))
				{
					this._LogDate = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ApplianceID")]
		public int ApplianceID
		{
			get
			{
				return this._ApplianceID;
			}
			set
			{
				if ((this._ApplianceID != value))
				{
					this._ApplianceID = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Usage_peak")]
		public double Usage_peak
		{
			get
			{
				return this._Usage_peak;
			}
			set
			{
				if ((this._Usage_peak != value))
				{
					this._Usage_peak = value;
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Usage_offpeak")]
		public double Usage_offpeak
		{
			get
			{
				return this._Usage_offpeak;
			}
			set
			{
				if ((this._Usage_offpeak != value))
				{
					this._Usage_offpeak = value;
				}
			}
		}
	}
}
#pragma warning restore 1591
