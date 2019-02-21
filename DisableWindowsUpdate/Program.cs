using Microsoft.Win32;
using System;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Management;
using System.Security.Principal;

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

            Console.WriteLine("We will also Disable SuperFetch as in most cases it Decreases System Performence just like update service.");
            //Disable SuperFetch
            DisableTheSerivce("SysMain");
            Console.WriteLine("Attempting to Disable the Update Service...");

			// disable the update service
            DisableTheSerivce("wuauserv");
            Task.Delay(100);
            Console.WriteLine("Now attempting to Stop the Update Service...");
            Console.WriteLine("If this process takes more than 6 secs, manually close the program as windows is preventing the service to stop...");
            Console.WriteLine("As the update service is disabled, it wont Auto Update Next time. ( i guess ?! Its WINDOWS Bruhh )");

			// try to stop the update serivce after disabling it.
			// windows might prevent us from stopping it automatically as if like a fail safe.
            ServiceController sc = new ServiceController("wuauserv");
            try
            {
                if (sc != null && sc.Status == ServiceControllerStatus.Running)
                {
                    sc.Stop();
                }

                sc.WaitForStatus(ServiceControllerStatus.Stopped);
                sc.Close();
				// finished all process. press any key to exit. 
                Console.WriteLine("Update Service Stopped! Press any key to exit...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
        public static void DisableTheSerivce(string serviceName)
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
            }
            
            Console.WriteLine("Update Service Disabled Sucessfully!");
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
