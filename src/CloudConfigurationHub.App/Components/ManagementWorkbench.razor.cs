using System.Globalization;
using CloudConfigurationHub.Application.Projects;
using Mediator;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.WebUtilities;

namespace CloudConfigurationHub.App.Components;

public partial class ManagementWorkbench {
    [Inject]
    internal ISender Sender { get; set; } = null!;

    [Inject]
    internal NavigationManager NavigationManager { get; set; } = null!;

    [Parameter]
    public string InitialView { get; set; } = "projects";

    [Parameter]
    public Guid? ProjectId { get; set; }

    [Parameter]
    public Guid? EnvironmentId { get; set; }

    [Parameter]
    public Guid? ConfigurationId { get; set; }

    [Parameter]
    public string? RoutePanel { get; set; }

    internal IReadOnlyList<ProjectCard> projects = [];
    internal readonly Dictionary<Guid, ProjectDetail> details = [];
    internal bool isLoading = true;
    internal string projectQuery = string.Empty;
    internal string configQuery = string.Empty;
    internal string projectPickerQuery = string.Empty;
    internal bool projectPickerOpen;
    internal Guid? selectedProjectId;
    internal Guid? selectedEnvironmentId;
    internal Guid? selectedConfigurationId;
    internal string configMode = "config";
    internal bool projectModalOpen;
    internal bool environmentModalOpen;
    internal bool configModalOpen;
    internal bool publishPanelOpen;
    internal bool releaseHistoryPanelOpen;
    internal bool accessKeyPanelOpen;
    internal bool configValueSheetOpen;
    internal Guid? editingProjectId;
    internal Guid? editingEnvironmentId;
    internal Guid? editingConfigId;
    internal Guid? deleteProjectTarget;
    internal Guid? deleteConfigTarget;
    internal string formError = string.Empty;
    internal string panelError = string.Empty;
    internal string panelStatus = string.Empty;
    internal bool isPublishing;
    internal bool isRollingBack;
    internal bool isRotatingAccessKey;
    internal Guid? rollingBackReleaseId;
    internal string? generatedAccessKey;
    internal ProjectFormModel projectForm = new();
    internal EnvironmentFormModel environmentForm = new();
    internal ConfigFormModel configForm = new();
    internal PublishFormModel publishForm = new();

    internal ProjectDetail? SelectedProject =>
        selectedProjectId is { } id && details.TryGetValue(id, out var detail) ? detail : null;

    internal EnvironmentSummary? CurrentEnvironment =>
        SelectedProject?.Environments.FirstOrDefault(item => item.Id == selectedEnvironmentId);

    internal ConfigurationReleaseSummary? LatestRelease =>
        SelectedProject?.Releases
            .Where(item => item.EnvironmentId == selectedEnvironmentId)
            .OrderByDescending(item => item.Version)
            .FirstOrDefault();

    internal IReadOnlyList<ConfigurationReleaseSummary> ReleaseRows =>
        SelectedProject?.Releases
            .Where(item => item.EnvironmentId == selectedEnvironmentId)
            .OrderByDescending(item => item.Version)
            .ToArray() ?? [];

    internal IReadOnlyList<DiffPreviewRow> DiffRows =>
        SelectedProject is null || selectedEnvironmentId is null ? [] : BuildDiffRows(SelectedProject, selectedEnvironmentId.Value);

    internal string ProjectModalTitle => editingProjectId is null ? L("新建项目", "New project") : L("编辑项目", "Edit project");
    internal string EnvironmentModalTitle => editingEnvironmentId is null ? L("新增环境", "New environment") : L("编辑环境", "Edit environment");
    internal string ConfigModalTitle => editingConfigId is null ? L("新建配置项", "New config") : L("编辑配置项", "Edit config");
    internal string ProjectPickerText =>
        projectPickerOpen ? projectPickerQuery : projects.FirstOrDefault(item => item.Id == selectedProjectId)?.Name ?? string.Empty;

    protected override async Task OnInitializedAsync() {
        await ReloadAsync();
        ApplyRouteState();
        isLoading = false;
    }

