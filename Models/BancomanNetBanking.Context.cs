﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class BancomanNetBankingEntities : DbContext
    {
        public BancomanNetBankingEntities()
            : base("name=BancomanNetBankingEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<C__MigrationHistory> C__MigrationHistory { get; set; }
        public virtual DbSet<AspNetRoles> AspNetRoles { get; set; }
        public virtual DbSet<AspNetUserClaims> AspNetUserClaims { get; set; }
        public virtual DbSet<AspNetUserLogins> AspNetUserLogins { get; set; }
        public virtual DbSet<AspNetUsers> AspNetUsers { get; set; }
        public virtual DbSet<UserStatusActivo> UserStatusActivo { get; set; }
        public virtual DbSet<NetBankingUserRequest> NetBankingUserRequest { get; set; }
        public virtual DbSet<tblFavoriteAcc> tblFavoriteAcc { get; set; }
        public virtual DbSet<tblAccounts> tblAccounts { get; set; }
        public virtual DbSet<tblTransactions> tblTransactions { get; set; }
        public virtual DbSet<Log> Log { get; set; }
    }
}
