using System;
using System.IO;
using Xunit;

namespace BlockRowDisappear.Tests;

public class SoundTests
{
    [Fact]
    public void Constructing_WithAMissingFolder_DoesNotThrow()
    {
        string folder = Path.Combine(Path.GetTempPath(), "no-such-folder-" + Guid.NewGuid());
        var sound = new Sound(folder);

        Assert.True(sound.On);
    }

    [Fact]
    public void Play_OnASoundThatNeverLoaded_DoesNothing()
    {
        string folder = Path.Combine(Path.GetTempPath(), "no-such-folder-" + Guid.NewGuid());
        var sound = new Sound(folder);

        var exception = Record.Exception(() => sound.Move());
        Assert.Null(exception);
    }
}
