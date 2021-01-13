using NetBanking.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NetBanking.Controllers
{
    public class vOfficeController : Controller
    {
        private BancomanNetBankingEntities db = new BancomanNetBankingEntities();
        // GET: vOffice
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult CuentasPropias()
        {
            return View();
        }

        public ActionResult Createuser()
        {
            RegisterViewModel rv = new RegisterViewModel();
            rv.Email = "fulano@fulano.com";
            rv.Password = "123456@Ab";
            rv.ConfirmPassword = "123456@Ab";


        }
    }
}