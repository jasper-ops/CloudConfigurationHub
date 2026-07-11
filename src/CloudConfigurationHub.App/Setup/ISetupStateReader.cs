namespace CloudConfigurationHub.App.Setup;

/// <summary>
/// 读取系统首次启动配置状态，用于判断是否已经存在初始管理员。
/// </summary>
public interface ISetupStateReader {
    /// <summary>
    /// 判断启动向导是否已经完成。
    /// </summary>
    /// <param name="cancellationToken">取消异步操作的令牌。</param>
    /// <returns>存在至少一个管理员用户时返回 <c>true</c>。</returns>
    Task<bool> IsSetupCompletedAsync(CancellationToken cancellationToken);
}
