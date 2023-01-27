using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace Reboot
{
    class Program
    {
        static void Main(string[] args)
        {
            // The path to the executable file
            string exePath = @"writer.exe";

            // Add the executable file to the startup registry key
            RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            key.SetValue("writer", exePath);
        }
    }
}




