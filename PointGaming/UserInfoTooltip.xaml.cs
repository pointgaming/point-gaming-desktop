using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PointGaming
{
    /// <summary>
    /// Interaction logic for UserInfoTooltip.xaml
    /// </summary>
    public partial class UserInfoTooltip : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyChanged(string propertyName)
        {
            var changedCallback = PropertyChanged;
            if (changedCallback == null)
                return;
            var args = new PropertyChangedEventArgs(propertyName);
            changedCallback(this, args);
        }

        public UserInfoTooltip()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty UsernameProperty = DependencyProperty.Register("Username", typeof(string), typeof(UserInfoTooltip));
        public string Username
        {
            get { return this.GetValue(UsernameProperty) as string; }
            set { this.SetValue(UsernameProperty, value); }
        }

        public static readonly DependencyProperty FullNameProperty = DependencyProperty.Register("FullName", typeof(string), typeof(UserInfoTooltip));
        public string FullName
        {
            get { return this.GetValue(FullNameProperty) as string; }
            set { this.SetValue(FullNameProperty, value); }
        }

        public static readonly DependencyProperty AvatarProperty = DependencyProperty.Register("Avatar", typeof(string), typeof(UserInfoTooltip));
        public string Avatar
        {
            get { return this.GetValue(AvatarProperty) as string; }
            set { this.SetValue(AvatarProperty, value); }
        }

        public static readonly DependencyProperty PrimaryGameProperty = DependencyProperty.Register("PrimaryGame", typeof(string), typeof(UserInfoTooltip));
        public string PrimaryGame
        {
            get { return this.GetValue(PrimaryGameProperty) as string; }
            set { this.SetValue(PrimaryGameProperty, value); }
        }

        public static readonly DependencyProperty JoinDateProperty = DependencyProperty.Register("JoinDate", typeof(string), typeof(UserInfoTooltip));
        public string JoinDate
        {
            get { return this.GetValue(JoinDateProperty) as string; }
            set { this.SetValue(JoinDateProperty, value); }
        }

        public static readonly DependencyProperty PointsProperty = DependencyProperty.Register("Points", typeof(int), typeof(UserInfoTooltip));
        public int Points
        {
            get { return (int)this.GetValue(PointsProperty); }
            set { this.SetValue(PointsProperty, value); }
        }
        public string DisplayPoints { get { return this.GetValue(PointsProperty).ToString() + " Points"; } }

        public static readonly DependencyProperty TeamNameProperty = DependencyProperty.Register("TeamName", typeof(string), typeof(UserInfoTooltip));
        public string TeamName
        {
            get { return this.GetValue(TeamNameProperty) as string; }
            set { this.SetValue(TeamNameProperty, value); }
        }

        public static readonly DependencyProperty TeamSlugProperty = DependencyProperty.Register("TeamSlug", typeof(string), typeof(UserInfoTooltip));
        public string TeamSlug
        {
            get { return this.GetValue(TeamSlugProperty) as string; }
            set { this.SetValue(TeamSlugProperty, value); }
        }

        public static readonly DependencyProperty TeamAvatarProperty = DependencyProperty.Register("TeamAvatar", typeof(string), typeof(UserInfoTooltip));
        public string TeamAvatar
        {
            get { return this.GetValue(TeamAvatarProperty) as string; }
            set { this.SetValue(TeamAvatarProperty, value); }
        }

        public static readonly DependencyProperty TeamPointsProperty = DependencyProperty.Register("TeamPoints", typeof(string), typeof(UserInfoTooltip));
        public int TeamPoints
        {
            get { return (int)this.GetValue(TeamPointsProperty); }
            set { this.SetValue(TeamPointsProperty, value); }
        }

        public static readonly DependencyProperty HasTeamProperty = DependencyProperty.Register("HasTeam", typeof(bool), typeof(UserInfoTooltip));
        public bool HasTeam
        {
            get { return (bool)this.GetValue(HasTeamProperty); }
            set { this.SetValue(HasTeamProperty, value); }
        }

        public static readonly DependencyProperty CountryProperty = DependencyProperty.Register("Country", typeof(string), typeof(UserInfoTooltip));
        public string Country
        {
            get { return this.GetValue(CountryProperty) as string; }
            set { this.SetValue(CountryProperty, value); }
        }

        public static readonly DependencyProperty BioProperty = DependencyProperty.Register("Bio", typeof(string), typeof(UserInfoTooltip));
        public string Bio
        {
            get { return this.GetValue(BioProperty) as string; }
            set { this.SetValue(BioProperty, value); }
        }
    }
}
