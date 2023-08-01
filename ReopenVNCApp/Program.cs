using System;
using System.Xml.Linq;
using Microsoft.Win32.TaskScheduler;

namespace ReopenVNCApp
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string domainName = Environment.UserDomainName;
                string userName = Environment.UserName;
                string userId = $"{domainName}\\{userName}";

                XNamespace ns = "http://schemas.microsoft.com/windows/2004/02/mit/task";
                XDocument doc = new XDocument(
                    new XDeclaration("1.0", "utf-16", null),
                    new XElement(ns + "Task",
                        new XAttribute("version", "1.2"),
                        CreateRegistrationInfo(ns),
                        new XElement(ns + "Triggers",
                            new XElement(ns + "EventTrigger",
                                new XElement(ns + "Enabled", "true"),
                                new XElement(ns + "Subscription", "<QueryList><Query Id=\"0\" Path=\"Application\"><Select Path=\"Application\">*[System[Provider[@Name='VNC Server'] and EventID=256]]</Select></Query></QueryList>")
                            )
                        ),
                        CreatePrincipals(ns, userId),
                        new XElement(ns + "Settings",
                            new XElement(ns + "MultipleInstancesPolicy", "IgnoreNew"),
                            new XElement(ns + "DisallowStartIfOnBatteries", "true"),
                            new XElement(ns + "StopIfGoingOnBatteries", "true"),
                            new XElement(ns + "AllowHardTerminate", "true"),
                            new XElement(ns + "StartWhenAvailable", "false"),
                            new XElement(ns + "RunOnlyIfNetworkAvailable", "false"),
                            new XElement(ns + "IdleSettings",
                                new XElement(ns + "Duration", "PT10M"),
                                new XElement(ns + "WaitTimeout", "PT1H"),
                                new XElement(ns + "StopOnIdleEnd", "true"),
                                new XElement(ns + "RestartOnIdle", "false")
                            ),
                            new XElement(ns + "AllowStartOnDemand", "true"),
                            new XElement(ns + "Enabled", "true"),
                            new XElement(ns + "Hidden", "false"),
                            new XElement(ns + "RunOnlyIfIdle", "false"),
                            new XElement(ns + "WakeToRun", "false"),
                            new XElement(ns + "ExecutionTimeLimit", "PT72H"),
                            new XElement(ns + "Priority", "7")
                        ),
                        new XElement(ns + "Actions",
                            new XAttribute("Context", "Author"),
                            new XElement(ns + "Exec",
                                new XElement(ns + "Command", "\"C:\\Program Files\\RealVNC\\VNC Server\\vncguihelper.exe\""),
                                new XElement(ns + "Arguments", "vncserver.exe -_fromGui -start")
                            )
                        )
                    )
                );

                string taskXml = doc.ToString(SaveOptions.DisableFormatting);

                using (TaskService ts = new TaskService())
                {
                    TaskDefinition td = ts.NewTask();
                    td.RegistrationInfo.Description = "Reopen VNC Application";

                    td.XmlText = taskXml;

                    ts.RootFolder.RegisterTaskDefinition("ReopenVNCApp", td);
                }

                Console.WriteLine("Tarefa agendada para reabrir o VNC criada com sucesso!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao criar a tarefa: " + ex.Message);
            }
        }

        private static XElement CreateRegistrationInfo(XNamespace ns)
        {
            return new XElement(ns + "RegistrationInfo",
                new XElement(ns + "Date", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffffff")),
                new XElement(ns + "Author", Environment.UserDomainName + "\\" + Environment.UserName),
                new XElement(ns + "URI", "\\Explorer.fx")
            );
        }

        private static XElement CreatePrincipals(XNamespace ns, string userId)
        {
            return new XElement(ns + "Principals",
                new XElement(ns + "Principal",
                    new XAttribute("id", "Author"),
                    new XElement(ns + "UserId", userId),
                    new XElement(ns + "LogonType", "InteractiveToken"),
                    new XElement(ns + "RunLevel", "LeastPrivilege")
                )
            );
        }
    }
}
