using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared.Mind;
using Robust.Shared.Network;

namespace Content.Shared.SS220.RoundEndInfo;

/// <summary>
/// Shared realization server manager
/// On a client all method's must return null or do nothing.
/// </summary>
public interface IRoundEndInfoManager
{
    /// <summary>
    /// Ensures that an instance of the specified IRoundEndInfo type exists and returns it.
    /// If none exists, a new one is created, initialized if applicable, and stored.
    /// </summary>
    /// <typeparam name="T">The type implementing IRoundEndInfo.</typeparam>
    /// <returns>An instance of the requested IRoundEndInfo type.</returns>
    T EnsureInfo<T>() where T : class, IRoundEndInfo, new();

    bool TryGetInfo<T>([NotNullWhen(true)] out T? info) where T : IRoundEndInfo;

    /// <summary>
    /// Clears all stored IRoundEndInfo instances from the manager.
    /// </summary>
    void ClearAllData();

    /// <summary>
    /// Returns an enumeration of all currently stored IRoundEndInfo instances.
    /// </summary>
    IEnumerable<IRoundEndInfo> GetAllInfos();
}

public static class RoundEndInfoUtils
{
    public static string GetMindName(IEntityManager entMan, EntityUid? uid)
    {
        return entMan.TryGetComponent(uid, out MindComponent? mind)
            ? mind.CharacterName ?? Loc.GetString("game-ticker-unknown-role")
            : Loc.GetString("game-ticker-unknown-role");
    }

    public static (EntityUid?, int) GetTopBy<TData>(
        Dictionary<EntityUid, TData> dict,
        Func<TData, int> selector)
    {
        if (dict.Count == 0)
            return (null, 0);

        var topPair = dict.MaxBy(kvp => selector(kvp.Value));
        return (topPair.Key, selector(topPair.Value));
    }
}
