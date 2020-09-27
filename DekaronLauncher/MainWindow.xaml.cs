using IniParser;
using IniParser.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows;
using System.Windows.Input;

namespace DekaronLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var parser = new FileIniDataParser();
            IniData data = parser.ReadFile("settings.ini");

            // set buttons
            if (data["Buttons"]["button1"] == "" || data["Buttons"]["link1"] == "")
            {
                Button1.Visibility = Visibility.Hidden;
            }
            else
            {
                Button1.Content = data["Buttons"]["button1"];
                Button1.Click += Button1_Click;
            }

            if (data["Buttons"]["button2"] == "" || data["Buttons"]["link2"] == "")
            {
                Button1.Visibility = Visibility.Hidden;
            }
            else
            {
                Button2.Content = data["Buttons"]["button2"];
                Button2.Click += Button2_Click;
            }
            if (data["Buttons"]["button3"] == "" || data["Buttons"]["link3"] == "")
            {
                Button1.Visibility = Visibility.Hidden;
            }
            else
            {
                Button3.Content = data["Buttons"]["button3"];
                Button3.Click += Button3_Click;
            }
        }


        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            var parser = new FileIniDataParser();
            IniData data = parser.ReadFile("settings.ini");

            var url = data["Buttons"]["link3"];
            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            var parser = new FileIniDataParser();
            IniData data = parser.ReadFile("settings.ini");

            var url = data["Buttons"]["link2"];
            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            var parser = new FileIniDataParser();
            IniData data = parser.ReadFile("settings.ini");

            var url = data["Buttons"]["link1"];
            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
        }

        private void Close_window_click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Move_window_drag(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Launcher_Loaded(object sender, EventArgs e)
        {
            //load settings from ini
            var parser = new FileIniDataParser();
            IniData data = parser.ReadFile("settings.ini");

            try
            {
                System.Net.WebRequest myRequest = System.Net.WebRequest.Create(data["Settings"]["update"]);
                System.Net.WebResponse myResponse = myRequest.GetResponse();

                //need to have to download files
                WebClient myWebClient = new WebClient();

                int counter = 0;
                string line;
                System.IO.StreamReader file;

                //download patch notes from news url
                if (data["Settings"]["news"] != "")
                {
                    //download patch notes/news
                    myWebClient.DownloadFile(data["Settings"]["news"], "updatenews.txt");

                    List<PatchNotes> items = new List<PatchNotes>();

                    //read the downloaded patch notes
                    file = new System.IO.StreamReader(@"updatenews.txt");
                    while ((line = file.ReadLine()) != null)
                    {
                        items.Add(new PatchNotes() { Title = ((line.StartsWith(">")) ? line.Substring(1) : line), FontWeight = (!line.StartsWith(">") ? FontWeights.Bold : FontWeights.Normal) });
                        counter++;
                    }
                    PatchNotes.ItemsSource = items;
                }

                var version = 0;

                //download version file
                myWebClient.DownloadFile(data["Settings"]["update"] + "version", "version");
                file = new System.IO.StreamReader(@"version");
                while ((line = file.ReadLine()) != null)
                {
                    version = Convert.ToInt32(line);
                    counter++;
                }
                file.Close();

                //delete version file
                File.Delete("version");

                StartButton.Content = "Updating...";

                var current_version = Convert.ToInt32(data["Settings"]["version"]);
                while (current_version < version)
                {
                    current_version++;
                    UpdateProgress.Value = current_version / version * 100;
                    //download patch
                    myWebClient.DownloadFile(data["Settings"]["update"] + current_version.ToString() + ".zip", current_version.ToString() + ".zip");

                    //extract patch
                    ZipArchiveExtensions.ExtractToDirectory(ZipFile.OpenRead(current_version.ToString() + ".zip"), Directory.GetCurrentDirectory(), true);

                    //delete patch zip file
                    File.Delete(current_version.ToString() + ".zip");

                    //update version number
                    data["Settings"]["version"] = current_version.ToString();
                    parser.WriteFile("settings.ini", data);
                }

                UpdateProgress.Value = current_version / version * 100;
                //enable game start button
                StartButton.Content = "Start Game";
                StartButton.IsEnabled = true;

            }
            catch (System.Net.WebException)
            {
                StartButton.Content = "Server Down";
            }



        }

        private void StartGame_Click(object sender, RoutedEventArgs e)
        {
            Process p = new Process();
            p.StartInfo = new ProcessStartInfo("bin\\dekaron.exe");
            p.Start();
            Application.Current.Shutdown();
        }
    }

    public class PatchNotes
    {
        public string Title { get; set; }
        public FontWeight FontWeight { get; set; }
    }

    public static class ZipArchiveExtensions
    {
        public static void ExtractToDirectory(this ZipArchive archive, string destinationDirectoryName, bool overwrite)
        {
            if (!overwrite)
            {
                archive.ExtractToDirectory(destinationDirectoryName);
                return;
            }
            foreach (ZipArchiveEntry file in archive.Entries)
            {
                string completeFileName = System.IO.Path.Combine(destinationDirectoryName, file.FullName);
                string directory = System.IO.Path.GetDirectoryName(completeFileName);

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                if (file.Name != "")
                    file.ExtractToFile(completeFileName, true);
            }

            archive.Dispose();

            return;
        }
    }

}