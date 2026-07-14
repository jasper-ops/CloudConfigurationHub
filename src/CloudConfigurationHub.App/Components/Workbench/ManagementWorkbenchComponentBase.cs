using CloudConfigurationHub.App.Components;
using CloudConfigurationHub.Application.Projects;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace CloudConfigurationHub.App.Components.Workbench;

public abstract class ManagementWorkbenchComponentBase : ComponentBase {
    [CascadingParameter]
    internal ManagementWorkbench Workbench { get; set; } = null!;

    internal IReadOnlyList<ProjectCard> projects => Workbench.projects;
    internal string projectQuery { get => Workbench.projectQuery; set => Workbench.projectQuery = value; }
    internal string configQuery { get => Workbench.configQuery; set => Workbench.configQuery = value; }
    internal string configMode { get => Workbench.configMode; set => Workbench.configMode = value; }
    internal bool projectPickerOpen { get => Workbench.projectPickerOpen; set => Workbench.projectPickerOpen = value; }
    internal Guid? selectedProjectId { get => Workbench.selectedProjectId; set => Workbench.selectedProjectId = value; }
    internal Guid? selectedEnvironmentId { get => Workbench.selectedEnvironmentId; set => Workbench.selectedEnvironmentId = value; }
    internal Guid? selectedConfigurationId { get => Workbench.selectedConfigurationId; set => Workbench.selectedConfigurationId = value; }
    internal bool projectModalOpen { get => Workbench.projectModalOpen; set => Workbench.projectModalOpen = value; }
    internal bool environmentModalOpen { get => Workbench.environmentModalOpen; set => Workbench.environmentModalOpen = value; }
    internal bool configModalOpen { get => Workbench.configModalOpen; set => Workbench.configModalOpen = value; }
    internal bool publishPanelOpen { get => Workbench.publishPanelOpen; set => Workbench.publishPanelOpen = value; }
    internal bool releaseHistoryPanelOpen { get => Workbench.releaseHistoryPanelOpen; set => Workbench.releaseHistoryPanelOpen = value; }
    internal bool accessKeyPanelOpen { get => Workbench.accessKeyPanelOpen; set => Workbench.accessKeyPanelOpen = value; }
    internal bool configValueSheetOpen { get => Workbench.configValueSheetOpen; set => Workbench.configValueSheetOpen = value; }
    internal Guid? editingProjectId => Workbench.editingProjectId;
    internal Guid? deleteProjectTarget { get => Workbench.deleteProjectTarget; set { Workbench.deleteProjectTarget = value; Workbench.RequestRender(); } }
    internal Guid? deleteConfigTarget { get => Workbench.deleteConfigTarget; set { Workbench.deleteConfigTarget = value; Workbench.RequestRender(); } }
    internal string formError => Workbench.formError;
    internal string panelError => Workbench.panelError;
    internal string panelStatus => Workbench.panelStatus;
    internal bool isPublishing => Workbench.isPublishing;
    internal bool isRollingBack => Workbench.isRollingBack;
    internal bool isRotatingAccessKey => Workbench.isRotatingAccessKey;
    internal Guid? rollingBackReleaseId => Workbench.rollingBackReleaseId;
    internal string? generatedAccessKey => Workbench.generatedAccessKey;
    internal ProjectFormModel projectForm { get => Workbench.projectForm; set => Workbench.projectForm = value; }
    internal EnvironmentFormModel environmentForm { get => Workbench.environmentForm; set => Workbench.environmentForm = value; }
    internal ConfigFormModel configForm { get => Workbench.configForm; set => Workbench.configForm = value; }
    internal PublishFormModel publishForm { get => Workbench.publishForm; set => Workbench.publishForm = value; }

    internal ProjectDetail? SelectedProject => Workbench.SelectedProject;
    internal EnvironmentSummary? CurrentEnvironment => Workbench.CurrentEnvironment;
    internal ConfigurationReleaseSummary? LatestRelease => Workbench.LatestRelease;
    internal IReadOnlyList<ConfigurationReleaseSummary> ReleaseRows => Workbench.ReleaseRows;
    internal IReadOnlyList<DiffPreviewRow> DiffRows => Workbench.DiffRows;
    internal string ProjectModalTitle => Workbench.ProjectModalTitle;
    internal string EnvironmentModalTitle => Workbench.EnvironmentModalTitle;
    internal string ConfigModalTitle => Workbench.ConfigModalTitle;
    internal string ProjectPickerText => Workbench.ProjectPickerText;

