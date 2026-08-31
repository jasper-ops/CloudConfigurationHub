using CloudConfigurationHub.Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CloudConfigurationHub.Infrastructure.Persistence.Configurations;

internal sealed class ConfigReleaseConfiguration : IEntityTypeConfiguration<ConfigRelease> {
    public void Configure(EntityTypeBuilder<ConfigRelease> builder) {
        builder.ToTable("ConfigReleases");
        builder.HasKey(release => release.Id);
        builder.Property<Guid>("ProjectId");
        builder.Property(release => release.Id).ValueGeneratedNever();
        builder.Property(release => release.Version).IsRequired();
        builder.Property(release => release.Note).HasMaxLength(500).IsRequired();
        builder.Property(release => release.PublishedBy).HasMaxLength(200).IsRequired();
        builder.Property(release => release.PublishedAt).IsRequired();
        builder.HasIndex("ProjectId", nameof(ConfigRelease.EnvironmentId), nameof(ConfigRelease.Version)).IsUnique();
        builder.HasMany(release => release.Values)
            .WithOne()
            .HasForeignKey("ReleaseId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(release => release.Values).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
