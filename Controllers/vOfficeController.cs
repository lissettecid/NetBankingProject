using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using NetBanking.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace NetBanking.Controllers
{
    [Authorize]
    public class vOfficeController : Controller
    {
        private BancomanNetBankingEntities db = new BancomanNetBankingEntities();

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public vOfficeController()
        {
        }

        public vOfficeController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

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

        // GET: vOffice
        //[Authorize(Roles = "Cliente")]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Cliente")]
        public ActionResult CuentasPropias()
        {
            return View();
        }

        [Authorize(Roles = "Administrador")]
        public ActionResult Authorization()
        {
            return View(db.NetBankingUserRequest.Where(x => x.StatusText == "Solicitud").ToList());
        }

        [Authorize(Roles = "Administrador")]
        public ActionResult UserAuthorization(int id)
        {
            var solicitante = db.NetBankingUserRequest.Find(id);
            return View(solicitante);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UserAuthorization([Bind(Include = "Id,IdCard,Name,LastName,BirthDate,PhoneNumber,CellPhone,WorkTel,Address,WorkAddress,PersonalEmail,WorkEmail,RequestDate,RequestStatus,StatusText,StatusComment,EmployeeAuthorizationID,DateAuthorization")] NetBankingUserRequest netBankingUser)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = netBankingUser.PersonalEmail, Email = netBankingUser.PersonalEmail };
                var result = await UserManager.CreateAsync(user, "123456@Ab");
                if (result.Succeeded)
                {
                    string UserId = db.AspNetUsers.Where(x => x.Email == netBankingUser.PersonalEmail).FirstOrDefault().Id;
                    await UserManager.AddToRoleAsync(UserId, "3");
                    var row = db.NetBankingUserRequest.Find(netBankingUser.Id);
                    row.StatusText = netBankingUser.StatusText;
                    row.StatusComment = netBankingUser.StatusComment;
                    row.UserId = UserId;

                    db.Entry(row).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Authorization");
                }  
            }
            return View(netBankingUser);
        }
    }
}