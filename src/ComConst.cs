using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySQLInstaller
{
    public class ComConst
    {

        public const string aespwd = "1234567890123456";
        public const string aesiv  = "0000000000000000";
        /// <summary>
        /// 这里填写你的数据库名称，如果用户名不是root，会把这个数据库权限给予该用户
        /// </summary>
        public const string mydbname = "zixingtest";

    }
}
