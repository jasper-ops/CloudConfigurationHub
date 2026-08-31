using CloudConfigurationHub.Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CloudConfigurationHub.Infrastructure.Persistence.Configurations;

internal sealed class ConfigDefinitionConfiguration : IEntityTypeConfiguration<ConfigDefinition> {
    public void Configure(EntityTypeBuilder<ConfigDefinition> builder) {
        builder.ToTable("ConfigDefinitions");
        builder.HasKey("Id");
        builder.Property<Guid>("ProjectId");
        builder.Property(configuration => configuration.Id).ValueGeneratedNever();
        builder.Property(configuration => configuration.Group).HasMaxLength(100).IsRequired();
        builder.Property(configuration => configuration.Key).HasMaxLength(200).IsRequired();
        builder.Property(configuration => configuration.Description).HasMaxLength(1000).IsRequired();
        builder.Property(configuration => configuration.IsSensitive).IsRequired();
        builder.HasIndex("ProjectId", nameof(ConfigDefinition.Group), nameof(ConfigDefinition.Key)).IsUnique();
    }
}
