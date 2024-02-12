using QuanshengDock.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
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

namespace QuanshengDock.UI
{
    /// <summary>
    /// Interaction logic for TextPrompt.xaml
    /// </summary>
    public partial class TextPrompt : Window
    {
        private readonly bool canBeEmpty;
        public TextPrompt(string prompt, bool canBeEmpty, string initial = "")
        {
            DataContext = Context.Instance;
            InitializeComponent();
            Prompt = prompt;
            InputText = initial;
            this.canBeEmpty = canBeEmpty;
            InputBox.Focus();
            InputBox.SelectAll();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (Mouse.DirectlyOver is Border)
                this.DragMove();
        }

        public string Prompt
        {
            get { return (string)GetValue(PromptProperty); }
            set { SetValue(PromptProperty, value); }
        }
        public static readonly DependencyProperty PromptProperty =
            DependencyProperty.Register("Prompt", typeof(string), typeof(TextPrompt), new PropertyMetadata(string.Empty));

        public string InputText
        {
            get { return (string)GetValue(InputTextProperty); }
            set { SetValue(InputTextProperty, value); }
        }
        public static readonly DependencyProperty InputTextProperty =
            DependencyProperty.Register("InputText", typeof(string), typeof(TextPrompt), new PropertyMetadata(string.Empty));

        private void Okay_Click(object sender, RoutedEventArgs e)
        {
            if (InputText.Length > 0 || canBeEmpty)
                DialogResult = true;
            else
            {
                SystemSounds.Exclamation.Play();
                InputBox.Focus();
            }

        }

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                Okay.Focus();
                e.Handled = true;
                Okay_Click(sender, e);
            }
        }
    }
}
