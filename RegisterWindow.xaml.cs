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
using System.Windows.Shapes;

namespace WpfQuizManager
{
    /// <summary>
    /// Interaction logic for RegisterWindow.xaml
    /// </summary>
    public partial class RegisterWindow : Window
    {
        public Action goBack;
        public RegisterWindow()
        {
            InitializeComponent();
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            goBack();
        }

        private void signUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(userNameTB.Text ?? emailTB.Text ?? password1TB.Password ?? password2TB.Password))
            {
                MessageBox.Show("Fields must be completed!");
            }
            else
            {
                if (password1TB.Password.ToString() != password2TB.Password.ToString())
                {
                    MessageBox.Show("Passwords must match!");
                }
                var countUsers = (from u in Utils.context.Users
                                  where u.Username.Equals(userNameTB.Text)
                                  select u).Count();
                if (countUsers > 0)
                {
                    MessageBox.Show("Username already taken! Please try another one.");
                }
                else
                {
                    var newUser = new User()
                    {
                        Username = userNameTB.Text,
                        HashedPassword = Utils.ComputeSha256Hash(password1TB.Password),
                        Email = emailTB.Text,
                        Rank = "Iron",
                        RankPoints = 0
                    };
                    try
                    {
                        Utils.context.Users.Add(newUser);
                        Utils.context.SaveChanges();//poti sa verifici daca asta e successful doar daca nu a aruncat exceptie
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Insertion failed: {ex.Message}");
                    }
                }

            }
        }
    }
}
