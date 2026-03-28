using System.IO;
using UnityEngine;

/// <summary>
/// 撮影出力・入力ファイルのパス解決。空欄時は従来どおり プロジェクトルート/Output を使用。
/// </summary>
public static class CapturePathUtility
{
    public static string ProjectRoot => Directory.GetParent(Application.dataPath).FullName;

    /// <summary>
    /// 出力のルートディレクトリ。空欄 → プロジェクト直下の Output。相対パスはプロジェクトルート基準。絶対パス可。
    /// </summary>
    public static string ResolveOutputRoot(string configuredRoot)
    {
        if (string.IsNullOrWhiteSpace(configuredRoot))
            return Path.Combine(ProjectRoot, "Output");

        string t = configuredRoot.Trim();
        if (Path.IsPathRooted(t))
            return Path.GetFullPath(t);

        return Path.GetFullPath(Path.Combine(ProjectRoot, t));
    }

    /// <summary>
    /// ファイルパス。相対はプロジェクトルート基準。絶対パス可。空欄は null。
    /// </summary>
    public static string ResolveFilePath(string configuredPath)
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
            return null;

        string t = configuredPath.Trim();
        if (Path.IsPathRooted(t))
            return Path.GetFullPath(t);

        return Path.GetFullPath(Path.Combine(ProjectRoot, t));
    }
}
