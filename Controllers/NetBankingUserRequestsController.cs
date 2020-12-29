﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using NetBanking.Models;

namespace NetBanking.Controllers
{
    public class NetBankingUserRequestsController : Controller
    {
        private BancomanNetBankingEntities db = new BancomanNetBankingEntities();

        // GET: NetBankingUserRequests
        public ActionResult Index()
        {
            return View(db.NetBankingUserRequest.ToList());
        }

        // GET: NetBankingUserRequests/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NetBankingUserRequest netBankingUserRequest = db.NetBankingUserRequest.Find(id);
            if (netBankingUserRequest == null)
            {
                return HttpNotFound();
            }
            return View(netBankingUserRequest);
        }

        // GET: NetBankingUserRequests/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: NetBankingUserRequests/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,IdCard,Name,LastName,BirthDate,PhoneNumber,CellPhone,WorkTel,Address,WorkAddress,PersonalEmail,WorkEmail,RequestDate,RequestStatus,StatusComment,EmployeeAuthorizationID,DateAuthorization")] NetBankingUserRequest netBankingUserRequest)
        {
            if (ModelState.IsValid)
            {
                db.NetBankingUserRequest.Add(netBankingUserRequest);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(netBankingUserRequest);
        }

        // GET: NetBankingUserRequests/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NetBankingUserRequest netBankingUserRequest = db.NetBankingUserRequest.Find(id);
            if (netBankingUserRequest == null)
            {
                return HttpNotFound();
            }
            return View(netBankingUserRequest);
        }

        // POST: NetBankingUserRequests/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,IdCard,Name,LastName,BirthDate,PhoneNumber,CellPhone,WorkTel,Address,WorkAddress,PersonalEmail,WorkEmail,RequestDate,RequestStatus,StatusComment,EmployeeAuthorizationID,DateAuthorization")] NetBankingUserRequest netBankingUserRequest)
        {
            if (ModelState.IsValid)
            {
                db.Entry(netBankingUserRequest).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(netBankingUserRequest);
        }

        // GET: NetBankingUserRequests/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            NetBankingUserRequest netBankingUserRequest = db.NetBankingUserRequest.Find(id);
            if (netBankingUserRequest == null)
            {
                return HttpNotFound();
            }
            return View(netBankingUserRequest);
        }

        // POST: NetBankingUserRequests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            NetBankingUserRequest netBankingUserRequest = db.NetBankingUserRequest.Find(id);
            db.NetBankingUserRequest.Remove(netBankingUserRequest);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult probando()
        {
            var tab = db.NetBankingUserRequest.Find(2);
            return View(tab);
        }
    }
}
