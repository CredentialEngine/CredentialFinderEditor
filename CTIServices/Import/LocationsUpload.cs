using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using LumenWorks.Framework.IO.Csv;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using Factories;
using ImportMgr = Factories.Import.CredentialImport;
using Models;
using Models.Common;
using Models.Import;
using Models.ProfileModels;
using Utilities;

namespace CTIServices.Import
{
    public class LocationsUpload
    {
        public LocationImportHelper importHelper = new LocationImportHelper();
        static int RequiredNbrOfColumns = 5; //??

		public bool UploadLocationsFromText( string inputText, string action, AppUser user, ref List<string> messages )
		{
			using ( CsvReader csv = new CsvReader( new StringReader( inputText ), true ) )
			{
				return UploadLocations( csv, user, ref messages );
			}
		}
		//

		public bool UploadLocationsFromFile( string file, AppUser user, ref List<string> messages )
		{
			using ( CsvReader csv = new CsvReader( new StreamReader( file ), true ) )
			{
				return UploadLocations( csv, user, ref messages );
			}
		}
		//

		public bool UploadLocations( CsvReader csv, AppUser user, ref List<string> messages )
		{
			bool isOK = true;
			var list = new List<ImportUtilities.UploadAttempt<Address>>();

			using ( csv )
			{
				int fieldCount = csv.FieldCount;

				var headers = csv.GetFieldHeaders();
				var headerList = headers.ToList();
				//validate headers
				if ( !ValidateHeaders( headers, ref messages ) )
				{
					return false;
				}
				while ( csv.ReadNextRecord() )
				{
					var item = new ImportUtilities.UploadAttempt<Address>();

					//use header columns rather than hard-code index numbers to enable flexibility
					for ( int i = 0; i < fieldCount; i++ )
					{
						Debug.Write( string.Format( "{0} = {1};",
										headers[ i ], csv[ i ] ) );
						Debug.WriteLine( "" );

						//may want to make case insensitive!
						switch ( headers[ i ] )
						{
							case "ExternalIdentifier":
							case "External Identifier":
								//use this or CTID to get the related credential
								item.UploadItem.ExternalIdentifier = csv[ i ];
								break;
							case "CTID":
							case "Organization CTID":
							case "Existing Organization CTID":
								//ctid will not be required if external identifier is present
								item.OwningOrganization = new Organization() { CTID = csv[ i ] };
								break;
							case "LocationIdentifier":
							case "Identifier": //Not sure if this should be a location identifier or an external identifier
								//unique identifier for the location
								item.UploadItem.ExternalIdentifier = csv[ i ];
								break;
							case "Name":
							case "Address Name":
								item.UploadItem.Name = csv[ i ];
								break;
							case "Address1":
							case "Address Line 1":
								item.UploadItem.Address1 = csv[ i ];
								break;
							case "Address2":
							case "Address Line 2":
								item.UploadItem.Address2 = csv[ i ];
								break;
							case "City":
								item.UploadItem.City = csv[ i ];
								break;
							case "Region":
							case "State, Province, or Region":
								item.UploadItem.AddressRegion = csv[ i ];
								break;
							case "PostalCode":
							case "Postal Code":
								item.UploadItem.PostalCode = csv[ i ];
								break;
							case "PostOfficeBoxNumber":
							case "Post Office Box Number":
								item.UploadItem.PostOfficeBoxNumber = csv[ i ];
								break;
							case "Country":
								item.UploadItem.Country = csv[ i ];
								break;
							default:
								//action?
								break;
						}
					}
					//probably will save immediately unless taking an all or none approach.
					if ( ValidateAddress( item.UploadItem, ref messages ) )
					{
						list.Add( item );
					}
				}

				var loadedOrganizations = new List<Organization>();
				var profileServices = new ProfileServices();
				foreach( var item in list )
				{
					//Get org data if it was already loaded or get it from the database
					var org = loadedOrganizations.FirstOrDefault( m => m.CTID == item.OwningOrganization.CTID );
					var statusText = "";
					if( org == null )
					{
						org = OrganizationServices.GetByCtid( item.OwningOrganization.CTID, false, true );
						loadedOrganizations.Add( org );
					}
					
					//Ensure user can edit this org
					if( !OrganizationServices.CanUserUpdateOrganization(user, org.RowId ) )
					{
						item.WasSuccessful = false;
						item.Message = "You don't have permission to update the organization: " + org.Name + " (CTID: " + org.CTID + ")";
						messages.Add( item.Message );
						continue;
					}

					//TODO: figure out some kind of duplicate checking that is better than this
					if( org.Addresses.FirstOrDefault( m => m.DisplayAddress().ToLower() == item.UploadItem.DisplayAddress().ToLower() ) == null && 
						org.Addresses.FirstOrDefault( m => m.ExternalIdentifier != null && m.ExternalIdentifier == item.UploadItem.ExternalIdentifier ) == null )
					{
						item.WasSuccessful = profileServices.Address_Import( item.UploadItem, org.RowId, org.Id, "Add", "Organization", user, ref statusText );
						item.Message = statusText;
						messages.Add( item.WasSuccessful ? "Address: " + item.UploadItem.DisplayAddress() + " was created successfully" : "Error creating address: " + item.UploadItem.DisplayAddress() + " - " + statusText );
					}
					else
					{
						item.Message = "Address already exists.";
						messages.Add( "Note: Address: " + item.UploadItem.DisplayAddress() + " already exists. No action was taken for this address." );
					}
				}

				return isOK;
			}
		}
		//

