﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Models.Common;
using Models.ProfileModels;
using EM = Data;
using Utilities;
using Views = Data.Views;
using ViewContext = Data.Views.CTIEntities1;
namespace Factories
{
	public class DurationProfileManager : BaseFactory
	{
		#region persistance ==================


		public bool Save( DurationProfile profile, int userId, ref List<string> messages )
		{
			bool isValid = true;
			if ( profile == null || profile.EntityId == 0 )
			{
				messages.Add( "Error: the parent identifier was not provided." );
                return false;
            }

			int count = 0;

			EM.Entity_DurationProfile efEntity = new EM.Entity_DurationProfile();

			using ( var context = new Data.CTIEntities() )
			{
				//check add/updates first
				
				bool isEmpty = false;

				if ( ValidateDurationProfile( profile, ref isEmpty, ref messages ) == false )
				{
					return false;
				}
				if ( isEmpty )
				{
					messages.Add( "Error - no data was entered." );
					return false;
				}

				//just in case
				//profile.ParentTypeId = parent.EntityTypeId;

				if ( profile.Id == 0 )
				{
					//How to check if already exists for parent?
					//add
					efEntity = new EM.Entity_DurationProfile();
					MapToDB( profile, efEntity );
					efEntity.EntityId = profile.EntityId;
					efEntity.Created = efEntity.LastUpdated = DateTime.Now;
					efEntity.CreatedById = efEntity.LastUpdatedById = userId;

					//LoggingHelper.DoTrace( 2, string.Format( "DurationProfileManager.DurationProfile_Update. EntityId: {0} DOING ADD - context.Add: ", profile.EntityId ) );

					context.Entity_DurationProfile.Add( efEntity );
					count = context.SaveChanges();
					//update profile record so doesn't get deleted
					profile.Id = efEntity.Id;

					if ( count == 0 )
					{
						messages.Add( " Unable to add Duration Profile" ) ;
						isValid = false;
					}
				}
				else
				{
					efEntity = context.Entity_DurationProfile.SingleOrDefault( s => s.Id == profile.Id );
					if ( efEntity != null && efEntity.Id > 0 )
					{
						//update
						MapToDB( profile, efEntity );
						//has changed?
						if ( HasStateChanged( context ) )
						{
							//note: testing - the latter may be true if the child has changed - but shouldn't as the mapping only updates the parent
							efEntity.LastUpdated = System.DateTime.Now;
							efEntity.LastUpdatedById = userId;

							count = context.SaveChanges();
						}
					}
					else
					{
						//??? shouldn't happen unless deleted somehow
						messages.Add( " Unable to update Duration Profile - the profile was not found." );
					}
				}
			}

			return isValid;
		}
		public bool DurationProfile_Delete( int recordId, ref string statusMessage )
		{
			bool isOK = true;
			using ( var context = new Data.CTIEntities() )
			{
				EM.Entity_DurationProfile p = context.Entity_DurationProfile.FirstOrDefault( s => s.Id == recordId );
				if ( p != null && p.Id > 0 )
				{
					context.Entity_DurationProfile.Remove( p );
					int count = context.SaveChanges();
				}
				else
				{
					statusMessage = string.Format( "DurationProflie record was not found: {0}", recordId );
					isOK = false;
				}
			}
			return isOK;

		}
        public bool DeleteAll( Guid parentUid, int typeId, ref List<string> messages )
        {
            bool isValid = true;
            Entity parent = EntityManager.GetEntity( parentUid );
            if ( parent == null || parent.Id == 0 )
            {
                messages.Add( "Error - the provided target parent entity was not found." );
                return false;
            }
            using ( var context = new EM.CTIEntities() )
            {
                if ( typeId == 3 ) //renewal frequency only
                {
                    context.Entity_DurationProfile.RemoveRange( context.Entity_DurationProfile
                        .Where( s => s.EntityId == parent.Id
                        && s.TypeId == typeId ) );
                }
                else
                {
                    context.Entity_DurationProfile.RemoveRange( context.Entity_DurationProfile
                        .Where( s => s.EntityId == parent.Id
                        && s.TypeId < 3 ) );
                }
                int count = context.SaveChanges();
                if ( count > 0 )
                {
                    isValid = true;
                }
            }

            return isValid;
        }
        /// <summary>
		private void MapToDB( DurationProfile from, EM.Entity_DurationProfile to )
		{
			int totalMinutes = 0;
			to.Id = from.Id;
			//make sure EntityId is not wiped out. Also can't actually chg
			if ( (to.EntityId ?? 0) == 0)
				to.EntityId = from.EntityId;

			if ( to.Id == 0 )
			{
				//to.ParentUid = from.ParentUid;
				//to.ParentTypeId = from.ParentTypeId;
			}

			//TODO - remove profileName
			to.ProfileName = from.ProfileName ?? "";
			to.DurationComment = from.Description;

			bool hasExactDuration = false;
			bool hasRangeDuration = false;
			if ( HasDurationItems( from.ExactDuration ) )
				hasExactDuration = true;
			if ( HasDurationItems( from.MinimumDuration ) || HasDurationItems( from.MaximumDuration ) )
				hasRangeDuration = true;

			to.TypeId = 0;
			//validations should be done before here
			if ( hasExactDuration )
			{
				//if ( hasRangeDuration )
				//{
				//	//inconsistent, take exact for now
				//	ConsoleMessageHelper.SetConsoleErrorMessage( "Error - you must either enter just an exact duration or a 'from - to' duration, not both. For now, the exact duration was used", "", false );
				//}
				to.FromYears = from.ExactDuration.Years;
				to.FromMonths = from.ExactDuration.Months;
				to.FromWeeks = from.ExactDuration.Weeks;
				to.FromDays = from.ExactDuration.Days;
				to.FromHours = from.ExactDuration.Hours;

				to.FromMinutes  = from.ExactDuration.Minutes;
				to.FromDuration = AsSchemaDuration( from.ExactDuration, ref totalMinutes );
                to.AverageMinutes = totalMinutes;

				to.TypeId = from.DurationProfileTypeId == 3 ? from.DurationProfileTypeId : 1 ;
				//reset any to max duration values
				to.ToYears = null;
				to.ToMonths = null;
				to.ToWeeks = null;
				to.ToDays = null;
				to.ToHours = null;
				to.ToMinutes = null;
				to.ToDuration = "";
			}
			else if ( hasRangeDuration )
			{
				to.FromYears = from.MinimumDuration.Years;
				to.FromMonths = from.MinimumDuration.Months;
				to.FromWeeks = from.MinimumDuration.Weeks;
				to.FromDays = from.MinimumDuration.Days;
				to.FromHours = from.MinimumDuration.Hours;
				to.FromMinutes = from.MinimumDuration.Minutes;
				to.FromDuration = AsSchemaDuration( from.MinimumDuration, ref totalMinutes );
                int fromMin = totalMinutes;
				to.ToYears = from.MaximumDuration.Years;
				to.ToMonths = from.MaximumDuration.Months;
				to.ToWeeks = from.MaximumDuration.Weeks;
				to.ToDays = from.MaximumDuration.Days;
				to.ToHours = from.MaximumDuration.Hours;
				to.ToMinutes = from.MaximumDuration.Minutes;
				to.ToDuration = AsSchemaDuration( from.MaximumDuration, ref totalMinutes );
				to.TypeId = 2;
                to.AverageMinutes = ( fromMin + totalMinutes ) / 2;
			}

		}

