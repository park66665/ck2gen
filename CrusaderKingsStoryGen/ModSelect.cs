using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CrusaderKingsStoryGen
{
    public partial class ModSelect : Form
    {
        public ModSelect()
        {
            InitializeComponent();

            String[] mods = Directory.GetFiles(Globals.ModRootDir);

            List<String> modList = new List<string>(mods);

            modList = modList.Where(m => m.EndsWith(".mod")).ToList();
            var used = Globals.Settings.Where(k => k.Key.StartsWith("Mod")).ToList();

            foreach (var m in modList)
            {
                String modN = m.Substring(m.LastIndexOf("\\") + 1).Replace(".mod", "");
                bool found = false;
                foreach (var keyValuePair in used)
                {
                    if (keyValuePair.Value == modN)
                    {
                        activeMods.Items.Add(modN);
                        found = true;
                    }
                }
                if(!found)
                    inactiveMods.Items.Add(modN);
            }
        }

        private void inactiveMods_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void inactiveMods_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Add(inactiveMods.SelectedItem.ToString());

        }

        public void Add(String str)
        {
            inactiveMods.Items.Remove(str);
            activeMods.Items.Add(str);
        }
        public void Remove(String str)
        {
            activeMods.Items.Remove(str);
            inactiveMods.Items.Add(str);
            
        }
        private void add_Click(object sender, EventArgs e)
        {
            Add(inactiveMods.SelectedItem.ToString());
        }

        private void remove_Click(object sender, EventArgs e)
        {
            Remove(activeMods.SelectedItem.ToString());
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            Remove(activeMods.SelectedItem.ToString());

        }

        private void moveUp_Click(object sender, EventArgs e)
        {
            if (activeMods.SelectedItem != null)
            {
                var item = activeMods.SelectedItem;
                int pos = activeMods.SelectedIndex;
                if (pos == 0)
                    return;

                activeMods.Items.Remove(activeMods.SelectedItem);
                
                activeMods.Items.Insert(pos-1, item.ToString());
                activeMods.SelectedItem = item;
            }
        }

        private void moveDown_Click(object sender, EventArgs e)
        {
            if (activeMods.SelectedItem != null)
            {
                var item = activeMods.SelectedItem;
                int pos = activeMods.SelectedIndex;
                if (pos == activeMods.Items.Count-1)
                    return;

                activeMods.Items.Remove(activeMods.SelectedItem);

                activeMods.Items.Insert(pos + 1, item.ToString());
                activeMods.SelectedItem = item;
            }
        }
    }
}
