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
    /// Interaction logic for ChartWindow.xaml
    /// </summary>
    public partial class ChartWindow : Window
    {
        public Action goBack;       
        public ChartWindow()
        {
            InitializeComponent();

            var quiezesGroupedByDate = (from q in Utils.context.Quizs.Include("User")
                      where q.User.Username == MainWindow.currentUser
                      group q by (q.QuizDate.Month*30+q.QuizDate.Day) into qz 
                      select new QuizAux
                      {
                          Date = qz.Key,
                          Points = qz.Sum(p => (int)p.TotalPoints)
                      }).ToList();
            this.DataContext = new ViewModel(quiezesGroupedByDate);
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            goBack();
        }
    }
    public class QuizAux
    {
        public int Points { get; set; }

        public int Date { get; set; }
    }
    public class ViewModel
    {
        public List<QuizAux> Data { get; set; }

        public ViewModel(List<QuizAux> list)
        {
            Data = new List<QuizAux>(list);
        }
    }
}
