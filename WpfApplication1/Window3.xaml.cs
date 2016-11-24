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
using System.Windows.Shapes;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for Window3.xaml
    /// </summary>
    public partial class Window3 : Window
    {
        public Setting mein = new Setting();
        
        public Window3(Setting _default)
        {
            InitializeComponent();

            combo1.SelectedIndex = GetIndex(_default.Brush1);
            combo2.SelectedIndex = GetIndex(_default.Brush2);
            combo3.SelectedIndex = GetIndex(_default.Brush3);
            combo4.SelectedIndex = (_default.AutoCompleteDisplay / 1000) - 1;

            checkBox2.IsChecked = _default.IsSyntaxHighLightingEnabled;
            checkBox3.IsChecked = _default.IsAutoCompleteEnabled;
            checkBox1.IsChecked = _default.IsAddressGenerationEnabled;

            mein.Set(_default);
        }

        private Brush GetColor(int arg)
        {
            switch (arg)
            {
                case 0:
                    {
                        return Brushes.Black;
                    }

                case 1:
                    {
                        return Brushes.Blue;
                    }

                case 2:
                    {
                        return Brushes.Chartreuse;
                    }

                case 3:
                    {
                        return Brushes.Gold;
                    }

                case 4:
                    {
                        return Brushes.Gray;
                    }

                case 5:
                    {
                        return Brushes.Green;
                    }

                case 6:
                    {
                        return Brushes.Orange;
                    }

                case 7:
                    {
                        return Brushes.Red;
                    }

                case 8:
                    {
                        return Brushes.Yellow;
                    }

                case 9:
                    {
                        return Brushes.YellowGreen;
                    }

                default:
                    {
                        return Brushes.Magenta;
                    }
            }
        }

        private int   GetIndex(Brush color)
        {
            if (color == Brushes.Black)
            {
                return 0;
            }
            else if (color == Brushes.Blue)
            {
                return 1;
            }
            else if (color == Brushes.Chartreuse)
            {
                return 2;
            }
            else if (color == Brushes.Gold)
            {
                return 3;
            }
            else if (color == Brushes.Gray)
            {
                return 4;
            }
            else if (color == Brushes.Green)
            {
                return 5;
            }
            else if (color == Brushes.Orange)
            {
                return 6;
            }
            else if (color == Brushes.Red)
            {
                return 7;
            }
            else if (color == Brushes.Yellow)
            {
                return 8;
            }
            else //YellowGreen
            {
                return 9;
            }
        }

        private void checkBox2_Unchecked(object sender, RoutedEventArgs e)
        {
            Brush1.IsEnabled = false;
            Brush2.IsEnabled = false;
            Brush3.IsEnabled = false;

            combo1.IsEnabled = false;
            combo2.IsEnabled = false;
            combo3.IsEnabled = false;

            mein.IsSyntaxHighLightingEnabled = false;
        }

        private void checkBox3_Unchecked(object sender, RoutedEventArgs e)
        {
            combo4.IsEnabled   = false;
            AutoText.IsEnabled = false;

            mein.IsAutoCompleteEnabled = false;
        }

        private void checkBox2_Checked(object sender, RoutedEventArgs e)
        {
            if (Brush1 != null)
            {
                Brush1.IsEnabled = true;
                Brush2.IsEnabled = true;
                Brush3.IsEnabled = true;
                combo1.IsEnabled = true;
                combo2.IsEnabled = true;
                combo3.IsEnabled = true;

                mein.IsSyntaxHighLightingEnabled = true;
            }
        }

        private void checkBox3_Checked(object sender, RoutedEventArgs e)
        {
            combo4.IsEnabled   = true;
            if (AutoText != null) { AutoText.IsEnabled = true; }
            mein.IsAutoCompleteEnabled = true; 
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void combo4_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mein.AutoCompleteDisplay = (combo4.SelectedIndex+1)*1000;
        }

        private void combo1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mein.Brush1 = GetColor(combo1.SelectedIndex);
        }

        private void combo2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mein.Brush2 = GetColor(combo2.SelectedIndex);
        }

        private void combo3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mein.Brush3 = GetColor(combo3.SelectedIndex);
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            combo1.SelectedIndex = GetIndex(Brushes.Blue);
            combo2.SelectedIndex = GetIndex(Brushes.Orange);
            combo3.SelectedIndex = GetIndex(Brushes.Chartreuse);
            combo4.SelectedIndex = 6;

            combo1.IsEnabled = true;
            combo2.IsEnabled = true;
            combo3.IsEnabled = true;
            combo4.IsEnabled = true;

            checkBox1.IsChecked = true;
            checkBox2.IsChecked = true;
            checkBox3.IsChecked = true;

            mein = new Setting();
        }
    }
}
