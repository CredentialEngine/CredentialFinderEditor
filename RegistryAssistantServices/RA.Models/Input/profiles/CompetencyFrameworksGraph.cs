using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using System.Collections;

namespace RA.Models.Input
{
    public class CompetencyFrameworksGraph
    {

        [JsonProperty( "@context" )]
        public string Context { get; set; }

        /// <summary>
        /// Main graph object
        /// </summary>
        [JsonProperty( "@graph" )]
        public object Graph { get; set;  }
        //public object Graph { get; set;  }

        //[JsonProperty( "ceterms:ctid" )]
        //public string CTID { get; set; }



    }
}
