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
using System.Windows.Threading;
using System.Xml;

namespace WpfQuizManager
{
    /// <summary>
    /// Interaction logic for QuizWindow.xaml
    /// </summary>
    public partial class QuizWindow : Window, INotifyPropertyChanged
    {
        private int QuestionIndex = 0;

        private string question;
        public string Question
        {
            get { return question; }
            set
            {
                question = value;
                NotifyPropertyChanged();
            }
        }

        private int currenQuiztResult = 0;
        public int CurrentQuizResult
        {
            get { return currenQuiztResult; }
            set
            {
                currenQuiztResult = value;
                NotifyPropertyChanged();
            }
        }

        private List<string> answers = new List<string>();
        public List<string> Answers
        {
            get { return answers; }
            set
            {
                answers = value;
                NotifyPropertyChanged();
            }
        }

        private List<bool> answersMode = new List<bool>();
        public List<bool> AnswersMode
        {
            get { return answersMode; }
        }
        public int SelectedMode
        {
            get { return answersMode.IndexOf(true); }
        } //pentru ce e asta?

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }

        }

        private MainGameWindow parent;
        public Action Finish;
        public Action goBack;

        private XmlDocument QuizXML;
        private User user;
        private Quiz quiz;
        private int timerFlag = 1;
        private Question currentQuestion;
        private Answer correctAnswer;
        private DateTime startTime = DateTime.Now;
        private DispatcherTimer timer = new DispatcherTimer();
        public QuizWindow(MainGameWindow p)
        {
            InitializeComponent();
            parent = p;
            this.DataContext = this;

            Utils.InitTimer(ref timer, timer_Tick);

            QuizXML = new XmlDocument();
            QuizXML.AppendChild(QuizXML.CreateXmlDeclaration("1.0", "utf-8", null));
            XmlElement root = QuizXML.CreateElement("Quiz");
            QuizXML.AppendChild(root);



            user = (from u in Utils.context.Users
                    where u.Username.Equals(MainWindow.currentUser)
                    select u).FirstOrDefault();

            XmlAttribute userId = QuizXML.CreateAttribute("UserId");
            userId.Value = user.UserID.ToString();
            root.Attributes.Append(userId);

            XmlAttribute userName = QuizXML.CreateAttribute("Username");
            userName.Value = user.Username;
            root.Attributes.Append(userName);

            XmlAttribute QuizDate = QuizXML.CreateAttribute("Date");
            QuizDate.Value = DateTime.Now.ToString();
            root.Attributes.Append(QuizDate);

            XmlAttribute QuizSubcategory = QuizXML.CreateAttribute("Subcategory");
            QuizSubcategory.Value = MainGameWindow.subcategory.SubcategoryID.ToString();
            root.Attributes.Append(QuizSubcategory);

            OneQuestion(QuestionIndex);           
        }
        private void OneQuestion(int index)
        {
            int qID = Utils.questionsSetForCurrentQuiz[index]; 
            currentQuestion = (from q in Utils.context.Questions.Include("Answers")
                               where q.QuestionID == qID
                               select q).FirstOrDefault();
            answers.Clear();
            foreach(var a in currentQuestion.Answers) 
            {
                answers.Add(a.Text);
                answersMode.Add(Convert.ToBoolean(a.isTheCorrectOne));
                if (a.isTheCorrectOne==true)
                {
                    correctAnswer = a;
                }
            }
            question = currentQuestion.Text;
            startTime = DateTime.Now;
            
        }
        

        private void ImportXML(XmlDocument QuizXML)
        {
            XmlNodeList QuizDetails = QuizXML.GetElementsByTagName("Quiz");

            int UserId = Int32.Parse(QuizDetails[0].Attributes["UserId"].Value);
            int SubCategoryId = Int32.Parse(QuizDetails[0].Attributes["Subcategory"].Value);
            int TotalPoints = currenQuiztResult;
            DateTime QuizDate = DateTime.Parse(QuizDetails[0].Attributes["Date"].Value);

            var newQuiz = new Quiz()
            {
                UserID = UserId,
                SubcategoryID = SubCategoryId,
                TotalPoints = currenQuiztResult,
                QuizDate = QuizDate
            };
            try
            {
                Utils.context.Quizs.Add(newQuiz);
                Utils.context.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception adding quiz: {ex.Message}");
            }
            quiz = newQuiz;

            XmlNodeList Questions = QuizXML.GetElementsByTagName("Question");

            for (int i = 0; i < Questions.Count; i++)
            {
                var history = new History()
                {
                    QuizID = quiz.QuizID,
                    QuestionID = Int32.Parse(Questions[i].Attributes["QuestionID"].Value),
                    wasAnsweredCorrectly = Convert.ToBoolean(Int32.Parse(Questions[i].Attributes["IsCorrect"].Value))
                };
                try
                {
                    Utils.context.Histories.Add(history);
                    Utils.context.SaveChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not add History: {ex.Message}");
                }
            }
            

        }

        private void next_finish_Click(object sender, RoutedEventArgs e)
        {
            if (answ1.IsChecked == false && answ2.IsChecked == false && answ3.IsChecked == false && answ4.IsChecked == false)
            {
                MessageBox.Show("Select one answer");
                return;
            }
            var checkedAnswer = groupOfAnswers.Children.OfType<RadioButton>().FirstOrDefault(ans => ans.IsChecked.HasValue && ans.IsChecked.Value);
            var isCorrect = 0;
            List<int> points = Utils.GetNrOfPoints(currentQuestion.Difficulty);
            if (timerFlag == 1)
            {
                if (checkedAnswer.Content.ToString() == correctAnswer.Text)
                {
                    currenQuiztResult += points[0];
                    isCorrect = 1;
                }
                else
                    currenQuiztResult -= points[0];
            }
            else
            {
                //a expirat timpul, marcam intrebarea ca fiind gresita
                //isCorrect ramane 0
                currenQuiztResult -= points[0];
            }
            CurrentQuizResult = currenQuiztResult;

            XmlElement QuestionTag = QuizXML.CreateElement("Question");

            XmlAttribute QuestionIdTag = QuizXML.CreateAttribute("QuestionID");
            QuestionIdTag.Value = currentQuestion.QuestionID.ToString();
            QuestionTag.Attributes.Append(QuestionIdTag);

            XmlAttribute QuestionText = QuizXML.CreateAttribute("Text");
            QuestionText.Value = question;
            QuestionTag.Attributes.Append(QuestionText);

            XmlAttribute wasCorrectTag = QuizXML.CreateAttribute("IsCorrect");
            wasCorrectTag.Value = isCorrect.ToString();
            QuestionTag.Attributes.Append(wasCorrectTag);

            QuizXML.DocumentElement.AppendChild(QuestionTag);

            if (next_finish.Content.ToString() == "Finish")
            {

                ImportXML(QuizXML);
                try
                {
                    quiz.TotalPoints = currenQuiztResult;
                    Utils.context.SaveChanges();
                }
                catch(Exception ex)
                {
                    MessageBox.Show($"Could not update points for quiz: {ex.Message}");
                }
                MessageBox.Show("Your performance has been registered!");
                
                var pointsToCheck = MainWindow.points + currenQuiztResult;
                var rankToCheck = MainWindow.rank;
                var resultRank = (from r in Utils.context.Ranks
                              where r.LowerThreshold <= pointsToCheck && pointsToCheck < r.UppperThreshold
                              select r).FirstOrDefault();


                if (resultRank != null)
                {
                    user.Rank = resultRank.RankID;
                }
                try
                {
                    user.RankPoints = pointsToCheck;
                    Utils.context.SaveChanges();
                }
                catch(Exception ex)
                {
                    MessageBox.Show($"Could not update rank points: {ex.Message}");
                }
                parent.Points = pointsToCheck;
                parent.Rank = user.Rank;
                timer.Stop();


                goBack();

                this.Close();
                return;

            }
            //Schimbam contextul
            checkedAnswer.IsChecked = false;
            QuestionIndex++;
            OneQuestion(QuestionIndex);
            Question = question;
            Answers = answers;
            if (QuestionIndex == Utils.questionsSetForCurrentQuiz.Count - 1)
                next_finish.Content = "Finish";
        }
        
        private void Window_Closed(object sender, EventArgs e)
        {
            goBack();
        }
        void timer_Tick(object sender, EventArgs e)
        {
            string time = (DateTime.Now - startTime).ToString(@"mm\:ss");
            int seconds =Int32.Parse( time.Split(':')[1])+ Int32.Parse(time.Split(':')[0])*60;
            if (seconds > Utils.GetTimerPerRank(user.Rank.ToUpper()))
            {
                //am depasit timpul, marcam intrebarea ca fiind gresita
                timerFlag = 0;
                next_finish_Click(new object(), new RoutedEventArgs());
            }
            timerLabel.Content = time;
        }

    }
}
