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
            return View(db.NetBankingUserRequest.ToList());
        }

        public ActionResult CuentasPropias()
        {
            return View();
        }
    }
}