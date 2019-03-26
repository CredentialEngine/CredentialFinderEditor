using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Models;
using Models.Common;
using Models.ProfileModels;
using EM = Data;
using Utilities;
using DBEntity = Data.Import_Microsoft;

namespace Factories
{
	public class IvyTechImport : BaseFactory
	{
		CredentialManager cmgr = new CredentialManager();
		AssessmentManager amgr = new AssessmentManager();
		Entity_AssessmentManager eaMgr = new Entity_AssessmentManager();
		Entity_ConditionProfileManager cpMgr = new Entity_ConditionProfileManager();
		int defaultUserId = 10;
		int english = 40;
		int managingOrgId = 1128;
		CodeItem credStatus = new CodeItem();
		CodeItem credTypeCode = new CodeItem();
	}
}
