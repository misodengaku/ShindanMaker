using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using ShindanMaker;

namespace meshiyosoi
{
    public partial class MainPage : PhoneApplicationPage
    {
        // コンストラクター
        public MainPage()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Method.GetShindan(int.Parse(textBox1.Text), ec =>
            {
                var item = (ShindanItem)ec;
                //MessageBox.Show("Title:"+item.Title+"\nDescription:"+item.Description);
                textBlock2.Text = "診断名：" + item.Title;
                textBlock3.Text = "説明：" + item.Description;
            });
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            Method.ExecShindan(int.Parse(textBox1.Text), textBox2.Text, result =>
                {
                    Dispatcher.BeginInvoke(() => MessageBox.Show(result.ToString()));
                });
        }
    }
}