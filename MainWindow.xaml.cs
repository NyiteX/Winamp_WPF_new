using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using Microsoft.WindowsAPICodePack.Shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;
using System.Windows.Ink;

namespace Winamp_WPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<string> playlist_list = new List<string>();
        DispatcherTimer timer = new DispatcherTimer();
        DispatcherTimer timer2 = new DispatcherTimer();
        uint pause_time = 0;
        string tmpSource = "";
        Point p;
        public MainWindow()
        {
            InitializeComponent();
            Player.MediaEnded += media_MediaEnded;
            
            timer.Tick += new EventHandler(Update);
            timer.Start();
            stereo_label.Foreground = new SolidColorBrush(Color.FromArgb(255,77,253,0));

            timer2.Interval = TimeSpan.FromMilliseconds(250);
            timer2.Tick += new EventHandler(RunningName);
            //Main.WindowStyle = WindowStyle.None;
        }
        public void Total_seconds()
        {
            try
            {
                using (var shell = ShellObject.FromParsingName(playlist_list[List.SelectedIndex]))
                {
                    IShellProperty prop = shell.Properties.System.Media.Duration;
                    var t = (ulong)prop.ValueAsObject;
                    progress_player.Maximum = TimeSpan.FromTicks((long)t).TotalSeconds;
                    songName_label.Content = shell.Name + "   ";
                    kbps_tb.Text = (shell.Properties.System.Audio.EncodingBitrate.Value / 1000).ToString();
                    khz_tb.Text = shell.Properties.System.Audio.SampleSize.Value.ToString();
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }    
        }
        public void Update(object sender, EventArgs e)
        {
            time_file_label.Content = Player.Position.ToString("mm") + ':' + Player.Position.ToString("ss");


            progress_player.Value = Player.Position.TotalSeconds;    
        }
        public void RunningName(object sender, EventArgs e)
        {
            if (songName_label.Content.ToString().Count() > 0)
            {
                songName_label.Content = songName_label.Content.ToString() + songName_label.Content.ToString()[0];
                songName_label.Content = songName_label.Content.ToString().Remove(0, 1);
            }
        }
        private void media_MediaEnded(object sender, RoutedEventArgs e)
        {
            Next_song();       
        }
        public void Prev_song() 
        {
            if (List.SelectedIndex - 1 >= 0)
            {
                Player.Source = new Uri(playlist_list[List.SelectedIndex - 1]);
                List.SelectedIndex = List.SelectedIndex - 1;
            }
        }
        public void Next_song()
        {
            if (List.SelectedIndex + 1 < List.Items.Count)
            {
                Player.Source = new Uri(playlist_list[List.SelectedIndex + 1]);
                List.SelectedIndex = List.SelectedIndex + 1;
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Player.Source != null)
                {
                    pause_time = (uint)Player.Position.TotalSeconds;
                    tmpSource = Player.Source.ToString();
                    Player.Source = null;
                }
                else
                {
                    if (tmpSource != "" && pause_time != 0)
                    {
                        Player.Source = new Uri(tmpSource);
                        Player.Position = new TimeSpan(0, 0, 0, (int)pause_time, 0);

                        pause_time = 0;
                        tmpSource = "";
                    }
                }
            }catch(Exception ex) { MessageBox.Show(ex.Message); }
        }
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Player.Source = null;
            timer2.Stop();
            pause_time = 0;
            tmpSource = "";
        }
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Next_song();
        }
        private void volume_menu_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Player.Volume = (double)e.NewValue/100;
            volume_menu.SelectionEnd = Convert.ToInt32(e.NewValue);
        }
        private void balance_menu_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue > 50)
                Player.Balance = 1;
            else if (e.NewValue < 50)
                Player.Balance = -1;
            else Player.Balance = 0;
        }
        private void add_click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Multiselect = true;
            openFileDialog1.Filter = "mp3 files (*.mp3)|*.mp3";
            openFileDialog1.ShowDialog();
            
            if (List.Items.Count > 0) List.Items.Clear();
            {
                for (int i = 0; i < openFileDialog1.FileNames.Length; i++)
                {
                    playlist_list.Add(openFileDialog1.FileNames[i]);
                    List.Items.Add("["+(i+1)+"]  "+openFileDialog1.SafeFileNames[i]);
                }               
            }
            //for (int i = 0; i < playlist_list.Count; i++)
            //{
            //    List.Items.Add(playlist_list[i]);
            //}
        }
        private void rem_click(object sender, RoutedEventArgs e)
        {
            playlist_list.RemoveAt(List.SelectedIndex);
            List.Items.Remove(List.SelectedItem);          
        }
        private void play_click(object sender, RoutedEventArgs e)
        {
            if (List.SelectedIndex != -1)
            {
                Player.Source = new Uri(playlist_list[List.SelectedIndex]);
                Total_seconds();
                timer2.Start();
            }
        }
        private void List_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (List.SelectedIndex != -1)
            {
                Player.Source = new Uri(playlist_list[List.SelectedIndex]);
                Total_seconds();
                timer2.Start();
            }
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Prev_song();
        }
        private void Rewind_Button(object sender, RoutedEventArgs e)
        {
            double n = Player.Position.TotalSeconds;
            if(n - 10 > 0)
                Player.Position = new TimeSpan(0, 0, 0, Convert.ToInt32(n) - 10, 0);
        }
        private void Forward_Button(object sender, RoutedEventArgs e)
        {
            double n = Player.Position.TotalSeconds;
            if (n + 10 < progress_player.Maximum)
                Player.Position = new TimeSpan(0, 0, 0,Convert.ToInt32(n) + 10, 0);
        }
    }
}
