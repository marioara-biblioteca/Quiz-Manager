using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Xml;

namespace WpfQuizManager
{
    /// <summary>
    /// Interaction logic for CareerWindow.xaml
    /// </summary>
    /// 

    public class QueryResult
    {
        public List<string> Question { get; set; }
        public List<bool> Answers { get; set; }
    }

    public partial class CareerWindow : Window, INotifyPropertyChanged
    {
        public Action goBack;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }

        }
        public CareerWindow()
        {
            InitializeComponent();
            var res = (from q in Utils.context.Quizs.Include("Users")
                       where q.User.Username == MainWindow.currentUser
                       select new
                       {
                           QuizID = q.QuizID.ToString(),
                           QuizPoints = q.TotalPoints.ToString()
                       }).ToList();

            var result = res.Select(s => new {QuizIDAndTotalPoints = s.QuizID + "\t" + s.QuizPoints }).ToList();

            quizList.ItemsSource =result;
            quizList.Columns[0].Visibility = Visibility.Collapsed;
        }
        private void quizList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (quizList.SelectedItem != null)
            {
                int selectedQuizID=Int32.Parse (quizList.SelectedCells[0].Item.ToString().Split('=')[1].Split('\t')[0]);

                var res = (from q in Utils.context.Quizs
                           where q.QuizID == selectedQuizID
                           select new
                           {
                               Question = q.Histories.Select(quest => quest.Question.Text).ToList(),
                               Answers = q.Histories.Select(answer => (bool)answer.wasAnsweredCorrectly).ToList()

                           }).ToList();
                List<object> listaBuna = new List<object>();             
                for (int i = 0; i < res[0].Question.Count; i++)
                {
                    listaBuna.Add(new { Question = res[0].Question[i], Answer = res[0].Answers[i].ToString().ToUpper() });
                }

                quizDetails.Visibility = Visibility.Visible;
                quizDetails.ItemsSource = listaBuna;

            }
        }
        private void quizDetails_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
          
        }
        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete record?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {

                foreach (var item in this.quizList.SelectedItems )
                {
                    int id = int.Parse(item.ToString().Split('=')[1].Split('\t')[0]);
                    var selectedQuiz = (from q in Utils.context.Quizs
                                        where q.QuizID == id
                                        select q).FirstOrDefault();
                    Utils.context.Quizs.Remove(selectedQuiz);
                }
                try
                {
                    Utils.context.SaveChanges();
                }
                catch(Exception ex)
                {
                    MessageBox.Show($"Exception in deleting data: {ex.Message}");
                }

            }
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            goBack();
        }

       private void ExportBtn_Click(object sender, RoutedEventArgs e)
        {
            /*Create Xml object*/
            var xmlDoc = new XmlDocument();
            xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null));
            XmlElement root = xmlDoc.CreateElement("History");
            xmlDoc.AppendChild(root);

            XmlAttribute Username = xmlDoc.CreateAttribute("User");
            Username.Value = MainWindow.currentUser;
            root.Attributes.Append(Username);


            /*Get QuestionList from current context*/
            var QuizList = (from q in Utils.context.Quizs
                            select new
                            {

                                QuizPoints = q.TotalPoints,
                                Question = q.Histories.Select(quest => quest.Question.Text),                               
                                Answers = q.Histories.Select(answer => (bool)answer.wasAnsweredCorrectly).ToList(),
                                Date = q.QuizDate,
                                QuizId = q.QuizID

                            }).ToList();

            foreach (var Quiz in QuizList)
            {
                /*Create quiz node*/
                XmlElement NewNode = xmlDoc.CreateElement("Quiz");


                /*Create attributes for node*/
                XmlAttribute QuizIdAttr = xmlDoc.CreateAttribute("QuizID");
                QuizIdAttr.Value = Quiz.QuizId.ToString();
                NewNode.Attributes.Append(QuizIdAttr);

                XmlAttribute QuizTimeAttr = xmlDoc.CreateAttribute("Date");
                QuizTimeAttr.Value = Quiz.Date.ToString();
                NewNode.Attributes.Append(QuizTimeAttr);

                XmlAttribute QuizPointsAttr = xmlDoc.CreateAttribute("TotalPoints");
                QuizPointsAttr.Value = Quiz.QuizPoints.ToString();
                NewNode.Attributes.Append(QuizPointsAttr);

                root.AppendChild(NewNode);
                int AnswerIndex = 0;
                foreach (var question in Quiz.Question)
                {
                    XmlElement QuestionNode = xmlDoc.CreateElement("Question");

                    XmlAttribute QuestionAttr = xmlDoc.CreateAttribute("Text");
                    QuestionAttr.Value = question;
                    QuestionNode.Attributes.Append(QuestionAttr);

                    XmlAttribute QuestionAnswer = xmlDoc.CreateAttribute("WasAnswerCorrectly");
                    QuestionAnswer.Value = Quiz.Answers[AnswerIndex++].ToString();
                    QuestionNode.Attributes.Append(QuestionAnswer);

                    NewNode.AppendChild(QuestionNode);
                }

            }

            string XMLfilename = MainWindow.currentUser + "_History";

            string folderName = MainWindow.currentUser;
            string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), folderName);

            if(!System.IO.Directory.Exists(path))
            {

                try
                {
                    System.IO.Directory.CreateDirectory(path);
                    path += $"\\{XMLfilename}.xml";
                    MessageBox.Show($"XML file was exported succesfully with name {XMLfilename}.xml on Desktop");
                }
                catch (Exception exception)
                {

                    Console.WriteLine("General error " + exception.Message);
                }


            }
            else
            {
                path += $"\\{XMLfilename}.xml";
                xmlDoc.Save(path);
                MessageBox.Show($"XML file was exported succesfully with name {XMLfilename}.xml on Desktop");
            }
            

            
        }
    }
}