    internal IReadOnlyList<ProjectCard> FilteredProjects() => Workbench.FilteredProjects();
    internal IReadOnlyList<ProjectCard> FilteredProjectOptions() => Workbench.FilteredProjectOptions();
    internal IReadOnlyList<ConfigurationDetail> FilteredConfigs(ProjectDetail project) => Workbench.FilteredConfigs(project);
    internal void GoConfig(Guid projectId) => Workbench.GoConfig(projectId);
    internal void OpenProjectPicker() {
        Workbench.OpenProjectPicker();
        Workbench.RequestRender();
    }
    internal void OnProjectPickerInput(ChangeEventArgs args) => Workbench.OnProjectPickerInput(args);
    internal async Task CloseProjectPickerSoon() {
        await Workbench.CloseProjectPickerSoon();
        Workbench.RequestRender();
    }
    internal void PickProject(Guid projectId) {
        Workbench.PickProject(projectId);
        Workbench.RequestRender();
    }
    internal void OpenNewProject() {
        Workbench.OpenNewProject();
        Workbench.RequestRender();
    }
    internal void OpenEditProject(ProjectCard project) {
        Workbench.OpenEditProject(project);
        Workbench.RequestRender();
    }
    internal async Task SaveProjectAsync() {
        await Workbench.SaveProjectAsync();
        Workbench.RequestRender();
    }
    internal void OpenNewConfig() {
        Workbench.OpenNewConfig();
        Workbench.RequestRender();
    }
    internal void OpenNewEnvironment() {
        Workbench.OpenNewEnvironment();
        Workbench.RequestRender();
    }
    internal void OpenEditEnvironment(EnvironmentSummary environment) {
        Workbench.OpenEditEnvironment(environment);
        Workbench.RequestRender();
    }
    internal async Task SaveEnvironmentAsync() {
        await Workbench.SaveEnvironmentAsync();
        Workbench.RequestRender();
    }
    internal void OpenEditConfig(ConfigurationDetail config) {
        Workbench.OpenEditConfig(config);
        Workbench.RequestRender();
    }
    internal void OpenConfigValueSheet(ConfigurationDetail config) {
        Workbench.OpenConfigValueSheet(config);
        Workbench.RequestRender();
    }
    internal void CloseConfigValueSheet() {
        Workbench.CloseConfigValueSheet();
        Workbench.RequestRender();
    }
    internal async Task SaveConfigAsync() {
        await Workbench.SaveConfigAsync();
        Workbench.RequestRender();
    }
    internal Task SaveDraftValueAsync(ProjectDetail project, ConfigurationDetail config, Guid environmentId, string value) =>
        Workbench.SaveDraftValueAsync(project, config, environmentId, value);
    internal void OpenPublishPanel() {
        Workbench.OpenPublishPanel();
        Workbench.RequestRender();
    }
    internal async Task PublishEnvironmentAsync() {
        await Workbench.PublishEnvironmentAsync();
        Workbench.RequestRender();
    }
    internal void OpenReleaseHistoryPanel() {
        Workbench.OpenReleaseHistoryPanel();
        Workbench.RequestRender();
    }
    internal async Task RollbackEnvironmentAsync(ConfigurationReleaseSummary release) {
        await Workbench.RollbackEnvironmentAsync(release);
        Workbench.RequestRender();
    }
    internal void OpenAccessKeyPanel(Guid? projectId = null) {
        Workbench.OpenAccessKeyPanel(projectId);
        Workbench.RequestRender();
    }
    internal async Task RotateAccessKeyAsync() {
        await Workbench.RotateAccessKeyAsync();
        Workbench.RequestRender();
    }
    internal async Task ConfirmDeleteAsync() {
        await Workbench.ConfirmDeleteAsync();
        Workbench.RequestRender();
    }
    internal string CurrentEnvironmentName(ProjectDetail project) => Workbench.CurrentEnvironmentName(project);
    internal string CurrentValue(ConfigurationDetail config) => Workbench.CurrentValue(config);
    internal RenderFragment PublicationStatusBadge(EnvironmentDraftValue? value) => Workbench.PublicationStatusBadge(value);
    internal string GetConfigFormValue(Guid environmentId) => Workbench.GetConfigFormValue(environmentId);
    internal void SetConfigFormValue(Guid environmentId, string value) => Workbench.SetConfigFormValue(environmentId, value);
    internal string ConfigDisplayKey(ConfigurationDetail config) => ManagementWorkbench.ConfigDisplayKey(config);
    internal string FormatDate(DateTimeOffset value) => Workbench.FormatDate(value);
    internal string FormatDateTime(DateTimeOffset value) => Workbench.FormatDateTime(value);
    internal string L(string zh, string en) => Workbench.L(zh, en);
    internal string DotStyle(string key) => ManagementWorkbench.DotStyle(key);
    internal void CloseProjectModal() {
        Workbench.CloseProjectModal();
        Workbench.RequestRender();
    }
    internal void CloseEnvironmentModal() {
        Workbench.CloseEnvironmentModal();
        Workbench.RequestRender();
    }
    internal void CloseConfigModal() {
        Workbench.CloseConfigModal();
        Workbench.RequestRender();
    }
    internal void ClosePublishPanel() {
        Workbench.ClosePublishPanel();
        Workbench.RequestRender();
    }
    internal void CloseReleaseHistoryPanel() {
        Workbench.CloseReleaseHistoryPanel();
        Workbench.RequestRender();
    }
    internal void CloseAccessKeyPanel() {
        Workbench.CloseAccessKeyPanel();
        Workbench.RequestRender();
    }
    internal void CloseConfirm() {
        Workbench.CloseConfirm();
        Workbench.RequestRender();
    }
    internal EnvironmentDraftValue? ValueForEnvironment(ConfigurationDetail config, Guid environmentId) =>
        ManagementWorkbench.ValueForEnvironment(config, environmentId);
}
