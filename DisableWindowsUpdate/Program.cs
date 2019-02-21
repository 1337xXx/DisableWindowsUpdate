using Microsoft.Win32;
using System;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Management;
using System.Security.Principal;
using System.Diagnostics;

namespace DisableWindowsUpdate
{
    class Program
    {
        static void Main(string[] args)
        {
			// if the os is not windows 10, abort.
            if (!IsWindows10())
            {
                Console.WriteLine("OS Isnt windows 10. Press any key to exit...");
                Console.ReadKey();
                return;
            }

			// if no admin rights, abort
            if (!IsAdministrator())
            {
                Console.WriteLine("Program was not started with Administrator rights. Press any key to exit...");                
                Console.ReadKey();
                return;
            }

            Console.WriteLine("We will also attempt to disable SuperFetch.");
            Console.WriteLine("Attempting to disable SuperFetch...");
            //Disable SuperFetch
            if (DisableTheSerivce("SysMain"))
            {
                Console.WriteLine("Disabled SuperFetch Sucessfully!");
            }
            else
            {
                Console.WriteLine("Failed to Disable SuperFetch.");
            }

            Console.WriteLine("Attempting to disable Update Service...");

            // disable the update service
            if (DisableTheSerivce("wuauserv"))
            {
                Console.WriteLine("Disabled Update Service Sucessfully!");
            }
            else
            {
                Console.WriteLine("Failed to disable Update Service.");
            }


            Console.WriteLine("Now attempting to stop running SuperFetch and Update Services.");
            Console.WriteLine("Manually close this program if the process takes more than 5 seconds.");

            if (StopService("wuauserv"))
            {
                Console.WriteLine("Sucessfully stopped Windows Update Service.");
            }
            else
            {
                Console.WriteLine("Failed to Stop Windows Update Service.");
            }

            if (StopService("SysMain"))
            {
                Console.WriteLine("Sucessfully stopped SuperFetch Service.");
            }
            else
            {
                Console.WriteLine("Failed to stop SuperFetch Service.");
            }

            Environment.Exit(0);
        }

        public static bool StopService(string ServiceName)
        {
            ServiceController sc = new ServiceController(ServiceName);
            try
            {
                if (sc != null && sc.Status == ServiceControllerStatus.Running)
                {
                    sc.Stop();
                }
                sc.WaitForStatus(ServiceControllerStatus.Stopped);
                sc.Close();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

		//Check if running in administrator mode
        public static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

		// function to disable the windows update service
		// only disable, not stop
        public static bool DisableTheSerivce(string serviceName)
        {
            try
            {
                using (var mo = new ManagementObject(string.Format("Win32_Service.Name=\"{0}\"", serviceName)))
                {
                    mo.InvokeMethod("ChangeStartMode", new object[] { "Disabled" });                    
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            
            Console.WriteLine("Update Service Disabled Sucessfully!");
            return true;
        }

		// function to check if the OS running is windows 10
        public static bool IsWindows10()
        {
            var reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            string productName = (string)reg.GetValue("ProductName");
            return productName.StartsWith("Windows 10");
        }
    }
}
