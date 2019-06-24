using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Text.RegularExpressions;

namespace FTP
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string prevAddress = "ftp://";
        public static MainWindow window;
        public FtpClient client;
        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;
            window = this;
        }

        public static string UpdateStatus
        {
            set
            {
                window.sb.Text = value;
            }
        }

        private void btn_connect_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                // Создаем объект подключения по FTP
                client = new FtpClient(txt_address.Text, txt_login.Text, txt_password.Password);

                List<FileDir> list = client.ListDirectoryDetails();

                // Добавить поле, которое будет возвращать пользователя на директорию выше
                list.Reverse();
                list.Add(new FileDir("", "prevDir", "...", "", txt_address.Text));
                list.Reverse();

                // Отобразить список в ListView
                lbx_files.ItemsSource = list;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString() + ": \n" + ex.Message);
                client = null;
            }
        }

        private void folder_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                FileDir fdi = (FileDir)(sender as ListView).SelectedItem;
                if (fdi == null)
                    return;
                switch (fdi.Type)
                {
                    // Если клик по папке
                    case "directory":
                        prevAddress = fdi.address;
                        txt_address.Text = fdi.address + fdi.Name + "/";
                        btn_connect_Click_1(null, null);
                        break;
                    // Если клик по кнопке перехода в родительскую директорию
                    case "prevDir":
                        if (txt_address.Text.LastIndexOf('/') == txt_address.Text.IndexOf('/', 7))
                            break;
                        int slashIndex = txt_address.Text.LastIndexOf('/', txt_address.Text.Length - 2);
                        txt_address.Text = txt_address.Text.Substring(0, slashIndex + 1);
                        btn_connect_Click_1(null, null);
                        break;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString() + ": \n" + ex.Message);
                client = null;
            }
        }

        private void btn_delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (client != null)
                {
                    DeleteWindow deleteWindow = new DeleteWindow();
                    deleteWindow.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString() + ": \n" + ex.Message);
                client = null;
            }
            
        }
    }
}
