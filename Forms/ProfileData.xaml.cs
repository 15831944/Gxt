using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Gxt;

namespace Gxt.Forms
{
    /// <summary>
    /// Interaction logic for ProfileData.xaml
    /// </summary>
    public partial class ProfileData : Window
    {
        public ProfileData()
        {
            InitializeComponent();

            this.ProfileDataGrid.ItemsSource = Commands.ReadFromNod();
        }
    }
}