		#region Validations
		public bool ValidateHeaders( string[] headers, ref List<string> messages )
        {
            bool isValid = true;
            if (headers == null || headers.Count() < RequiredNbrOfColumns)
            {
                messages.Add( "Error - the input file must have a header row with at least the required columns" );
                return false;
            }
            int cntr = -1;
            try
            {
                foreach (var item in headers)
                {
                    cntr++;
                    string colname = item.ToLower().Replace( " ", "" ).Replace( "*", "" );
                    switch (colname)
                    {
                        case "externalidentifier":
                        case "uniqueidentifier":
                        case "identifier":
                            importHelper.IdentifierHdr = cntr;
                            break;
                        case "existingorganizationctid":
                        case "orgctid":
                            importHelper.OrganizationCtidHdr = cntr;
                            break;
                        case "name":
                        case "addressname":
                            importHelper.AddressNameHdr = cntr;
                            break;

                        case "address1":
						case "addressline1":
                            //todo verification
                            importHelper.Address1Hdr = cntr;
                            break;
                        case "address2":
						case "addressline2":
                            importHelper.Address2Hdr = cntr;
                            break;

                        case "city":
                            importHelper.CityHdr = cntr;
                            break;
                        case "region":
                        case "state":
						case "state,province,orregion":
                            importHelper.RegionHdr = cntr;
                            break;

                        case "postalcode":
                            importHelper.PostalcodeHdr = cntr;
                            break;
                        case "postofficeboxnumber":
                            importHelper.POBoxHdr = cntr;
                            break;
                        case "country":
                            importHelper.CountryHdr = cntr;
                            break;
                        
                        default:
                            //action?
                            if (colname.IndexOf( "column" ) > -1)
                                break;
                            messages.Add( "Error unknown column header encountered: " + item );
                            break;
                    }
                }

                if (importHelper.IdentifierHdr == -1)
                    messages.Add( "Error - An identifier code for the address, from the source system must be provided to uniquely identify an input record." );
                if (importHelper.OrganizationCtidHdr == -1)
                    messages.Add( "Error - An owning organization  CTID column must be provided" );

                if (importHelper.AddressNameHdr == -1)
                    messages.Add( "Error - A credential name column must be provided" );
                if (importHelper.Address1Hdr == -1)
                    messages.Add( "Error - A credential description column must be provided" );
                if (importHelper.CityHdr == -1)
                    messages.Add( "Error - A city column must be provided" );
                if (importHelper.PostalcodeHdr == -1)
                    messages.Add( "Error - A postal code must be provided" );

            }
            catch (Exception ex)
            {
                string msg = BaseFactory.FormatExceptions( ex );
                LoggingHelper.DoTrace( 1, "Exception encountered will validating headers for locations upload: " + msg );
                messages.Add( "Exception encountered while validating headers for locations upload: " + msg );
            }
            if (messages.Count > 0)
                isValid = false;

            return isValid;
        }//
        public bool ValidateAddress( Address entity, ref List<string> messages )
        {
            bool valid = true;

			RequireText( entity.Name, "Name is required.", ref valid, ref messages );
			RequireText( entity.Address1, "Address Line 1 is required.", ref valid, ref messages );
			RequireText( entity.City, "City is required.", ref valid, ref messages );
			RequireText( entity.AddressRegion, "State, Province, or Region is required.", ref valid, ref messages );
			RequireText( entity.PostalCode, "Postal Code is required.", ref valid, ref messages );

			return valid;
        }

		//Could probably expand this to do a minimum/maximum length check as well
		public void RequireText( string textToCheck, string errorMessage, ref bool valid, ref List<string> messages )
		{
			if ( string.IsNullOrWhiteSpace( textToCheck ) )
			{
				messages.Add( errorMessage );
				valid = false;
			}
		}
        #endregion
    }


}
