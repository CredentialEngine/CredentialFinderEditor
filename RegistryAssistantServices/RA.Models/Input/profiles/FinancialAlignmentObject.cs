using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Models.Input
{
    /// <summary>
    /// Common input class for all verification profiles
    /// </summary>
    public class FinancialAlignmentObject   
    {
        public FinancialAlignmentObject()
        {
           
        }        
        public string AlignmentType { get; set; }
        public string CodedNotation { get; set; }
        public string TargetNode { get; set; }
        public string TargetNodeDescription { get; set; }
        public string TargetNodeName { get; set; }
        public string Framework  { get; set; }
        public string FrameworkName { get; set; }
        public decimal Weight { get; set; }
        public string AlignmentDate { get; set; }
    }
}



