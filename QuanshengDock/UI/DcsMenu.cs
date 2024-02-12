using QuanshengDock.Channels;
using QuanshengDock.ExtendedVFO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace QuanshengDock.UI
{
    internal class DcsMenu : ContextMenu
    {
        public DcsMenu(bool tx) : base()
        {
            Grid grid = new();
            for (int i = 0; i < 8; i++)
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            for (int i = 0; i < 13; i++)
                grid.RowDefinitions.Add(new RowDefinition());
            int cnt = 0;
            for (int y = 0; y < 13; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    string s = Beautifiers.DcsStrings[cnt];
                    MenuItem mi = new()
                    {
                        Header = s,
                        Tag = cnt
                    };
                    mi.Click += (sender, e) =>
                    {
                        if(sender is MenuItem cmi) 
                        {
                            if(cmi.Tag is int i)
                            {
                                XVFO.SetDcs(i, tx);
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
