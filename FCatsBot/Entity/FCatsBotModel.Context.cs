﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FCatsBot.Entity
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class FCatsBotEntities : DbContext
    {
        public FCatsBotEntities()
            : base("name=FCatsBotEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<cats> cats { get; set; }
        public virtual DbSet<cats_on_moderation> cats_on_moderation { get; set; }
        public virtual DbSet<users> users { get; set; }
        public virtual DbSet<cats_viewed_by_users> cats_viewed_by_users { get; set; }
    }
}