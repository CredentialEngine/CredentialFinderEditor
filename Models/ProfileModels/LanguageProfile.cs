using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ProfileModels
{
	public class LanguageProfile : BaseProfile
	{
        public int LanguageCodeId { get; set; }
        public string LanguageName { get; set; }
        public string LanguageCode { get; set; }
    }

}
