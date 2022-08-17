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
using System.Windows.Threading;
using System.Threading;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;

namespace EdinInstrum_Library
{
    /// <summary>
    /// Interaction logic for Wnd.xaml
    /// </summary>
    /// 

    public struct Branch
    {
        public int id { get; }
        public string name { get; }
        public string address { get; }
        public int activeUsers { get; }
        public int availableBooks { get; set; }

        public Branch(MySqlDataReader input)
        {
            id = (int)input["ID"];
            name = (string)input["Name"];
            address = (string)input["Address"];
            activeUsers = (int)input["Number_Of_Active_Users"];
            availableBooks = (int)input["Number_Of_Available_Books"];
        }

        public Branch(int nID, string nName, string nAddress, int nActiveUsers, int nAvailableBooks)
        {
            id = nID;
            name = nName;
            address = nAddress;
            activeUsers = nActiveUsers;
            availableBooks = nAvailableBooks;
        }

        public void IncDecAvailableBooks(int change)
        {
            availableBooks += change;
        }

        public string GetUpdateQueryString()
        {
            return "update branches set Number_Of_available_books = " + availableBooks.ToString() + " where ID=" + id.ToString();
        }

        public void db()
        {
            System.Diagnostics.Debug.WriteLine("ID: " + id.ToString() + ". Name: " + name + ". Address: " + address + ". Active Users: " + activeUsers.ToString() + ". Available Books: " + availableBooks.ToString());
        }
    }

    public class Book
    {
        public int id { get; set; }
        public string name { get; set; }
        public int publishedYear { get; set; }
        public Branch branch { get; set; }
        public int availabilty { get; set; }

        public Book()
        {
            id = 0;
            name = "";
            publishedYear = 0;
            availabilty = 0;
        }

        //Initialise the class with data straight from the database
        public Book(MySqlDataReader input, Branch newBranch)
        {
            id = (int)input["ID"];
            name = (string)input["Name"];
            publishedYear = (int)input["Published_Year"];
            branch = newBranch;
            availabilty = (int)input["Availability"];
        }

        //Initialise the class with data stored locally
        public Book(TextBox[] editInputFields, Branch newBranch, int newID)
        {
            id = newID;
            name = editInputFields[1].Text;
            publishedYear = Int32.Parse(editInputFields[2].Text);
            availabilty = Int32.Parse(editInputFields[3].Text);
            branch = newBranch;
        }

        //Initialise the class with an instance of another Book for duplication
        public Book(Book b, int newId)
        {
            id = newId;
            name = b.name;
            publishedYear = b.publishedYear;
            availabilty = b.availabilty;
            branch = b.branch;
        }

        public string GetInfo()
        {
            return id.ToString() + "\n" + name + "\n" + publishedYear.ToString() + "\n" + branch.name + "\n" + availabilty.ToString();
        }

        public string GetUpdateQueryString()
        {
            return "update books set Name = '" + name + "', Published_Year = " + publishedYear.ToString() +", Branch_Location = " + branch.id.ToString() + ", Availability = " + availabilty.ToString() + " where ID=" + id.ToString();
        }

        public string GetInsertQueryString()
        {
            return "insert into books (ID, Name, Published_Year, Branch_Location, Availability) values (" + id.ToString() + ", '" + name + "', " + publishedYear.ToString() + ", " + branch.id.ToString() + ", " + availabilty.ToString() + ")";
        }
        public string GetRemoveQueryString()
        {
            return "DELETE FROM books WHERE ID = " + id.ToString();
        }

        public string GetUpdateIDString(int newID)
        {
            return "update books set ID = " + newID.ToString() + " where ID=" + id.ToString();

        }

        public void db()
        {
            System.Diagnostics.Debug.WriteLine("Book ID: " + id.ToString() + ". Name: " + name + ". Pub Year: " + publishedYear.ToString() + ". Branch Loc: " + branch.id.ToString() + ". Avail: " + availabilty.ToString());
        }
    }



       


    public partial class Wnd : Window
    {
        public static MySqlConnection databaseConnection = new MySqlConnection();
        public static List<Book> allBooks = new List<Book>();
        public static List<Branch> allBranches = new List<Branch>();
        public static int highestIndex = -1;

        private DispatcherTimer serverConnectionTimer = new DispatcherTimer();

        const string mainPagePath = "/MainPage.xaml",
            searchPagePath = "/SearchPage.xaml";
        string currentPage;

        public void db(string s)
        {
            System.Diagnostics.Debug.WriteLine(s);
        }

        public Wnd()
        {
            InitializeComponent();

            currentPage = mainPagePath;

            //Start a timer to retry the connection if none is found;
            serverConnectionTimer.Interval = TimeSpan.FromSeconds(1);
            serverConnectionTimer.Tick += PostLoad;
            serverConnectionTimer.Start();

            //Try to connect to the database straight away
            PostLoad(null, null);
        }

        void PostLoad(object sender, EventArgs e)
        {
            if (ConnectToDatabase()) 
            {
                //Load data from the datbase and show the main page
                PostConnected();
                serverConnectionTimer.Stop();
            } 
            else
            {
                //Show error message if there's no connection
                txtErrorNoConnection.Visibility = Visibility.Visible;
            }
        }

        void PostConnected()
        {
            //Hide the error message
            txtErrorNoConnection.Visibility = Visibility.Hidden;

            //Store the data from the databse locally
            GetAllBranches();
            GetAllBooks();

            //Show the main page
            MainFrame.Source = new Uri(mainPagePath, UriKind.RelativeOrAbsolute);
            currentPage = mainPagePath;
        }

        private bool ConnectToDatabase()
        {
            string server = "sql8.freesqldatabase.com",
                database = "sql8513351",
                username = "sql8513351",
                password = "C2m8IVWAgE",
                port = "3306",
                constring = "SERVER=" + server + ", " + port + "; DATABASE=" + database + ";UID=" + username + ";PASSWORD=" + password + ";";

            databaseConnection.ConnectionString = constring;

            try
            {
                databaseConnection.Open();
            }
            catch (MySqlException e)
            {
                db(e.Message + constring);
                return false;
            }

            return true;
        }

        private void GetAllBranches()
        {
            //Get the entire Branches table and store it locally
            string query = "select * from branches";
            MySqlCommand command = new MySqlCommand(query, databaseConnection);
            MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                allBranches.Add(new Branch(reader));
            }

            reader.Close();
        }
        
        private void GetAllBooks()
        {
            //Get the entire Books table and store it locally
            string query = "select * from books";
            MySqlCommand command = new MySqlCommand(query, databaseConnection);
            MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                allBooks.Add(new Book(reader, allBranches[(int)reader["Branch_Location"] - 1]));
                if ((int)reader["id"] > highestIndex) highestIndex = (int)reader["id"];
            }

            reader.Close();
        }

        //Executing queries where nothing will be done with the outcome
        public static void ExecuteQuery(string str)
        {
            MySqlCommand command = new MySqlCommand(str, Wnd.databaseConnection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read()) { }
            reader.Close();
        }

        //Change the current page and switch the button (top-right) text
        public void Event_SwitchPage(object sender, RoutedEventArgs e)
        {
            if(currentPage == mainPagePath)
            {
                MainFrame.Source = new Uri(searchPagePath, UriKind.RelativeOrAbsolute);
                currentPage = searchPagePath;
                btnSearch.Content = "Back to Main";
            } 
            else if (currentPage == searchPagePath)
            {
                MainFrame.Source = new Uri(mainPagePath, UriKind.RelativeOrAbsolute);
                currentPage = mainPagePath;
                btnSearch.Content = "Search";
            }
        }
    }
}
