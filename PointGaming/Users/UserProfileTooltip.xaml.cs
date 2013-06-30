using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace PointGaming
{
    public partial class UserProfileTooltip : UserControl
    {
        public UserProfileTooltip()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty UserProfileProperty = DependencyProperty.Register("UserProfile", typeof(string), typeof(UserProfileTooltip));
        public PgUser UserProfile
        {
            set
            {
                SetValue(UserProfileProperty, value);
                DataContext = value;
            }
        }
    }
}
