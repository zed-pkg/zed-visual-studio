using System;
using System.Linq;
using Zed.VisualStudio.Core;
using Xunit;

namespace Zed.VisualStudio.Core.Tests;

public sealed class ZedWorkspaceModelTests {
    [Fact]
    public void ProjectsMultiRootToolWindowAndErrorListState() {
        var warning = new ZedIssue("lock.stale", "warning", "Stale lock", "lock is stale", new[] { ".zpkg.lock" }, Array.Empty<ZedAction>());
        var error = new ZedIssue("manifest.invalid", "error", "Invalid manifest", "bad toml", Array.Empty<string>(), Array.Empty<ZedAction>());
        var snapshot = ZedWorkspaceModel.Project(new[] {
            new ZedReport(1, "zeta", null, new[] { warning }),
            new ZedReport(1, "alpha", null, new[] { error })
        });
        Assert.Equal(2, snapshot.Packages.Count);
        Assert.EndsWith("alpha", snapshot.Packages[0].Root, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1, snapshot.Packages[0].Errors);
        Assert.Equal(1, snapshot.Packages[1].Warnings);
        Assert.Equal(2, snapshot.ErrorList.Count);
        Assert.Contains(snapshot.ErrorList, entry => entry.File.EndsWith(".zpkg.toml", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(snapshot.ErrorList, entry => entry.File.EndsWith(".zpkg.lock", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void PreviewsOnlyConfirmationGatedCommands() {
        var action = new ZedAction("install", "Install", "command", "zed", new[] { "install" }, true);
        var preview = ZedWorkspaceModel.Preview(action, "workspace");
        Assert.Equal("zed", preview.Executable);
        Assert.Equal(new[] { "install" }, preview.Arguments.ToArray());
        Assert.True(preview.RequiresConfirmation);
        Assert.Throws<ArgumentException>(() => ZedWorkspaceModel.Preview(
            new ZedAction("bad", "Bad", "command", "zed", new[] { "install" }, false), "workspace"
        ));
    }
}
