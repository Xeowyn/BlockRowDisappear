using System;
using System.Collections.Generic;
using System.IO;
using System.Media;

// Plays the .wav files from the assets folder.
// Windows only lets one of these play at a time, so a new
// sound cuts off the one before it. That is fine for this game.
public class Sound
{
    public bool On = true;

    private Dictionary<string, SoundPlayer> players = new Dictionary<string, SoundPlayer>();

    public Sound(string folder)
    {
        Add(folder, "move");
        Add(folder, "spin");
        Add(folder, "land");
        Add(folder, "line");
        Add(folder, "quad");
        Add(folder, "boom");
        Add(folder, "levelup");
        Add(folder, "go");
        Add(folder, "gameover");
    }

    private void Add(string folder, string name)
    {
        string path = Path.Combine(folder, name + ".wav");

        if (File.Exists(path) == false)
        {
            // no sound file is annoying but it should not stop the game
            return;
        }

        SoundPlayer player = new SoundPlayer(path);

        try
        {
            player.Load();
            players[name] = player;
        }
        catch (Exception e)
        {
            // some computers have no sound card
        }
    }

    public void Play(string name)
    {
        if (On == false)
        {
            return;
        }

        if (players.ContainsKey(name) == false)
        {
            return;
        }

        try
        {
            players[name].Play();
        }
        catch (Exception e)
        {
        }
    }

    public void Move()
    {
        Play("move");
    }

    public void Spin()
    {
        Play("spin");
    }

    public void Land()
    {
        Play("land");
    }

    public void Boom()
    {
        Play("boom");
    }

    // more rows cleared = fancier sound
    public void Lines(int howMany)
    {
        if (howMany == 4)
        {
            Play("quad");
        }
        else
        {
            Play("line");
        }
    }

    public void LevelUp()
    {
        Play("levelup");
    }

    public void Go()
    {
        Play("go");
    }

    public void Dead()
    {
        Play("gameover");
    }
}
