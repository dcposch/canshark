using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Text.RegularExpressions;
using SSCP.Telem.Receivers;
using SSCP.Telem.Can;
using SSCP.Telem.Catalog;

namespace SSCP.Telem.CanShark {
    public partial class TelemForm : Form
    {
        // model
        private TelemFormModel model;

        // view
        private TreeNode root = new TreeNode("root");
        private Thread uiUpdater;

        public TelemForm()
        {
            InitializeComponent();
            ConnectDataView();
        }

        private void timerUpdateUI_tick()
        {
            UpdateValue();
            UpdateTable();
        }

        private void ConnectDataView() {
            dataViewMessages.VirtualMode = true;
            dataViewMessages.CellValueNeeded += new DataGridViewCellValueEventHandler(dataViewMessages_CellValueNeeded);
        }

        private void dataViewMessages_CellValueNeeded(object sender, DataGridViewCellValueEventArgs args) {
            CanMessage? msg = model.GetCanMessageRow(args.RowIndex);
            if (!msg.HasValue) {
                args.Value = "";
                return;
            }
            CatalogID id = new CatalogID(msg.Value.id);
            switch (args.ColumnIndex) {
                case 0:
                    args.Value = msg.Value.utc.ToString("u");
                    break;
                case 1:
                    args.Value = string.Format("0x{0:x}", id.DevID);
                    //TODO: wtf
                    //args.Value = string.Format("0x{0:x} ({1})", id.DevID, protocol.GetDeviceName(id.DevID));
                    break;
                case 2:
                    args.Value = string.Format("0x{0:x}", id.VarID);
                    break;
                case 3:
                    args.Value = string.Format("0x{0:x16} ", msg.Value.data);
                    break;
            }
        }

        private void UpdateMeasurements(){
            CatalogRepo repo = model.CatalogRepo;
            treeView.Nodes.Clear();
            foreach (CatalogRepo.Device device in repo.GetDevices()) {
                TreeNode deviceNode = new TreeNode(device.Name);
                deviceNode.Name = device.Name;
                foreach (CatalogRepo.Var var in repo.GetVars(device.DevID)) {
                    TreeNode varNode = new TreeNode(var.Name);
                    varNode.Name = var.Name;
                    deviceNode.Nodes.Add(varNode);
                }
                treeView.Nodes.Add(deviceNode);
                deviceNode.Expand();
            }
        }


        private void model_NewValue(CanValue value) {
            if (treeView.SelectedNode == null){
                return;
            }
            string selectedVarName = treeView.SelectedNode.FullPath; //.Substring(5);
            string varName = model.CatalogRepo.GetDevice(value.var.DevID).Name 
                + "." +value.var.Name;
            if(varName == selectedVarName) {
                UpdateValue();
            }
        }

        private void UpdateTable()
        {
            //TODO
        }

        private void UpdateValue()
        {
            if (treeView.SelectedNode == root || treeView.SelectedNode == null) {
                return;
            }

            //hackhack: exclude "root."
            //Debug.Assert(treeView.SelectedNode.FullPath.StartsWith("root."));
            /*string name = treeView.SelectedNode.FullPath; //.Substring(5);
            string strValue;
            if (latestVals.ContainsKey(name))
            {
                CanValue value = latestVals[name];
                double secs = (DateTime.UtcNow - value.message.utc).TotalSeconds;
                double millisecs = secs * 1000.0;
                double mins = secs / 60.0;
                string strAge;
                if (secs < 1)
                {
                    strAge = string.Format("{0:0.0} ms", millisecs);
                }
                else if (mins < 1)
                {
                    strAge = string.Format("{0:0.0} secs", secs);
                }
                else
                {
                    strAge = string.Format("{0}:{1} mins", (int)mins, ((int)secs)%60);
                }
                strValue = string.Format("{0:0.0} {1} ({2} ago)",
                        value.value,
                        value.measurement.units,
                        strAge);
            }
            else if (treeView.SelectedNode.Nodes.Count == 0)
            {
                strValue = "no data";  
            }
            else
            {
                strValue = "";
            }

            labelName.Text = name;
            labelVal.Text = strValue;*/
        }

        private void ConnectUDP()
        {
            Log("connecting via udp multicast...");
            //TODO
        }

        private void ConnectTCP()
        {
            Log("connecting via tcp...");
            //TODO
        }

        private void ConnectCanUsb()
        {
            Log("connecting via CanUSB + LAWICEL drivers...");
            CanUsbDuplex canBus = new CanUsbDuplex();
            TryConnect(canBus);
            CatalogClient reader = new CatalogClient(canBus);
        }

        private void TryConnect(CanBus bus)
        {
            try {
                bus.Connect();
                model.CanBus = bus;
                Log("canbus connected!");
            } catch (Exception e) {
                Log("connection failed: " + e);
            }
        }

        private void Log(Object msg)
        {
            Debug.WriteLine(msg.ToString());
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            UpdateValue();
        }

        private void TelemForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Log("form closing");
            //TODO
            //model.Disconnect();
        }

        private void textBoxFilter_TextChanged(object sender, EventArgs e)
        {
            string expression = textBoxFilter.Text.Trim().ToLower();
            if (expression == "") {
                textBoxFilter.BackColor = Color.White;
            } else {
                if (model.GetPredicate(textBoxFilter.Text) != null) {
                    textBoxFilter.BackColor = Color.LightGreen;
                } else {
                    textBoxFilter.BackColor = Color.Pink;
                }
            }
        }

        private void webToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //TODO: open github url
            Process.Start("http://solarcar.stanford.edu/");
        }

        private unsafe void sendMessageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SendMessageForm form = new SendMessageForm();
            //TODO: set default destination based on current source
            DialogResult result = form.ShowDialog();
            Log("SendMessageForm closed, returned " + result);
            if (result == DialogResult.OK)
            {
                CanMessage message = form.CanMessage;
                Log("Sending "+message.ToString());
                //TODO: send, get conf, retry if necessary, inform user of success
            }
        }

        private void openLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult res = openFileDialogLog.ShowDialog();
            if (res == DialogResult.OK)
            {
                Log("opening log file " + openFileDialogLog.FileName);
                throw new NotImplementedException();
            }
        }

        private void connectUDPMulticastToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Custom UDP not yet implemented. Connecting with default settings...");
            ConnectUDP();
        }

        private void connectTCPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Custom TCP not yet implemented. Connecting with default settings...");
            ConnectTCP();
        }

        private void connectCanUSBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Custom CanUSB not yet implemented. Connecting with default settings...");
            ConnectCanUsb();
        }

    }
}
