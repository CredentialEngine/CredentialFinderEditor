using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Import
{
    public class QAPerformedDTO : BaseDTO
    {
        public bool DeleteProfile { get; set; }
        public Guid Identifier { get; set; }

        //maybe retain for display
        public string ArtifactType { get; set; }
        public int EntityId { get; set; }
        public int CredentialTypeId { get; set; }
        public List<string> Assertions { get; set; } = new List<string>();

    }
}
