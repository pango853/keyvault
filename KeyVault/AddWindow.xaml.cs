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
using System.Data.SQLite;
using System.Data.Common;

namespace KeyVault
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class AddWindow : Window
    {
        public AddWindow()
        {
            InitializeComponent();
        }
        public AddWindow(string name, string url = null, string user = null, string pswd = null, string note = null) : this()
        {
            if (!String.IsNullOrEmpty(name))
                nameTxt.Text = name;
            if (!String.IsNullOrEmpty(url))
                urlTxt.Text = url;
            if (!String.IsNullOrEmpty(user))
                userTxt.Text = user;
            if (!String.IsNullOrEmpty(pswd))
                passwordBox.Password = pswd;
            if (!String.IsNullOrEmpty(note))
                noteTxt.Text = note;
        }

        private void AddRecord(object sender, RoutedEventArgs e)
        {
            string pswd = passwordBox.Password;
            string confirm = passwordBox2.Password;

            if( !pswd.Equals(confirm) )
            {
                MessageBox.Show("Entered Password is not matching!", "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            string name = nameTxt.Text;
            string url = urlTxt.Text;
            string user = userTxt.Text;
            string note = noteTxt.Text;

            try
            {
                DBHelper.AddRecord(name, url, user, pswd, note);
                this.Close();
            }
            catch (DBInsertServiceException ex)
            {
                MessageBox.Show(ex.Message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (DBInsertKeypairException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void CloseMe(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
