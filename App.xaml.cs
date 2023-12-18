using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MVCADTest
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var email = "nichongben@sjtu.edu.cn";
            var uuid = "1210000042500615X0-20230508";
            var sn = "2cc4e34bafade6ca9f0c6ef9684e0af6";

            AnyCAD.Foundation.GlobalInstance.Initialize();
            AnyCAD.Foundation.GlobalInstance.RegisterSDK(email, uuid, sn);
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            AnyCAD.Foundation.GlobalInstance.Destroy();
        }
    }
}
