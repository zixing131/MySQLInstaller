using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace MySQLInstaller
{
    public class RegHelper
    {
        private const string regKey = "software\\zixingmysqlinstaller";

        public static string DefaultInstallDir = "C:\\Program Files\\MySQL";
        public static string DefaultDataDir = "C:\\Program Files\\MySQL\\Data";
        public static string DefaultMySQLUser = "user";
        public static string DefaultMySQLPassword = "123456";
        public static string DefaultMySQLPort = "3306";
        public static string DefaultMySQLServerName = "mysql";


        public static bool CreateKey()
        {
            try
            {
                RegistryKey key = Registry.LocalMachine;
                RegistryKey software = key.CreateSubKey(regKey);
                key.Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public static bool DeleteKey()
        {
            try
            {
                RegistryKey key = Registry.LocalMachine;
                key.DeleteSubKeyTree(regKey, false); //该方法无返回值，直接调用即可
                key.Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool setKey(string name, string value)
        {
            try
            {
                RegistryKey key = Registry.LocalMachine;
                RegistryKey software = key.OpenSubKey(regKey, true); //该项必须已存在
                software.SetValue(name, value);
                key.Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static string getKey(string name, string defaultvalue)
        {
            try
            {
                RegistryKey key = Registry.LocalMachine;
                RegistryKey software = key.OpenSubKey(regKey); //该项必须已存在
                string ret = software.GetValue(name)?.ToString();
                if (string.IsNullOrWhiteSpace(ret))
                {
                    ret = defaultvalue;
                }
                key.Close();
                return ret;
            }
            catch (Exception ex)
            {
                return defaultvalue;
            }
        }
    }
}
