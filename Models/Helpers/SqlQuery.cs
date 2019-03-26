using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Helpers
{
	///<summary>
	///Represents an object that describes a SqlQuery
	///</summary>
	[Serializable]
	public class SqlQuery : Models.Common.BaseObject
	{
		///<summary>
		///Initializes a new instance of the workNet.BusObj.Entity.SqlQuery class.
		///</summary>
		public SqlQuery()
		{
			this.Title = "New Adhoc Query";
			this.Description = "TBD";
			this.Category = "General";
			this.IsPublic = true;

		}


		#region Properties created from dictionary for SqlQuery

		//public int Id { get; set; }

		private string _title = "";
		/// <summary>
		/// Gets/Sets Title
		/// </summary>
		public string Title
		{
			get
			{
				return this._title;
			}
			set
			{
				if ( this._title == value )
				{
					//Ignore set
				}
				else
				{
					this._title = value.Trim();
					
				}
			}
		}

		private string _description = "";
		/// <summary>
		/// Gets/Sets Description
		/// </summary>
		public string Description
		{
			get
			{
				return this._description;
			}
			set
			{
				if ( this._description == value )
				{
					//Ignore set
				}
				else
				{
					this._description = value.Trim();
					
				}
			}
		}

		private string _queryCode = "";
		/// <summary>
		/// Gets/Sets QueryCode
		/// </summary>
		public string QueryCode
		{
			get
			{
				return this._queryCode;
			}
			set
			{
				if ( this._queryCode == value )
				{
					//Ignore set
				}
				else
				{
					this._queryCode = value.Trim();
					
				}
			}
		}

		private string _category = "";
		/// <summary>
		/// Gets/Sets Category
		/// </summary>
		public string Category
		{
			get
			{
				return this._category;
			}
			set
			{
				if ( this._category == value )
				{
					//Ignore set
				}
				else
				{
					this._category = value.Trim();
					
				}
			}
		}

		private string _sql = "";
		/// <summary>
		/// Gets/Sets SQL
		/// </summary>
		public string SQL
		{
			get
			{
				return this._sql;
			}
			set
			{
				if ( this._sql == value )
				{
					//Ignore set
				}
				else
				{
					this._sql = value.Trim();
					
				}
			}
		}

		private int _ownerId;
		/// <summary>
		/// Gets/Sets OwnerId
		/// </summary>
		public int OwnerId
		{
			get
			{
				return this._ownerId;
			}
			set
			{
				if ( this._ownerId == value )
				{
					//Ignore set
				}
				else
				{
					this._ownerId = value;
					
				}
			}
		}

		private bool _isPublic;
		/// <summary>
		/// Gets/Sets IsPublic
		/// </summary>
		public bool IsPublic
		{
			get
			{
				return this._isPublic;
			}
			set
			{
				if ( this._isPublic == value )
				{
					//Ignore set
				}
				else
				{
					this._isPublic = value;
					
				}
			}
		}


		#endregion
	}
}
