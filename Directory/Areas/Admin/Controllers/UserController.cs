using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using CTIDirectory.Models;
using CTIServices;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Models;
using Newtonsoft.Json;
using Utilities;

namespace CTI.Directory.Areas.Admin.Controllers
{
    [Authorize]
    public class UserController : CTI.Directory.Controllers.BaseController
    {
        private ApplicationUserManager _userManager;
        public UserController()
        {
            ViewBag.Theme = "light";
        }

        string statusMessage = "";
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // GET: Admin/Account
        public ActionResult Index()
        {
            //perhaps a user related menu
            return View();
        }
        public ActionResult Search(string sidx, string sord, int page, int rows
            , bool _search
            , string firstName, string lastName, string email, string id
            , string roles
            , string filters)
        {

            int pTotalRows = 0;
            string where = "";
            int pageSize = 5000;
            //if ( ( filters ?? "" ).Length > 20 )
            //{
            //	GridFilter data = JsonConvert.DeserializeObject<GridFilter>( filters, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore } );

            //	foreach ( Rule r in data.rules )
            //	{
            //		SetRuleFilter( r.field, r.op, r.data, data.groupOp, ref where );
            //	}
            //	if ( data.groupOp == "OR" )
            //	{
            //		where = " (" + where + ") ";
            //	}
            //}

            if (_search)
            {
                //need an operator
                SetKeywordFilter("firstName", firstName, ref where);
                SetKeywordFilter("lastName", lastName, ref where);
                SetKeywordFilter("email", email, ref where);
                SetKeywordFilter("roles", roles, ref where);
            }

            List<AppUser> list = AccountServices.Search(where, sidx, sord, page, pageSize, ref pTotalRows);

            //if ( list != null )
            //{
            //	model.TotalCount = list.Count();
            //	model.Accounts = list;
            //}

            int pageIndex = Convert.ToInt32(page) - 1;

            int totalPages = (int)Math.Ceiling((float)pTotalRows / (float)rows);

            //var data = products.OrderBy( x => x.Id )
            //			 .Skip( pageSize * ( page - 1 ) )
            //			 .Take( pageSize ).ToList();

            var jsonData = new
            {
                total = totalPages,
                page = page,
                records = pTotalRows,
                rows = list
            };

            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }
        public ActionResult WebGridSearch(string sort = "", string sortdir = "ASC")
        {
            AccountSearchModel model = new AccountSearchModel();
            model.PageSize = 25;
            int pTotalRows = 0;
            List<AppUser> list = AccountServices.SearchByKeyword("", sort, sortdir, 1, model.PageSize, ref pTotalRows);

            if (list != null)
            {
                model.TotalCount = pTotalRows;
                model.Accounts = list;
            }

            return View(model);

        }


