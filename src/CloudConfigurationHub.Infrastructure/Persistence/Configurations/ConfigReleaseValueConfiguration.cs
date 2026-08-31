using CloudConfigurationHub.Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CloudConfigurationHub.Infrastructure.Persistence.Configurations;

internal sealed class ConfigReleaseValueConfiguration : IEntityTypeConfiguration<ConfigReleaseValue> {
    public void Configure(EntityTypeBuilder<ConfigReleaseValue> builder) {
        builder.ToTable("ConfigReleaseValues");
        builder.Property<Guid>("Id");
        builder.Property<Guid>("ReleaseId");
        builder.HasKey("Id");
        builder.Property(value => value.ConfigurationKey).HasMaxLength(400).IsRequired();
        builder.Property(value => value.Value).IsRequired();
        builder.Property(value => value.IsSensitive).IsRequired();
    }
}