		private static void MapFromDB( EM.Entity_DurationProfile from, DurationProfile to )
		{
			DurationItem duration = new DurationItem();
			int totalMinutes = 0;
			to.Id = from.Id;
			to.EntityId = from.EntityId ?? 0;
			to.ProfileName = from.ProfileName ?? "";

			//to.ParentUid = from.ParentUid;
			//to.ParentTypeId = from.ParentTypeId;

			to.Conditions = from.DurationComment;
			to.Created = from.Created != null ? ( DateTime ) from.Created : DateTime.Now;
			to.CreatedById = from.CreatedById != null ? ( int ) from.CreatedById : 0;
			to.Created = from.LastUpdated != null ? ( DateTime ) from.LastUpdated : DateTime.Now;
			to.LastUpdatedById = from.LastUpdatedById != null ? ( int ) from.LastUpdatedById : 0;

			duration = new DurationItem();
			duration.Years = from.FromYears == null ? 0 : ( int ) from.FromYears;
			duration.Months = from.FromMonths == null ? 0 : ( int ) from.FromMonths;
			duration.Weeks = from.FromWeeks == null ? 0 : ( int ) from.FromWeeks;
			duration.Days = from.FromDays == null ? 0 : ( int ) from.FromDays;
			duration.Hours = from.FromHours == null ? 0 : ( int ) from.FromHours;
			duration.Minutes = from.FromMinutes == null ? 0 : ( int ) from.FromMinutes;

			if ( HasToDurations( from ) )
			{
				//format as from and to
				to.MinimumDuration = duration;
				to.MinimumDurationISO8601 = AsSchemaDuration( duration, ref totalMinutes );
				to.ProfileSummary = DurationSummary( to.Conditions, duration );

				duration = new DurationItem();
				duration.Years = from.ToYears == null ? 0 : ( int ) from.ToYears;
				duration.Months = from.ToMonths == null ? 0 : ( int ) from.ToMonths;
				duration.Weeks = from.ToWeeks == null ? 0 : ( int ) from.ToWeeks;
				duration.Days = from.ToDays == null ? 0 : ( int ) from.ToDays;
				duration.Hours = from.ToHours == null ? 0 : ( int ) from.ToHours;
				duration.Minutes = from.ToMinutes == null ? 0 : ( int ) from.ToMinutes;

				to.MaximumDuration = duration;
				to.MaximumDurationISO8601 = AsSchemaDuration( duration, ref totalMinutes );

				to.ProfileSummary += DurationSummary( " to ", duration );

			}
			else
			{
				to.ExactDuration = duration;
				to.ExactDurationISO8601 = AsSchemaDuration( duration, ref totalMinutes );
				to.ProfileSummary = DurationSummary( to.Conditions, duration );
			}

			if ( string.IsNullOrWhiteSpace( to.ProfileName ) )
				to.ProfileName = to.ProfileSummary;
		}
		#endregion