        [HttpGet]
        public ActionResult List()
        {
            return View();
        }
        public ActionResult GetRecords(string sidx, string sord, int page, int rows
            , bool _search
            , string firstName, string lastName, string email, string id
            , string rolesList
            , string filters)
        {
            //RolesList
            //AccountSearchModel model = new AccountSearchModel();
            //model.PageSize = 25;
            int pTotalRows = 0;
            string where = "";
            if ((filters ?? "").Length > 20)
            {
                GridFilter data = JsonConvert.DeserializeObject<GridFilter>(filters, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                foreach (Rule r in data.rules)
                {
                    SetRuleFilter(r.field, r.op, r.data, data.groupOp, ref where);
                }
                if (data.groupOp == "OR")
                {
                    where = " (" + where + ") ";
                }
            }

            if (_search)
            {
                //need an operator
                SetKeywordFilter("firstName", firstName, ref where);
                SetKeywordFilter("lastName", lastName, ref where);
                SetKeywordFilter("email", email, ref where);
                SetKeywordFilter("roles", rolesList, ref where);
            }

            List<AppUser> list = AccountServices.Search(where, sidx, sord, page, rows, ref pTotalRows);

            //if ( list != null )
            //{
            //	model.TotalCount = list.Count();
            //	model.Accounts = list;
            //}

            int pageIndex = Convert.ToInt32(page) - 1;
            int pageSize = rows;
            int totalPages = (int)Math.Ceiling((float)pTotalRows / (float)pageSize);

            //var data = products.OrderBy( x => x.Id )
            //			 .Skip( pageSize * ( page - 1 ) )
            //			 .Take( pageSize ).ToList();

            var jsonData = new
            {
                total = totalPages,
                page = page,
                records = pTotalRows,
                rows = list
            };

            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadRecords()
        {
            var result = new JsonResult();

            try
            {
                #region According to Datatables.net, Server side parameters

                //global search
                var search = Request.Form["search[value]"];
                var draw = Request.Form["draw"];

                var orderBy = string.Empty;
                //column index
                var order = int.Parse(Request.Form["order[0][column]"]);
                //sort direction
                var orderDir = Request.Form["order[0][dir]"];

                int startRec = int.Parse(Request.Form["start"]);
                int pageSize = int.Parse(Request.Form["length"]);

                #endregion

                #region Where filter

                //individual column wise search
                var columnSearch = new List<string>();
                var globalSearch = new List<string>();
				DateTime dt = new DateTime();

				//Get all keys starting with columns    
				foreach (var index in Request.Form.AllKeys.Where(x => Regex.Match(x, @"columns\[(\d+)]").Success).Select(x => int.Parse(Regex.Match(x, @"\d+").Value)).Distinct().ToList())
                {
                    //get individual columns search value
                    var value = Request.Form[string.Format("columns[{0}][search][value]", index)];
					if ( !string.IsNullOrWhiteSpace( value ) )
					{
                        value = value.Trim();
                        string colName = Request.Form[ string.Format( "columns[{0}][data]", index ) ];
						if ( colName == "lastLogon" )
						{
							if ( DateTime.TryParse( value, out dt ) )
							{
								columnSearch.Add( string.Format( " (convert(varchar(10),{0},120) = '{1}') ", colName, dt.ToString("yyyy-MM-dd") ) );
							}
						} else 
							columnSearch.Add( string.Format( "({0} LIKE '%{1}%')", Request.Form[ string.Format( "columns[{0}][data]", index ) ], value ) );
					}
                    //get column filter for global search
                    if (!string.IsNullOrWhiteSpace( search))
                        globalSearch.Add(string.Format("({0} LIKE '%{1}%')", Request.Form[string.Format("columns[{0}][data]", index)], search));

                    //get order by from order index
                    if (order == index)
                        orderBy = Request.Form[string.Format("columns[{0}][data]", index)];
                }

                var where = string.Empty;
                //concat all filters for global search
                if (globalSearch.Any())
                    where = globalSearch.Aggregate((current, next) => current + " OR " + next);

                if (columnSearch.Any())
                    if (!string.IsNullOrEmpty(where))
                        where = string.Format("({0}) AND ({1})", where, columnSearch.Aggregate((current, next) => current + " AND " + next));
                    else
                        where = columnSearch.Aggregate((current, next) => current + " AND " + next);

                #endregion

                var totalRecords = 0;
                var list = AccountServices.Search(where, orderBy, orderDir, startRec / pageSize, pageSize, ref totalRecords);

                result = Json(new { data = list, draw = int.Parse(draw), recordsTotal = totalRecords, recordsFiltered = totalRecords }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
            }

            return result;
        }

        private static void SetRuleFilter(string column, string filterType, string value, string groupOp, ref string where)
        {
            //may need a type, to properly format equals, etc
            if (string.IsNullOrWhiteSpace(value))
                return;

            string GROUP_OP = "";
            if (where.Length > 0)
                GROUP_OP = string.Format(" {0} ", groupOp);
            value = ServiceHelper.HandleApostrophes(value);
            if (column == "id")
            {
                if (filterType == "eq")
                {
                    where = where + GROUP_OP + string.Format(" ( {0} = {1}  ) ", column, value);
                }
                else if (filterType == "le")
                {
                    where = where + GROUP_OP + string.Format(" ( {0} <= {1}  ) ", column, value);
                }
                else if (filterType == "ge")
                {
                    where = where + GROUP_OP + string.Format(" ( {0} >= {1}  ) ", column, value);
                }
            }
            //else if ( column == "Created" )
            //{
            //	if ( value.ToLower() == "today" )
            //		value = DateTime.Now.ToString( "yyyy-MM-dd" );
            //	if ( filterType == "bw" )
            //		where = where + GROUP_OP + string.Format( " ( convert(varchar(10),CreatedDate, 120) = '{0}'  ) ", value );
            //	else if ( filterType == "eq" )
            //	{
            //		where = where + GROUP_OP + string.Format( " ( convert(varchar(10),CreatedDate, 120) = '{0}'  ) ", value );
            //	}
            //	else
            //		where = where + GROUP_OP + string.Format( " ( CreatedDate > '{0}'  ) ", value );
            //}
            else
            {
                //for strings
                if (filterType == "eq")
                {
                    where = where + GROUP_OP + string.Format(" ( {0} = '{1}'  ) ", column, value);
                }
                else if (filterType == "bw") //begins with
                {
                    if (value.IndexOf("%") == -1)
                        value = value.Trim() + "%";
                    where = where + GROUP_OP + string.Format(" ( {0} like '{1}' ) ", column, value);
                }
                else if (filterType == "bn")  //does not begin with
                {
                    if (value.IndexOf("%") == -1)
                        value = value.Trim() + "%";
                    where = where + GROUP_OP + string.Format(" ( {0} NOT like '{1}' ) ", column, value);
                }
                else if (filterType == "cn")  //contains
                {
                    if (value.IndexOf("%") == -1)
                        value = "%" + value.Trim() + "%";
                    where = where + GROUP_OP + string.Format(" ( {0} like '{1}' ) ", column, value);
                }
                else if (filterType == "nc")  //does not contain
                {
                    if (value.IndexOf("%") == -1)
                        value = "%" + value.Trim() + "%";
                    where = where + GROUP_OP + string.Format(" ( {0} NOT like '{1}' ) ", column, value);
                }
                else if (filterType == "ew") //ends with
                {
                    if (value.IndexOf("%") == -1)
                        value = "%" + value.Trim();
                    where = where + GROUP_OP + string.Format(" ( {0} like '{1}' ) ", column, value);
                }
                else if (filterType == "en")  //does not ends with
                {
                    if (value.IndexOf("%") == -1)
                        value = "%" + value.Trim();
                    where = where + GROUP_OP + string.Format(" ( {0} NOT like '{1}' ) ", column, value);
                }
            }


        } //
        private static void SetKeywordFilter(string column, string value, ref string where)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;
            string text = " ({0} like '{1}' ) ";

            string AND = "";
            if (where.Length > 0)
                AND = " AND ";
            //
            value = ServiceHelper.HandleApostrophes(value);
            if (value.IndexOf("%") == -1)
                value = "%" + value.Trim() + "%";

            where = where + AND + string.Format(" ( " + text + " ) ", column, value);

        }

        public ActionResult GetAccountById(int id)
        {
            var account = AccountServices.GetUser(id);

            if (account != null)
            {
                AppUser model = AccountServices.GetUser(id);

                //foreach ( var item in accounts )
                //{
                //	model.Name = item.Name;
                //	model.Price = item.Price;
                //	model.Department = item.Department;
                //}

                return PartialView("_GridEditPartial", account);
            }

            return View();
        }

        public ActionResult EditAccount(int id)
        {
            var account = AccountServices.GetAccount(id);
            if (account != null)
            {
                var model = new AccountViewModel { UserId = account.Id, Email = account.Email, FirstName = account.FirstName, LastName = account.LastName };
                var roles = AccountServices.GetRoles();
                model.SelectedRoles = roles.Where(x => account.UserRoles.Contains(x.Name)).Select(x => x.Id).ToArray();
                model.Roles = roles.Select(x => new SelectListItem { Text = x.Name, Value = x.Id, Selected = account.UserRoles.Contains(x.Name) }).ToList();

                model.SelectedOrgs = account.Organizations.Select(x => x.Id).ToArray();

                //If Organizations not found, get top 3 Organizations to show
                //if (!account.Organizations.Any())
                //    account.Organizations = AccountServices.GetOrganizations(string.Empty).OrderBy(x => x.Name).Take(3).Select(x => new CodeItem { Id = x.Id, Name = x.Name }).ToList();

                model.Organizations = account.Organizations.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString(), Selected = true }).ToList();
                return PartialView(model);
            }

            return HttpNotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditAccount(AccountViewModel model)
        {
            var account = AccountServices.GetAccount(model.UserId);
            if (account != null)
            {
                if (ModelState.IsValid)
                {
                    account.FirstName = model.FirstName;
                    account.LastName = model.LastName;
                    account.Email = model.Email;

                    //Update Account and AspNetUser
                    var message = string.Empty;
                    var success = new AccountServices().Update(account, false, ref message);
                    if (success)
                    {
                        //if null, should there be a check to delete all??
                        //if (model.SelectedRoles != null)
                        //Bulk Add/Remove Roles
                        new AccountServices().UpdateRoles(account.AspNetUserId, model.SelectedRoles);

                        //if null, should there be a check to delete all??
                        //Bulk Add/Remove Organizations
                        OrganizationServices.UpdateUserOrganizations(model.UserId, model.SelectedOrgs, User.Identity.Name);
                    }

                    Response.StatusCode = (int)HttpStatusCode.OK;
                    return RedirectToAction("EditAccount", new { id = model.UserId });
                }
                else
                {
                    Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    ModelState.AddModelError("", string.Join(Environment.NewLine, ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage)));
                }

                var roles = AccountServices.GetRoles();
                model.SelectedRoles = roles.Where(x => account.UserRoles.Contains(x.Name)).Select(x => x.Id).ToArray();
                model.Roles = roles.Select(x => new SelectListItem { Text = x.Name, Value = x.Id, Selected = account.UserRoles.Contains(x.Name) }).ToList();

                model.SelectedOrgs = account.Organizations.Select(x => x.Id).ToArray();
                model.Organizations = account.Organizations.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString(), Selected = true }).ToList();
            }
            return PartialView(model);
        }

        public void DeleteAccount(int id)
        {
            //Update Account IsActive
            var message = string.Empty;
            new AccountServices().Delete(id, ref message);
        }

        #region Reset password
        [Authorize(Roles = "Administrator, Site Staff")]
        public ActionResult ResetPassword()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

			//AppUser user = AccountServices.GetUserByEmail( model.Email );

			var user = await UserManager.FindByEmailAsync( model.Email);
            if (user == null )
            {
                ModelState.AddModelError("", "The requested user email was not found.");
                return View(model);
            }
            string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);

            //var result = await UserManager.ChangePasswordAsync( User.Identity.GetUserId(), model.OldPassword, model.NewPassword );

            var result = await UserManager.ResetPasswordAsync(user.Id, code, model.Password);
            if (result.Succeeded)
            {
                new AccountServices().SetUserEmailConfirmed(user.Id);

                //ConsoleMessageHelper.SetConsoleSuccessMessage( "Successfully reset password for " + user.FirstName, "", false );

                return RedirectToAction("ResetPasswordConfirmation", "User");
            }
            AddErrors(result);


            return View();

        }
        public ActionResult ResetPasswordConfirmation()
        {
            return View();

        }
        #endregion

        #region Activate user
        [Authorize(Roles = "Administrator, Site Staff")]
        public ActionResult ActivateUser()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ActivateUser(ResetPasswordViewModel model)
        {
            //will want a different model
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByEmailAsync( model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "The requested user email was not found.");
                return View(model);
            }
            string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);

            //var result = await UserManager.ChangePasswordAsync( User.Identity.GetUserId(), model.OldPassword, model.NewPassword );

            var result = await UserManager.ResetPasswordAsync(user.Id, code, model.Password);
            if (result.Succeeded)
            {
                new AccountServices().SetUserEmailConfirmed(user.Id);

                //ConsoleMessageHelper.SetConsoleSuccessMessage( "Successfully reset password for " + user.FirstName, "", false );

                return RedirectToAction("ResetPasswordConfirmation", "User");
            }
            AddErrors(result);


            return View();

        }

        public ActionResult ActivateUserConfirmation()
        {
            return View();

        }
        #endregion


        //
        // Import users
        [Authorize(Roles = "Administrator, Site Staff")]
        public async Task<ActionResult> ImportUsers(int maxRecords = 100)
        {
            if (!User.Identity.IsAuthenticated
                || (User.Identity.Name != "mparsons"
                && User.Identity.Name != "email@email.com"
                && User.Identity.Name != "nathan.argo@siuccwd.com")
                )
            {
                //

                SetSystemMessage("Unauthorized Action", "You are not authorized to import users.");

                return RedirectToAction("Index", "Message");
            }
            AccountServices mgr = new AccountServices();
            List<AppUser> users = AccountServices.ImportUsers_GetAll(maxRecords);
            List<string> messages = new List<string>();
            string password = "";
            int cntr = 0;
            int created = 0;
            foreach (AppUser newUser in users)
            {
                cntr++;
                string statusMessage = "";
                var user = new ApplicationUser
                {
                    UserName = newUser.Email.Trim(),
                    Email = newUser.Email.Trim(),
                    FirstName = newUser.FirstName.Trim(),
                    LastName = newUser.LastName.Trim()
                };
                Random rnd = new Random();
                int nbr = rnd.Next(1000, 9999);
				if ( !string.IsNullOrWhiteSpace( newUser.Password ) )
					password = newUser.Password;
				else 
					password = string.Format("ceAcct_{0}", nbr);

				//add check if user already exists
				AppUser u = AccountServices.GetUserByEmail( user.Email );
				if (u != null && u.Id > 0 )
				{

				}
                var result = await UserManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    created++;
                    int userId = new AccountServices().Import(user.Email,
                        user.FirstName, user.LastName,
                        user.Email, user.Id,
                        password,
                        false, User.Identity.Name,
                        ref statusMessage);

                    if (userId > 0)
                    {
                        //no don't chg userId:
                        //newUser.Id = userId;
                        //log
                        LoggingHelper.DoTrace(2, "Imported user: " + newUser.Email + ". initial password: " + password);

                        //add to org
                        if (newUser.PrimaryOrgId > 0)
                            new OrganizationServices().OrganizationMember_Save(newUser.PrimaryOrgId, userId, 1, 12, ref statusMessage);

                        if (newUser.DefaultRoleId > 0)
                            mgr.AddRole(userId, newUser.DefaultRoleId, 12, ref statusMessage);

                        //updated
                        mgr.ImportUsers_SetCompleted(newUser.Id, userId, password);
                    }
                    else
                    {
                        //log error
                        LoggingHelper.LogError(string.Format("Unable to create Account (aspUser was created). Email: {0}, Message", newUser.Email, statusMessage));

                        messages.Add(string.Format("Unable to create Account (aspUser was created). Email: {0}, Message", newUser.Email, statusMessage));
                    }
                }
                else
                {
                    //error??
                    //if is email already taken, may imply needing to add to a second org
                    string status = string.Join("<br/>", result.Errors.ToArray());
                    LoggingHelper.LogError(string.Format("Unable to create AspUser. Email: {0}, Message", user.Email, status));
                    messages.Add(string.Format("Unable to create AspUser. Email: {0}, Message: {1}", user.Email, status));
                }
            } //loop

            string summary = string.Format("Account Import. Read {0} accounts, successfully imported {1} accounts.", cntr, created) + string.Join("<br/>", messages.ToArray());

            SetSystemMessage("Account Import", summary);

            return RedirectToAction("Index", "Message", new { area = "" });
        }

        [Authorize(Roles = "Administrator, Site Staff")]
        public ActionResult AddUser()
        {
            //return View();
            return View();
        }

        [HttpPost]
        //[RequireHttps]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddUser(RegisterViewModel model)
        {
            int currentUserId = AccountServices.GetCurrentUserId();
            if (ModelState.IsValid)
            {
                string statusMessage = "";
                var user = new ApplicationUser
                {
                    UserName = model.Email.Trim(),
                    Email = model.Email.Trim(),
                    FirstName = model.FirstName.Trim(),
                    LastName = model.LastName.Trim()
                };
                var result = await UserManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    int id = new AccountServices().AddAccount(model.Email,
                        model.FirstName, model.LastName,
                        model.Email, user.Id,
                        model.Password, ref statusMessage);
                    if (id > 0)
                    {
                        string msg = "Successfully created account for {0}. ";
                        if (model.OrganizationId > 0)
                        {
                            int ombrId = new OrganizationServices().OrganizationMember_Save((int)model.OrganizationId, id, 2, currentUserId, ref statusMessage);
                            if (ombrId > 0)
                            {
                                msg += " Added user as member of " + model.Organization;
                            }
                            else
                            {
                                ConsoleMessageHelper.SetConsoleErrorMessage("Error encountered adding user to organization " + statusMessage);
                            }
                        }
                        ConsoleMessageHelper.SetConsoleSuccessMessage(string.Format(msg, user.FirstName));
                        //return View( "ConfirmationRequired" );
                        ModelState.Clear();
                        return View();
                    }
                    else
                    {
                        ConsoleMessageHelper.SetConsoleErrorMessage("Error - " + statusMessage);
                        return View();
                    }


                    //return RedirectToAction( "Index", "Home" );
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form

            return View();
        }
        public JsonResult AddOrganization(int orgId, int userId)
        {

            int memberTypeId = 2; //employee - only option for now
            int currentUserId = AccountServices.GetCurrentUserId();

            int newId = new OrganizationServices().OrganizationMember_Save(orgId, userId, memberTypeId, currentUserId, ref statusMessage);
            if (newId == 0)
            {
                //handle error
                return Json(newId, statusMessage, JsonRequestBehavior.AllowGet);
            }

            return Json(newId, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Need to handle updates!!!
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public JsonResult AddRole(int userId, int roleId)
        {
            string statusMessage = "";
            int currentUserId = AccountServices.GetCurrentUserId();
            //TBD
            bool isOk = new AccountServices().AddRole(userId, roleId, currentUserId, ref statusMessage);
            if (!isOk)
            {
                //handle error
                return Json(isOk, statusMessage, JsonRequestBehavior.AllowGet);
            }

            return Json(statusMessage, JsonRequestBehavior.AllowGet);
        }

        public JsonResult OrgListByName(string keyword, int maxTerms = 25)
        {
            var result = OrganizationServices.OrgAutocomplete(keyword, maxTerms);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetOrganizations(string keyword)
        {
            var result = OrganizationServices.GetOrganizations(keyword);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UserListByEmail(string keyword, int maxTerms = 25)
        {

            var result = AccountServices.EmailAutocomplete(keyword, maxTerms);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
    }
}