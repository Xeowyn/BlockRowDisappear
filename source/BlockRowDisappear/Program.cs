using System;
using System.Windows.Forms;

// This just opens the window. All the real work is in the other classes.
class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        try
        {
            Application.Run(new GameForm());
        }
        catch (Exception e)
        {
            // if a picture or sound is missing, say so instead of
            // just closing with no explanation
            MessageBox.Show(e.Message, "Block Row Disappear could not start",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
