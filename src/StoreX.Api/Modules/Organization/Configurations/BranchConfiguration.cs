using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreX.Api.Modules.Organization.Entities;

namespace StoreX.Api.Modules.Organization.Configurations;

public sealed class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("Branches", "organization");

        builder.HasKey(branch => branch.Id);
builder.HasQueryFilter(branch => !branch.IsDeleted);

        builder.Property(branch => branch.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(branch => branch.Code)
            .IsUnique();

        builder.Property(branch => branch.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(branch => branch.Address)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(branch => branch.PhoneNumber)
            .HasMaxLength(30);

        builder.Property(branch => branch.IsActive)
            .IsRequired();

        builder.Property(branch => branch.CreatedBy)
            .HasMaxLength(100);

        builder.Property(branch => branch.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(branch => branch.DeletedBy)
            .HasMaxLength(100);
    }
}