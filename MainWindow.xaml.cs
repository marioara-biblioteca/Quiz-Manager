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

namespace WpfQuizManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string currentUser;
        public static int points;
        public static string rank;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void CheckRememberMe()
        {
            if (rememberMe.IsChecked == false)
            {
                username.Clear();
                password.Clear();
            }
        }
        private void ShowWindow()
        {
            this.CheckRememberMe();
            this.Show();
        }
        private void notRegistered_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow rw = new RegisterWindow();
            rw.goBack = this.ShowWindow;
            rw.Show();
            this.Hide();
        }

        private void logIn_Click(object sender, RoutedEventArgs e)
        {
            if(String.IsNullOrEmpty(username.Text ?? password.Password))
            {
                MessageBox.Show("Fields can't be empty!");
            }
            else
            {
                string hashedPass = Utils.ComputeSha256Hash(password.Password);
                var user = (from u in Utils.context.Users
                            where ( u.Username.Equals(username.Text) || u.Email.Equals(username.Text)) && u.HashedPassword.Equals(hashedPass)
                            select u ).FirstOrDefault();
                if(user != null)
                {
                    currentUser = user.Username;
                    points = user.RankPoints;
                    rank = user.Rank;

                    MainGameWindow mainGame = new MainGameWindow();
                    mainGame.goBack = ShowWindow;
                    mainGame.Show();

                    this.Hide();
                }
                else
                {
                    MessageBox.Show("User/Password wrong! Please try again.");
                }
            }
        }

        private void password_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                logIn_Click(this, e);
            }
        }
    }
}
