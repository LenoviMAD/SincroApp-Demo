using System;
using System.Threading;
using System.Windows.Forms;
using SincroDatosApp;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        using var mutex = new Mutex(true, "SincroAppDemo_SingleInstance", out bool createdNew);
        if (!createdNew) return;

        ApplicationConfiguration.Initialize();
        Application.Run(new Form1());
    }
}
