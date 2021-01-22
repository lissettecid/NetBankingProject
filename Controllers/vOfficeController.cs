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
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
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
        //        transactions.TransactType = "cuentas propias";
        //        transactions.MoneyType = "DOP";
        //        transactions.TransactDate = DateTime.Now;
        //        transactions.TransactState = "Pendiente";

        //        db.tblTransactions.Add(transactions);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }

        //    return View(transactions);
        //}
        #endregion

        #region Código Fraulin
        static IQueueClient queueClientTransaccionRecibida;
        static IQueueClient queueClieTransaccionEnviada;
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CuentasPropias([Bind(Include = "Id,IdTransact,AccIssuer,AccBeneficiary,TransactType,MoneyType,TransactDate,TransactMount,Concept,TransactState")] tblTransactions transactions)
        {
            if (ModelState.IsValid)
            {
                transactions.TransactType = "cuentas propias";
                transactions.MoneyType = "DOP";
                transactions.TransactDate = DateTime.Now;
                transactions.TransactState = "Pendiente";

                string transaccionnetString = "Endpoint=sb://integracion.servicebus.windows.net/;SharedAccessKeyName=todo;SharedAccessKey=MoqbbZxfWFpiAo9MB9P5J3jK5Ew474d9MTyhjQQVp80=";
                string queuetransaccionet = "transaccionnet";

                string messageBody2 = JsonConvert.SerializeObject(transactions);
                queueClieTransaccionEnviada = new QueueClient(transaccionnetString, queuetransaccionet);

                var messageSend = new Message(Encoding.UTF8.GetBytes(messageBody2));

                queueClieTransaccionEnviada.SendAsync(messageSend);

                string CustomerString = "Endpoint=sb://integracion.servicebus.windows.net/;SharedAccessKeyName=todo;SharedAccessKey=g3a+bsOoenW1fQkTzU9wvO3+JozWdb9EKf8ZohU/HEM=";
                string queue = "transactioncore";

                try
                {
                    queueClientTransaccionRecibida = new QueueClient(CustomerString, queue);
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

                }

                async Task ReceiveMessagesAsync(Message message, CancellationToken token)
                {
                    await queueClientTransaccionRecibida.CompleteAsync(message.SystemProperties.LockToken);
                    string reply = Encoding.UTF8.GetString(message.Body);
                    tblTransactions transactions2 = JsonConvert.DeserializeObject<tblTransactions>(reply,

                     new JsonSerializerSettings
                     {
                         NullValueHandling = NullValueHandling.Ignore
                     });

                    if (transactions2.TransactState.Trim() == "Procesado" || transactions2.TransactState.Trim() == "Procesando")
                    {
                        db.tblTransactions.Add(transactions2);
                        db.SaveChanges();
                        var id = User.Identity.Name;
                        var idCard = db.NetBankingUserRequest.Where(x => x.PersonalEmail == id).FirstOrDefault().PersonalEmail;
                        log.Info($"El usuario {idCard} ha hecho un transacción.");
                    }
                    else
                    {
                        ViewBag.Err = "No se pudo realizar la transacción";
                    }
                }

                Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
                {
                    return Task.CompletedTask;
                }

                return RedirectToAction("Index");
            }

            return View(transactions);
        }
        #endregion

        [Authorize(Roles = "Cliente")]
        public ActionResult ATerceros()
        {
            ViewBag.Err = "";
            return View();
        }

        #region Mi código
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult ATerceros([Bind(Include = "Id,IdTransact,AccIssuer,AccBeneficiary,TransactType,MoneyType,TransactDate,TransactMount,Concept,TransactState,UserId")] tblTransactions transactions)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        transactions.IdTransact = "102";
        //        transactions.TransactType = "A terceros";
        //        transactions.MoneyType = "DOP";
        //        transactions.TransactDate = DateTime.Now;
        //        transactions.TransactState = "Pendiente";

        //        db.tblTransactions.Add(transactions);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }

        //    return View(transactions);
        //}
        #endregion

        #region Código Fraulin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ATerceros([Bind(Include = "Id,IdTransact,AccIssuer,AccBeneficiary,TransactType,MoneyType,TransactDate,TransactMount,Concept,TransactState")] tblTransactions transactions)
        {
            if (ModelState.IsValid)
            {
                transactions.TransactType = "A Terceros";
                transactions.MoneyType = "DOP";
                transactions.TransactDate = DateTime.Now;
                transactions.TransactState = "Pendiente";

                string transaccionnetString = "Endpoint=sb://integracion.servicebus.windows.net/;SharedAccessKeyName=todo;SharedAccessKey=MoqbbZxfWFpiAo9MB9P5J3jK5Ew474d9MTyhjQQVp80=";
                string queuetransaccionet = "transaccionnet";

                string messageBody2 = JsonConvert.SerializeObject(transactions);
                queueClieTransaccionEnviada = new QueueClient(transaccionnetString, queuetransaccionet);

                var messageSend = new Message(Encoding.UTF8.GetBytes(messageBody2));

                queueClieTransaccionEnviada.SendAsync(messageSend);

                string CustomerString = "Endpoint=sb://integracion.servicebus.windows.net/;SharedAccessKeyName=todo;SharedAccessKey=g3a+bsOoenW1fQkTzU9wvO3+JozWdb9EKf8ZohU/HEM=";
                string queue = "transactioncore";

                try
                {
                    queueClientTransaccionRecibida = new QueueClient(CustomerString, queue);
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

                }

                async Task ReceiveMessagesAsync(Message message, CancellationToken token)
                {
                    await queueClientTransaccionRecibida.CompleteAsync(message.SystemProperties.LockToken);
                    string reply = Encoding.UTF8.GetString(message.Body);
                    tblTransactions transactions2 = JsonConvert.DeserializeObject<tblTransactions>(reply,

                     new JsonSerializerSettings
                     {
                         NullValueHandling = NullValueHandling.Ignore
                     });

                    if (transactions2.TransactState.Trim() == "Procesado" || transactions2.TransactState.Trim() == "Procesando")
                    {
                        db.tblTransactions.Add(transactions2);
                        db.SaveChanges();
                        var id = User.Identity.Name;
                        var idCard = db.NetBankingUserRequest.Where(x => x.PersonalEmail == id).FirstOrDefault().PersonalEmail;
                        log.Info($"El usuario {idCard} ha hecho un transacción.");
                    }
                    else
                    {
                        ViewBag.Err = "No se pudo realizar la transacción";
                    }
                }

                Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
                {
                    return Task.CompletedTask;
                }

                return RedirectToAction("Index");
            }

            return View(transactions);
        }
        #endregion

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

        #region Mi código
        //[Authorize(Roles = "Cliente")]
        //public ActionResult AccUserConsult()
        //{
        //    var id = User.Identity.Name;
        //    var idCard = db.NetBankingUserRequest.Where(x => x.PersonalEmail == id).FirstOrDefault().IdCard.Trim();
        //    return View(db.tblAccounts.Where(x => x.IdCard == idCard).ToList());
        //}
        #endregion

        #region Código Fraulin
        static IQueueClient queueCustomerClient;
        static IQueueClient queuecedulaClient;
        [Authorize(Roles = "Cliente")]
        public ActionResult AccUserConsult()
        {
            ViewBag.Actualizar = "";
            //var id = User.Identity.Name;
            var idCard = "550";/*db.NetBankingUserRequest.Where(x => x.PersonalEmail == id).FirstOrDefault().IdCard.Trim();*/
            string cedulaString = "Endpoint=sb://integracion.servicebus.windows.net/;SharedAccessKeyName=todo;SharedAccessKey=RCP3GyGxzKFJAsywsh4urvs/bSVanEzNY5RPpAlGAdQ=";
            string queuetcedula = "cedulanet";
            var messageBody = JsonConvert.SerializeObject(idCard);
            queueCustomerClient = new QueueClient(cedulaString, queuetcedula);

            var message2 = new Microsoft.Azure.ServiceBus.Message(Encoding.UTF8.GetBytes(messageBody));


            queueCustomerClient.SendAsync(message2);

            string cedulaStringrecived = "Endpoint=sb://integracion.servicebus.windows.net/;SharedAccessKeyName=todo;SharedAccessKey=H4GLh/Odbha5Rpy35pPhXImZvd/5J2Lx7onkTCMib30=";
            string queuecedularecived = "ceudlacore";

            try
            {
                queuecedulaClient = new QueueClient(cedulaStringrecived, queuecedularecived);

                var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
                {
                    MaxConcurrentCalls = 1,
                    AutoComplete = false
                };
                queuecedulaClient.RegisterMessageHandler(ReceiveMessagesAsync, messageHandlerOptions);

            }
            catch (Exception)
            {

            }
            finally
            {

            }

            async Task ReceiveMessagesAsync(Microsoft.Azure.ServiceBus.Message message, CancellationToken token)
            {
                await queuecedulaClient.CompleteAsync(message.SystemProperties.LockToken);
                string reply = Encoding.UTF8.GetString(message.Body);
                tblAccounts accounts = JsonConvert.DeserializeObject<tblAccounts>(reply,

                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                var ifExists = db.tblAccounts.Where(x => x.Accountnumber == accounts.Accountnumber && x.IdCard == accounts.IdCard).FirstOrDefault();
                if (ifExists == null)
                {
                    db.tblAccounts.Add(accounts);
                    db.SaveChanges();
                }
                else
                {
                    db.Entry(ifExists).State = EntityState.Modified;
                    db.SaveChanges();
                }

            }

            Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
            {
                Console.WriteLine(exceptionReceivedEventArgs.Exception);
                return Task.CompletedTask;
            }

            return View(db.tblAccounts.Where(x => x.IdCard == idCard).ToList());
        }
        #endregion

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
                log.Info($"El usuario de cédula {favoriteAcc.IdCard}, acaba de agregar un beneficiario");
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