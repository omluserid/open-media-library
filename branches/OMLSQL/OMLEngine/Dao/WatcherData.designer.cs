﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.3053
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OMLEngine.Dao
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
	
	
	[System.Data.Linq.Mapping.DatabaseAttribute(Name="OML")]
	public partial class WatcherDataDataContext : System.Data.Linq.DataContext
	{
		
		private static System.Data.Linq.Mapping.MappingSource mappingSource = new AttributeMappingSource();
		
    #region Extensibility Method Definitions
    partial void OnCreated();
    partial void InsertWatchedFolder(WatchedFolder instance);
    partial void UpdateWatchedFolder(WatchedFolder instance);
    partial void DeleteWatchedFolder(WatchedFolder instance);
    partial void InsertScannerSetting(ScannerSetting instance);
    partial void UpdateScannerSetting(ScannerSetting instance);
    partial void DeleteScannerSetting(ScannerSetting instance);
    #endregion
		
		public WatcherDataDataContext() : 
				base(global::OMLEngine.Properties.Settings.Default.OMLConnectionString, mappingSource)
		{
			OnCreated();
		}
		
		public WatcherDataDataContext(string connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public WatcherDataDataContext(System.Data.IDbConnection connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public WatcherDataDataContext(string connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public WatcherDataDataContext(System.Data.IDbConnection connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		internal System.Data.Linq.Table<WatchedFolder> WatchedFolders
		{
			get
			{
				return this.GetTable<WatchedFolder>();
			}
		}
		
		public System.Data.Linq.Table<ScannerSetting> ScannerSettings
		{
			get
			{
				return this.GetTable<ScannerSetting>();
			}
		}
	}
	
	[Table(Name="dbo.WatchedFolders")]
	internal partial class WatchedFolder : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private string _Folder;
		
		private System.Nullable<bool> _IncludeSubFolders;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnFolderChanging(string value);
    partial void OnFolderChanged();
    partial void OnIncludeSubFoldersChanging(System.Nullable<bool> value);
    partial void OnIncludeSubFoldersChanged();
    #endregion
		
		public WatchedFolder()
		{
			OnCreated();
		}
		
		[Column(Storage="_Folder", DbType="NVarChar(255) NOT NULL", CanBeNull=false, IsPrimaryKey=true)]
		public string Folder
		{
			get
			{
				return this._Folder;
			}
			set
			{
				if ((this._Folder != value))
				{
					this.OnFolderChanging(value);
					this.SendPropertyChanging();
					this._Folder = value;
					this.SendPropertyChanged("Folder");
					this.OnFolderChanged();
				}
			}
		}
		
		[Column(Storage="_IncludeSubFolders", DbType="Bit")]
		public System.Nullable<bool> IncludeSubFolders
		{
			get
			{
				return this._IncludeSubFolders;
			}
			set
			{
				if ((this._IncludeSubFolders != value))
				{
					this.OnIncludeSubFoldersChanging(value);
					this.SendPropertyChanging();
					this._IncludeSubFolders = value;
					this.SendPropertyChanged("IncludeSubFolders");
					this.OnIncludeSubFoldersChanged();
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
	
	[Table(Name="dbo.ScannerSettings")]
	public partial class ScannerSetting : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private System.Nullable<bool> _Enabled;
		
		private string _MetaData;
		
		private System.DateTime _LastModified;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnEnabledChanging(System.Nullable<bool> value);
    partial void OnEnabledChanged();
    partial void OnMetaDataChanging(string value);
    partial void OnMetaDataChanged();
    partial void OnLastModifiedChanging(System.DateTime value);
    partial void OnLastModifiedChanged();
    #endregion
		
		public ScannerSetting()
		{
			OnCreated();
		}
		
		[Column(Storage="_Enabled", DbType="Bit")]
		public System.Nullable<bool> Enabled
		{
			get
			{
				return this._Enabled;
			}
			set
			{
				if ((this._Enabled != value))
				{
					this.OnEnabledChanging(value);
					this.SendPropertyChanging();
					this._Enabled = value;
					this.SendPropertyChanged("Enabled");
					this.OnEnabledChanged();
				}
			}
		}
		
		[Column(Storage="_MetaData", DbType="NVarChar(500)")]
		public string MetaData
		{
			get
			{
				return this._MetaData;
			}
			set
			{
				if ((this._MetaData != value))
				{
					this.OnMetaDataChanging(value);
					this.SendPropertyChanging();
					this._MetaData = value;
					this.SendPropertyChanged("MetaData");
					this.OnMetaDataChanged();
				}
			}
		}
		
		[Column(Storage="_LastModified", AutoSync=AutoSync.Always, DbType="DateTime NOT NULL", IsPrimaryKey=true)]
		public System.DateTime LastModified
		{
			get
			{
				return this._LastModified;
			}
			set
			{
				if ((this._LastModified != value))
				{
					this.OnLastModifiedChanging(value);
					this.SendPropertyChanging();
					this._LastModified = value;
					this.SendPropertyChanged("LastModified");
					this.OnLastModifiedChanged();
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}
#pragma warning restore 1591
