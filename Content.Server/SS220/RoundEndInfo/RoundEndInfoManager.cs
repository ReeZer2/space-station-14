using System.Diagnostics.CodeAnalysis;
using Content.Shared.SS220.RoundEndInfo;

namespace Content.Server.SS220.RoundEndInfo;

/// <summary>
/// Manages all IRoundEndInfo data used for compiling round-end summaries.
/// Responsible for creating, storing, retrieving, and clearing instances of various info providers.
/// </summary>
public sealed class RoundEndInfoManager : IRoundEndInfoManager
{
    private readonly Dictionary<Type, IRoundEndInfo> _infos = new();

    public T EnsureInfo<T>() where T : class, IRoundEndInfo, new()
    {
        if (_infos.TryGetValue(typeof(T), out var existing))
            return (T) existing;

        var instance = IoCManager.Resolve<IDynamicTypeFactory>().CreateInstance<T>();
        _infos.Add(typeof(T), instance);
        return instance;
    }

    public bool TryGetInfo<T>([NotNullWhen(true)] out T? info) where T : IRoundEndInfo
    {
        if (!_infos.TryGetValue(typeof(T), out var existing))
        {
            info = default;
            return false;
        }

        info = (T) existing;
        return true;
    }

    public void ClearAllData()
    {
        _infos.Clear();
    }

    public IEnumerable<IRoundEndInfo> GetAllInfos()
    {
        return _infos.Values;
    }
}
