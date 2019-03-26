using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models;
using Models.Common;
using CM = Models.Common;
using MN = Models.Node;
using EM = Data;
using DBEntity = Data.Location;
using ThisEntity = Models.Common.Location;
using Address = Models.Common.Address;

using Utilities;
using Models.Search.ThirdPartyApiModels;

namespace Factories
{
    public class LocationManager : BaseFactory
    {
        static string thisClassName = "LocationManager";

        #region Persistance - Location
        public bool Save( Address address, int orgId, int userId, ref List<string> messages )
        {
            ThisEntity entity = new ThisEntity();
            entity.OrganizationId = orgId;
            entity.Name = address.Name;
            entity.ExternalIdentifier = address.ExternalIdentifier;
            entity.Address1 = address.Address1;
            entity.Address2 = address.Address2;
            entity.City = address.City;
            entity.PostalCode = address.PostalCode;
            entity.PostOfficeBoxNumber = address.PostOfficeBoxNumber;
            entity.AddressRegion = address.AddressRegion;
            entity.CountryId = address.CountryId;
            entity.Country = address.Country;

            return Save( entity, userId, ref messages );
        }
        public bool Save( ThisEntity entity, int userId, ref List<string> messages )
        {
            bool isValid = true;
            int intialCount = messages.Count;

            if ( entity.OrganizationId < 1 )
            {
                messages.Add( "Error: a valid organization identifier was not provided." );
                return false;
            }

            int count = 0;

            DBEntity efEntity = new DBEntity();

            try
            {
                using ( var context = new Data.CTIEntities() )
                {
                    //Need a duplicates check
                    //may not always have an external identifier
                    if ( ValidateProfile( entity, ref messages ) == false )
                    {
                        return false;
                    }
                    bool foundByIdentifier = false;
                    if ( entity.Id == 0 )
                    {
                        ThisEntity exists = Exists( entity.OrganizationId, entity.ExternalIdentifier, entity.Name, ref foundByIdentifier );
                        entity.Id = exists.Id;
                    }

                    if ( entity.Id == 0 )
                    {
                        //add
                        efEntity = new DBEntity();
                        efEntity.OrganizationId = entity.OrganizationId;
                        MapToDB( entity, efEntity );

                        efEntity.Created = efEntity.LastUpdated = DateTime.Now;
                        efEntity.CreatedById = efEntity.LastUpdatedById = userId;
                        efEntity.RowId = Guid.NewGuid();

                        context.Location.Add( efEntity );
                        count = context.SaveChanges();

                        //update profile record so doesn't get deleted
                        entity.Id = efEntity.Id;
                        entity.RowId = efEntity.RowId;
                        if ( count == 0 )
                        {
                            messages.Add( string.Format( " Unable to add Profile: {0} <br\\> ", string.IsNullOrWhiteSpace( entity.Name ) ? "no description" : entity.Name ) );
                        }
           
                    }
                    else
                    {

                        efEntity = context.Location.SingleOrDefault( s => s.Id == entity.Id );
                        if ( efEntity != null && efEntity.Id > 0 )
                        {
                            entity.RowId = efEntity.RowId;
                            //update
                            MapToDB( entity, efEntity);
                            //has changed?
                            if ( HasStateChanged( context ) )
                            {
                                efEntity.LastUpdated = System.DateTime.Now;
                                efEntity.LastUpdatedById = userId;

                                count = context.SaveChanges();
                            }
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                string message = FormatExceptions( ex );
                messages.Add( "Error - the save was not successful. " + message );
                LoggingHelper.LogError( ex, thisClassName + string.Format( ".Save(), OrganizationId: {0}, UserId: {1}", entity.OrganizationId, userId ) );
                isValid = false;
            }
            return isValid;
        }
        public bool Delete( int profileId, ref string statusMessage )
        {
            bool isOK = true;
            using ( var context = new Data.CTIEntities() )
            {
                DBEntity p = context.Location.FirstOrDefault( s => s.Id == profileId );
                if ( p != null && p.Id > 0 )
                {
                    context.Location.Remove( p );
                    int count = context.SaveChanges();
                }
                else
                {
                    statusMessage = string.Format( "Requested record was not found: {0}", profileId );
                    isOK = false;
                }
            }
            return isOK;

        }

        public bool ValidateProfile( ThisEntity profile, ref List<string> messages )
        {
            bool isValid = true;

            //check minimum
            if ( string.IsNullOrWhiteSpace( profile.Address1 )
            && string.IsNullOrWhiteSpace( profile.PostOfficeBoxNumber )
                )
            {
                messages.Add( "Please enter at least Street Address 1 or a Post Office Box Number" );
            }
            if ( string.IsNullOrWhiteSpace( profile.City ) )
            {
                messages.Add( "Please enter a valid Locality/City" );
            }
            if ( string.IsNullOrWhiteSpace( profile.AddressRegion ) )
            {
                messages.Add( "Please enter a valid Region/State/Province" );
            }
            if ( string.IsNullOrWhiteSpace( profile.PostalCode )
               )
            {
                messages.Add( "Please enter a valid Postal Code" );
            }
            if ( string.IsNullOrWhiteSpace( profile.Name ) )
            {
                if ( !string.IsNullOrWhiteSpace( profile.City ) )
                    profile.Name = profile.City;
                else if ( !string.IsNullOrWhiteSpace( profile.Address1 ) )
                    profile.Name = profile.Address1;
                else
                    profile.Name = "Main Address";
            }

            if ( ( profile.Name ?? "" ).Length > 200 )
                messages.Add( "The address name must be less than 200 characters" );
            if ( ( profile.Address1 ?? "" ).Length > 200 )
                messages.Add( "The address1 must be less than 200 characters" );
            if ( ( profile.Address2 ?? "" ).Length > 200 )
                messages.Add( "The address2 must be less than 200 characters" );

            if ( messages.Count > 0 )
                isValid = false;
            return isValid;
        }
        #endregion
        #region  retrieval ==================

        public static List<ThisEntity> GetAll( int orgId )
        {
            ThisEntity entity = new ThisEntity();
            List<ThisEntity> list = new List<ThisEntity>();
            try
            {
                using ( var context = new Data.CTIEntities() )
                {
                    List<EM.Location> results = context.Location
                            .Where( s => s.OrganizationId == orgId )
                            .OrderByDescending( s => s.City )
                            .ThenBy( s => s.Id )
                            .ToList();

                    if ( results != null && results.Count > 0 )
                    {
                        foreach ( EM.Location item in results )
                        {
                            entity = new ThisEntity();
                            MapFromDB( item, entity );

                            list.Add( entity );
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".GetAll" );
            }
            return list;
        }//

        /// <summary>
        /// Get list of addresses using a list of Ids.
        /// the get are done separately so can confirm the id is valid
        /// </summary>
        /// <param name="idsList"></param>
        /// <param name="messages"></param>
        /// <returns></returns>
        public static List<ThisEntity> GetList( List<int> idsList, ref List<string> messages )
        {
            ThisEntity entity = new ThisEntity();
            List<ThisEntity> list = new List<ThisEntity>();
            try
            {
                using ( var context = new Data.CTIEntities() )
                {
                    foreach ( int id in idsList )
                    {
                        entity = Get( id );
                        if ( entity == null || entity.Id == 0 )
                            messages.Add( string.Format( "Error: the address identifier of: {0} was not found.", id ) );
                        else
                            list.Add( entity );
                    }
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".GetList" );
            }
            return list;
        }//
        public static bool HasAddress( Guid parentUid )
        {

            try
            {
                using ( var context = new Data.CTIEntities() )
                {
                    List<EM.Location> results = context.Location
                            .Where( s => s.Organization.RowId == parentUid )
                            .OrderByDescending( s => s.City )
                            .ThenBy( s => s.Id )
                            .ToList();

                    if ( results != null && results.Count > 0 )
                    {
                        return true;
                    }
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".HasAddress" );
            }
            return false;
        }//

        public static ThisEntity Get( int profileId )
        {
            ThisEntity entity = new ThisEntity();
            try
            {

                using ( var context = new Data.CTIEntities() )
                {
                    DBEntity item = context.Location
                            .SingleOrDefault( s => s.Id == profileId );

                    if ( item != null && item.Id > 0 )
                    {
                        MapFromDB( item, entity );
                    }
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + string.Format( ".Get. profileId: {0}", profileId ) );
            }
            return entity;
        }//
        public static ThisEntity Exists( int orgId, string extIdentifier, string addressName, ref bool foundByIdentifier )
        {
            ThisEntity entity = new ThisEntity();
            DBEntity item = new DBEntity();
            try
            {

                using ( var context = new Data.CTIEntities() )
                {
                    if ( !string.IsNullOrWhiteSpace( extIdentifier ) )
                    {
                        item = context.Location
                            .FirstOrDefault( s => s.Id == orgId && s.ExternalIdentifier.ToLower() == extIdentifier.ToLower() );
                        if ( item != null && item.Id > 0 )
                        {
                            foundByIdentifier = true;
                            MapFromDB( item, entity );
                        }
                    }
                    else if ( !string.IsNullOrWhiteSpace( addressName ) )
                    {
                        item = context.Location
                            .FirstOrDefault( s => s.Id == orgId && s.Name.ToLower() == addressName.ToLower() );
                        if ( item != null && item.Id > 0 )
                        {
                            MapFromDB( item, entity );
                        }
                    }
                    else
                    {
                       //others?
                    }

                    
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + string.Format( ".Get. profileId: {0}", orgId ) );
            }
            return entity;
        }//
        public static void MapFromDB( EM.Location from, CM.Address to )
        {
            to.Id = from.Id;
            to.RowId = from.RowId;

            to.Name = from.Name;
            to.ExternalIdentifier = from.ExternalIdentifier;

            to.Address1 = from.Address1;
            to.Address2 = from.Address2 ?? "";
            to.PostOfficeBoxNumber = from.PostOfficeBoxNumber ?? "";
            to.City = from.City;
            to.PostalCode = from.PostalCode;
            to.AddressRegion = from.Region;
            //to.Country = from.Country;
            to.CountryId = ( int ) ( from.CountryId ?? 0 );
			if ( from.Codes_Countries != null )
			{
				to.Country = from.Codes_Countries.CommonName;
			}
			to.Latitude = from.Latitude ?? 0;
            to.Longitude = from.Longitude ?? 0;

            to.ContactPoint = Entity_ContactPointManager.GetAll( to.RowId );

            if ( IsValidDate( from.Created ) )
                to.Created = ( DateTime ) from.Created;
            to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;
            if ( IsValidDate( from.LastUpdated ) )
                to.LastUpdated = ( DateTime ) from.LastUpdated;
            to.LastUpdatedById = from.LastUpdatedById == null ? 0 : ( int ) from.LastUpdatedById;

            //to.LastUpdatedBy = SetLastUpdatedBy( to.LastUpdatedById, from.Account_Modifier );


        }

        public static void MapToDB( CM.Address from, EM.Location to )
        {
            //want to ensure fields from create are not wiped
            //if ( to.Id == 0 )
            //{
            //	if ( IsValidDate( from.Created ) )
            //		to.Created = from.Created;
            //	to.CreatedById = from.CreatedById;
            //}
            //NOTE: the parentId - currently orgId, is handled in the update code
            to.Id = from.Id;
            to.Name = from.Name;
            //ensure to not wipe out the identifier, as not on input form
            if (string.IsNullOrWhiteSpace( to.ExternalIdentifier ) )
                to.ExternalIdentifier = from.ExternalIdentifier;
            else if ( !string.IsNullOrWhiteSpace( from.ExternalIdentifier ) && from.ExternalIdentifier.Length > 0 )
            {
                to.ExternalIdentifier = from.ExternalIdentifier;
            }
            bool hasChanged = false;
            bool hasAddress = false;

            if ( from.HasAddress() )
            {
                hasAddress = true;
                if ( to.Latitude == null || to.Latitude == 0 )
                    hasChanged = true;
            }
            if ( hasChanged == false )
            {
                if ( to.Id == 0 )
                    hasChanged = true;
                else
                    hasChanged = HasAddressChanged( from, to );
            }

            to.Address1 = GetData( from.Address1, null );
            to.Address2 = GetData( from.Address2, null );
            to.PostOfficeBoxNumber = GetData( from.PostOfficeBoxNumber, null );

            to.City = from.City != null ? from.City.Trim() : "";
            to.PostalCode = from.PostalCode != null ? from.PostalCode.Trim() : "";
            to.Region = from.AddressRegion != null ? from.AddressRegion.Trim() : "";
            to.Country = from.Country != null ? from.Country.Trim() : "";

            if ( from.CountryId == 0 )
                to.CountryId = null;
            else
                to.CountryId = from.CountryId;
            to.Latitude = from.Latitude;
            to.Longitude = from.Longitude;

            //these will likely not be present? 
            //If new, or address has changed, do the geo lookup
            if ( hasAddress )
            {
                //input usually doesn't have lat/lng, unless copied
                if ( hasChanged && !from.HasLatLng() )
                {
                    UpdateGeo( from, to );
                }
            }
            else
            {
                to.Latitude = 0;
                to.Longitude = 0;
            }

            //these will be set in the update code anyway
            if ( IsValidDate( from.LastUpdated ) )
                to.LastUpdated = from.LastUpdated;
            to.LastUpdatedById = from.LastUpdatedById;
        }

        public List<ThisEntity> ResolveMissingGeodata( ref string messages, int maxRecords = 100 )
        {
            ThisEntity entity = new ThisEntity();
            List<ThisEntity> list = new List<ThisEntity>();
            List<string> messageList = new List<string>();
            bool resetIsPrimaryFlag = false;
            string prevAddr = "";
            string prevAddr2 = "";
            string prevCity = "";
            string prevRegion = "";

            string prevPostalCode = "";
            double prevLat = 0.0;
            double prevLng = 0.0;
            int cntr = 0;
            try
            {
                using ( var context = new Data.CTIEntities() )
                {
                    List<EM.Location> results = context.Location
                            .Where( s => ( s.Latitude == null || s.Latitude == 0.0 )
                            || ( s.Longitude == null || s.Longitude == 0.0 ) )
                            .OrderBy( s => s.Address1 ).ThenBy( s => s.Address2 ).ThenBy( s => s.City ).ThenBy( s => s.PostalCode ).ThenBy( s => s.Region )
                            .ToList();

                    if ( results != null && results.Count > 0 )
                    {
                        foreach ( EM.Location efEntity in results )
                        {
                            cntr++;
                            entity = new ThisEntity();
                            if ( ( efEntity.City ?? "" ).ToLower() == "city"
                                || ( efEntity.Address1 ?? "" ).ToLower() == "address"
                                || ( efEntity.Address1 ?? "" ).ToLower().IndexOf( "123 main" ) > -1
                                || ( efEntity.Address1 ?? "" ).ToLower().IndexOf( "some street" ) > -1
                                || ( efEntity.Region ?? "" ).ToLower().IndexOf( "state" ) == 0
                                )
                                continue;

                            //quick approach, map to address, which will call the geo code. If there was an update, update the entity
                            //check if the same address to avoid many hits against google endpoint
                            if ( efEntity.Address1 == prevAddr
                                && ( efEntity.Address2 ?? "" ) == prevAddr2
                                && ( efEntity.City ?? "" ) == prevCity
                                && ( efEntity.Region ?? "" ) == prevRegion
                                && ( efEntity.PostalCode ?? "" ) == prevPostalCode
                                )
                            {
                                efEntity.Latitude = prevLat;
                                efEntity.Longitude = prevLng;
                            }
                            else
                            {
                                //save prev region now, in case it gets expanded, although successive ones will not be expanded!
                                prevRegion = efEntity.Region ?? "";
                                MapFromDB( efEntity, entity );
                                MapToDB( entity, efEntity );
                            }
                            if ( HasStateChanged( context ) )
                            {
                                efEntity.LastUpdated = System.DateTime.Now;
                                //efEntity.LastUpdatedById = userId;

                                int count = context.SaveChanges();
                                messageList.Add( string.Format( "___Updated address: {0}", DisplayAddress( efEntity ) ) );
                                prevLat = ( double ) ( efEntity.Latitude ?? 0.0 );
                                prevLng = ( double ) ( efEntity.Longitude ?? 0.0 );
                            }
                            else
                            {
                                //addresses that failed
                                list.Add( entity );
                            }
                            prevAddr = efEntity.Address1 ?? "";
                            prevAddr2 = efEntity.Address2 ?? "";
                            prevCity = efEntity.City ?? "";
                            //prevRegion = efEntity.Region ?? "";
                            prevPostalCode = efEntity.PostalCode ?? "";
                            if ( maxRecords > 0 && cntr > maxRecords )
                            {
                                messages = string.Format( "Early completion. Processed {0} of {1} candidate records.", cntr, results.Count );
                                break;
                            }

                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".ResolveMissingGeodata" );
            }

            messages += string.Join( "<br/>", messageList.ToArray() );

            return list;
        }//
        public static void UpdateGeo( CM.Address from, EM.Location to )
        {
            //GoogleGeocoding.Results results = GeoServices.GeocodeAddress( from.DisplayAddress() );
            bool doingExpandOfRegion = UtilityManager.GetAppKeyValue( "doingExpandOfRegion", false );
            //Try with a looser address if 0/0 lat/lng
            var hasLatLng = false;
            var results = new GoogleGeocoding.Results();
            var addressesToTry = new List<string>()
            {
                from.DisplayAddress(),
                from.LooseDisplayAddress(),
                from.PostalCode ?? "",
                from.AddressRegion ?? "",
                from.Country ?? ""
            };
            foreach ( var test in addressesToTry )
            {
                results = TryGetAddress( test, ref hasLatLng );
                if ( hasLatLng )
                {
                    break;
                }
                System.Threading.Thread.Sleep( 3000 ); //Don't spam the Geocoding API
            }

            //Continue
            if ( results != null )
            {
                GoogleGeocoding.Location location = results.GetLocation();
                if ( location != null )
                {
                    to.Latitude = location.lat;
                    to.Longitude = location.lng;
                }
                try
                {
                    if ( results.results.Count > 0 )
                    {
                        //this is inconsistant [0] -postal code, [5]-country, [4] region
                        //                  int pIdx = 0;// results.results[ 0 ].address_components.Count - 1;
                        //int cIdx = results.results[ 0 ].address_components.Count - 1;
                        //                  int regionIdx = results.results[ 0 ].address_components.Count - 2;
                        string postalCode = "";// results.results[ 0 ].address_components[ 0 ].short_name;
                        string country = "";// results.results[ 0 ].address_components[ cIdx ].long_name;
                        string fullRegion = ""; // results.results[ 0 ].address_components[ regionIdx ].long_name;
                        //can we expand the region here? - determine the index number of the region
                        string suffix = "";
                        //want to at least implement in the import
                        foreach ( var part in results.results[ 0 ].address_components )
                        {
                            if ( part.types.Count > 0 )
                            {
                                if ( part.types[ 0 ] == "country" )
                                    country = part.long_name;
                                else if ( part.types[ 0 ] == "administrative_area_level_1" )
                                    fullRegion = part.long_name;
                                else if ( part.types[ 0 ] == "postal_code" )
                                    postalCode = part.long_name;
                                else if ( part.types[ 0 ] == "postal_code_suffix" )
                                {
                                    suffix = part.long_name;
                                    postalCode += "-" + suffix;
                                }
                            }
                            //
                        }

                        if ( string.IsNullOrEmpty( to.PostalCode ) ||
     ( !string.IsNullOrEmpty( postalCode ) && to.PostalCode != postalCode ) )
                        {
                            //?not sure if should assume the google result is accurate
                            to.PostalCode = postalCode;
                        }
                        if ( !string.IsNullOrEmpty( country ) &&
                            to.CountryId == null )
                        {
                            //set country string, and perhaps plan update process.
                            to.Country = country;
                            //do lookup, OR at least notify for now
                            CodeItem item = CodesManager.GetCountry( country );
                            if ( item != null && item.Id > 0 )
                                to.CountryId = item.Id;

                            //probably should make configurable - or spin off process to attempt update
                            //EmailManager.NotifyAdmin( "CTI Missing country to update", string.Format( "Address without country entered, but resolved via GoogleGeocoding.Location. entity.ParentId: {0}, country: {1}", from.ParentId, country ) );
                        }
                        //expand region
                        if ( doingExpandOfRegion
                            && ( to.Region ?? "" ).Length < fullRegion.Length )
                        {
                            to.Region = fullRegion;
                        }
                    }

                }
                catch ( Exception ex )
                {
                    LoggingHelper.LogError( ex, thisClassName + "UpdateGeo" );
                }
            }
        }
        private static GoogleGeocoding.Results TryGetAddress( string address, ref bool hasLatLng )
        {
            try
            {
                var results = GeoServices.GeocodeAddress( address );
                hasLatLng = results != null && results.GetLocation().lat != 0 && results.GetLocation().lng != 0;
                return results;
            }
            catch
            {
                hasLatLng = false;
                return null;
            }
        }
        public string DisplayAddress( DBEntity dbaddress, string separator = ", " )
        {
            string address = "";
            if ( !string.IsNullOrWhiteSpace( dbaddress.Address1 ) )
                address = dbaddress.Address1;
            if ( !string.IsNullOrWhiteSpace( dbaddress.Address2 ) )
                address += separator + dbaddress.Address2;
            if ( !string.IsNullOrWhiteSpace( dbaddress.City ) )
                address += separator + dbaddress.City;
            if ( !string.IsNullOrWhiteSpace( dbaddress.Region ) )
                address += separator + dbaddress.Region;
            if ( !string.IsNullOrWhiteSpace( dbaddress.PostalCode ) )
                address += " " + dbaddress.PostalCode;
            if ( !string.IsNullOrWhiteSpace( dbaddress.Country ) )
                address += separator + dbaddress.Country;

            address += separator + string.Format( "Lat: {0}, lng: {1}", dbaddress.Latitude, dbaddress.Longitude );
            return address;
        }
        public static bool HasAddressChanged( CM.Address from, EM.Location to )
        {
            bool hasChanged = false;

            if ( to.Address1 != from.Address1
            || to.Address2 != from.Address2
            || to.City != from.City
            || to.PostalCode != from.PostalCode
            || to.Region != from.AddressRegion
            || to.CountryId != from.CountryId )
                hasChanged = true;

            return hasChanged;
        }


        public static List<string> Autocomplete( string keyword, int typeId, int maxTerms = 25 )
        {
            int pTotalRows = 0;
            List<string> results = new List<string>();
            string address1 = "";
            string city = "";
            string postalCode = "";
            if ( typeId == 3 )
                postalCode = keyword;
            else if ( typeId == 2 )
                city = keyword;
            else
                address1 = keyword;
            string result = "";

            List<ThisEntity> list = QuickSearch( address1, city, postalCode, 1, maxTerms, ref pTotalRows );

            string prevName = "";
            string suffix = "";
            foreach ( ThisEntity item in list )
            {
                result = "";
                suffix = "";

                if ( typeId == 3 )
                {
                    result = item.PostalCode;
                    suffix = " [[" + item.City + "]] ";
                }
                else if ( typeId == 2 )
                {
                    result = item.City;
                }
                else
                {
                    result = item.Address1;
                    suffix = " [[" + item.City + "]] ";
                }

                if ( result.ToLower() != prevName )
                    results.Add( result + suffix );

                prevName = result.ToLower();
            }

            return results;
        }
        public static List<ThisEntity> QuickSearch( string keyword, int pageNumber, int pageSize, ref int pTotalRows )
        {
            List<ThisEntity> list = new List<ThisEntity>();
            ThisEntity entity = new ThisEntity();
            keyword = CleanTerm( keyword );

            int skip = ( pageNumber - 1 ) * pageSize;
            using ( var context = new Data.CTIEntities() )
            {
                //turn off - so Entity et will not be included!!!!
                context.Configuration.LazyLoadingEnabled = false;


                var addresses = context.Location
                    .Where( s => keyword == ""
                                || s.Organization.Name.Contains( keyword )
                                || s.Name.Contains( keyword )
                                || s.ExternalIdentifier.Contains( keyword )
                                || s.Address1.Contains( keyword )
                                || s.City.Contains( keyword )
                                || s.PostalCode.Contains( keyword )
                                )
                    .GroupBy( a => new
                    {
                        Name = a.Name,
                        Address1 = a.Address1,
                        Address2 = a.Address2 ?? "",
                        City = a.City,
                        PostalCode = a.PostalCode,
                        AddressRegion = a.Region,
                        CountryId = a.CountryId,
                        Country = a.Country ?? ""
                    } )
                    .Select( g => new ThisEntity
                    {
                        Name = g.Key.Name,
                        Address1 = g.Key.Address1,
                        Address2 = g.Key.Address2 ?? "",
                        City = g.Key.City,
                        PostalCode = g.Key.PostalCode,
                        AddressRegion = g.Key.AddressRegion,
                        CountryId = g.Key.CountryId ?? 0,
                        Country = g.Key.Country
                    } )
                    .OrderByDescending( a => a.Address1 )
                    .ThenByDescending( a => a.City );
                //.ToList();


                pTotalRows = addresses.Count();
                List<ThisEntity> results = addresses
                    .OrderBy( s => s.Address1 )
                    .Skip( skip )
                    .Take( pageSize )
                    .ToList();
                if ( results != null && results.Count > 0 )
                {
                    //??enough
                    list = results;
                    //return list;
                }
            }

            return list;
        }
        public static List<ThisEntity> QuickSearch( string address1, string city, string postalCode, int pageNumber, int pageSize, ref int pTotalRows )
        {
            List<ThisEntity> list = new List<ThisEntity>();
            ThisEntity entity = new ThisEntity();
            address1 = CleanTerm( address1 );
            city = CleanTerm( city );
            postalCode = CleanTerm( postalCode );

            int skip = ( pageNumber - 1 ) * pageSize;
            using ( var context = new Data.CTIEntities() )
            {

                List<DBEntity> results = context.Location
                    .Where( s =>
                           ( address1 == "" || s.Address1.Contains( address1 ) )
                        && ( city == "" || s.City.Contains( city ) )
                        && ( postalCode == "" || s.PostalCode.Contains( postalCode ) )
                        )
                    .OrderBy( s => s.Address1 )
                    .Skip( skip )
                    .Take( pageSize )
                    .ToList();

                if ( results != null && results.Count > 0 )
                {
                    foreach ( DBEntity item in results )
                    {
                        entity = new ThisEntity();
                        MapFromDB( item, entity );

                        list.Add( entity );
                    }

                    //Other parts
                }
            }

            return list;
        }
        private static string CleanTerm( string item )
        {
            string term = item == null ? "" : item.Trim();
            if ( term.IndexOf( "[[" ) > 1 )
            {
                term = term.Substring( 0, term.IndexOf( "[[" ) );
                term = term.Trim();
            }
            return term;
        }
        #endregion
    }
}
