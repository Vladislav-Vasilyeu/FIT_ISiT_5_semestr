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
    public partial class MusicValueElement : UserControl
    {
        public static readonly DependencyProperty UserValueProperty = DependencyProperty.Register(
            "UserValue",
            typeof(float),
            typeof(MusicValueElement),
            new FrameworkPropertyMetadata(
                0.5f,
                FrameworkPropertyMetadataOptions.None,
                null,
                CoerceUserValue
            ),
            ValidateUserValue
        );

        public float UserValue
        {
            get => (float)GetValue(UserValueProperty);
            set => SetValue(UserValueProperty, value);
        }

        private static bool ValidateUserValue(object value)
        {
            float valueuser = (float)value;
            return valueuser >= 0.0f;
        }

        private static object CoerceUserValue(DependencyObject d, object value)
        {
            float valueuser = (float)value;
            return valueuser > 1.0f ? 1.0f : valueuser;
        }

        public MusicValueElement()
        {
            InitializeComponent();
        }
        private void ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int volume = (int)VolumeSlider.Value;
            Music.SetValue(volume);
        }
    }
}
