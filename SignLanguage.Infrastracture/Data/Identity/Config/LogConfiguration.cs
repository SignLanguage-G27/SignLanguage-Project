using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SignLanguage.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignLanguage.Infrastracture.Data.Identity.Config
{
    public class LogConfiguration :IEntityTypeConfiguration<PredictionLog>
    {
        public void Configure(EntityTypeBuilder<PredictionLog> builder)
        {
            builder.HasKey(l => l.Id);

            builder.Property(l => l.Id)
                   .ValueGeneratedOnAdd()
                   .UseIdentityColumn(0, 1);

            builder.Property(l => l.ImagePath).IsRequired();
            builder.Property(l => l.Result).IsRequired();

            builder.HasOne(l => l.User)
                   .WithMany(u => u.PredictionLogs)
                   .HasForeignKey(l => l.UserId);
        }
    }
    
    
}
