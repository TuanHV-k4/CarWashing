using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DataAccessLayer.Entity;

namespace DataAccessLayer.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.HasKey(r => r.RoleID);
            builder.Property(r => r.RoleName).IsRequired().HasMaxLength(100);
            builder.HasMany(r => r.Users).WithOne(u => u.Role).HasForeignKey(u => u.RoleID).OnDelete(DeleteBehavior.Restrict);

            builder.HasData(
                new Role { RoleID = Guid.Parse("11111111-1111-1111-1111-111111111111"), RoleName = "Admin" },
                new Role { RoleID = Guid.Parse("22222222-2222-2222-2222-222222222222"), RoleName = "Staff" },
                new Role { RoleID = Guid.Parse("33333333-3333-3333-3333-333333333333"), RoleName = "Customer" }
                ,new Role { RoleID = Guid.Parse("44444444-4444-4444-4444-444444444444"), RoleName = "BranchManager" }
            );
        }
    }
}
