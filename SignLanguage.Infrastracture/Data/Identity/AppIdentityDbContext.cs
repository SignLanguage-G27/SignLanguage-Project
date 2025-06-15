using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SignLanguage.Core.Entities;
using SignLanguage.Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SignLanguage.Infrastracture.Data.Identity
{
    public class AppIdentityDbContext :IdentityDbContext<AppUser>
    {
        public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options):base(options)
        {
         
            
            
        }
        public DbSet<PredictionLog> Logs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.ToTable("Users"); // تغيير الجدول الافتراضي "AspNetUsers" إلى "Users"

                // فرض الفهارس الفريدة
                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasIndex(u => u.PhoneNumber).IsUnique();
                entity.HasIndex(u => u.UserName).IsUnique();


                // تطبيق قيود الفاليديشن على DisplayName
                entity.Property(u => u.DisplayName)
                      .IsRequired()
                      .HasMaxLength(20)
                      .HasAnnotation("RegularExpression", @"^[A-Za-z0-9_.\- ]{1,20}$");

                // البريد الإلكتروني فقط @gmail.com
                entity.Property(u => u.Email)
                      .IsRequired()
                      .HasMaxLength(256)
                      .HasAnnotation("RegularExpression", @"^[a-zA-Z0-9](?:[a-zA-Z0-9._]*[a-zA-Z0-9])?@gmail\.com$");

                // رقم الهاتف بصيغة +20xxxxxxxxxx
                entity.Property(u => u.PhoneNumber)
                      .IsRequired()
                      .HasMaxLength(13)
                      .HasAnnotation("RegularExpression", @"^(\+20)[0-9]{10}$");
            });

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }


    }
}
