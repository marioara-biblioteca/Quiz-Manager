using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    /// Interaction logic for MainGameWindow.xaml
    /// </summary>
    public partial class MainGameWindow : Window, INotifyPropertyChanged
    {
        private string username;
        public string Username
        {
            get { return username; }
            set
            {
                username = value;
                NotifyPropertyChanged();
            }
        }
        private int points;
        public int Points
        {
            get { return points; }
            set
            {
                points = value;
                NotifyPropertyChanged();
            }
        }
        private string rank;
        public string Rank
        {
            get { return rank; }
            set
            {
                rank = value;
                NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }

        }

        public Action goBack;

        public static Subcategory subcategory;
        public MainGameWindow()
        {
            InitializeComponent();
            username = MainWindow.currentUser;
            points = MainWindow.points;
            rank = MainWindow.rank;

            this.DataContext = this;

            var categoriesList = (from c in Utils.context.Categories
                                  select c.CategoryName).ToList<string>();
            Categories.DataContext = categoriesList;
        }
        private void UpdateScore(int score)
        {
            pointsTextBlock.Text = score.ToString();
        }
        private void GetListOfDifficultyQuestions(string difficulty, string subcategory, int numberNeeded)
        {          
            var questionsList = (from q in Utils.context.Questions.Include("Subcategory")
                             where (q.Difficulty == difficulty && q.Subcategory.SubcategoryName.Equals(subcategory))
                             select q.QuestionID).ToList<int>();

            Utils.questionsSetForCurrentQuiz.AddRange(Utils.GetListOfRandoms(questionsList, numberNeeded));
        }
        private void GenerateQuiz()
        {
            List<int> numberOfQuestions = Utils.GetNrOfQuest(rank.ToUpper());
            Utils.questionsSetForCurrentQuiz.Clear();
            var easyQList = ((from q in Utils.context.Questions.Include("Subcategory")
                             where q.Difficulty.Equals("Easy") && q.Subcategory.SubcategoryName.Equals(subcategory.SubcategoryName)
                             orderby Guid.NewGuid()
                             select q.QuestionID).Take(numberOfQuestions[0])).ToList<int>();
            Utils.questionsSetForCurrentQuiz.AddRange(easyQList);
            var mediumQList = ((from q in Utils.context.Questions.Include("Subcategory")
                              where q.Difficulty.Equals("Medium") && q.Subcategory.SubcategoryName.Equals(subcategory.SubcategoryName)
                              orderby Guid.NewGuid()
                              select q.QuestionID).Take(numberOfQuestions[0])).ToList<int>();
            Utils.questionsSetForCurrentQuiz.AddRange(mediumQList);
            var hardQList = ((from q in Utils.context.Questions.Include("Subcategory")
                              where q.Difficulty.Equals("Hard") && q.Subcategory.SubcategoryName.Equals(subcategory.SubcategoryName)
                              orderby Guid.NewGuid()
                              select q.QuestionID).Take(numberOfQuestions[0])).ToList<int>();
            Utils.questionsSetForCurrentQuiz.AddRange(hardQList);


        }

        private void quizTimeButton_Click(object sender, RoutedEventArgs e)
        {
            if (Categories.SelectedIndex > -1 && Subcategories.SelectedIndex > -1)
            {
                GenerateQuiz();

                QuizWindow qw = new QuizWindow(this);
                qw.goBack = this.ShowWindow;
                qw.Show();
                this.Hide();
            }
            else
            {
                if (Categories.SelectedIndex < 0)
                {
                    MessageBox.Show("Please choose a category!");
                }
                else
                {
                    MessageBox.Show("Please choose a subcategory!");
                }
            }
        }

        private void CareerButton_Click(object sender, RoutedEventArgs e)
        {
            CareerWindow cw = new CareerWindow();
            cw.goBack = this.ShowWindow;
            cw.Show();
            this.Hide();
        }
        private void logOutButton_Click(object sender, RoutedEventArgs e)
        {
            goBack();
            this.Close();
        }
        private void ShowWindow()
        {
            this.Show();
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            goBack();
        }
        private void Categories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Categories.SelectedItem != null)
            {
                List<string> subcategories = (from s in Utils.context.Subcategories.Include("Categories")
                                              where s.Category.CategoryName == Categories.SelectedItem.ToString()
                                              select s.SubcategoryName).ToList();
                Subcategories.DataContext = subcategories;
            }
            else
            {
                MessageBox.Show("Please select a category!");
            }
        }
        private void Subcategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Subcategories.SelectedItem != null)
            {
                subcategory = (from s in Utils.context.Subcategories
                               where s.SubcategoryName == Subcategories.SelectedItem.ToString()
                               select s).FirstOrDefault();
            }
            else
            {
                MessageBox.Show("Please select a subcategory!");
            }
        }
        private void pointsTextBlock_TargetUpdated(object sender, DataTransferEventArgs e)
        {

          
        }

        private void performanceButton_Click(object sender, RoutedEventArgs e)
        {
            ChartWindow chw = new ChartWindow();
            chw.goBack = this.ShowWindow;
            chw.Show();
            this.Hide();
        }
    }
}
