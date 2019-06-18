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
using System.Text.RegularExpressions;
using System.Configuration;

namespace KeyVault
{
    enum ClipState
    {
        INIT, WAIT, READY, URL, USER, SECRET, CLEANUP
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ClipState clipState = ClipState.INIT;

        public MainWindow()
        {
            InitializeComponent();

            //this.PreviewKeyDown += new KeyEventHandler(HandleKeyInput);

            LblCount.Content = String.Format("({0:D})", DBHelper.Count());
        }

        private void clipFlow(ClipState nextState)
        {
            if (this.clipState < ClipState.READY) return;

            // First roll back current state to the previous one
            this.clipState = nextState - 1;
            this.clipFlow(false);
        }
        private void clipFlow()
        {
            this.clipFlow(false);
        }
        private void clipFlow(bool reset)
        {
            this.clipState = reset? ClipState.INIT :
                (this.clipState <= ClipState.WAIT || this.clipState == ClipState.CLEANUP) ? this.clipState : // just keep holding
                    Enum.GetValues(typeof(ClipState)).Cast<ClipState>().SkipWhile(e => e != this.clipState).Skip(1).FirstOrDefault();

            if (null != LblURL.Background) LblURL.Background = null;
            if (null != LblUser.Background) LblUser.Background = null;
            if (null != LblPswd.Background) LblPswd.Background = null;

            switch (this.clipState)
            {
                case ClipState.URL:
                    Clipboard.SetText(LblURL.Content.ToString());
                    LblURL.Background = Brushes.GreenYellow;
                    LblCount.Content = "Copied!";
                    break;
                case ClipState.USER:
                    Clipboard.SetText(LblUser.Content.ToString());
                    LblUser.Background = Brushes.GreenYellow;
                    LblCount.Content = "Copied!!";
                    break;
                case ClipState.SECRET:
                    string raw = SecretHelper.Decrypt(LblPswd.Content.ToString());
                    Clipboard.SetText(raw);
                    LblPswd.Background = Brushes.OrangeRed;
                    LblCount.Content = "Copied!!!";
                    break;
                case ClipState.CLEANUP:
                    LblCount.Content = "Cleared!";
                    Clipboard.Clear();
                    return;
            }
        }

        private void OnKeyDownHandler(object sender, System.Windows.Input.KeyEventArgs e)
        {
            TextBox inputTxt = (TextBox)sender;
            if (e.Key != System.Windows.Input.Key.Enter)
            {
                if (e.Key == System.Windows.Input.Key.Escape)
                {
                    e.Handled = true;
                    inputTxt.Text = "";
                }
                return;
            }
            else
            {
                clipFlow();
            }

            string input = inputTxt.Text;
            if (input.StartsWith("/add"))
            {
                e.Handled = true;
                inputTxt.Clear();

                Window addWindows;
                string[] args = Regex.Split(input, "\\s+", RegexOptions.IgnorePatternWhitespace);
                int len = args.Length;
                if (1 < len)
                {
                    addWindows = new AddWindow(args[1],
                        (len > 2) ? args[2] : null,
                        (len > 3) ? args[3] : null,
                        (len > 4) ? args[4] : null,
                        (len > 5) ? args[5] : null);
                }
                else
                {
                    addWindows = new AddWindow();
                }
                addWindows.Show();

#if DEBUG
                //ReadKey();
#endif
            }
        }

        private void OnTextChangedHandler(object sender, TextChangedEventArgs e)
        {
            // Reset flow
            clipFlow(true);

            TextBox obj = (TextBox)sender;
            string pattern = obj.Text;

            // Command then do nothing
            if (pattern.StartsWith("/"))
                return;

            LblCount.Content = String.Format("({0:D})", DBHelper.Count(pattern));

            var records = DBHelper.Find(pattern);
            if(records.Count > 0)
            {
                this.clipState = ClipState.READY;

                string[] record = records.First();
                LblName.Content = record[0];
                LblURL.Content = record[1];
                LblUser.Content = record[2];
                LblPswd.Content = record[3];
            }
            else
            {
                this.clipState = ClipState.WAIT;

                LblName.Content = null;
                LblURL.Content = null;
                LblUser.Content = null;
                LblPswd.Content = null;
            }
        }

        private void OnLabelDoubleClick(object sender, RoutedEventArgs e)
        {
            if (sender is Label)
            {
                var label = (Label)sender;
                switch (label.Name)
                {
                    case "LblName":
                        clipFlow(ClipState.CLEANUP);
                        break;
                    case "LblURL":
                        clipFlow(ClipState.URL);
                        break;
                    case "LblUser":
                        clipFlow(ClipState.USER);
                        break;
                    case "LblPswd":
                        clipFlow(ClipState.SECRET);
                        break;
                }                
            }
        }

        private void HandleKeyInput(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.Escape)
            //    this.Close();
        }

        private void OnClickClose(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class CloseThisWindowCommand : ICommand
    {
        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            // only close AddWindow
            return (parameter is AddWindow);
        }

        public event EventHandler CanExecuteChanged;
        public void RaiseCanExecuteChanged()
        {
            // Acutally do nothing
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Execute(object parameter)
        {
            if (this.CanExecute(parameter))
            {
                ((Window)parameter).Close();
            }
        }

        #endregion

        private CloseThisWindowCommand() {}
        public static readonly ICommand Instance = new CloseThisWindowCommand();
    }
}
