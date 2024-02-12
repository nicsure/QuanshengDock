using QuanshengDock.Channels;
using QuanshengDock.ExtendedVFO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace QuanshengDock.UI
{
    internal class CtcssMenu : ContextMenu
    {   
        public CtcssMenu(bool tx) : base()
        {
            Grid grid = new();
            for (int i = 0; i < 5; i++)
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            for (int i = 0; i < 10; i++)
                grid.RowDefinitions.Add(new RowDefinition());
            int cnt = 0;
            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 5; x++)
                {
                    string s = Beautifiers.CtcssStrings[cnt];
                    MenuItem mi = new()
                    {
                        Header = s,
                        Tag = cnt
                    };
                    mi.Click += (sender, e) => 
                    {
                        if (sender is MenuItem cmi)
                        {
                            if (cmi.Tag is int i)
                            {
                                XVFO.SetCtcss(i, tx);
                            }
                        }
                    };
                    Grid.SetRow(mi, y);
                    Grid.SetColumn(mi, x);
                    grid.Children.Add(mi);
                    cnt++;
                }
            }
            this.Items.Add(grid);
        }
    }
}
