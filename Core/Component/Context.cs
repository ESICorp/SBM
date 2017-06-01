using SBM.Model;
using System;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;

namespace SBM.Component
{
    internal class Context : DbContext
    {
        public Context() : base("name=Entities")
        {
            Configuration.ProxyCreationEnabled = false;
            Configuration.LazyLoadingEnabled = false;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //throw new UnintentionalCodeFirstException();

            modelBuilder.Entity<SBM_SERVICE>()
                .HasMany<SBM_OWNER>(s => s.SBM_OWNER)
                .WithMany(o => o.SBM_SERVICE)
                .Map(so =>
               {
                   so.MapLeftKey("ID_SERVICE");
                   so.MapRightKey("ID_OWNER");
                   so.ToTable("SBM_SERVICE_OWNER");
               });

            base.OnModelCreating(modelBuilder);
        }
    
        public DbSet<SBM_DISPATCHER> SBM_DISPATCHER { get; set; }
        public DbSet<SBM_DONE> SBM_DONE { get; set; }
        public DbSet<SBM_DONE_STATUS> SBM_DONE_STATUS { get; set; }
        public DbSet<SBM_EVENT> SBM_EVENT { get; set; }
        public DbSet<SBM_EVENT_LOG> SBM_EVENT_LOG { get; set; }
        public DbSet<SBM_OBJ_POOL> SBM_OBJ_POOL { get; set; }
        public DbSet<SBM_OWNER> SBM_OWNER { get; set; }
        public DbSet<SBM_SERVICE> SBM_SERVICE { get; set; }
        public DbSet<SBM_SERVICE_INTERNAL> SBM_SERVICE_INTERNAL { get; set; }
        public DbSet<SBM_SERVICE_TYPE> SBM_SERVICE_TYPE { get; set; }
        public DbSet<SBM_REMOTING> SBM_REMOTING { get; set; }
        public DbSet<SBM_SERVICE_TIMER> SBM_SERVICE_TIMER { get; set; }

        //public DbSet<SBM_SERVICE_OWNER> SBM_SERVICE_OWNER { get; set; }

        public virtual ObjectResult<byte[]> Decrypt(string pass_phrase, byte[] cipher_text)
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteStoreQuery<byte[]>(
                "select DecryptByPassPhrase(@phrase, @cipher)",
                new SqlParameter("phrase", pass_phrase),
                new SqlParameter("cipher", cipher_text));
        }
        public virtual ObjectResult<byte[]> Crypt(string pass_phrase, string clear_text)
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteStoreQuery<byte[]>(
                "select EncryptByPassPhrase(@phrase, @text)",
                new SqlParameter("phrase", pass_phrase),
                new SqlParameter("text", clear_text));
        }

        public virtual ObjectResult<Guid> Enqueue(string request)
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteStoreQuery<Guid>(
                "sp_enqueue @RequestMsg", new SqlParameter("RequestMsg", request));
        }

        public virtual ObjectResult<string> Join(Guid handle, long timeout)
        {
            var context = ((IObjectContextAdapter)this).ObjectContext;
            
            context.CommandTimeout = Convert.ToInt32(Math.Ceiling(timeout * 1.1));
            
            return context.ExecuteStoreQuery<string>( 
                "sp_join @Handle, @timeout", 
                new SqlParameter("Handle", handle),
                new SqlParameter("timeout", timeout * 1000L));
        }
    }
}
