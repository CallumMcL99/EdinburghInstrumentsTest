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
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;

namespace EdinInstrum_Library
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class SearchPage : Page
    {
        private List<Book> searchResults = new List<Book>();
        private int selectedBook = -1;

        public SearchPage()
        {
            InitializeComponent();

            cmbBoxBranches.SelectedValuePath = "Key";
            cmbBoxBranches.DisplayMemberPath = "Value";

            cmbBoxBranches.Items.Add(new KeyValuePair<int, string>(0, "All"));
            for (int i = 1; i < Wnd.allBranches.Count + 1; i++)
            {
                cmbBoxBranches.Items.Add(new KeyValuePair<int, string>(i, Wnd.allBranches[i - 1].name));
            }

            cmbBoxBranches.SelectedIndex = 0;
        }

        public void db(string s)
        {
            System.Diagnostics.Debug.WriteLine(s);
        }

        public void Event_Search(object sender, RoutedEventArgs e)
        {
            //Clear previous search
            txtSelectedBookInfoData.Text = "No Book Selected!";
            selectedBook = -1;
            searchResults.Clear();
            lstResults.Items.Clear();

            string search = txtSearchBox.Text;
            string query = "SELECT * FROM Books WHERE Published_Year like '%" + search + "%' OR Name like '%" + search + "%'";

            if(cmbBoxBranches.SelectedIndex != 0)
            {
                query += " AND Branch_Location like '%" + cmbBoxBranches.SelectedIndex.ToString() + "%'";
            }

            MySqlCommand command = new MySqlCommand(query, Wnd.databaseConnection);
            MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                Book b = new Book(reader, Wnd.allBranches[(int)reader["Branch_Location"] - 1]);
                b.db();
                lstResults.Items.Add(b.name);
                searchResults.Add(b);
            }

            reader.Close();
        }

        public void Event_ChangeSelectedBook(object sender, SelectionChangedEventArgs e)
        {
            int i = ((ListBox)sender).SelectedIndex;

            if (i != -1)
            {
                txtSelectedBookInfoData.Text = searchResults[i].GetInfo();
                selectedBook = i;
            }
        }

    }
}
