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
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using System.Text;
using System.Threading;

namespace NetBanking.Controllers
{
    
    [Authorize]
    public class vOfficeController : Controller
    {
        static ITopicClient topicClient;
        static ISubscriptionClient subscriptionClient;

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
            var id = User.Identity.Name;
            var idCard = db.NetBankingUserRequest.Where(x => x.PersonalEmail == id).FirstOrDefault().IdCard;
            return View(db.tblFavoriteAcc.Where(x => x.IdCard == idCard).ToList());
        }

        [Authorize(Roles = "Cliente")]
        public ActionResult CuentasPropias()
        {
            ViewBag.Err = "";
            return View();
        }

        //TODO: HttPost CuentaPropia
        //Crear View Terceros, Otro Banco
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CuentasPropias([Bind(Include = "Id,IdTransact,AccIssuer,AccBeneficiary,TransactType,MoneyType,TransactDate,TransactMount,Concept,TransactState,UserId")] tblTransactions transactions)
        {
            string sbConnectionString = "Endpoint=sb://serviceprueban3.servicebus.windows.net/;SharedAccessKeyName=Transaccion-Tot;SharedAccessKey=2UKKar0wbx34AWx2abEKCUro8+mtunJM7avryCfU4po=";
            string sbTopic = "transaccion";
            string messageBody = string.Empty;

            //string sbConnectionString2 = "Endpoint=sb://serviceprueban3.servicebus.windows.net/;SharedAccessKeyName=EscuchoWilliam;SharedAccessKey=My3W71j+bOH+8IJXLivAGDX+uQfVyElutQGGlHN67jw=";
            //string sbTopic2 = "transaccion";
            //string sbSubscription = "EscuchoWilliam";
            try
            {
                if (ModelState.IsValid)
                {
                    transactions.IdTransact = "101";
                    var id = User.Identity.GetUserId();
                    transactions.UserId = id;
                    transactions.TransactType = "cuentas propias";
                    transactions.MoneyType = "$RD";
                    transactions.TransactDate = DateTime.Now;
                    transactions.TransactState = "Pendiente";

                    messageBody = JsonConvert.SerializeObject(transactions);
                    topicClient = new TopicClient(sbConnectionString, sbTopic);

                    var message = new Message(Encoding.UTF8.GetBytes(messageBody));

                    topicClient.SendAsync(message);

                    //subscriptionClient = new SubscriptionClient(sbConnectionString2, sbTopic2, sbSubscription);

                    //var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
                    //{
                    //    MaxConcurrentCalls = 1,
                    //    AutoComplete = false
                    //};
                    //subscriptionClient.RegisterMessageHandler(ReceiveMessagesAsync, messageHandlerOptions);

                    db.tblTransactions.Add(transactions);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (Exception)
            {
                ViewBag.Err = "No se pudo realizar la transacción. Intente más tarde.";
                throw;
            }
            finally
            {
                topicClient.CloseAsync();
            }


            return View(transactions);
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
            ViewBag.OpcList = new SelectList(db.UserStatusActivo.Where(x => x.Inicial == true), "Status", "Status", solicitante.StatusText);
            return View(solicitante);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UserAuthorization([Bind(Include = "Id,IdCard,Name,LastName,BirthDate,PhoneNumber,CellPhone,WorkTel,Address,WorkAddress,PersonalEmail,WorkEmail,RequestDate,RequestStatus,StatusText,StatusComment,EmployeeAuthorizationID,DateAuthorization")] NetBankingUserRequest netBankingUser)
        {
            if (ModelState.IsValid)
            {
                var row = db.NetBankingUserRequest.Find(netBankingUser.Id);
                if (netBankingUser.StatusText == "Solicitud" || netBankingUser.StatusText == "Inactivo")
                {
                    return RedirectToAction("Authorization");
                }

                if (netBankingUser.StatusText == "Rechazado")
                {
                    row.StatusText = netBankingUser.StatusText;
                    row.StatusComment = netBankingUser.StatusComment;
                    row.EmployeeAuthorizationID = User.Identity.GetUserId();
                    row.DateAuthorization = DateTime.Now;


                    db.Entry(row).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Authorization");
                }

                RegisterViewModel rvm = new RegisterViewModel
                {
                    Email = row.PersonalEmail,
                    Password = "123456@Ab",
                    ConfirmPassword = "123456@Ab"
                };
                var user = new ApplicationUser { UserName = rvm.Email, Email = rvm.Email };
                var result = await UserManager.CreateAsync(user, rvm.Password);
                if (result.Succeeded)
                {
                    string UserId = db.AspNetUsers.Where(x => x.Email == row.PersonalEmail).FirstOrDefault().Id;
                    await UserManager.AddToRoleAsync(UserId, "Cliente");

                    row.StatusText = netBankingUser.StatusText;
                    row.StatusComment = netBankingUser.StatusComment;
                    row.EmployeeAuthorizationID = User.Identity.GetUserId();
                    row.DateAuthorization = DateTime.Now;
                    row.UserId = UserId;

                    db.Entry(row).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Authorization");
                }
            }
            return View(netBankingUser);
        }

        [Authorize(Roles = "Administrador")]
        public ActionResult HistoryRequest()
        {
            return View(db.NetBankingUserRequest);
        }

        [Authorize(Roles = "Cliente")]
        public ActionResult AccUserConsult()
        {
            var id = User.Identity.Name;
            var IdCard = db.NetBankingUserRequest.Where(x => x.PersonalEmail == id).FirstOrDefault().IdCard;
            //ViewBag.User = IdCard;
            //TODO: Solicitar a Integración/CORE las cuentas de este usuario con cédula "IdCard"
            return View();
        }

        [Authorize(Roles = "Cliente")]
        public ActionResult CreateBeneficiary()
        {
            ViewBag.TitleErr = "";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateBeneficiary([Bind(Include = "Id,IdCard,BeneficiaryName,AccBeneficiary,BeneficiaryEmail,AddDate")] tblFavoriteAcc favoriteAcc)
        {
            var accBeneficiary = db.tblFavoriteAcc.Where(x => x.AccBeneficiary == favoriteAcc.AccBeneficiary).FirstOrDefault();
            if (accBeneficiary != null)
            {
                ViewBag.TitleErr = "El número de cuenta ya está agregado como favorito";
                return View();
            }

            var beneficiaryName = db.tblFavoriteAcc.Where(x => x.BeneficiaryName == favoriteAcc.BeneficiaryName).FirstOrDefault();
            if (beneficiaryName != null)
            {
                ViewBag.TitleErr = "Este alias está ocupado. Debe cambiarlo.";
                return View();
            }

            //TODO: Confirmar con el CORE que la cuenta existe
            //if(no existe)

            var id = User.Identity.Name;
            var idCard = db.NetBankingUserRequest.Where(x => x.PersonalEmail == id).FirstOrDefault().IdCard;
            favoriteAcc.IdCard = idCard;
            favoriteAcc.AddDate = DateTime.Now;
            if (ModelState.IsValid)
            {
                db.tblFavoriteAcc.Add(favoriteAcc);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(favoriteAcc);
        }

        //static async Task ReceiveMessagesAsync(Message message, CancellationToken token)
        //{
        //    await subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);

        //    string reply = Encoding.UTF8.GetString(message.Body);

        //    TransactionRecived transactions2 = JsonConvert.DeserializeObject<TransactionRecived>(reply,

        //     new JsonSerializerSettings
        //     {
        //         NullValueHandling = NullValueHandling.Ignore
        //     });
        //}

        //static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        //{
        //    return Task.CompletedTask;
        //}

        //public class TransactionRecived
        //{
        //    public string IdTransact { get; set; }
        //    public string AccIssuer { get; set; }
        //    public string AccBeneficiary { get; set; }
        //    public string TransactType { get; set; }
        //    public string MoneyType { get; set; }
        //    public DateTime TransactDate { get; set; }
        //    public decimal TransactMount { get; set; }
        //    public string Concept { get; set; }
        //    public string TransactState { get; set; }

        //    public TransactionRecived(string idtrans, string accissuer, string accbene, string transacttype, string moneytype, DateTime transactdate, decimal transmount, string concept, string transtate)
        //    {
        //        IdTransact = idtrans;
        //        AccIssuer = accissuer;
        //        AccBeneficiary = accbene;
        //        TransactType = transacttype;
        //        MoneyType = moneytype;
        //        TransactDate = transactdate;
        //        TransactMount = transmount;
        //        Concept = concept;
        //        TransactState = transtate;
        //    }

        //    public TransactionRecived()
        //    {

        //    }
        //}
    }
}

