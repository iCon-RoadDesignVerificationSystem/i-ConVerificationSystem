///----------------------------------------------------------------------------------------------------------------------------------------------------------------------
/// Copyright (c) 2021 The University of Tokyo, School of Engineering Construction System Management for Innovation & Yachiyo Engineering Co., Ltd. All Rights Reserved.
/// This softwear is released under the MIT License.
/// see LICENSE.txt
/// ----------------------------------------------------------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace i_ConVerificationSystem
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        async void App_Startup(object sender, StartupEventArgs e)
        {
            var sp = new SplashScreen();
            await ShowSplashScreen(sp);

            var main = new MainWindow();
            main.ShowDialog();
            sp.Close();
        }

        async Task ShowSplashScreen(SplashScreen sp)
        {
            await Task.Run(() =>
            {
                sp.Dispatcher.Invoke(() =>
                {
                    sp.Show();
                });
            });
        }
    }
}