    internal async Task ReloadAsync() {
        var list = await Sender.Send(new ListProjectsQuery(), CancellationToken.None);
        projects = list.Projects;
        details.Clear();
        foreach (var project in projects) {
            var detail = await Sender.Send(new GetProjectDetailQuery(project.Id), CancellationToken.None);
            if (detail is not null) {
                details[project.Id] = detail;
            }
        }

        selectedProjectId = ProjectId is not null && details.ContainsKey(ProjectId.Value)
            ? ProjectId
            : selectedProjectId ?? projects.FirstOrDefault()?.Id;
        projectPickerQuery = projects.FirstOrDefault(item => item.Id == selectedProjectId)?.Name ?? string.Empty;
        if (SelectedProject is { } selected) {
            selectedEnvironmentId = selected.Environments.Any(item => item.Id == selectedEnvironmentId)
                ? selectedEnvironmentId
                : selected.Environments.FirstOrDefault()?.Id;
            selectedConfigurationId = selected.Configurations.Any(item => item.Id == selectedConfigurationId)
                ? selectedConfigurationId
                : selected.Configurations.FirstOrDefault()?.Id;
        }
    }

    internal IReadOnlyList<ProjectCard> FilteredProjects() {
        var query = projectQuery.Trim();
        if (query.Length == 0) {
            return projects;
        }

        return projects
            .Where(project => project.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
                || project.Key.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    internal IReadOnlyList<ProjectCard> FilteredProjectOptions() {
        var query = projectPickerQuery.Trim();
        if (query.Length == 0) {
            return projects;
        }

        return projects
            .Where(project => project.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
                || project.Key.Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    internal IReadOnlyList<ConfigurationDetail> FilteredConfigs(ProjectDetail project) {
        var query = configQuery.Trim();
        if (query.Length == 0) {
            return project.Configurations;
        }

        return project.Configurations
            .Where(config => ConfigDisplayKey(config).Contains(query, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    internal void GoConfig(Guid projectId) {
        selectedProjectId = projectId;
        InitialView = "configs";
        selectedEnvironmentId = SelectedProject?.Environments.FirstOrDefault()?.Id;
        selectedConfigurationId = SelectedProject?.Configurations.FirstOrDefault()?.Id;
        NavigationManager.NavigateTo($"/projects/{projectId}");
    }

    internal void ApplyRouteState() {
        if (ProjectId is { } projectId && details.ContainsKey(projectId)) {
            selectedProjectId = projectId;
            InitialView = "configs";
        }

        if (EnvironmentId is { } environmentId) {
            selectedEnvironmentId = environmentId;
            configMode = "env";
        }

        if (ConfigurationId is { } configurationId) {
            selectedConfigurationId = configurationId;
            configMode = "config";
        }

        switch (RoutePanel) {
            case "project-new":
                OpenNewProject();
                break;
            case "environment-new":
                OpenNewEnvironment();
                break;
            case "configuration-new":
                OpenNewConfig();
                break;
            case "publish":
                OpenPublishPanel();
                break;
            case "releases":
                OpenReleaseHistoryPanel();
                break;
            case "access-key":
                OpenAccessKeyPanel();
                break;
        }
    }

    internal void OnProjectChanged(ChangeEventArgs args) {
        if (Guid.TryParse(args.Value?.ToString(), out var id)) {
            selectedProjectId = id;
            selectedEnvironmentId = SelectedProject?.Environments.FirstOrDefault()?.Id;
            selectedConfigurationId = SelectedProject?.Configurations.FirstOrDefault()?.Id;
        }
    }

    internal void OpenProjectPicker() {
        projectPickerOpen = true;
        projectPickerQuery = string.Empty;
    }

    internal void OnProjectPickerInput(ChangeEventArgs args) {
        projectPickerOpen = true;
        projectPickerQuery = args.Value?.ToString() ?? string.Empty;
    }

    internal async Task CloseProjectPickerSoon() {
        await Task.Delay(120);
        projectPickerOpen = false;
    }

    internal void PickProject(Guid projectId) {
        selectedProjectId = projectId;
        projectPickerOpen = false;
        projectPickerQuery = projects.FirstOrDefault(item => item.Id == projectId)?.Name ?? string.Empty;
        selectedEnvironmentId = SelectedProject?.Environments.FirstOrDefault()?.Id;
        selectedConfigurationId = SelectedProject?.Configurations.FirstOrDefault()?.Id;
    }

    internal void OpenNewProject() {
        editingProjectId = null;
        projectForm = new ProjectFormModel { EnvironmentNames = "dev,test,prod" };
        formError = string.Empty;
        projectModalOpen = true;
    }

    internal void OpenEditProject(ProjectCard project) {
        editingProjectId = project.Id;
        projectForm = new ProjectFormModel {
            Name = project.Name,
            Key = project.Key,
            Description = project.Description
        };
        formError = string.Empty;
        projectModalOpen = true;
    }

    internal async Task SaveProjectAsync() {
        if (string.IsNullOrWhiteSpace(projectForm.Name)) {
            formError = L("请输入名称", "Enter a name");
            return;
        }

        var key = string.IsNullOrWhiteSpace(projectForm.Key) ? Slug(projectForm.Name) : projectForm.Key;
        if (editingProjectId is { } projectId) {
            await Sender.Send(new UpdateProjectCommand(projectId, projectForm.Name.Trim(), key, projectForm.Description.Trim()), CancellationToken.None);
        }
        else {
            var created = await Sender.Send(new CreateProjectCommand(projectForm.Name.Trim(), key, projectForm.Description.Trim()), CancellationToken.None);
            foreach (var name in SplitNames(projectForm.EnvironmentNames)) {
                await Sender.Send(new AddEnvironmentCommand(created.Id, name, Slug(name)), CancellationToken.None);
            }
        }

        CloseProjectModal();
        await ReloadAsync();
    }

    internal void OpenNewConfig() {
        editingConfigId = null;
        configForm = new ConfigFormModel();
        formError = string.Empty;
        configModalOpen = true;
    }

    internal void OpenNewEnvironment() {
        editingEnvironmentId = null;
        environmentForm = new EnvironmentFormModel();
        formError = string.Empty;
        environmentModalOpen = true;
    }

    internal void OpenEditEnvironment(EnvironmentSummary environment) {
        selectedEnvironmentId = environment.Id;
        editingEnvironmentId = environment.Id;
        environmentForm = new EnvironmentFormModel {
            Name = environment.Name,
            Key = environment.Key
        };
        formError = string.Empty;
        environmentModalOpen = true;
    }

    internal async Task SaveEnvironmentAsync() {
        if (selectedProjectId is null || string.IsNullOrWhiteSpace(environmentForm.Name)) {
            formError = L("请输入名称", "Enter a name");
            return;
        }

        var key = string.IsNullOrWhiteSpace(environmentForm.Key) ? Slug(environmentForm.Name) : environmentForm.Key.Trim();
        var environment = editingEnvironmentId is { } environmentId
            ? await Sender.Send(
                new UpdateEnvironmentCommand(selectedProjectId.Value, environmentId, environmentForm.Name.Trim(), key),
                CancellationToken.None)
            : await Sender.Send(
                new AddEnvironmentCommand(selectedProjectId.Value, environmentForm.Name.Trim(), key),
                CancellationToken.None);
        selectedEnvironmentId = environment.Id;
        CloseEnvironmentModal();
        await ReloadAsync();
    }

    internal void OpenEditConfig(ConfigurationDetail config) {
        editingConfigId = config.Id;
        configForm = new ConfigFormModel {
            Key = ConfigDisplayKey(config),
            Description = config.Description,
            IsSensitive = config.IsSensitive,
            Values = config.Values.ToDictionary(item => item.EnvironmentId, item => item.DisplayValue)
        };
        formError = string.Empty;
        configModalOpen = true;
    }

    internal void OpenConfigValueSheet(ConfigurationDetail config) {
        selectedConfigurationId = config.Id;
        configValueSheetOpen = true;
    }

    internal void CloseConfigValueSheet() {
        configValueSheetOpen = false;
    }

    internal async Task SaveConfigAsync() {
        if (selectedProjectId is null || string.IsNullOrWhiteSpace(configForm.Key)) {
            formError = L("请输入配置键", "Enter a config key");
            return;
        }

        var (group, key) = SplitConfigKey(configForm.Key);
        if (editingConfigId is { } configId) {
            await Sender.Send(new UpdateConfigurationCommand(
                selectedProjectId.Value,
                configId,
                group,
                key,
                configForm.IsSensitive,
                configForm.Description.Trim(),
                configForm.Values),
                CancellationToken.None);
        }
        else {
            var created = await Sender.Send(new AddConfigurationCommand(
                selectedProjectId.Value,
                group,
                key,
                configForm.IsSensitive,
                configForm.Description.Trim()),
                CancellationToken.None);
            foreach (var (environmentId, value) in configForm.Values) {
                await Sender.Send(new SetDraftValueCommand(selectedProjectId.Value, environmentId, created.Id, value), CancellationToken.None);
            }
        }

        CloseConfigModal();
        await ReloadAsync();
    }

    internal async Task SaveDraftValueAsync(ProjectDetail project, ConfigurationDetail config, Guid environmentId, string value) {
        await Sender.Send(new SetDraftValueCommand(project.Id, environmentId, config.Id, value), CancellationToken.None);
        await ReloadAsync();
    }

    internal void OpenPublishPanel() {
        if (selectedEnvironmentId is null) {
            return;
        }

        panelError = string.Empty;
        panelStatus = string.Empty;
        publishForm = new PublishFormModel();
        publishPanelOpen = true;
    }

    internal async Task PublishEnvironmentAsync() {
        if (selectedProjectId is null || selectedEnvironmentId is null) {
            return;
        }

        isPublishing = true;
        panelError = string.Empty;
        try {
            await Sender.Send(
                new PublishEnvironmentCommand(selectedProjectId.Value, selectedEnvironmentId.Value, publishForm.Note, publishForm.PublishedBy),
                CancellationToken.None);
            ClosePublishPanel();
            await ReloadAsync();
        }
        catch (Exception exception) {
            panelError = exception.Message;
        }
        finally {
            isPublishing = false;
        }
    }

    internal void OpenReleaseHistoryPanel() {
        if (selectedEnvironmentId is null) {
            return;
        }

        panelError = string.Empty;
        panelStatus = string.Empty;
        releaseHistoryPanelOpen = true;
    }

    internal async Task RollbackEnvironmentAsync(ConfigurationReleaseSummary release) {
        if (selectedProjectId is null || selectedEnvironmentId is null) {
            return;
        }

        isRollingBack = true;
        rollingBackReleaseId = release.Id;
        panelError = string.Empty;
        panelStatus = string.Empty;
        try {
            var result = await Sender.Send(
                new RollbackEnvironmentCommand(
                    selectedProjectId.Value,
                    selectedEnvironmentId.Value,
                    release.Id,
                    string.Format(L("回滚到 v{0}", "Rollback to v{0}"), release.Version),
                    "admin"),
                CancellationToken.None);
            panelStatus = string.Format(L("已创建回滚版本 v{0}", "Created rollback version v{0}"), result.Version);
            await ReloadAsync();
        }
        catch (Exception exception) {
            panelError = exception.Message;
        }
        finally {
            isRollingBack = false;
            rollingBackReleaseId = null;
        }
    }

    internal void OpenAccessKeyPanel(Guid? projectId = null) {
        if (projectId is { } id) {
            selectedProjectId = id;
        }

        generatedAccessKey = null;
        panelError = string.Empty;
        accessKeyPanelOpen = true;
    }

    internal async Task RotateAccessKeyAsync() {
        if (selectedProjectId is null) {
            return;
        }

        isRotatingAccessKey = true;
        generatedAccessKey = null;
        panelError = string.Empty;
        try {
            var result = await Sender.Send(new RotateProjectAccessKeyCommand(selectedProjectId.Value, "admin"), CancellationToken.None);
            generatedAccessKey = result.AccessKey;
        }
        catch (Exception exception) {
            panelError = exception.Message;
        }
        finally {
            isRotatingAccessKey = false;
        }
    }

    internal async Task ConfirmDeleteAsync() {
        if (deleteProjectTarget is { } projectId) {
            await Sender.Send(new DeleteProjectCommand(projectId), CancellationToken.None);
            selectedProjectId = null;
        }
        else if (deleteConfigTarget is { } configId && selectedProjectId is { } projectIdForConfig) {
            await Sender.Send(new DeleteConfigurationCommand(projectIdForConfig, configId), CancellationToken.None);
            selectedConfigurationId = null;
        }

        CloseConfirm();
        await ReloadAsync();
    }

    internal string CurrentEnvironmentName(ProjectDetail project) =>
        project.Environments.FirstOrDefault(item => item.Id == selectedEnvironmentId)?.Name ?? string.Empty;

    internal string CurrentValue(ConfigurationDetail config) =>
        selectedEnvironmentId is { } envId
            ? ValueForEnvironment(config, envId)?.DisplayValue ?? string.Empty
            : string.Empty;

    internal static EnvironmentDraftValue? ValueForEnvironment(ConfigurationDetail config, Guid environmentId) =>
        config.Values.SingleOrDefault(item => item.EnvironmentId == environmentId);

    internal RenderFragment PublicationStatusBadge(EnvironmentDraftValue? value) => builder => {
        var state = value?.PublicationState ?? ConfigurationValuePublicationState.NotSet;
        builder.OpenElement(0, "span");
        builder.AddAttribute(1, "class", $"cch-publication-pill {PublicationStatusCssClass(state)}");
        builder.AddAttribute(2, "title", PublicationStatusTitle(value));
        builder.AddContent(3, PublicationStatusText(state));
        builder.CloseElement();
    };

    internal string PublicationStatusText(ConfigurationValuePublicationState state) =>
        state switch {
            ConfigurationValuePublicationState.Published => L("已发布", "Published"),
            ConfigurationValuePublicationState.PendingPublish => L("待发布", "Pending"),
            ConfigurationValuePublicationState.NotPublished => L("未发布", "Unpublished"),
            ConfigurationValuePublicationState.PendingRemoval => L("待移除", "Remove"),
            _ => L("未设置", "Not set")
        };

    internal static string PublicationStatusCssClass(ConfigurationValuePublicationState state) =>
        $"publication-{state.ToString().ToLowerInvariant()}";

    internal string PublicationStatusTitle(EnvironmentDraftValue? value) {
        var state = value?.PublicationState ?? ConfigurationValuePublicationState.NotSet;
        if (value?.LatestPublishedVersion is not { } version || value.LatestPublishedAt is not { } publishedAt) {
            return PublicationStatusText(state);
        }

        return string.Format(
            L("最新发布 v{0} · {1}", "Latest release v{0} · {1}"),
            version,
            FormatDateTime(publishedAt));
    }

    internal string GetConfigFormValue(Guid environmentId) =>
        configForm.Values.TryGetValue(environmentId, out var value) ? value : string.Empty;

    internal void SetConfigFormValue(Guid environmentId, string value) {
        configForm.Values[environmentId] = value;
    }

    internal static string ConfigDisplayKey(ConfigurationDetail config) =>
        string.IsNullOrWhiteSpace(config.Group) || config.Group == "default" ? config.Key : $"{config.Group}.{config.Key}";

    internal static (string Group, string Key) SplitConfigKey(string value) {
        var normalized = value.Trim();
        var index = normalized.LastIndexOf('.');
        return index > 0
            ? (normalized[..index], normalized[(index + 1)..])
            : ("default", normalized);
    }

    internal static string Slug(string value) =>
        string.Join('-', value.Trim().ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries));

    internal static IReadOnlyList<string> SplitNames(string value) =>
        value.Split([',', '，', ';', '；'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    internal string FormatDate(DateTimeOffset value) {
        if (value == default) {
            return "—";
        }

        var local = value.ToLocalTime();
        return CurrentLanguage switch {
            "en" => local.ToString("MMM d, yyyy", CultureInfo.CurrentUICulture),
            "ja" => local.ToString("yyyy年M月d日", CultureInfo.CurrentUICulture),
            "ko" => local.ToString("yyyy. M. d.", CultureInfo.CurrentUICulture),
            "es" => local.ToString("d MMM yyyy", CultureInfo.CurrentUICulture),
            _ => local.ToString("yyyy年M月d日")
        };
    }

    internal string FormatDateTime(DateTimeOffset value) {
        if (value == default) {
            return "—";
        }

        return value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentUICulture);
    }

    internal IReadOnlyList<DiffPreviewRow> BuildDiffRows(ProjectDetail project, Guid environmentId) {
        var latestRelease = project.Releases
            .Where(item => item.EnvironmentId == environmentId)
            .OrderByDescending(item => item.Version)
            .FirstOrDefault();
        return project.Configurations
            .Select(configuration => {
                var configurationKey = $"{configuration.Group}:{configuration.Key}";
                var draftValue = configuration.Values.SingleOrDefault(item => item.EnvironmentId == environmentId);
                var latestReleaseValue = latestRelease?.Values.SingleOrDefault(item => item.ConfigurationKey == configurationKey);
                var status = ResolveDiffStatus(draftValue, latestReleaseValue);
                return new DiffPreviewRow {
                    ConfigurationKey = configurationKey,
                    HasDraftValue = draftValue?.HasValue == true,
                    DraftValue = draftValue?.DisplayValue ?? string.Empty,
                    HasLatestReleaseValue = latestReleaseValue is not null,
                    LatestReleaseValue = latestReleaseValue?.DisplayValue ?? string.Empty,
                    StatusText = DiffStatusText(status),
                    StatusCssClass = $"diff-{status.ToLowerInvariant()}"
                };
            })
            .ToArray();
    }

    internal string DiffStatusText(string status) =>
        status switch {
            "Added" => L("新增", "Added"),
            "Modified" => L("修改", "Modified"),
            "Removed" => L("删除", "Removed"),
            _ => L("未变", "Unchanged")
        };

    internal static string ResolveDiffStatus(
        EnvironmentDraftValue? draftValue,
        ConfigurationReleaseValueSummary? latestReleaseValue) {
        var hasDraft = draftValue?.HasValue == true;
        var hasLatest = latestReleaseValue is not null;
        if (hasDraft && !hasLatest) {
            return "Added";
        }

        if (!hasDraft && hasLatest) {
            return "Removed";
        }

        if (!hasDraft && !hasLatest) {
            return "Unchanged";
        }

        return string.Equals(draftValue!.DisplayValue, latestReleaseValue!.DisplayValue, StringComparison.Ordinal)
            ? "Unchanged"
            : "Modified";
    }

    internal string CurrentLanguage => ResolveLanguage();

    internal string ResolveLanguage() {
        var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
        var query = QueryHelpers.ParseQuery(uri.Query);
        var culture = query.TryGetValue("ui-culture", out var uiCulture)
            ? uiCulture.ToString()
            : query.TryGetValue("culture", out var routeCulture)
                ? routeCulture.ToString()
                : CultureInfo.CurrentUICulture.Name;
        return culture.Split('-', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.ToLowerInvariant()
            ?? CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
    }

    internal string L(string zh, string en) =>
        ManagementWorkbenchText.Translate(CurrentLanguage, zh, en);

    internal void RequestRender() {
        _ = InvokeAsync(StateHasChanged);
    }

    internal static string DotStyle(string key) {
        var colors = new[] { "#22c55e", "#3b82f6", "#f59e0b", "#a855f7", "#ef4444" };
        var index = Math.Abs(key.GetHashCode()) % colors.Length;
        return $"background-color:{colors[index]}";
    }

    internal void CloseProjectModal() => projectModalOpen = false;
    internal void CloseEnvironmentModal() {
        environmentModalOpen = false;
        editingEnvironmentId = null;
    }
    internal void CloseConfigModal() => configModalOpen = false;
    internal void ClosePublishPanel() => publishPanelOpen = false;
    internal void CloseReleaseHistoryPanel() => releaseHistoryPanelOpen = false;
    internal void CloseAccessKeyPanel() => accessKeyPanelOpen = false;
    internal void CloseConfirm() {
        deleteProjectTarget = null;
        deleteConfigTarget = null;
    }
}
