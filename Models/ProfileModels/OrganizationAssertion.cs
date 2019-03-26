using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Web.Script.Serialization;

using Models.Common;

namespace Models.ProfileModels
{
    public class OrganizationAssertion : BaseProfile
    {

        public OrganizationAssertion()
        {
            TargetOrganization = new Organization();
            Relationship = "";
            ProfileSummary = "";
            Description = "";
            AgentAssertion = new Enumeration();
            ActedUponEntity = new Entity();
        }

        public int OrganizationId { get; set; }
        public Guid AgentUid { get; set; }
        //always the parent, so probably don't need.
        public Organization TargetOrganization { get; set; }

        public int TargetEntityTypeId { get; set; }
        public int AssertionTypeId { get; set; }

        public string Relationship { get; set; }
        public Enumeration AgentAssertion { get; set; }
        public Guid TargetUid { get; set; }

        //public bool IsQAActionRole { get; set; }
        public Entity ActedUponEntity { get; set; }
        public TargetEntity Recipient { get; set; } = new TargetEntity();

        public string TargetEntityType { get; set; }
        public int TargetEntityBaseId { get; set; }
        public string TargetEntityName { get; set; }
        public string TargetEntityDescription { get; set; }
        public string TargetCTID { get; set; }
        public string TargetEntitySubjectWebpage { get; set; }
        public string TargetEntityImageUrl { get; set; }
        public string CtdlType { get; set; }
    }
    public class TargetEntity
    {
        public TargetEntity()
        {
            Type = this.GetType();
            RowId = new Guid(); //All zeroes
            OwningAgentUid = new Guid(); //All zeroes
        }

        public int Id { get; set; }
        public Guid RowId { get; set; }
        public string Name { get; set; }
        public string Property { get; set; }
        public string TypeName
        {
            get
            {
                if ( Type != null )
                    return Type.Name;
                else
                    return "";
            }
            set { this.Type = Type.GetType( "Models.Node." + value ); }
        }

        public Guid ParentEntityRowId { get; set; }
        public int ParentEntityTypeId { get; set; }

        public Guid OwningAgentUid { get; set; }

        public bool IsReferenceEntity { get; set; }

        [JsonIgnore]
        [ScriptIgnore]
        public Type Type { get; set; }
    }
}
