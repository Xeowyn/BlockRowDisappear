using System;
using System.Drawing;
using System.IO;

// Loads all the pictures out of the assets folder.
// If a picture is missing the game cannot run, so this
// complains loudly instead of showing a blank screen.
class Assets
{
    public Image[] Blocks = new Image[8];   // 1 to 7, number 0 is not used
    public Image Empty;
    public Image Ghost;
    public Image Background;
    public Image[] Boom;

    public string Folder;

    // how many parent folders to try when looking for the assets folder
    private const int MaxParentFolderSearchDepth = 8;

    // normal way: look for the assets folder next to the .exe
    public Assets() : this(FindFolder())
    {
    }

    // or you can say exactly where the folder is
    public Assets(string folder)
    {
        Folder = folder;

        for (int i = 1; i <= 7; i++)
        {
            Blocks[i] = Load("block_" + i + ".png");
        }

        Empty = Load("empty.png");
        Ghost = Load("ghost.png");
        Background = Load("background.png");

        // count how many explosion pictures there are, so you can
        // add more frames just by dropping in boom_7.png and so on
        int frames = 0;
        while (File.Exists(Path.Combine(Folder, "boom_" + (frames + 1) + ".png")))
        {
            frames = frames + 1;
        }

        if (frames == 0)
        {
            throw new Exception("There are no explosion pictures (boom_1.png) in:\n" + Folder);
        }

        Boom = new Image[frames];
        for (int i = 0; i < frames; i++)
        {
            Boom[i] = Load("boom_" + (i + 1) + ".png");
        }
    }

    // the assets folder sits next to the .exe
    private static string FindFolder()
    {
        string here = AppContext.BaseDirectory;
        string folder = Path.Combine(here, "assets");

        if (Directory.Exists(folder))
        {
            return folder;
        }

        // when running from Visual Studio the exe is buried in bin\Debug,
        // so look back up the folders too
        string up = here;
        for (int i = 0; i < MaxParentFolderSearchDepth; i++)
        {
            up = Path.Combine(up, "..");
            string tryHere = Path.GetFullPath(Path.Combine(up, "assets"));
            if (Directory.Exists(tryHere))
            {
                return tryHere;
            }
        }

        throw new Exception("Could not find the 'assets' folder. It should sit next to BlockRowDisappear.exe.");
    }

    private Image Load(string name)
    {
        string path = Path.Combine(Folder, name);

        if (File.Exists(path) == false)
        {
            throw new Exception("The game needs this picture but it is missing:\n" + path);
        }

        // read the bytes first so the file does not stay locked,
        // that way you can swap the png out while the game is closed
        byte[] bytes = File.ReadAllBytes(path);
        return Image.FromStream(new MemoryStream(bytes));
    }
}
