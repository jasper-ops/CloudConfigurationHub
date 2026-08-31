using CloudConfigurationHub.Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CloudConfigurationHub.Infrastructure.Persistence.Configurations;

internal sealed class ProjectConfiguration : IEntityTypeConfiguration<Project> {
    public void Configure(EntityTypeBuilder<Project> builder) {
        builder.ToTable("Projects");
        builder.HasKey(project => project.Id);
        builder.Property(project => project.Name).HasMaxLength(200).IsRequired();
        builder.Property(project => project.Key).HasMaxLength(100).IsRequired();
        builder.Property(project => project.Description).HasMaxLength(1000).IsRequired();
        builder.Property(project => project.CreatedAt).IsRequired();
        builder.Property(project => project.AccessKeyHash).HasMaxLength(128).IsRequired();
        builder.HasIndex(project => project.Key).IsUnique();
        builder.Ignore(project => project.DraftValues);

        builder.Navigation(project => project.Environments).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(project => project.Configurations).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(project => project.Releases).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(project => project.Environments)
            .WithOne()
            .HasForeignKey("ProjectId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(project => project.Configurations)
            .WithOne()
            .HasForeignKey("ProjectId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany<ConfigDraftValue>("_draftValues")
            .WithOne()
            .HasForeignKey("ProjectId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(project => project.Releases)
            .WithOne()
            .HasForeignKey("ProjectId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation("_draftValues").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
