using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Models.Input
{
    /// <summary>
    /// Class for handling references to an organization
    /// Either the Id as an resolvable URL, a CTID (that will be use to format the Id as a URI) or provide all of the properities:
    /// - Type
    /// - Name
    /// - Description
    /// - Subject webpage
    /// - Social media
    /// </summary>
    public class OrganizationReference : EntityReference
    {
        public static string CredentialOrganization = "CredentialOrganization";
        public static string QACredentialOrganization = "QACredentialOrganization";
        /// <summary>
        /// The type of organization is one of :
        /// - CredentialOrganization
        /// - QACredentialOrganization
        /// Required
        /// </summary>
        public override string Type { get; set; }

        /// <summary>
        /// Social Media URL links
        /// For example, Facebook, LinkedIn
        /// </summary>
        public List<string> SocialMedia { get; set; } //URL

        public new bool HasNecessaryProperties()
        {
            //skip social media for now
            //	|| ( SocialMedia == null || SocialMedia.Count == 0 )
            //				|| string.IsNullOrWhiteSpace( Description )
            if ( string.IsNullOrWhiteSpace( Type )
                || string.IsNullOrWhiteSpace( Name )
                || string.IsNullOrWhiteSpace( SubjectWebpage )
                )
                return false;
            else
                return true;
        }

        /// <summary>
        /// Purpose is to determine if class has data
        /// </summary>
        /// <returns></returns>
        public override bool IsEmpty()
        {
            if ( string.IsNullOrWhiteSpace( Id )
                && string.IsNullOrWhiteSpace( Name )
                && string.IsNullOrWhiteSpace( Description )
                && string.IsNullOrWhiteSpace( SubjectWebpage )
                && ( SocialMedia == null || SocialMedia.Count == 0 )
                )
                return true;
            else
                return false;
        }
    }

    /// <summary>
    /// Class for handling references to an entity such as an Assessment, Organization, Learning opportunity, or credential
    /// Either the Id as an resolvable URL, or provide all of the properities
    /// </summary>
    public class EntityReference
    {
        /// <summary>
        /// Id is a resovable URI
        /// If the entity exists in the registry, provide the URI. 
        /// If not sure of the exact URI, especially if just publishing the entity, then provide the CTID and the API will format the URI.
        /// Alterate URIs are under consideration. For example
        /// http://dbpedia.com/Stanford_University
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Optionally, a CTID can be entered instead of an Id. 
        /// Only enter Id or CTID, but not both
        /// </summary>
        public string CTID { get; set; }

        //if there is no available Id/CTID, enter the following, where Type, Name, Description, and subjectwebpage would be required

        /// <summary>
        /// the type of the entity must be provided if the Id was not provided. examples
        /// ceterms:AssessmentProfile
        /// ceterms:LearningOpportunityProfile
        /// ceterms:ConditionManifest
        /// ceterms:CostManifest
        /// or the many credential subclasses!!
        /// </summary>
        public virtual string Type { get; set; }

        /// <summary>
        /// Name of the entity (required)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Subject webpage of the entity
        /// </summary> (required)
        public string SubjectWebpage { get; set; }

        /// <summary>
        /// Description of the entity (optional)
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Check if all properties for a reference request are present
        /// 17-08-27 We do need a type if only providing reference data
        /// </summary>
        /// <returns></returns>
        public bool HasNecessaryProperties()
        {
            //	|| string.IsNullOrWhiteSpace( Description )
            if ( string.IsNullOrWhiteSpace( Name )
                || string.IsNullOrWhiteSpace( Type )
                || string.IsNullOrWhiteSpace( SubjectWebpage )
                )
                return false;
            else
                return true;
        }
        public virtual bool IsEmpty()
        {
            if ( string.IsNullOrWhiteSpace( Id )
                && string.IsNullOrWhiteSpace( CTID )
                && string.IsNullOrWhiteSpace( Name )
                && string.IsNullOrWhiteSpace( Description )
                && string.IsNullOrWhiteSpace( SubjectWebpage )
                )
                return true;
            else
                return false;
        }
    }
}
