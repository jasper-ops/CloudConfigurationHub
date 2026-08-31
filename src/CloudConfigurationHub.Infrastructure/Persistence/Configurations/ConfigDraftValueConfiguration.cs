using CloudConfigurationHub.Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CloudConfigurationHub.Infrastructure.Persistence.Configurations;

internal sealed class ConfigDraftValueConfiguration : IEntityTypeConfiguration<ConfigDraftValue> {
    public void Configure(EntityTypeBuilder<ConfigDraftValue> builder) {
        builder.ToTable("ConfigDraftValues");
        builder.Property<Guid>("ProjectId");
        builder.HasKey(
            "ProjectId",
            nameof(ConfigDraftValue.EnvironmentId),
            nameof(ConfigDraftValue.ConfigurationId));
        builder.Property(draftValue => draftValue.Value).IsRequired();
    }
}
