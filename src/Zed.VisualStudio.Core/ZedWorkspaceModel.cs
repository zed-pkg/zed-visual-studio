using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Zed.VisualStudio.Core;

public sealed record ZedPackageNode(string Root, int Errors, int Warnings, IReadOnlyList<ZedIssue> Issues);
public sealed record ZedErrorListEntry(string WorkspaceRoot, string File, string Severity, string IssueId, string Message);
public sealed record ZedActionPreview(string Executable, IReadOnlyList<string> Arguments, string WorkingDirectory, bool RequiresConfirmation);
public sealed record ZedWorkspaceSnapshot(IReadOnlyList<ZedPackageNode> Packages, IReadOnlyList<ZedErrorListEntry> ErrorList);

public static class ZedWorkspaceModel {
    public static ZedWorkspaceSnapshot Project(IEnumerable<ZedReport>? reports) {
        var packages = new List<ZedPackageNode>();
        var entries = new List<ZedErrorListEntry>();
        foreach (var report in reports ?? Array.Empty<ZedReport>()) {
            var root = Path.GetFullPath(report.WorkspaceRoot);
            var issues = report.Issues ?? Array.Empty<ZedIssue>();
            packages.Add(new ZedPackageNode(
                root,
                issues.Count(issue => string.Equals(issue.Severity, "error", StringComparison.OrdinalIgnoreCase)),
                issues.Count(issue => string.Equals(issue.Severity, "warning", StringComparison.OrdinalIgnoreCase)),
                issues.ToArray()
            ));
            foreach (var issue in issues) {
                var files = issue.Files is { Count: > 0 } ? issue.Files : new[] { ".zpkg.toml" };
                foreach (var file in files) {
                    var absolute = Path.IsPathRooted(file) ? Path.GetFullPath(file) : Path.GetFullPath(Path.Combine(root, file));
                    entries.Add(new ZedErrorListEntry(root, absolute, issue.Severity, issue.Id, $"{issue.Title}: {issue.Detail}"));
                }
            }
        }
        return new ZedWorkspaceSnapshot(
            packages.OrderBy(item => item.Root, StringComparer.OrdinalIgnoreCase).ToArray(),
            entries.OrderBy(item => item.File, StringComparer.OrdinalIgnoreCase).ThenBy(item => item.IssueId, StringComparer.Ordinal).ToArray()
        );
    }

    public static ZedActionPreview Preview(ZedAction action, string workspaceRoot) {
        if (!string.Equals(action.Kind, "command", StringComparison.Ordinal)) throw new ArgumentException("Only command actions have an execution preview.", nameof(action));
        if (!action.RequiresConfirmation) throw new ArgumentException("Command actions must require explicit confirmation.", nameof(action));
        if (string.IsNullOrWhiteSpace(action.Command)) throw new ArgumentException("Command executable is required.", nameof(action));
        return new ZedActionPreview(action.Command, action.Arguments ?? Array.Empty<string>(), Path.GetFullPath(workspaceRoot), true);
    }
}
