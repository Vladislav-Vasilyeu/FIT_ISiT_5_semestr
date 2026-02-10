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

namespace COURSEPROJECT
{
    public partial class CustomPasswordBox : UserControl
    {
        public static readonly DependencyProperty PasswordLengthProperty = DependencyProperty.Register(
            "PasswordLength",
            typeof(int),
            typeof(CustomPasswordBox), 
            new FrameworkPropertyMetadata(
                8, 
                FrameworkPropertyMetadataOptions.None,
                null, 
                CoercePasswordLength 
            ),
            ValidatePasswordLength
        );
        public int PasswordLength
        {
            get => (int)GetValue(PasswordLengthProperty);
            set => SetValue(PasswordLengthProperty, value);
        }
        public CustomPasswordBox() {
            InitializeComponent();
        }
        private static bool ValidatePasswordLength(object value)
        {
            int length = (int)value;
            return length >= 8;
        }
        private static object CoercePasswordLength(DependencyObject d, object value)
        {
            int length = (int)value;
            return length > 30 ? 30 : length;
        }
        public string Password
        {
            get => PasswordInput.Password;
            set => PasswordInput.Password = value;
        }
    }
}
