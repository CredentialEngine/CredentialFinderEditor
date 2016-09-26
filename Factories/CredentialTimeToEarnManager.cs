using System;
using System.Collections.Generic;
using System.Data.Entity;
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
	public class CredentialTimeToEarnManager : BaseFactory
	{

		#region persistance ==================
		/// <summary>
		/// Persist time to earn
		/// </summary>
		/// <param name="credential"></param>
		/// <returns></returns>
		//public bool Credential_TimeToEarnUpdate( Credential credential )
		//{
		//	bool isValid = true;
		//	int count = 0;
		//	if ( credential.EstimatedTimeToEarn == null)
		//		credential.EstimatedTimeToEarn = new List<DurationProfile>();

		//	EM.Credential_TimeToEarn efEntity = new EM.Credential_TimeToEarn();
		//	string status = "";
		//	using ( var context = new Data.CTIEntities() )
		//	{
		//		//check add/updates first
		//		if ( credential.EstimatedTimeToEarn.Count() > 0 )
		//		{
		//			bool isEmpty = false;	
		//			foreach ( DurationProfile dp in credential.EstimatedTimeToEarn )
		//			{
		//				if ( ValidateDurationProfile( dp, ref isEmpty, ref status ) == false )
		//				{
		//					//can't really scrub from here - too late
		//				}
		//				if ( isEmpty ) //skip
		//					continue;

		//				//just in case
		//				dp.CredentialId = credential.Id;

		//				if ( dp.Id == 0 )
		//				{
		//					//add
		//					efEntity = new EM.Credential_TimeToEarn();
		//					FromMap( dp, efEntity );
		//					efEntity.Created = efEntity.LastUpdated = DateTime.Now;
		//					efEntity.CreatedById = efEntity.LastUpdatedById = credential.LastUpdatedById;

		//					context.Credential_TimeToEarn.Add( efEntity );
		//					count = context.SaveChanges();
		//					//update profile record so doesn't get deleted
		//					dp.Id = efEntity.Id;

		//					if ( count == 0 )
		//					{
		//						ConsoleMessageHelper.SetConsoleErrorMessage( string.Format( " Unable to add Time to Earn: {0} <br\\> ", string.IsNullOrWhiteSpace( dp.Conditions ) ? "no description" : dp.Conditions ) );
		//						//isAllValid = false;
		//					}
		//				}
		//				else
		//				{
		//					efEntity = context.Credential_TimeToEarn.SingleOrDefault( s => s.Id == dp.Id );
		//					if ( efEntity != null && efEntity.Id > 0 )
		//					{
		//						//update
		//						FromMap( dp, efEntity );
		//						//has changed?
		//						if ( HasStateChanged( context ) )
		//						{
		//							//note: testing - the latter may be true if the child has changed - but shouldn't as the mapping only updates the parent
		//							efEntity.LastUpdated = System.DateTime.Now;
		//							efEntity.LastUpdatedById = credential.LastUpdatedById;

		//							count = context.SaveChanges();
		//						}
		//					}
		//					else
		//					{
		//						//??? shouldn't happen unless deleted somehow
								
		//					}
		//				}
						
		//			} //foreach

		//		}

		//		//check for deletes ====================================
		//		//need to ensure ones just added don't get deleted

		//		//get existing 
		//		List<EM.Credential_TimeToEarn> results = context.Credential_TimeToEarn
		//				.Where( s => s.CredentialId == credential.Id )
		//				.OrderBy( s => s.Id )
		//				.ToList();

		//		//if credential.EstimatedTimeToEarn is null, need to delete all!!
		//		if ( results.Count() > 0 && credential.EstimatedTimeToEarn.Count() == 0 )
		//		{
		//			foreach ( var item in results )
		//				context.Credential_TimeToEarn.Remove( item );

		//			context.SaveChanges();
		//		}
		//		else 
		//		{
		//			//should only have existing ids, where not in current list, so should be deletes
		//			var deleteList = from existing in results
		//							 join item in credential.EstimatedTimeToEarn
		//									 on existing.Id equals item.Id
		//									 into joinTable
		//							 from result in joinTable.DefaultIfEmpty( new DurationProfile { Id = 0, CredentialId = 0 } )
		//							 select new { DeleteId = existing.Id, ParentId = ( result.CredentialId ) };

		//			foreach ( var v in deleteList )
		//			{
		//				if ( v.ParentId == 0 )
		//				{
		//					//delete item
		//					EM.Credential_TimeToEarn p = context.Credential_TimeToEarn.FirstOrDefault( s => s.Id == v.DeleteId );
		//					if ( p != null && p.Id > 0 )
		//					{
		//						context.Credential_TimeToEarn.Remove( p );
		//						count = context.SaveChanges();
		//					}
		//				}
		//			}
		//		}
				

		//		//check for adds ====================================
		//	}

		//	return isValid;
		//}
		
		//public bool TimeToEarn_Update( DurationProfile profile, int credentialId, ref string status )
		//{
		//	bool isValid = true;
		//	int count = 0;
		
		//	EM.Credential_TimeToEarn efEntity = new EM.Credential_TimeToEarn();

		//	using ( var context = new Data.CTIEntities() )
		//	{
		//			bool isEmpty = false;
					
		//			if ( ValidateDurationProfile( profile, ref isEmpty, ref status ) == false )
		//			{
		//				//status contains message
		//				return false;
		//			}
		//			if ( isEmpty ) //skip
		//				return false;

		//			//just in case
		//			profile.CredentialId = credentialId;

		//			if ( profile.Id == 0 )
		//			{
		//				//add
		//				efEntity = new EM.Credential_TimeToEarn();
		//				FromMap( profile, efEntity );
		//				efEntity.Created = efEntity.LastUpdated = DateTime.Now;
		//				efEntity.CreatedById = efEntity.LastUpdatedById = profile.LastUpdatedById;

		//				context.Credential_TimeToEarn.Add( efEntity );
		//				count = context.SaveChanges();
		//				//update profile record so doesn't get deleted
		//				profile.Id = efEntity.Id;

		//				if ( count == 0 )
		//				{
		//					status = string.Format( " Unable to add Time to Earn: {0} <br\\> ", string.IsNullOrWhiteSpace( profile.Conditions ) ? "no description" : profile.Conditions ) ;
		//					isValid = false;
		//				}
		//			}
		//			else
		//			{
		//				efEntity = context.Credential_TimeToEarn.SingleOrDefault( s => s.Id == profile.Id );
		//				if ( efEntity != null && efEntity.Id > 0 )
		//				{
		//					//update
		//					FromMap( profile, efEntity );
		//					//has changed?
		//					if ( HasStateChanged( context ) )
		//					{
		//						//note: testing - the latter may be true if the child has changed - but shouldn't as the mapping only updates the parent
		//						efEntity.LastUpdated = System.DateTime.Now;
		//						efEntity.LastUpdatedById = profile.LastUpdatedById;

		//						count = context.SaveChanges();
		//					}
		//				}
		//				else
		//				{
		//					//??? shouldn't happen unless deleted somehow

		//				}
		//			}

		//	}

		//	return isValid;
		//}

		//public bool Credential_TimeToEarnDelete( int recordId, ref string statusMessage )
		//{
		//	bool isOK = true;
		//	using ( var context = new Data.CTIEntities() )
		//	{
		//		EM.Credential_TimeToEarn p = context.Credential_TimeToEarn.FirstOrDefault( s => s.Id == recordId );
		//		if ( p != null && p.Id > 0 )
		//		{
		//			context.Credential_TimeToEarn.Remove( p );
		//			int count = context.SaveChanges();
		//		}
		//		else
		//		{
		//			statusMessage = string.Format( "Credential_TimeToEarn record was not found: {0}", recordId );
		//			isOK = false;
		//		}
		//	}
		//	return isOK;

		//}
		//private void FromMap( DurationProfile from, EM.Credential_TimeToEarn to )
		//{
			
		//	to.Id = from.Id;
		//	to.CredentialId = from.CredentialId;
		//	to.ProfileName = from.ProfileName ?? "";
		//	to.DurationComment = from.Conditions;

		//	bool hasExactDuration = false;
		//	bool hasRangeDuration = false;
		//	if ( HasDurationItems( from.ExactDuration ) )
		//		hasExactDuration = true;
		//	if ( HasDurationItems( from.MinimumDuration ) || HasDurationItems( from.MaximumDuration ) )
		//		hasRangeDuration = true;
		//	to.TypeId = 0;
		//	//validations should be done before here
		//	if ( hasExactDuration )
		//	{
		//		//if ( hasRangeDuration )
		//		//{
		//		//	//inconsistent, take exact for now
		//		//	ConsoleMessageHelper.SetConsoleErrorMessage( "Error - you must either enter just an exact duration or a 'from - to' duration, not both. For now, the exact duration was used", "", false );
		//		//}
		//		to.FromYears = from.ExactDuration.Years;
		//		to.FromMonths = from.ExactDuration.Months;
		//		to.FromWeeks = from.ExactDuration.Weeks;
		//		to.FromDays = from.ExactDuration.Days;
		//		to.FromHours = from.ExactDuration.Hours;
		//		to.FromDuration = AsSchemaDuration( from.ExactDuration );
		//		to.TypeId = 1;
		//	}
		//	else if ( hasRangeDuration )
		//	{
		//		to.FromYears = from.MinimumDuration.Years;
		//		to.FromMonths = from.MinimumDuration.Months;
		//		to.FromWeeks = from.MinimumDuration.Weeks;
		//		to.FromDays = from.MinimumDuration.Days;
		//		to.FromHours = from.MinimumDuration.Hours;
		//		to.FromDuration = AsSchemaDuration( from.MinimumDuration );

		//		to.ToYears = from.MaximumDuration.Years;
		//		to.ToMonths = from.MaximumDuration.Months;
		//		to.ToWeeks = from.MaximumDuration.Weeks;
		//		to.ToDays = from.MaximumDuration.Days;
		//		to.ToHours = from.MaximumDuration.Hours;
		//		to.ToDuration = AsSchemaDuration( from.MaximumDuration );
		//		to.TypeId = 2;
		//	}
		//	//else
		//	//{
		//	//	//nothing, 
		//	//	ConsoleMessageHelper.SetConsoleErrorMessage( "Error - you must enter either an exact duration or a 'from - to' duration (but not both). <br/>", "", false );
		//	//}
			

		//}
		#endregion

		#region  retrieval ==================

		/// <summary>
		/// Retrieve and fill time to earn for credential
		/// </summary>
		/// <param name="credential"></param>
		//public static void TimeToEarn_Get( Credential credential )
		//{
		//	DurationProfile row = new DurationProfile();
		//	DurationItem duration = new DurationItem();
		//	credential.EstimatedTimeToEarn = new List<DurationProfile>();

		//	using ( var context = new Data.CTIEntities() )
		//	{
		//		List<EM.Credential_TimeToEarn> results = context.Credential_TimeToEarn
		//				.Where( s => s.CredentialId == credential.Id)
		//				.OrderBy( s => s.Id )
		//				.ToList();

		//		if ( results != null && results.Count > 0 )
		//		{
		//			foreach ( EM.Credential_TimeToEarn item in results )
		//			{
		//				row = new DurationProfile();
		//				row.Id = item.Id;
		//				row.CredentialId = item.CredentialId;
		//				row.ProfileName = item.ProfileName ?? "";
		//				row.Conditions = item.DurationComment;
		//				row.Created = item.Created != null ? (DateTime) item.Created : DateTime.Now;
		//				row.CreatedById = item.CreatedById != null ? ( int ) item.CreatedById : 0;
		//				row.Created = item.LastUpdated != null ? ( DateTime ) item.LastUpdated : DateTime.Now;
		//				row.LastUpdatedById = item.LastUpdatedById != null ? ( int ) item.LastUpdatedById : 0;

		//				duration = new DurationItem();
		//				duration.Years = item.FromYears == null ? 0 : (int) item.FromYears;
		//				duration.Months = item.FromMonths == null ? 0 : ( int ) item.FromMonths;
		//				duration.Weeks = item.FromWeeks == null ? 0 : ( int ) item.FromWeeks;
		//				duration.Days = item.FromDays == null ? 0 : ( int ) item.FromDays;
		//				duration.Hours = item.FromHours == null ? 0 : ( int ) item.FromHours;
						
		//				if ( HasToDurations( item ) )
		//				{
		//					//format as from and to
		//					row.MinimumDuration = duration;
		//					row.MinimumDurationISO8601 = AsSchemaDuration( duration );
		//					row.ProfileSummary = DurationSummary( row.Conditions, duration );

		//					duration = new DurationItem();
		//					duration.Years = item.ToYears == null ? 0 : ( int ) item.ToYears;
		//					duration.Months = item.ToMonths == null ? 0 : ( int ) item.ToMonths;
		//					duration.Weeks = item.ToWeeks == null ? 0 : ( int ) item.ToWeeks;
		//					duration.Days = item.ToDays == null ? 0 : ( int ) item.ToDays;
		//					duration.Hours = item.ToHours == null ? 0 : ( int ) item.ToHours;
		//					row.MaximumDuration = duration;
		//					row.MaximumDurationISO8601 = AsSchemaDuration( duration );

		//					row.ProfileSummary += DurationSummary( " to ", duration );

		//				}
		//				else
		//				{
		//					row.ExactDuration = duration;
		//					row.ExactDurationISO8601 = AsSchemaDuration( duration );
		//					row.ProfileSummary = DurationSummary( row.Conditions, duration );
		//				}

		//				LoggingHelper.DoTrace( 5, "CredentialTimeToEarnManager. TimeToEarn_Get() row.ProfileSummary: " + row.ProfileSummary );

		//				credential.EstimatedTimeToEarn.Add( row );
		//			}
		//		}
		//	}

		//}//

		//public static List<DurationProfile> DurationProfile_GetAll( int credentialId )
		//{
		//	DurationProfile row = new DurationProfile();
		//	DurationItem duration = new DurationItem();
		//	List<DurationProfile> profiles = new List<DurationProfile>();

		//	using ( var context = new Data.CTIEntities() )
		//	{
		//		List<EM.Credential_TimeToEarn> results = context.Credential_TimeToEarn
		//				.Where( s => s.CredentialId == credentialId )
		//				.OrderBy( s => s.Id )
		//				.ToList();

		//		if ( results != null && results.Count > 0 )
		//		{
		//			foreach ( EM.Credential_TimeToEarn item in results )
		//			{
		//				row = new DurationProfile();
		//				ToMap( item, row );



		//				profiles.Add( row );
		//			}
		//		}
		//	}
		//	return profiles;
		//}//

		/// <summary>
		/// Get TimeToEarn as a DurationProfile
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		//public static DurationProfile Get( int id )
		//{
		//	DurationProfile entity = new DurationProfile();
		//	using ( var context = new Data.CTIEntities() )
		//	{
		//		EM.Credential_TimeToEarn item = context.Credential_TimeToEarn
		//					.SingleOrDefault( s => s.Id == id );

		//		if ( item != null && item.Id > 0 )
		//		{
		//			ToMap( item, entity );
		//		}
		//	}

		//	return entity;
		//}
		//private static void ToMap( EM.Credential_TimeToEarn from, DurationProfile to )
		//{
		//	DurationItem duration = new DurationItem();

		//	to.Id = from.Id;
		//	to.CredentialId = from.CredentialId;
		//	//to.ParentTypeId = from.ParentTypeId;
		//	to.ProfileName = from.ProfileName ?? "";

		//	to.Conditions = from.DurationComment;
		//	to.Created = from.Created != null ? ( DateTime ) from.Created : DateTime.Now;
		//	to.CreatedById = from.CreatedById != null ? ( int ) from.CreatedById : 0;
		//	to.Created = from.LastUpdated != null ? ( DateTime ) from.LastUpdated : DateTime.Now;
		//	to.LastUpdatedById = from.LastUpdatedById != null ? ( int ) from.LastUpdatedById : 0;

		//	duration = new DurationItem();
		//	duration.Years = from.FromYears == null ? 0 : ( int ) from.FromYears;
		//	duration.Months = from.FromMonths == null ? 0 : ( int ) from.FromMonths;
		//	duration.Weeks = from.FromWeeks == null ? 0 : ( int ) from.FromWeeks;
		//	duration.Days = from.FromDays == null ? 0 : ( int ) from.FromDays;
		//	duration.Hours = from.FromHours == null ? 0 : ( int ) from.FromHours;
		//	//duration.Minutes = from.FromMinutes == null ? 0 : ( int ) from.FromMinutes;

		//	if ( HasToDurations( from ) )
		//	{
		//		//format as from and to
		//		to.MinimumDuration = duration;
		//		to.MinimumDurationISO8601 = AsSchemaDuration( duration );
		//		to.ProfileSummary = DurationSummary( to.Conditions, duration );

		//		duration = new DurationItem();
		//		duration.Years = from.ToYears == null ? 0 : ( int ) from.ToYears;
		//		duration.Months = from.ToMonths == null ? 0 : ( int ) from.ToMonths;
		//		duration.Weeks = from.ToWeeks == null ? 0 : ( int ) from.ToWeeks;
		//		duration.Days = from.ToDays == null ? 0 : ( int ) from.ToDays;
		//		duration.Hours = from.ToHours == null ? 0 : ( int ) from.ToHours;
		//		//duration.Minutes = from.ToMinutes == null ? 0 : ( int ) from.ToMinutes;

		//		to.MaximumDuration = duration;
		//		to.MaximumDurationISO8601 = AsSchemaDuration( duration );

		//		to.ProfileSummary += DurationSummary( " to ", duration );

		//	}
		//	else
		//	{
		//		to.ExactDuration = duration;
		//		to.ExactDurationISO8601 = AsSchemaDuration( duration );
		//		to.ProfileSummary = DurationSummary( to.Conditions, duration );
		//	}
		//	if ( string.IsNullOrWhiteSpace( to.ProfileName ) )
		//		to.ProfileName = to.ProfileSummary;
		//}
		//public bool ValidateDurationProfile( DurationProfile profile, ref bool isEmpty, ref string status )
		//{
		//	bool isValid = true;
		//	bool hasConditions = false;
		//	isEmpty = false;
		//	//string message = "";
		//	if ( string.IsNullOrWhiteSpace( profile.Conditions ) == false )
		//	{
		//		hasConditions = true;
		//	}
		//	bool hasExactDuration = false;
		//	bool hasRangeDuration = false;
		//	if ( HasDurationItems( profile.ExactDuration ) )
		//		hasExactDuration = true;
		//	if ( HasDurationItems( profile.MinimumDuration ) || HasDurationItems( profile.MaximumDuration ) )
		//		hasRangeDuration = true;

		//	//validations should be done before here
		//	if ( hasExactDuration )
		//	{
		//		if ( hasRangeDuration )
		//		{
		//			//inconsistent, take exact for now
		//			status= "Error - you must either enter just an exact duration or a 'profile - to' duration, not both. For now, the exact duration was used" ;
		//			isValid = false;
		//		}

		//	}
		//	else if ( hasRangeDuration == false && hasConditions  == false)
		//	{
		//		//nothing, 
		//		status = "Error - you must enter either an exact duration or a 'profile - to' duration (but not both). <br/>";
		//		isValid = false;
		//		isEmpty = true;
		//	}

		//	return isValid;
		//}
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
				)
				result = true;

			return result;
		}
		private static bool HasFromDurations( EM.Credential_TimeToEarn item )
		{
			bool result = false;
			if ( item.FromYears.HasValue
				|| item.FromMonths.HasValue
				|| item.FromWeeks.HasValue
				|| item.FromDays.HasValue
				|| item.FromHours.HasValue
				)
				result = true;

			return result;
		}
		private static bool HasToDurations( EM.Credential_TimeToEarn item )
		{
			bool result = false;
			if ( item.ToYears.HasValue
				|| item.ToMonths.HasValue
				|| item.ToWeeks.HasValue
				|| item.ToDays.HasValue
				|| item.ToHours.HasValue
				)
				result = true;

			return result;
		}

		//private static string DurationSummary( string conditions, DurationItem entity )
		//{
		//	string duration = "";
		//	string comma = "";
		//	if ( string.IsNullOrWhiteSpace( conditions ) )
		//		duration = "Duration: ";
		//	else
		//		duration = conditions + " ";

		//	if ( entity.Years > 0 )
		//	{
		//		duration += SetLabel( entity.Years, "Year" );
		//		comma = ", ";
		//	}
		//	if ( entity.Months > 0 )
		//	{
		//		duration += comma + SetLabel( entity.Months, "Month" );
		//		comma = ", ";
		//	}
		//	if ( entity.Weeks > 0 )
		//	{
		//		duration += comma + SetLabel( entity.Weeks, "Week" );
		//		comma = ", ";
		//	}
		//	if ( entity.Days > 0 )
		//	{
		//		duration += comma + SetLabel( entity.Days, "Day" );
		//		comma = ", ";
		//	}
		//	if ( entity.Hours > 0 )
		//	{
		//		duration += comma + SetLabel( entity.Hours, "Hour" );
		//		comma = ", ";
		//	}
		//	/*if ( entity.Minutes > 0 )
		//	{
		//		duration += comma + SetLabel( entity.Minutes, "Minute" );
		//		comma = ", ";
		//	}*/

		//	//TODO could replace last comma with And
		//	int lastPos = duration.LastIndexOf( "," );
		//	if ( lastPos > 0 )
		//	{
		//		duration = duration.Substring( 0, lastPos ) + " and " + duration.Substring( lastPos + 1 );
		//	}
		//	return duration;
		//}
		static string SetLabel( int value, string unit )
		{
			string label = "";
			if ( value > 1 )
				label = string.Format( "{0} {1}s", value, unit );
			else
				label = string.Format( "{0} {1}", value, unit );

			return label;
		}
		private static string AsSchemaDuration( DurationItem entity )
		{
			string duration = "P";

			if ( entity.Years > 0 )
				duration += entity.Years.ToString() + "Y";
			if ( entity.Months> 0 )
				duration += entity.Months.ToString() + "M";
			if ( entity.Weeks > 0 )
				duration += entity.Weeks.ToString() + "W";
			if ( entity.Days > 0 )
				duration += entity.Days.ToString() + "D";
			if ( entity.Hours > 0 )// || entity.Minutes > 0 )
				duration += "T";

			if ( entity.Hours > 0 )
				duration += entity.Hours.ToString() + "H";
			/*if ( entity.Minutes > 0 )
				duration += entity.Minutes.ToString() + "M";
			*/
			return duration;
		}
		private string AsSchemaDuration( int years, int mths, int weeks, int days = 0, int hours = 0, int minutes = 0)
		{
			string duration = "P";

			if ( years > 0 )
				duration += years.ToString() + "Y";
			if ( mths > 0 )
				duration += mths.ToString() + "M";
			if ( weeks > 0 )
				duration += weeks.ToString() + "W";
			if ( days > 0 )
				duration += days.ToString() + "D";
			if ( hours > 0 || minutes > 0 )
				duration += "T";

			if ( hours > 0 )
				duration += hours.ToString() + "H" ;
			if ( minutes > 0 )
				duration += minutes.ToString()  + "M";
			return duration;
		}
		#endregion
	}
}
