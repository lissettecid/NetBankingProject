//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NetBanking.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class tblTransactions
    {
        public int Id { get; set; }
        public string IdTransact { get; set; }
        public string AccIssuer { get; set; }
        public string AccBeneficiary { get; set; }
        public string TransactType { get; set; }
        public string MoneType { get; set; }
        public System.DateTime TransactDate { get; set; }
        public decimal TransactMount { get; set; }
        public string Concept { get; set; }
        public string TransactState { get; set; }
        public string UserId { get; set; }
    }
}