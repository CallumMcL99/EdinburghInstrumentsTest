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
    public partial class MainPage : Page
    {
        private TextBox[] editInputFields;
        private ComboBox editFieldBranch;

        private int selectedBook = -1;
        private bool fieldsEdited = false;

        private enum Modes { Main, Edit, AddNew }
        private static Modes mode = Modes.Main;

        public MainPage()
        {
            InitializeComponent();
            InitInputFields();

            for(int i = 0; i < Wnd.allBooks.Count; i++)
            {
                lstAllBooks.Items.Add(Wnd.allBooks[i].name);
            }
        }

        public void InitInputFields() { 
            editInputFields = new TextBox[]{ inpID, inpName, inpPubYear, inpAvail};

            editFieldBranch = inpBranchLoc;
            editFieldBranch.SelectedValuePath = "Key";
            editFieldBranch.DisplayMemberPath = "Value";

            //Add all the branches to the combo box
            for (int i = 0; i < Wnd.allBranches.Count; i++)
            {
                editFieldBranch.Items.Add(new KeyValuePair<int, string>(i + 1, Wnd.allBranches[i].name));
            }
        }
         private void ChangeModeGUI(Modes m)
         {
             switch (m)
             {
                 default:
                 case Modes.Main:
                     btnAddNewBook.IsEnabled = true;
                     btnAddSelected.IsEnabled = true;
                     btnDeleteSelected.IsEnabled = true;
                     btnEditSelected.IsEnabled = true;
                     btnDuplicateSelected.IsEnabled = true;
                    btnDeleteSelectedRecord.IsEnabled = true;
                    btnAddDeleteEdit.IsEnabled = false;
                     btnAddSaveEdit.IsEnabled = false;
                    

                     txtSelectedBookInfoData.Visibility = Visibility.Visible;

                     inpID.Visibility = Visibility.Hidden;
                     inpName.Visibility = Visibility.Hidden;
                     inpPubYear.Visibility = Visibility.Hidden;
                     inpBranchLoc.Visibility = Visibility.Hidden;
                     inpAvail.Visibility = Visibility.Hidden;

                     btnNewSave.Visibility = Visibility.Hidden;
                     btnNewDelete.Visibility = Visibility.Hidden;

                     break;

                 case Modes.Edit:
                     btnAddNewBook.IsEnabled = false;
                     btnAddSelected.IsEnabled = false;
                     btnDeleteSelected.IsEnabled = false;
                     btnEditSelected.IsEnabled = false;
                     btnDuplicateSelected.IsEnabled = false;
                    btnDeleteSelectedRecord.IsEnabled = false;
                    btnAddDeleteEdit.IsEnabled = true;
                     btnAddSaveEdit.IsEnabled = false;
                     

                     txtSelectedBookInfoData.Visibility = Visibility.Hidden;

                     inpID.Visibility = Visibility.Visible;
                     inpName.Visibility = Visibility.Visible;
                     inpPubYear.Visibility = Visibility.Visible;
                     inpBranchLoc.Visibility = Visibility.Visible;
                     inpAvail.Visibility = Visibility.Visible;

                     btnNewSave.Visibility = Visibility.Hidden;
                     btnNewDelete.Visibility = Visibility.Hidden;

                     break;

                 case Modes.AddNew:
                     btnAddNewBook.IsEnabled = false;
                     btnAddSelected.IsEnabled = false;
                     btnDeleteSelected.IsEnabled = false;
                     btnEditSelected.IsEnabled = false;
                     btnDuplicateSelected.IsEnabled = false;
                    btnDeleteSelectedRecord.IsEnabled = false;
                    btnAddDeleteEdit.IsEnabled = false;
                     btnAddSaveEdit.IsEnabled = false;

                     txtSelectedBookInfoData.Visibility = Visibility.Hidden;

                     inpID.Visibility = Visibility.Visible;
                     inpName.Visibility = Visibility.Visible;
                     inpPubYear.Visibility = Visibility.Visible;
                     inpBranchLoc.Visibility = Visibility.Visible;
                     inpAvail.Visibility = Visibility.Visible;

                     btnNewSave.Visibility = Visibility.Visible;
                     btnNewDelete.Visibility = Visibility.Visible;

                     break;
             }
         }

         public void Event_ChangeSelectedBook(object sender, SelectionChangedEventArgs e)
         {
             if(selectedBook == -1)
             {
                ChangeModeGUI(Modes.Main);
             }

             int i = ((ListBox)sender).SelectedIndex;

            //Add the selected books into to the text box
             if (i != -1)
             {
                 txtSelectedBookInfoData.Text = Wnd.allBooks[i].GetInfo();
                 selectedBook = i;
             }

             //If in the Edit or Add New mode, switch to the Main mode so the info can be seen
             if (mode != Modes.Main) ChangeModeGUI(Modes.Main);
         }

        public void Event_AddSelectedBook(object sender, RoutedEventArgs e)
        {
             if(selectedBook != -1)
             {
                //Update the local info
                Wnd.allBooks[selectedBook].availabilty += 1;
                txtSelectedBookInfoData.Text = Wnd.allBooks[selectedBook].GetInfo();

                //Update the book database
                string query = "update Books set Availability=" + (Wnd.allBooks[selectedBook].availabilty).ToString() +" where ID=" + selectedBook.ToString();
                Wnd.ExecuteQuery(query);

                //Update the branch database
                Branch oldB = Wnd.allBooks[selectedBook].branch;
                Branch newB = new Branch(oldB.id, oldB.name, oldB.address, oldB.activeUsers, oldB.availableBooks + 1);
                Wnd.allBooks[selectedBook].branch = newB;
                Wnd.ExecuteQuery(Wnd.allBooks[selectedBook].branch.GetUpdateQueryString());
             }
         }
         public void Event_RemoveSelectedBook(object sender, RoutedEventArgs e)
         {
             if (selectedBook != -1)
             {
                 if (Wnd.allBooks[selectedBook].availabilty > 0)
                 {
                    //Update the local info
                    Wnd.allBooks[selectedBook].availabilty -= 1;
                    txtSelectedBookInfoData.Text = Wnd.allBooks[selectedBook].GetInfo();

                    //Update the database
                    string query = "update Books set Availability=" + (Wnd.allBooks[selectedBook].availabilty).ToString() + " where ID=" + (selectedBook + 1).ToString();
                    Wnd.ExecuteQuery(query);

                    //Update the branch database
                    Branch oldB = Wnd.allBooks[selectedBook].branch;
                    Branch newB = new Branch(oldB.id, oldB.name, oldB.address, oldB.activeUsers, oldB.availableBooks - 1);
                    Wnd.allBooks[selectedBook].branch = newB;
                    Wnd.ExecuteQuery(Wnd.allBooks[selectedBook].branch.GetUpdateQueryString());
                }
             }
         }

        public void Event_DuplicateSelectedBook(object sender, RoutedEventArgs e)
        {
            if (selectedBook != -1)
            {
                //Create a local copy
                Book book = new Book(Wnd.allBooks[selectedBook], ++Wnd.highestIndex);
                Wnd.allBooks.Add(book);
                lstAllBooks.Items.Add(book.name);

                //Update the book database
                Wnd.ExecuteQuery(book.GetInsertQueryString());

                //Update the branch database
                Branch oldB = Wnd.allBooks[selectedBook].branch;
                Branch newB = new Branch(oldB.id, oldB.name, oldB.address, oldB.activeUsers, oldB.availableBooks + book.availabilty);
                Wnd.allBooks[selectedBook].branch = newB;
                Wnd.ExecuteQuery(Wnd.allBooks[selectedBook].branch.GetUpdateQueryString());
            }
        }
        public void Event_DeleteSelectedRecord(object sender, RoutedEventArgs e)
        {
            if (selectedBook != -1)
            {
                //Update the local copy
                lstAllBooks.Items.RemoveAt(selectedBook);
                Wnd.allBooks.RemoveAt(selectedBook);

                //Update the database
                Wnd.ExecuteQuery(Wnd.allBooks[selectedBook].GetRemoveQueryString());

                //Update the local index of all the books after the selected, so they still point to the right book
                for(int i = selectedBook; i < Wnd.allBooks.Count; i++)
                {
                    Wnd.ExecuteQuery(Wnd.allBooks[i].GetUpdateIDString(Wnd.allBooks[i].id - 1));
                    Wnd.allBooks[i].id--;
                }
            }
        }

        /*
         *  EDIT SELECTED BOOK
         */
        public void Event_EditSelectedBook(object sender, RoutedEventArgs e)
        {
            ChangeModeGUI(Modes.Edit);

            //Show the fields and fill them with the current book data
            string[] data = txtSelectedBookInfoData.Text.Split('\n');

            editInputFields[0].Text = data[0];
            editInputFields[1].Text = data[1];
            editInputFields[2].Text = data[2];
            editInputFields[3].Text = data[4];

            fieldsEdited = false;

            //Iterate all the branches to find the one with the matching name of the given data
            for (int k = 0; k < editFieldBranch.Items.Count; k++)
            {
                if (((KeyValuePair<int, string>)editFieldBranch.Items[k]).Value == data[3])
                {
                    editFieldBranch.SelectedIndex = k;
                }
            }
            mode = Modes.Edit;
        }

        public void Event_EditFieldChanged(object sender, EventArgs e)
        {
            //Handle the input fields differently based on the current mode
            if (mode == Modes.Edit)
            {
                if (!fieldsEdited)
                {
                    btnAddSaveEdit.IsEnabled = true;
                }

                fieldsEdited = true;
            } 
            else if (mode == Modes.AddNew)
            {
                if(inpName.Text.Length > 0 && inpPubYear.Text.Length > 0 && inpAvail.Text.Length > 0)
                {
                    btnNewSave.IsEnabled = true;
                } else
                {
                    btnNewSave.IsEnabled = false;
                }
            }
        }

        private static bool IsTextNumeric(string str)
        {
            Regex reg = new Regex("[^0-9]");
            return reg.IsMatch(str);
        }

        //Only allow numeric data in the input box
        public void NumericOnly(System.Object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = IsTextNumeric(e.Text);
        }

        //Only allow numeric data of 4 or less characters in the input box
        public void YearOnly(System.Object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = IsTextNumeric(e.Text) || ((TextBox)sender).Text.Length > 3;
        }

        public void Event_SaveChanges(object sender, RoutedEventArgs e)
        {
            ChangeModeGUI(Modes.Main);

            if (fieldsEdited)
            {
                //Hold old variables incase of certain changes
                int changeInAvailability = Wnd.allBooks[selectedBook].availabilty - Int32.Parse(editInputFields[3].Text);
                bool sameBranch = Wnd.allBooks[selectedBook].branch.id == Wnd.allBranches[((KeyValuePair<int, string>)editFieldBranch.SelectedItem).Key - 1].id;
                Branch oldBranch = Wnd.allBooks[selectedBook].branch;
                int oldAvailability = Wnd.allBooks[selectedBook].availabilty;

                //Edit the local copy
                Wnd.allBooks[selectedBook].name = editInputFields[1].Text;
                Wnd.allBooks[selectedBook].publishedYear = short.Parse(editInputFields[2].Text);
                Wnd.allBooks[selectedBook].availabilty = Int32.Parse(editInputFields[3].Text);
                Wnd.allBooks[selectedBook].branch = Wnd.allBranches[((KeyValuePair<int, string>)editFieldBranch.SelectedItem).Key - 1];

                //Update the displays
                txtSelectedBookInfoData.Text = Wnd.allBooks[selectedBook].GetInfo();
                lstAllBooks.Items[selectedBook] = Wnd.allBooks[selectedBook].name;

                //Update the book table
                Wnd.ExecuteQuery(Wnd.allBooks[selectedBook].GetUpdateQueryString());

                //Update the branch table
                if (!sameBranch)
                {
                    //Edit the branch which previously held the book
                    Branch newOtherB = new Branch(oldBranch.id, oldBranch.name, oldBranch.address, oldBranch.activeUsers, oldBranch.availableBooks - oldAvailability);
                    oldBranch = newOtherB;
                    Wnd.ExecuteQuery(oldBranch.GetUpdateQueryString());

                    //Edit the branch which now holds the book
                    Branch oldB = Wnd.allBooks[selectedBook].branch;
                    Branch newB = new Branch(oldB.id, oldB.name, oldB.address, oldB.activeUsers, oldB.availableBooks + Wnd.allBooks[selectedBook].availabilty);
                    Wnd.allBooks[selectedBook].branch = newB;
                    Wnd.ExecuteQuery(Wnd.allBooks[selectedBook].branch.GetUpdateQueryString());
                }
                else if(changeInAvailability != 0)
                {
                    
                    Branch oldB = Wnd.allBooks[selectedBook].branch;
                    Branch newB = new Branch(oldB.id, oldB.name, oldB.address, oldB.activeUsers, oldB.availableBooks - changeInAvailability);
                    Wnd.allBooks[selectedBook].branch = newB;
                    Wnd.ExecuteQuery(Wnd.allBooks[selectedBook].branch.GetUpdateQueryString());
                }
            }
        }
        public void Event_DeleteChanges(object sender, RoutedEventArgs e)
        {
            ChangeModeGUI(Modes.Main);
        }

        /*
         *  ADD NEW BOOK
         */

        public void Event_EnterAddNewBookMode(object sender, RoutedEventArgs e)
        {
            ChangeModeGUI(Modes.AddNew);

            //Show empty input fields. Prefil the index
            editInputFields[0].Text = (Wnd.highestIndex + 1).ToString(); ;
            editInputFields[1].Text = "";
            editInputFields[2].Text = "";
            editInputFields[3].Text = "";
            editFieldBranch.SelectedIndex = 0;

            mode = Modes.AddNew;
        }

        private void ExitAddBookMode()
        {
            ChangeModeGUI(Modes.Main);
            mode = Modes.Main;
        }

        public void Event_AddNewBook(object sender, RoutedEventArgs e)
        {
            ExitAddBookMode();

            //Add book to local books
            Book b = new Book(editInputFields, Wnd.allBranches[editFieldBranch.SelectedIndex + 1], ++Wnd.highestIndex);
            Wnd.allBooks.Add(b);
            lstAllBooks.Items.Add(b.name);

            //Add book to database
            Wnd.ExecuteQuery(b.GetInsertQueryString());
        }
        public void Event_DeleteNewBook(object sender, RoutedEventArgs e)
        {
            ExitAddBookMode();
        }
    }
}
