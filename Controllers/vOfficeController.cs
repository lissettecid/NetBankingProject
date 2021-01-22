using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Azure.ServiceBus;
using Microsoft.Owin.Security;
using NetBanking.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading;
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
        public ActionResult Index()
        {
            if (User.IsInRole("Cliente"))
            {
                var id = User.Identity.Name;
                var idCard = db.NetBankingUserRequest.Where(x => x.PersonalEmail == id).FirstOrDefault().IdCard;
                return View(db.tblFavoriteAcc.Where(x => x.IdCard == idCard).ToList());
            }
            return View();
        }

        [Authorize(Roles = "Cliente")]
        public ActionResult CuentasPropias()
        {
            ViewBag.Err = "";
            return View();
        }

        //TODO: HttPost CuentaPropia
        //Crear View Terceros, Otro Banco
        #region Mi código
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult CuentasPropias([Bind(Include = "Id,IdTransact,AccIssuer,AccBeneficiary,TransactType,MoneyType,TransactDate,TransactMount,Concept,TransactState,UserId")] tblTransactions transactions)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        transactions.IdTransact = "101";
        //        var id = User.Identity.GetUserId();
        //        transactions.UserId = id;
        //        transactions.TransactType = "cuentas propias";
        //        transactions.MoneyType = "$RD";
        //        transactions.TransactDate = DateTime.Now;
        //        transactions.TransactState = "Pendiente";

        //        db.tblTransactions.Add(transactions);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }

        //    return View(transactions);
        //}
        #endregion
        static IQueueClient queueClientTransaccionRecibida;
        static IQueueClient queueClieTransaccionEnviada;
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CuentasPropias([Bind(Include = "Id,IdTransact,AccIssuer,AccBeneficiary,TransactType,MoneyType,TransactDate,TransactMount,Concept,TransactState")] tblTransactions transactions)
        {
            if (ModelState.IsValid)
            {
                transactions.IdTransact = "101";
                transactions.TransactType = "cuentas propias";
                transactions.MoneyType = "$RD";
                transactions.TransactDate = DateTime.Now;
                transactions.TransactState = "Pendiente";

                string TransaccionConnectionEnviada = "Endpoint=sb://integracion.servicebus.windows.net/;SharedAccessKeyName=todo;SharedAccessKey=MoqbbZxfWFpiAo9MB9P5J3jK5Ew474d9MTyhjQQVp80=";
                string queueTransaccionEnviada = "transaccionnet";

                string messageBody2 = JsonConvert.SerializeObject(transactions);
                queueClieTransaccionEnviada = new QueueClient(TransaccionConnectionEnviada, queueTransaccionEnviada);

                var messageSend = new Message(Encoding.UTF8.GetBytes(messageBody2));

                queueClieTransaccionEnviada.SendAsync(messageSend);

                string TransaccionConnectionRecibir = "Endpoint=sb://integracion.servicebus.windows.net/;SharedAccessKeyName=Todo;SharedAccessKey=AHVSBlsdEUQzog7b5yJiOJh67fiUcNYJBfHbZiH2swI=";
                string queueTransaccionRecibir = "transaccion";

                try
                {
                    queueClientTransaccionRecibida = new QueueClient(TransaccionConnectionRecibir, queueTransaccionRecibir);
                    var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
                    {

                        MaxConcurrentCalls = 1,
                        AutoComplete = false
                    };
                    queueClientTransaccionRecibida.RegisterMessageHandler(ReceiveMessagesAsync, messageHandlerOptions);
                }
                catch (Exception)
                {
                }
                finally
                {
                    queueClientTransaccionRecibida.CloseAsync();
                }

                async Task ReceiveMessagesAsync(Message message, CancellationToken token)
                {
                    string reply = Encoding.UTF8.GetString(message.Body);
                    tblTransactions transactions2 = JsonConvert.DeserializeObject<tblTransactions>(reply,

                     new JsonSerializerSettings
                     {
                         NullValueHandling = NullValueHandling.Ignore
                     });

                    db.tblTransactions.Add(transactions2);
                    db.SaveChanges();
                    await queueClientTransaccionRecibida.CompleteAsync(message.SystemProperties.LockToken);
                }

                Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
                {
                    return Task.CompletedTask;
                }

                return RedirectToAction("Index");
            }

            return View(transactions);
        }

        [Authorize(Roles = "Cliente")]
        public ActionResult ATerceros()
        {
            ViewBag.Err = "";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ATerceros([Bind(Include = "Id,IdTransact,AccIssuer,AccBeneficiary,TransactType,MoneyType,TransactDate,TransactMount,Concept,TransactState")] tblTransactions transactions)
        {
            if (ModelState.IsValid)
            {
                transactions.IdTransact = "102";
                transactions.TransactType = "A terceros";
                transactions.MoneyType = "DOP";
                transactions.TransactDate = DateTime.Now;
                transactions.TransactState = "Pendiente";

                string TransaccionConnectionEnviada = "Endpoint=sb://integracion.servicebus.windows.net/;SharedAccessKeyName=todo;SharedAccessKey=MoqbbZxfWFpiAo9MB9P5J3jK5Ew474d9MTyhjQQVp80=";
                string queueTransaccionEnviada = "transaccionnet";

                string messageBody2 = JsonConvert.SerializeObject(transactions);
                queueClieTransaccionEnviada = new QueueClient(TransaccionConnectionEnviada, queueTransaccionEnviada);

                var messageSend = new Message(Encoding.UTF8.GetBytes(messageBody2));

                queueClieTransaccionEnviada.SendAsync(messageSend);

                string TransaccionConnectionRecibir = "Endpoint=sb://integracion.servicebus.windows.net/;SharedAccessKeyName=Todo;SharedAccessKey=AHVSBlsdEUQzog7b5yJiOJh67fiUcNYJBfHbZiH2swI=";
                string queueTransaccionRecibir = "transaccion";

                try
                {
                    queueClientTransaccionRecibida = new QueueClient(TransaccionConnectionRecibir, queueTransaccionRecibir);
                    var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
                    {

                        MaxConcurrentCalls = 1,
                        AutoComplete = false
                    };
                    queueClientTransaccionRecibida.RegisterMessageHandler(ReceiveMessagesAsync, messageHandlerOptions);
                }
                catch (Exception)
                {
                }
                finally
                {
                    queueClientTransaccionRecibida.CloseAsync();
                }

                async Task ReceiveMessagesAsync(Message message, CancellationToken token)
                {
                    string reply = Encoding.UTF8.GetString(message.Body);
                    tblTransactions transactions2 = JsonConvert.DeserializeObject<tblTransactions>(reply,

                     new JsonSerializerSettings
                     {
                         NullValueHandling = NullValueHandling.Ignore
                     });

                    db.tblTransactions.Add(transactions2);
                    db.SaveChanges();
                    await queueClientTransaccionRecibida.CompleteAsync(message.SystemProperties.LockToken);
                }

                Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
                {
                    return Task.CompletedTask;
                }

                return RedirectToAction("Index");
            }

            return View(transactions);
        }

        [Authorize(Roles = "Administrador")]
        public ActionResult Authorization()
        {
            return View(db.NetBankingUserRequest.Where(x => x.StatusText.Trim() == "Solicitud").ToList());
        }

        [Authorize(Roles = "Administrador")]
        public ActionResult UserAuthorization(int id)
        {
            var solicitante = db.NetBankingUserRequest.Find(id);
            ViewBag.StatusText = new SelectList(db.UserStatusActivo.Where(x => x.Inicial == true), "Status", "Status", solicitante.StatusText);
            return View(solicitante);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UserAuthorization([Bind(Include = "Id,IdCard,Name,LastName,BirthDate,PhoneNumber,CellPhone,WorkTel,Address,WorkAddress,PersonalEmail,WorkEmail,RequestDate,RequestStatus,StatusText,StatusComment,EmployeeAuthorizationID,DateAuthorization")] NetBankingUserRequest netBankingUser)
        {
            if (ModelState.IsValid)
            {
                var row = db.NetBankingUserRequest.Find(netBankingUser.Id);
                if (netBankingUser.StatusText.Trim() == "Solicitud" || netBankingUser.StatusText.Trim() == "Inactivo")
                {
                    return RedirectToAction("Authorization");
                }

                if (netBankingUser.StatusText.Trim() == "Rechazado")
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

        [Authorize(Roles = "Administrador")]
        public ActionResult UserDetails(int id)
        {
            var request = db.NetBankingUserRequest.Find(id);
            return View(request);
        }

        [Authorize(Roles = "Cliente")]
        public ActionResult AccUserConsult()
        {
            ViewBag.Actualizar = "";
            var id = User.Identity.Name;
            var idCard = db.NetBankingUserRequest.Where(x => x.PersonalEmail == id).FirstOrDefault().IdCard;
            //ViewBag.User = IdCard;
            //TODO: Solicitar a Integración/CORE las cuentas de este usuario con cédula "IdCard"
            return View(db.tblAccounts.Where(x => x.IdCard == idCard).ToList());
        }

        #region Actualizar
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult AccUserConsult(tblAccounts accounts)
        //{
        //    var id = User.Identity.Name;
        //    var idCard = db.NetBankingUserRequest.Where(x => x.PersonalEmail == id).FirstOrDefault().IdCard;
        //    //TODO: Pedir al CORE Actualizar la tabla de cuentas
        //    ViewBag.Actualizar = "Actualizado";
        //    return View(db.tblAccounts.Where(x => x.IdCard == idCard).ToList());
        //}
        #endregion

        [Authorize(Roles = "Cliente")]
        public ActionResult AccDetails(int id)
        {
            var account = db.tblAccounts.Find(id);
            return View(account);
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

        [Authorize(Roles = "Cliente")]
        public ActionResult Beneficiary()
        {
            var id = User.Identity.Name;
            var idCard = db.NetBankingUserRequest.Where(x => x.PersonalEmail == id).FirstOrDefault().IdCard;
            return View(db.tblFavoriteAcc.Where(x => x.IdCard == idCard).ToList());
        }
    }
}