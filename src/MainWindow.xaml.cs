using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using System.Diagnostics;
using System.ServiceProcess;

namespace MySQLInstaller
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            RegHelper.CreateKey();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            loadData();
        }

        private void loadData()
        {
            try
            {
                this.txt_mysqlbindir.Text = RegHelper.getKey("mysqlbindir", RegHelper.DefaultInstallDir);
                this.txt_mysqldatadir.Text = RegHelper.getKey("mysqldatadir", RegHelper.DefaultDataDir);
                this.txt_mysqlport.Text = RegHelper.getKey("mysqlport", RegHelper.DefaultMySQLPort);
                this.txt_mysqlusername.Text = RegHelper.getKey("mysqlusername", RegHelper.DefaultMySQLUser);
                this.txt_mysqlservername.Text = RegHelper.getKey("mysqlservername", RegHelper.DefaultMySQLServerName);

                showMessage("");
                this.txt_mysqlpassword.Password = AesHelper.AESDecrypt(RegHelper.getKey("mysqlpwd", AesHelper.AESEncrypt(RegHelper.DefaultMySQLPassword)));
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载数据出现错误：" + ex.Message);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var content = (sender as Button)?.Content?.ToString()?.Trim();
            if (string.IsNullOrWhiteSpace(content))
            {
                return;
            }

            switch (content)
            {
                case "选择安装位置":
                    selectMySQLBinDir();
                    break;
                case "选择数据位置":
                    selectMySQLDataDir();
                    break;
                case "恢复默认端口":
                    resetDefaultPort();
                    break;
                case "恢复默认用户":
                    resetDefaultUser();
                    break;
                case "恢复默认密码":
                    resetDefaultPassword();
                    break;
                case "恢复默认名称":
                    resetDefaultMysqlServername();
                    break;
                case "一键安装":
                    oneKeyInstall();
                    break;
                case "一键卸载":
                    oneKeyUnInstall();
                    break;
            }

        }

        private void resetDefaultMysqlServername()
        {
            this.txt_mysqlservername.Text = RegHelper.DefaultMySQLServerName;
        }

        /// <summary>
        /// 获取文件夹大小
        /// </summary>
        /// <param name="dirPath"></param>
        /// <returns></returns>
        static long GetDirectorySize(string dirPath)
        {
            if (!System.IO.Directory.Exists(dirPath))
                return 0;
            long len = 0;
            DirectoryInfo di = new DirectoryInfo(dirPath);
            //获取di目录中所有文件的大小
            foreach (FileInfo item in di.GetFiles())
            {
                len += item.Length;
            }

            //获取di目录中所有的文件夹,并保存到一个数组中,以进行递归
            DirectoryInfo[] dis = di.GetDirectories();
            if (dis.Length > 0)
            {
                for (int i = 0; i < dis.Length; i++)
                {
                    len += GetDirectorySize(dis[i].FullName);//递归dis.Length个文件夹,得到每隔dis[i]

                }
            }
            return len;
        }


        /// <summary>
        /// 获取文件大小
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        static long GetFileSize(string filePath)
        {
            long temp = 0;
            //判断当前路径是否指向某个文件
            if (!File.Exists(filePath))
            {
                string[] strs = Directory.GetFileSystemEntries(filePath);
                foreach (string item in strs)
                {
                    temp += GetFileSize(item);
                }
            }
            else
            {
                FileInfo fileInfo = new FileInfo(filePath);
                return fileInfo.Length;
            }
            return temp;
        }
        /// <summary>
        /// 删除文件夹以及文件
        /// </summary>
        /// <param name="directoryPath"> 文件夹路径 </param>
        /// <param name="fileName"> 文件名称 </param>
        public static void DeleteDirectory(string directoryPath)
        {
            DirectoryInfo di = new DirectoryInfo(directoryPath);
            di.Delete(true);
        }

        private void oneKeyUnInstall()
        {
            string mysqlservername = txt_mysqlservername.Text.Trim();
            if (checkMysqlServerName(mysqlservername) == false)
            {
                return;
            }
            if (isServiceInstall(mysqlservername) == false)
            {
                MessageBox.Show("MySQL【" + mysqlservername + "】服务没有安装！");
                return;
            }
            if (MessageBox.Show("真的要卸载MySQL【" + mysqlservername + "】服务吗？", "确认", MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
            {
                return;
            }
            int nowprocess = 0;
            setProcess(nowprocess);
            ChangeBtnStatus(false);
            string installbindir = txt_mysqlbindir.Text.Trim();
            string installdatadir = txt_mysqldatadir.Text.Trim();
            string mysqluser = txt_mysqlusername.Text.Trim();
            string mysqlpass = txt_mysqlpassword.Password.Trim();
            string mysqlport = txt_mysqlport.Text.Trim();

            Action<int> addProcessAction = (count) =>
            {
                for (int i = 0; i <= count; i++)
                {
                    setProcess(nowprocess++);
                    Thread.Sleep(50);
                }
            };
            Task.Run(() =>
            {
                setProcess(nowprocess++);
                showMessage("开始卸载MySQL");
                addProcessAction(5);
                try
                {
                    showMessage("正在停止MySQL服务");
                    addProcessAction(1);
                    runCmd("net stop " + mysqlservername);
                    addProcessAction(5);
                    showMessage("停止MySQL服务完成");
                    addProcessAction(5);
                    showMessage("正在删除MySQL服务");
                    runCmd("sc delete " + mysqlservername);
                    addProcessAction(5);
                    showMessage("删除MySQL服务完成");

                    addProcessAction(5);
                    showMessage("正在删除数据库文件");
                    addProcessAction(5);
                    var datalen = GetDirectorySize(installbindir);

                    if (datalen > 500 * 1024 * 1024)
                    {
                        showMessage("数据库安装文件大于500M，请手动删除！");
                        return;
                    }
                    addProcessAction(1);
                    datalen = GetDirectorySize(installdatadir);
                    addProcessAction(1);
                    if (datalen > 500 * 1024 * 1024)
                    {
                        showMessage("数据库数据文件大于500M，请手动删除！");
                        return;
                    }
                    addProcessAction(1);
                    DeleteDirectory(installdatadir);
                    addProcessAction(1);
                    DeleteDirectory(installbindir);
                    addProcessAction(1);
                    showMessage("数据库文件删除完成");
                    addProcessAction(5);
                    showMessage("MySQL卸载成功！", Colors.Green);
                }
                catch (Exception ex)
                {
                    showMessage("卸载过程中发生错误:" + ex.Message, Colors.Red);
                }
                finally
                {
                    setProcess(100);
                    ChangeBtnStatus(true);
                }
            });
        }

        private void showMessage(string msg, Color? colortips = null)
        {
            this.Dispatcher.Invoke(() =>
            {
                txt_tips.Foreground = new SolidColorBrush(colortips ?? Colors.Black);
                txt_tips.Text = msg;
                //MessageBox.Show(this, msg);
            });
        }

        private void setProcess(double value)
        {
            this.Dispatcher.Invoke(() =>
            {
                processbar1.Value = value;
            });
        }

        private void ChangeBtnStatus(bool isenable = false)
        {
            this.Dispatcher.Invoke(() =>
            {
                btn_install.IsEnabled = isenable;
                btn_uninstall.IsEnabled = isenable;
            });
        }

        private void oneKeyInstall()
        {
            string mysqlservername = txt_mysqlservername.Text.Trim();
            if (checkMysqlServerName(mysqlservername) == false)
            {
                return;
            }
            if (isServiceInstall(mysqlservername))
            {
                MessageBox.Show("MySQL【" + mysqlservername + "】服务已经安装过！");
                return;
            }
            int nowprocess = 0;
            setProcess(nowprocess);
            ChangeBtnStatus(false);
            string installbindir = txt_mysqlbindir.Text.Trim();
            string installdatadir = txt_mysqldatadir.Text.Trim();
            string mysqluser = txt_mysqlusername.Text.Trim();
            string mysqlpass = txt_mysqlpassword.Password.Trim();
            string mysqlport = txt_mysqlport.Text.Trim();

            Action<int> addProcessAction = (count) =>
            {
                for (int i = 0; i <= count; i++)
                {
                    setProcess(nowprocess++);
                    Thread.Sleep(50);
                }
            };

            Task.Run(() =>
            {
                setProcess(nowprocess++);
                showMessage("开始安装MySQL");
                addProcessAction(5);
                try
                {
                    int outport = 0;
                    if (int.TryParse(mysqlport, out outport) == false)
                    {
                        showMessage("MySQL安装失败，MySQL端口号必须是数字!", Colors.Red);
                        return;
                    }
                    if (outport <= 0 || outport >= 65535)
                    {
                        showMessage("MySQL安装失败，请输入正确的端口号！", Colors.Red);
                        return;
                    }
                    addProcessAction(5);
                    showMessage("开始检测安装文件夹");
                    addProcessAction(5);
                    try
                    {
                        setProcess(nowprocess++);
                        if (Directory.Exists(installbindir) == false)
                        {
                            Directory.CreateDirectory(installbindir);
                        }
                        if (Directory.Exists(installdatadir) == false)
                        {
                            Directory.CreateDirectory(installdatadir);
                        }
                        showMessage("安装文件夹检测完毕");
                        addProcessAction(5);
                    }
                    catch (Exception ex)
                    {
                        addProcessAction(5);
                        showMessage("安装文件夹检测失败，请确保文件夹可用！", Colors.Red);
                        return;
                    }

                    showMessage("开始解压数据");
                    addProcessAction(5);
                    UnZip(installbindir, addProcessAction);
                    addProcessAction(5);
                    showMessage("数据解压完毕");
                    addProcessAction(5);

                    showMessage("开始配置数据库");
                    writeMyIniAndConfig(installbindir, installdatadir, mysqluser, mysqlpass, mysqlport, addProcessAction, mysqlservername);
                    addProcessAction(5);
                    showMessage("数据库配置完毕");
                    addProcessAction(5);
                    showMessage("开始保存配置");
                    saveConfig();
                    addProcessAction(5);
                    showMessage("配置保存完毕");
                    addProcessAction(5);
                    showMessage("MySQL安装成功！", Colors.Green);
                }
                catch (Exception ex)
                {
                    showMessage("安装过程中发生错误:" + ex.Message, Colors.Red);
                }
                finally
                {
                    setProcess(100);
                    ChangeBtnStatus(true);
                }
            });
        }

        private bool checkMysqlServerName(string mysqlservername)
        {
            if (mysqlservername.IndexOf(" ") > -1)
            {
                MessageBox.Show("MySQL服务名不能包含空格！");
                return false;
            }
            return true;
        }

        private bool isServiceInstall(string mysqlservername)
        {
            try
            {
                ServiceController cs = new ServiceController();
                cs.MachineName = "localhost";
                cs.ServiceName = mysqlservername;
                cs.Refresh();
                var st = cs.Status;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void writeMyIniAndConfig(string installbindir, string installdatadir, string mysqluser, string mysqlpass, string mysqlport, Action<int> addProcessAction, string mysqlservername)
        {
            addProcessAction(1);
            //写入my.ini
            string myinipath = installbindir + "\\my.ini";
            string content = Properties.Resources.my;
            content = content.Replace("{mysqlport}", mysqlport);
            content = content.Replace("{mysqlbindir}", installbindir.Replace("\\", "/"));
            content = content.Replace("{mysqldatadir}", installdatadir.Replace("\\", "/"));
            File.WriteAllText(myinipath, content);
            addProcessAction(1);
            runCmd("\"" + installbindir + "\\bin\\mysqld.exe\" --initialize-insecure");
            addProcessAction(1);
            runCmd("\"" + installbindir + "\\bin\\mysqld.exe\" --install " + mysqlservername);
            addProcessAction(1);
            runCmd("net start " + mysqlservername);
            addProcessAction(1);
            //runCmd("\"" + installbindir + "\\bin\\mysqladmin.exe\" -uroot -p password " + mysqlpass);
            //addProcessAction(1);

            string connectstrroot = "server=localhost;port=" + mysqlport + ";user id=root;password=;database=mysql;Charset=utf8;";
            var msyqldbroot = new MySQLDBHelp(connectstrroot);

            addProcessAction(1);
            string sqlcontent = Properties.Resources.zixingtest;

            string zixingtestpath = installbindir + "\\zixingtest.sql";
            File.WriteAllText(zixingtestpath, sqlcontent);
            int ret = msyqldbroot.getMySqlCom(sqlcontent);
            addProcessAction(1);

            ret = msyqldbroot.getMySqlCom("update user set authentication_string=password('" + mysqlpass + "') where user='root';");
            addProcessAction(1);
            ret = msyqldbroot.getMySqlCom("flush privileges;");

            addProcessAction(1);

            //导入sql文件
            //runCmd("\"" + installbindir + "\\bin\\mysql.exe\" -uroot –p" + mysqlpass + " -Dzixingtest<\"" + zixingtestpath + "\"");

            addProcessAction(1);

            if (mysqluser != "root")
            {
                addProcessAction(1);
                string connectstr = "server=localhost;port=" + mysqlport + ";user id=root;password=" + mysqlpass + ";Charset=utf8;";
                var msyqldb = new MySQLDBHelp(connectstr);
                ret = msyqldb.getMySqlCom("create user '" + mysqluser + "'@'%' identified by '" + mysqlpass + "';");
                addProcessAction(1);
                ret = msyqldb.getMySqlCom("grant all privileges on "+ ComConst.mydbname + ".* to '" + mysqluser + "'@'%' identified by '" + mysqlpass + "' with grant option;");
                addProcessAction(1);
                ret = msyqldb.getMySqlCom("flush privileges;");

                addProcessAction(1);
            }
            showMessage("正在重启MySQL服务");
            runCmd("net stop " + mysqlservername);
            addProcessAction(1);
            runCmd("net start " + mysqlservername);
            showMessage("MySQL服务重启完成");
            addProcessAction(1);
        }

        private string runCmd(string cmdstr)
        {
            try
            {
                //创建一个进程
                Process p = new Process();
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.UseShellExecute = false;//是否使用操作系统shell启动
                p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
                p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
                p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
                p.StartInfo.CreateNoWindow = true;//不显示程序窗口
                p.Start();//启动程序 
                string strCMD = cmdstr;
                //向cmd窗口发送输入信息
                p.StandardInput.WriteLine(strCMD + "&exit");
                p.StandardInput.AutoFlush = true;
                //获取cmd窗口的输出信息
                string output = p.StandardOutput.ReadToEnd();
                //等待程序执行完退出进程
                p.WaitForExit();
                p.Close();
                return output;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        /// <summary>
        /// ZIP:解压一个zip文件
        /// add yuangang by 2016-06-13
        /// </summary>
        /// <param name="ZipFile">需要解压的Zip文件（绝对路径）</param>
        /// <param name="TargetDirectory">解压到的目录</param>
        /// <param name="Password">解压密码</param>
        /// <param name="OverWrite">是否覆盖已存在的文件</param>
        public void UnZip(string TargetDirectory, Action<int> addProcessAction, bool OverWrite = true)
        {
            addProcessAction(1);
            if (!TargetDirectory.EndsWith("\\")) { TargetDirectory = TargetDirectory + "\\"; }
            DateTime nowtime = DateTime.Now;

            using (ZipInputStream zipfiles = new ZipInputStream(new MemoryStream(Properties.Resources.MySql5_7_16)))
            {
                zipfiles.Password = null;
                ZipEntry theEntry;

                while ((theEntry = zipfiles.GetNextEntry()) != null)
                {
                    string directoryName = "";
                    string pathToZip = "";
                    pathToZip = theEntry.Name;

                    if (pathToZip != "")
                        directoryName = System.IO.Path.GetDirectoryName(pathToZip) + "\\";

                    string fileName = System.IO.Path.GetFileName(pathToZip);

                    Directory.CreateDirectory(TargetDirectory + directoryName);

                    if (fileName != "")
                    {
                        if (DateTime.Now - nowtime > TimeSpan.FromMilliseconds(200))
                        {
                            nowtime = DateTime.Now;
                            addProcessAction(1);
                        }
                        if ((File.Exists(TargetDirectory + directoryName + fileName) && OverWrite) || (!File.Exists(TargetDirectory + directoryName + fileName)))
                        {
                            using (FileStream streamWriter = File.Create(TargetDirectory + directoryName + fileName))
                            {
                                int size = 2048;
                                byte[] data = new byte[2048];
                                while (true)
                                {
                                    size = zipfiles.Read(data, 0, data.Length);

                                    if (size > 0)
                                        streamWriter.Write(data, 0, size);
                                    else
                                        break;
                                }
                                streamWriter.Close();
                            }
                        }
                    }
                }

                zipfiles.Close();
            }
        }

        private void saveConfig()
        {
            this.Dispatcher.Invoke(() =>
            {
                RegHelper.CreateKey();
                RegHelper.setKey("mysqlbindir", this.txt_mysqlbindir.Text.Trim());
                RegHelper.setKey("mysqldatadir", this.txt_mysqldatadir.Text.Trim());
                RegHelper.setKey("mysqlport", this.txt_mysqlport.Text.Trim());
                RegHelper.setKey("mysqlusername", this.txt_mysqlusername.Text.Trim());
                RegHelper.setKey("mysqlpwd", AesHelper.AESEncrypt(this.txt_mysqlpassword.Password.Trim()));
                RegHelper.setKey("mysqlservername", this.txt_mysqlservername.Text.Trim());


            });
        }

        private void resetDefaultPassword()
        {
            this.txt_mysqlpassword.Password = RegHelper.DefaultMySQLPassword;
        }

        private void resetDefaultUser()
        {
            this.txt_mysqlusername.Text = RegHelper.DefaultMySQLUser;
        }

        private void resetDefaultPort()
        {
            this.txt_mysqlport.Text = RegHelper.DefaultMySQLPort;
        }

        private void selectMySQLDataDir()
        {
            System.Windows.Forms.FolderBrowserDialog openFileDialog = new System.Windows.Forms.FolderBrowserDialog();  //选择文件夹 

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)//注意，此处一定要手动引入System.Window.Forms空间，否则你如果使用默认的DialogResult会发现没有OK属性
            {
                txt_mysqldatadir.Text = openFileDialog.SelectedPath;
            }
        }

        private void selectMySQLBinDir()
        {
            System.Windows.Forms.FolderBrowserDialog openFileDialog = new System.Windows.Forms.FolderBrowserDialog();  //选择文件夹 

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)//注意，此处一定要手动引入System.Window.Forms空间，否则你如果使用默认的DialogResult会发现没有OK属性
            {
                txt_mysqlbindir.Text = openFileDialog.SelectedPath;
            }
        }
    }
}
