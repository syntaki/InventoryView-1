using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace InventoryView
{
    public class InventoryViewForm : Form
    {
        private List<TreeNode> searchMatches = new List<TreeNode>();
        private TreeNode currentMatch;
        private IContainer components;
        private TreeView tv;
        private TextBox txtSearch;
        private CheckedListBox chkCharacters;
        private Label lblSearch;
        private Label lblFound;
        private Button btnSearch;
        private Button btnExpand;
        private Button btnCollapse;
        private Button btnWiki;
        private Button btnReset;
        private Button btnFindNext;
        private Button btnFindPrev;
        private Button btnScan;
        private Button btnReload;
        private ToolStripMenuItem copyTapToolStripMenuItem;
        private ToolStripMenuItem exportBranchToFileToolStripMenuItem;
        private ContextMenuStrip contextMenuStrip1;
        private Button btnExport;
        private ToolTip toolTip = new ToolTip();
        private TreeView lb1;
        private ContextMenuStrip listBox_Menu;
        private ToolStripMenuItem copyToolStripMenuItem;
        private ToolStripMenuItem wikiToolStripMenuItem;
        private ToolStripMenuItem copyAllToolStripMenuItem;
        private bool clickSearch;
        private ToolStripMenuItem wikiLookupToolStripMenuItem;
        private ToolStripMenuItem copySelectedToolStripMenuItem;
        private ListBox listBox = new ListBox();

        public bool UseShellExecute { get; private set; }

        public InventoryViewForm() => InitializeComponent();

        private void InventoryViewForm_Load(object sender, EventArgs e) => BindData();

        private void BindData()
        {
            chkCharacters.Items.Clear();
            tv.Nodes.Clear();
            // Get a distinct character list and load them into the checked list box... which is currently not shown/used.
            var characters = Class1.characterData.Select(tbl => tbl.name).Distinct().ToList();
            characters.Sort();

            // Recursively load all the items into the tree
            foreach (var character in characters)
            {
                chkCharacters.Items.Add(character, true);
                TreeNode charNode = tv.Nodes.Add(character);

                foreach (var source in Class1.characterData.Where(tbl => tbl.name == character))
                {
                    TreeNode sourceNode = charNode.Nodes.Add(source.source);
                    sourceNode.ToolTipText = sourceNode.FullPath;
                    PopulateTree(sourceNode, source.items);
                }
            }
        }


        private void PopulateTree(TreeNode treeNode, List<ItemData> itemList)
        {
            foreach (ItemData itemData in itemList)
            {
                TreeNode treeNode1 = treeNode.Nodes.Add(itemData.tap);
                treeNode1.ToolTipText = treeNode1.FullPath;
                treeNode1.Name = treeNode1.Text;  // I have no idea if this is an acceptable way to use name. Could use tag instead. This is to sync the search results
                if (itemData.items.Count<ItemData>() > 0)
                    PopulateTree(treeNode1, itemData.items);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtSearch.Text))
            {
                searchMatches.Clear();
                lb1.Nodes.Clear();
                currentMatch = (TreeNode)null;
                tv.CollapseAll();
                SearchTree(tv.Nodes);
                clickSearch = true;
            }
            btnFindNext.Visible = btnFindPrev.Visible = btnReset.Visible = (uint)searchMatches.Count<TreeNode>() > 0U;
            lblFound.Text = "Found: " + searchMatches.Count.ToString();
            if (searchMatches.Count<TreeNode>() == 0)
                return;
            btnFindNext.PerformClick();
        }

        private bool SearchTree(TreeNodeCollection nodes)
        {
            bool flag = false;
            string nodeList;
            foreach (TreeNode node in nodes)
            {
                node.BackColor = Color.White;
                if (SearchTree(node.Nodes))
                {
                    node.Expand();
                    flag = true;
                }
                bool result = node.Text.IndexOf(txtSearch.Text, StringComparison.OrdinalIgnoreCase) >= 0;
                if (result == true)
                {
                    node.Expand();
                    node.BackColor = Color.Yellow;
                    flag = true;

                    searchMatches.Add(node);

                    // Cloning the matched node - we are still using searchMatches List since it contains pointers to the original treeview
                   // New TreeNodes can't contain the same node, has to be a clone

                    TreeNode matchNode = (TreeNode)node.Clone();
                    matchNode.BackColor = Color.Empty;
                    matchNode.Text = Regex.Replace(matchNode.Text, @"\(\d+\)\s", ""); // Remove Vault ID's
                    lb1.Nodes.Add(matchNode);

                    //nodeList = node.ToString();
                    //if (nodeList.StartsWith("TreeNode: "))
                    //    nodeList = nodeList.Remove(0, 10);

                    
                    //nodeList = Regex.Replace(nodeList, @"\(\d+\)\s", "");
                    //if (nodeList[nodeList.Length - 1] == '.')
                    //    nodeList = nodeList.TrimEnd('.');
                    //lb1.Nodes.Add(nodeList);
                }
            }
            return flag;
        }

        private bool resetTree(TreeNodeCollection nodes)
        {
            bool flag = false;
            foreach (TreeNode node in nodes)
            {
                node.BackColor = Color.White;
                if (resetTree(node.Nodes))
                {
                    flag = true;
                }
                bool result = node.Text.IndexOf(txtSearch.Text, StringComparison.OrdinalIgnoreCase) >= 0;
                if (result == true)
                {
                    node.BackColor = Color.White;
                    flag = true;
                }
            }
            return flag;
        }

        private void btnExpand_Click(object sender, EventArgs e) => tv.ExpandAll();

        private void btnCollapse_Click(object sender, EventArgs e) => tv.CollapseAll();

        private void btnWiki_Click(object sender, EventArgs e)
        {                
            if (tv.SelectedNode == null)
            {
                int num = (int)MessageBox.Show("Select an item to lookup.");
            }
            else
                Process.Start(new ProcessStartInfo(string.Format("https://elanthipedia.play.net/index.php?search={0}", Regex.Replace(tv.SelectedNode.Text, @"\(\d+\)\s|\s\(closed\)", ""))) { UseShellExecute = true });
        }

        private void Wiki_Click(object sender, EventArgs e)
        {
            if (tv.SelectedNode == null)
            {
                int num = (int)MessageBox.Show("Select an item to lookup.");
            }
            else
                Process.Start(new ProcessStartInfo(string.Format("https://elanthipedia.play.net/index.php?search={0}", Regex.Replace(tv.SelectedNode.Text, @"\(\d+\)\s|\s\(closed\)", ""))) { UseShellExecute = true });
        }

        private void Listbox_Wiki_Click(object sender, EventArgs e)
        {
            if (lb1.SelectedNode == null)
            {
                int num = (int)MessageBox.Show("Select an item to lookup.");
            }
            else
                Process.Start(new ProcessStartInfo(string.Format("https://elanthipedia.play.net/index.php?search={0}", Regex.Replace(lb1.SelectedNode.Text, @"\(\d+\)\s|\s\(closed\)", ""))) { UseShellExecute = true });
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            btnSearch.PerformClick();
            btnFindNext.Visible = btnFindPrev.Visible = btnReset.Visible = clickSearch = false;
            tv.CollapseAll();
            lb1.Nodes.Clear();
            lblFound.Text = "Found: 0";
            resetTree(tv.Nodes);
            searchMatches.Clear();
            currentMatch = (TreeNode)null;
            txtSearch.Text = "";
        }

        private void btnFindPrev_Click(object sender, EventArgs e)
        {
            if (currentMatch == null)
            {
                currentMatch = searchMatches.Last<TreeNode>();
            }
            else
            {
                currentMatch.BackColor = Color.Yellow;
                int index = searchMatches.IndexOf(currentMatch) - 1;
                if (index == -1)
                    index = searchMatches.Count<TreeNode>() - 1;
                currentMatch = searchMatches[index];
            }
            currentMatch.EnsureVisible();
            currentMatch.BackColor = Color.LightBlue;
        }

        private void btnFindNext_Click(object sender, EventArgs e)
        {
            if (currentMatch == null)
            {
                currentMatch = searchMatches.First<TreeNode>();
            }
            else
            {
                currentMatch.BackColor = Color.Yellow;
                int index = searchMatches.IndexOf(currentMatch) + 1;
                if (index == searchMatches.Count<TreeNode>())
                    index = 0;
                currentMatch = searchMatches[index];
            }
            currentMatch.EnsureVisible();
            currentMatch.BackColor = Color.LightBlue;
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            Class1._host.SendText("/InventoryView scan");
            Close();
        }

        private void btnReload_Click(object sender, EventArgs e)
        {
            Class1.LoadSettings();
            Class1._host.EchoText("Inventory reloaded.");
            BindData();
        }

        private void copyTapToolStripMenuItem_Click(object sender, EventArgs e) => Clipboard.SetText(Regex.Replace(tv.SelectedNode.Text, @"\(\d+\)\s", ""));

        private void exportBranchToFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<string> branchText = new List<string>();
            branchText.Add(Regex.Replace(tv.SelectedNode.Text, @"\(\d+\)\s", ""));
            CopyBranchText(tv.SelectedNode.Nodes, branchText, 1);
            Clipboard.SetText(string.Join("\r\n", branchText.ToArray()));
        }

        private void CopyBranchText(TreeNodeCollection nodes, List<string> branchText, int level)
        {
            foreach (TreeNode node in nodes)
            {
                branchText.Add(new string('\t', level) + Regex.Replace(node.Text, @"\(\d+\)\s", ""));
                CopyBranchText(node.Nodes, branchText, level + 1);
            }
        }

        private void ListBox_Copy_Click(object sender, EventArgs e)
        {
           
            if (lb1.SelectedNode == null)
            {
                int num = (int)MessageBox.Show("Select an item to copy.");
            }
            else
            {
                //TreeView doesn't support multiple selection currently 

                //StringBuilder txt = new StringBuilder();
                //foreach (object row in lb1.SelectedNodes)
                //{
                //    txt.Append(row.ToString());
                //    txt.AppendLine();
                //}
                //txt.Remove(txt.Length - 1, 1);
                //Clipboard.SetData(System.Windows.Forms.DataFormats.Text, txt.ToString());

                Clipboard.SetData(System.Windows.Forms.DataFormats.Text, lb1.SelectedNode.Text);

            }
        }

        public void ListBox_Copy_All_Click(Object sender, EventArgs e)
        {
            if (clickSearch == false)
            {
                int num = (int)MessageBox.Show("Must search first to copy all.");
            }
            else
            {
                StringBuilder buffer = new StringBuilder();

                for (int i = 0; i < lb1.Nodes.Count; i++)
                {
                    buffer.Append(lb1.Nodes[i].Text);
                    buffer.Append("\n");
                }
                Clipboard.SetText(buffer.ToString());
            }
        }

        public void ListBox_Copy_All_Selected_Click(Object sender, EventArgs e)
        {
            if (lb1.SelectedNode == null)
            {
                int num = (int)MessageBox.Show("Select items to copy.");
            }
            else
            {

                // TreeView doesn't support multiple item selection out of the box so commenting out for now

                //StringBuilder buffer = new StringBuilder();

                //for (int i = 0; i < lb1.SelectedNodes.Count; i++)
                //{
                //    buffer.Append(lb1.SelectedItems[i].ToString());
                //    buffer.Append("\n");
                //}

                //  Clipboard.SetText(buffer.ToString());

                Clipboard.SetText(lb1.SelectedNode.Text);
            }
        }

        private void Lb1_MouseDown(object sender, MouseEventArgs e)
        {
            // don't think this works with treeview 

        //    if (Control.ModifierKeys == Keys.Control || (Control.ModifierKeys == Keys.Control || Control.ModifierKeys == Keys.ShiftKey || e.Button == MouseButtons.Left))
        //    {
        //        lb1.SelectionMode = SelectionMode.MultiExtended;
        //    }
        //    else if (e.Button == MouseButtons.Left)
        //    {
        //        lb1.SelectionMode = SelectionMode.One;
        //    }
        }

        private void Lb1_MouseUp(object sender, MouseEventArgs e)
        {
            // don't think works with treeview 

            //if (e.Button != MouseButtons.Right)
            //    return;

            //if (lb1.SelectedItem != null)
            //{
            //    if (Control.ModifierKeys == Keys.Control || (Control.ModifierKeys == Keys.Control || Control.ModifierKeys == Keys.ShiftKey || e.Button == MouseButtons.Right))
            //    {
            //        lb1.SelectionMode = SelectionMode.MultiExtended;
            //    }
            //    else if (e.Button == MouseButtons.Right)
            //    {
            //        lb1.SelectionMode = SelectionMode.One;
            //    }
            //}
        }

        private void tv_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            Point point = new Point(e.X, e.Y);
            TreeNode nodeAt = tv.GetNodeAt(point);
            if (nodeAt == null)
                return;
            tv.SelectedNode = nodeAt;
            contextMenuStrip1.Show((Control)tv, point);
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV file|*.csv";
            saveFileDialog.Title = "Save the CSV file";
            int num1 = (int)saveFileDialog.ShowDialog();
            if (!(saveFileDialog.FileName != ""))
                return;
            using (StreamWriter text = File.CreateText(saveFileDialog.FileName))
            {
                List<InventoryViewForm.ExportData> list = new List<InventoryViewForm.ExportData>();
                exportBranch(tv.Nodes, list, 1);
                text.WriteLine("Character,Tap,Path");
                foreach (InventoryViewForm.ExportData exportData in list)
                {
                    if (exportData.Path.Count >= 1)
                    {
                        if (exportData.Path.Count == 3)
                        {
                            if (((IEnumerable<string>)new string[2]
                            {
                "Vault",
                "Home"
                            }).Contains<string>(exportData.Path[1]))
                                continue;
                        }
                        text.WriteLine(string.Format("{0},{1},{2}", (object)CleanCSV(exportData.Character), (object)CleanCSV(exportData.Tap), (object)CleanCSV(string.Join("\\", (IEnumerable<string>)exportData.Path))));
                    }
                }
            }
            int num2 = (int)MessageBox.Show("Export Complete.");
        }

        private string CleanCSV(string data)
        {
            if (!data.Contains(","))
                return data;
            return !data.Contains("\"") ? string.Format("\"{0}\"", (object)data) : string.Format("\"{0}\"", (object)data.Replace("\"", "\"\""));
        }

        private void exportBranch(
          TreeNodeCollection nodes,
          List<InventoryViewForm.ExportData> list,
          int level)
        {
            foreach (TreeNode node in nodes)
            {
                InventoryViewForm.ExportData exportData = new InventoryViewForm.ExportData()
                {
                    Tap = node.Text
                };
                TreeNode treeNode = node;
                while (treeNode.Parent != null)
                {
                    treeNode = treeNode.Parent;
                    if (treeNode.Parent != null)
                        exportData.Path.Insert(0, treeNode.Text);
                }
                exportData.Character = treeNode.Text;
                list.Add(exportData);
                exportBranch(node.Nodes, list, level + 1);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tv = new System.Windows.Forms.TreeView();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.chkCharacters = new System.Windows.Forms.CheckedListBox();
            this.lblSearch = new System.Windows.Forms.Label();
            this.lblFound = new System.Windows.Forms.Label();
            this.btnSearch = new System.Windows.Forms.Button();
            this.btnExpand = new System.Windows.Forms.Button();
            this.btnCollapse = new System.Windows.Forms.Button();
            this.btnWiki = new System.Windows.Forms.Button();
            this.btnReset = new System.Windows.Forms.Button();
            this.btnFindNext = new System.Windows.Forms.Button();
            this.btnFindPrev = new System.Windows.Forms.Button();
            this.btnScan = new System.Windows.Forms.Button();
            this.btnReload = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.copyTapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportBranchToFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.wikiLookupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lb1 = new System.Windows.Forms.TreeView();
            this.listBox_Menu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wikiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copySelectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.listBox_Menu.SuspendLayout();
            this.SuspendLayout();
            // 
            // tv
            // 
            this.tv.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tv.Location = new System.Drawing.Point(5, 55);
            this.tv.Name = "tv";
            this.tv.ShowNodeToolTips = true;
            this.tv.Size = new System.Drawing.Size(646, 407);
            this.tv.TabIndex = 10;
            this.tv.MouseUp += new System.Windows.Forms.MouseEventHandler(this.tv_MouseUp);
            // 
            // txtSearch
            // 
            this.txtSearch.Location = new System.Drawing.Point(62, 18);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(262, 20);
            this.txtSearch.TabIndex = 1;
            // 
            // chkCharacters
            // 
            this.chkCharacters.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkCharacters.FormattingEnabled = true;
            this.chkCharacters.Location = new System.Drawing.Point(1241, 21);
            this.chkCharacters.Name = "chkCharacters";
            this.chkCharacters.Size = new System.Drawing.Size(136, 19);
            this.chkCharacters.TabIndex = 9;
            this.chkCharacters.Visible = false;
            // 
            // lblSearch
            // 
            this.lblSearch.AutoSize = true;
            this.lblSearch.Location = new System.Drawing.Point(12, 21);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(44, 13);
            this.lblSearch.TabIndex = 0;
            this.lblSearch.Text = "Search:";
            // 
            // lblFound
            // 
            this.lblFound.AutoSize = true;
            this.lblFound.Location = new System.Drawing.Point(353, 42);
            this.lblFound.Name = "lblFound";
            this.lblFound.Size = new System.Drawing.Size(49, 13);
            this.lblFound.TabIndex = 0;
            this.lblFound.Text = "Found: 0";
            // 
            // btnSearch
            // 
            this.btnSearch.Location = new System.Drawing.Point(353, 16);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(75, 23);
            this.btnSearch.TabIndex = 2;
            this.btnSearch.Text = "Search";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // btnExpand
            // 
            this.btnExpand.Location = new System.Drawing.Point(596, 5);
            this.btnExpand.Name = "btnExpand";
            this.btnExpand.Size = new System.Drawing.Size(75, 23);
            this.btnExpand.TabIndex = 6;
            this.btnExpand.Text = "Expand All";
            this.btnExpand.UseVisualStyleBackColor = true;
            this.btnExpand.Click += new System.EventHandler(this.btnExpand_Click);
            // 
            // btnCollapse
            // 
            this.btnCollapse.Location = new System.Drawing.Point(596, 27);
            this.btnCollapse.Name = "btnCollapse";
            this.btnCollapse.Size = new System.Drawing.Size(75, 23);
            this.btnCollapse.TabIndex = 7;
            this.btnCollapse.Text = "Collapse All";
            this.btnCollapse.UseVisualStyleBackColor = true;
            this.btnCollapse.Click += new System.EventHandler(this.btnCollapse_Click);
            // 
            // btnWiki
            // 
            this.btnWiki.Location = new System.Drawing.Point(677, 16);
            this.btnWiki.Name = "btnWiki";
            this.btnWiki.Size = new System.Drawing.Size(75, 23);
            this.btnWiki.TabIndex = 8;
            this.btnWiki.Text = "Wiki Lookup";
            this.btnWiki.UseVisualStyleBackColor = true;
            this.btnWiki.Click += new System.EventHandler(this.btnWiki_Click);
            // 
            // btnReset
            // 
            this.btnReset.Location = new System.Drawing.Point(515, 16);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(75, 23);
            this.btnReset.TabIndex = 5;
            this.btnReset.Text = "Reset";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Visible = false;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // btnFindNext
            // 
            this.btnFindNext.Location = new System.Drawing.Point(434, 27);
            this.btnFindNext.Name = "btnFindNext";
            this.btnFindNext.Size = new System.Drawing.Size(75, 23);
            this.btnFindNext.TabIndex = 4;
            this.btnFindNext.Text = "Find Next";
            this.btnFindNext.UseVisualStyleBackColor = true;
            this.btnFindNext.Visible = false;
            this.btnFindNext.Click += new System.EventHandler(this.btnFindNext_Click);
            // 
            // btnFindPrev
            // 
            this.btnFindPrev.Location = new System.Drawing.Point(434, 5);
            this.btnFindPrev.Name = "btnFindPrev";
            this.btnFindPrev.Size = new System.Drawing.Size(75, 23);
            this.btnFindPrev.TabIndex = 3;
            this.btnFindPrev.Text = "Find Prev";
            this.btnFindPrev.UseVisualStyleBackColor = true;
            this.btnFindPrev.Visible = false;
            this.btnFindPrev.Click += new System.EventHandler(this.btnFindPrev_Click);
            // 
            // btnScan
            // 
            this.btnScan.Location = new System.Drawing.Point(838, 16);
            this.btnScan.Name = "btnScan";
            this.btnScan.Size = new System.Drawing.Size(97, 23);
            this.btnScan.TabIndex = 11;
            this.btnScan.Text = "Scan Inventory";
            this.btnScan.UseVisualStyleBackColor = true;
            this.btnScan.Click += new System.EventHandler(this.btnScan_Click);
            // 
            // btnReload
            // 
            this.btnReload.Location = new System.Drawing.Point(941, 16);
            this.btnReload.Name = "btnReload";
            this.btnReload.Size = new System.Drawing.Size(97, 23);
            this.btnReload.TabIndex = 12;
            this.btnReload.Text = "Reload File";
            this.btnReload.UseVisualStyleBackColor = true;
            this.btnReload.Click += new System.EventHandler(this.btnReload_Click);
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(758, 16);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(75, 23);
            this.btnExport.TabIndex = 13;
            this.btnExport.Text = "Export";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // copyTapToolStripMenuItem
            // 
            this.copyTapToolStripMenuItem.Name = "copyTapToolStripMenuItem";
            this.copyTapToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.copyTapToolStripMenuItem.Text = "Copy Text";
            this.copyTapToolStripMenuItem.Click += new System.EventHandler(this.copyTapToolStripMenuItem_Click);
            // 
            // exportBranchToFileToolStripMenuItem
            // 
            this.exportBranchToFileToolStripMenuItem.Name = "exportBranchToFileToolStripMenuItem";
            this.exportBranchToFileToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.exportBranchToFileToolStripMenuItem.Text = "Copy Branch";
            this.exportBranchToFileToolStripMenuItem.Click += new System.EventHandler(this.exportBranchToFileToolStripMenuItem_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyTapToolStripMenuItem,
            this.exportBranchToFileToolStripMenuItem,
            this.wikiLookupToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(143, 70);
            // 
            // wikiLookupToolStripMenuItem
            // 
            this.wikiLookupToolStripMenuItem.Name = "wikiLookupToolStripMenuItem";
            this.wikiLookupToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.wikiLookupToolStripMenuItem.Text = "Wiki Lookup";
            this.wikiLookupToolStripMenuItem.Click += new System.EventHandler(this.Wiki_Click);
            // 
            // lb1
            // 
            this.lb1.AllowDrop = true;
            this.lb1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lb1.ContextMenuStrip = this.listBox_Menu;
            this.lb1.Location = new System.Drawing.Point(657, 55);
            this.lb1.Name = "lb1";
            this.lb1.ShowNodeToolTips = true;
            this.lb1.Size = new System.Drawing.Size(667, 411);
            this.lb1.TabIndex = 10;
            this.lb1.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.lb1_NodeMouseDoubleClick);
            this.lb1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Lb1_MouseUp);
            // 
            // listBox_Menu
            // 
            this.listBox_Menu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToolStripMenuItem,
            this.wikiToolStripMenuItem,
            this.copyAllToolStripMenuItem,
            this.copySelectedToolStripMenuItem});
            this.listBox_Menu.Name = "listBox_Menu";
            this.listBox_Menu.Size = new System.Drawing.Size(167, 92);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.copyToolStripMenuItem.Text = "Copy Selected";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.ListBox_Copy_Click);
            // 
            // wikiToolStripMenuItem
            // 
            this.wikiToolStripMenuItem.Name = "wikiToolStripMenuItem";
            this.wikiToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.wikiToolStripMenuItem.Text = "Wiki Selected";
            this.wikiToolStripMenuItem.Click += new System.EventHandler(this.Listbox_Wiki_Click);
            // 
            // copyAllToolStripMenuItem
            // 
            this.copyAllToolStripMenuItem.Name = "copyAllToolStripMenuItem";
            this.copyAllToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.copyAllToolStripMenuItem.Text = "Copy All";
            this.copyAllToolStripMenuItem.Click += new System.EventHandler(this.ListBox_Copy_All_Click);
            // 
            // copySelectedToolStripMenuItem
            // 
            this.copySelectedToolStripMenuItem.Name = "copySelectedToolStripMenuItem";
            this.copySelectedToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
            this.copySelectedToolStripMenuItem.Text = "Copy All Selected";
            this.copySelectedToolStripMenuItem.Click += new System.EventHandler(this.ListBox_Copy_All_Selected_Click);
            // 
            // InventoryViewForm
            // 
            this.AcceptButton = this.btnSearch;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(1426, 478);
            this.Controls.Add(this.lb1);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.btnReload);
            this.Controls.Add(this.btnScan);
            this.Controls.Add(this.btnFindNext);
            this.Controls.Add(this.btnFindPrev);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.btnWiki);
            this.Controls.Add(this.btnCollapse);
            this.Controls.Add(this.btnExpand);
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.lblSearch);
            this.Controls.Add(this.lblFound);
            this.Controls.Add(this.chkCharacters);
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.tv);
            this.Name = "InventoryViewForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Inventory View";
            this.Load += new System.EventHandler(this.InventoryViewForm_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.listBox_Menu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        public class ExportData
        {
            public string Character { get; set; }

            public string Tap { get; set; }

            public List<string> Path { get; set; } = new List<string>();
        }

        private void lb1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            //e.Node.EnsureVisible();
           currentMatch = searchMatches.Find(x => x.Name == e.Node.Name);
          //  currentMatch = searchMatches[searchMatches.IndexOf(e.Node)];
            //searchMatches.
            //if (currentMatch == null)
            //{
            //    currentMatch = searchMatches.First<TreeNode>();
            //}
            //else
            //{
            //    currentMatch.BackColor = Color.Yellow;
            //    int index = searchMatches.IndexOf(currentMatch) + 1;
            //    if (index == searchMatches.Count<TreeNode>())
            //        index = 0;
            //    currentMatch = searchMatches[index];
            //}
            currentMatch.EnsureVisible();
            //currentMatch.BackColor = Color.LightBlue;
            string message = e.Node.Name;
            MessageBox.Show(message);

        }
    }
}