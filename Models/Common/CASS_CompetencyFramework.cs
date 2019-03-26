using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Common
{
    public class CASS_CompetencyFramework : BaseObject
    {
        //public int Id { get; set; }
        //public System.Guid RowId { get; set; }
        public int OrgId { get; set; }
        public Organization OwningOrganization { get; set; } = new Organization();
        public string FrameworkName { get; set; }
        public string CTID { get; set; }
        public string CredentialRegistryId { get; set; }

        public bool IsApproved { get; set; }

        /// <summary>
        /// if never approved, or updated since last approval, then needs approval
        /// </summary>
        public bool NeedsApproval
        {
            get
            {
                if ( !IsApproved )
                    return true;

                if ( LastUpdated > LastApproved )
                    return true;
                else
                    return false;
            }
        }

        //public System.DateTime LastPublished { get; set; }
        public int LastPublishedById { get; set; }
        public bool IsPublished { get; set; }

        /// <summary>
        /// If never published, or has been updated since last published, then needs to be published. 
        /// </summary>
        //public bool NeedsPublishing
        //{
        //    get
        //    {
        //        if ( !IsPublished )
        //            return true;

        //        if ( LastUpdated > LastPublished )
        //            return true;
        //        else
        //            return false;
        //    }
        //}
        public string Payload { get; set; }
    }
}