		#region  retrieval ==================

		/// <summary>
		/// Retrieve and fill duration profiles for parent entity
		/// </summary>
		/// <param name="parentUid"></param>
		public static List<DurationProfile> GetAll( Guid parentUid, int typeId = 0 )
		{
			DurationProfile row = new DurationProfile();
			DurationItem duration = new DurationItem();
			List<DurationProfile> profiles = new List<DurationProfile>();
			Entity parent = EntityManager.GetEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				return profiles;
			}

			using ( var context = new Data.CTIEntities() )
			{
				List<EM.Entity_DurationProfile> results = context.Entity_DurationProfile
						.Where( s => s.EntityId == parent.Id 
							&& (( typeId == 0 && s.TypeId < 3 ) || s.TypeId == typeId)
							)
						.OrderBy( s => s.Id )
						.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach ( EM.Entity_DurationProfile item in results )
					{
						row = new DurationProfile();
						MapFromDB( item, row );
						profiles.Add( row );
					}
				}
				return profiles;
			}

		}//
		/// <summary>
		/// Get a single DurationProfile by integer Id
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static DurationProfile Get( int id )
		{
			DurationProfile entity = new DurationProfile();
			using ( var context = new Data.CTIEntities() )
			{
				EM.Entity_DurationProfile item = context.Entity_DurationProfile
							.SingleOrDefault( s => s.Id == id );

				if ( item != null && item.Id > 0 )
				{
					MapFromDB( item, entity );
				}
			}

			return entity;
		}


		public bool ValidateDurationProfile( DurationProfile profile, ref bool isEmpty, ref List<string> messages )
		{
			bool isValid = true;
			bool hasConditions = false;
			isEmpty = false;
			//string message = "";
			if ( string.IsNullOrWhiteSpace( profile.Conditions ) == false )
			{
				hasConditions = true;
			}
			bool hasExactDuration = false;
			bool hasRangeDuration = false;
			if ( HasDurationItems( profile.ExactDuration ) )
				hasExactDuration = true;
			if ( HasDurationItems( profile.MinimumDuration ) || HasDurationItems( profile.MaximumDuration ) )
				hasRangeDuration = true;

			//validations should be done before here
			if ( hasExactDuration )
			{
				if ( hasRangeDuration )
				{
					//inconsistent, take exact for now
					messages.Add( "Error - you must either enter an Exact duration or a Minimum/Maximum range duration but not both. For now, the exact duration was used" );
					isValid = false;
				}

			}
			else if ( hasRangeDuration == false && hasConditions == false )
			{
				//nothing, 
				messages.Add( "Error - you must enter either an Exact duration or a Minimum/Maximum range duration (but not both). <br/>");
				isValid = false;
				isEmpty = true;
			}
            else if ( hasRangeDuration )
            {
                var minDuration = profile.MinimumDuration.TotalMinutes();
                var maxDuration = profile.MaximumDuration.TotalMinutes();
                if ( minDuration > maxDuration )
                {
                    messages.Add( "Error - Minimum Duration should be less than Maximum Duration.<br/>" );
                    isValid = false;
                }
            }

            //if ( !string.IsNullOrWhiteSpace( profile.ProfileName ) && profile.ProfileName.Length > 200 )
            //{
            //	//nothing, 
            //	messages.Add( "Error - the profile name is too long, the maximum length is 200 characters.<br/>" );
            //	isValid = false;
            //	isEmpty = false;
            //}
            if ( !string.IsNullOrWhiteSpace( profile.Conditions ) && profile.Conditions.Length > 999 )
			{
				//nothing, 
				messages.Add( "Error - the description is too long, the maximum length is 1000 characters.<br/>" );
				isValid = false;
				isEmpty = false;
			}
			return isValid;
		}
		public static bool HasDurationItems( DurationItem item )
		{
			bool result = false;
			if ( item == null )
				return false;

			if ( item.Years > 0
				|| item.Months > 0
				|| item.Weeks > 0
				|| item.Days > 0
				|| item.Hours > 0
				|| item.Minutes > 0
				)
				result = true;

			return result;
		}

		private static bool HasToDurations( EM.Entity_DurationProfile item )
		{
			bool result = false;
			if ( item.ToYears.HasValue
				|| item.ToMonths.HasValue
				|| item.ToWeeks.HasValue
				|| item.ToDays.HasValue
				|| item.ToHours.HasValue
				|| item.ToMinutes.HasValue
				)
				result = true;

			return result;
		}

		private static string DurationSummary( string conditions, DurationItem entity )
		{
			string duration = "";
			string comma = "";
			if ( string.IsNullOrWhiteSpace( conditions ) )
				duration = "Duration: ";
			else
				duration = conditions + " ";

			if ( entity.Years > 0 )
			{
				duration += SetLabel( entity.Years, "Year" );
				comma = ", ";
			}
			if ( entity.Months > 0 )
			{
				duration += comma + SetLabel( entity.Months, "Month" );
				comma = ", ";
			}
			if ( entity.Weeks > 0 )
			{
				duration += comma + SetLabel( entity.Weeks, "Week" );
				comma = ", ";
			}
			if ( entity.Days > 0 )
			{
				duration += comma + SetLabel( entity.Days, "Day" );
				comma = ", ";
			}
			if ( entity.Hours > 0 )
			{
				duration += comma + SetLabel( entity.Hours, "Hour" );
				comma = ", ";
			}
			if ( entity.Minutes > 0 )
			{
				duration += comma + SetLabel( entity.Minutes, "Minute" );
				comma = ", ";
			}

			//TODO could replace last comma with And
			int lastPos = duration.LastIndexOf( "," );
			if ( lastPos > 0 )
			{
				duration = duration.Substring( 0, lastPos ) + " and " + duration.Substring( lastPos + 1 );
			}
			return duration;
		}
		static string SetLabel( int value, string unit )
		{
			string label = "";
			if ( value > 1 )
				label = string.Format( "{0} {1}s", value, unit );
			else
				label = string.Format( "{0} {1}", value, unit );

			return label;
		}
		public static string AsSchemaDuration( DurationItem entity, ref int totalMinutes )
		{
			string duration = "P";
			totalMinutes = 0;

			if ( entity.Years > 0 )
			{
				duration += entity.Years.ToString() + "Y";
				totalMinutes += entity.Years * 365 * 24 * 3600;
			}
			if ( entity.Months > 0 )
			{
				duration += entity.Months.ToString() + "M";
				totalMinutes += entity.Months * 30 * 24 * 3600;
			}

			if ( entity.Weeks > 0 )
			{
				duration += entity.Weeks.ToString() + "W";
				totalMinutes += entity.Weeks * 5 * 24 * 3600;
			}

			if ( entity.Days > 0 )
			{
				duration += entity.Days.ToString() + "D";
				totalMinutes += entity.Days * 24 * 3600;
			}

			if ( entity.Hours > 0 || entity.Minutes > 0 )
				duration += "T";

			if ( entity.Hours > 0 )
			{
				duration += entity.Hours.ToString() + "H";
				totalMinutes += entity.Hours * 3600;
			}

			if ( entity.Minutes > 0 )
			{
				duration += entity.Minutes.ToString() + "M";
				totalMinutes += entity.Minutes;
			}
			
			return duration;
		}
		//private string AsSchemaDuration( int years, int mths, int weeks, int days = 0, int hours = 0, int minutes = 0 )
		//{
		//	string duration = "P";

		//	if ( years > 0 )
		//		duration += years.ToString() + "Y";
		//	if ( mths > 0 )
		//		duration += mths.ToString() + "M";
		//	if ( weeks > 0 )
		//		duration += weeks.ToString() + "W";
		//	if ( days > 0 )
		//		duration += days.ToString() + "D";
		//	if ( hours > 0 || minutes > 0 )
		//		duration += "T";

		//	if ( hours > 0 )
		//		duration += hours.ToString() + "H";
		//	if ( minutes > 0 )
		//		duration += minutes.ToString() + "M";
		//	return duration;
		//}
		#endregion
	}
}
