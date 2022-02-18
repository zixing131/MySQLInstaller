# MySQLInstaller
 
 zixing's MySQLInstaller
 
 
这是一个MySQL自动安装工具，使用WPF进行开发，方便自动化部署MySQL。

你可以对资源文件中的zixingtest.sql文件进行替换，以进行自定义的数据库配置，sql文件种记得加入创建数据库的命令。

你可以通过替换资源文件中的 MySql5.7.16.zip 实现内置mysql版本的更改。

你可以通过修改资源文件中的 my.ini 文件进行自定义的配置，注意，这个文件要使用GBK编码进行保存，否则可能会报错！

点击安装后，你的配置会自动保存到注册表中，下次启动会自动加载。

ComConst类中的mydbname可以填写你的数据库名称，如果用户名不是root，会把这个数据库权限给予该用户。

ComConst类中的aespwd和aesiv可以自定义，用来对注册表中保存的mysql安装密码进行加密。
