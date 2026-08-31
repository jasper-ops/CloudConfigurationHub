using CloudConfigurationHub.Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CloudConfigurationHub.Infrastructure.Persistence.Configurations;

internal sealed class ProjectEnvironmentConfiguration : IEntityTypeConfiguration<ProjectEnvironment> {
    public void Configure(EntityTypeBuilder<ProjectEnvironment> builder) {
        builder.ToTable("ProjectEnvironments");
        builder.HasKey("Id");
        builder.Property<Guid>("ProjectId");
        builder.Property(environment => environment.Id).ValueGeneratedNever();
        builder.Property(environment => environment.Name).HasMaxLength(200).IsRequired();
        builder.Property(environment => environment.Key).HasMaxLength(100).IsRequired();
        builder.HasIndex("ProjectId", nameof(ProjectEnvironment.Key)).IsUnique();
    }
}
