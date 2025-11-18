using SmartHomeScadaDashboard.Models;
using SmartHomeScadaDashboard.Services;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SmartHomeScadaDashboard.Forms
{
    public partial class DashboardForm : Form
    {
        private List<DeviceModel> devices;
        private DataService dataService;

        public DashboardForm()
        {
            InitializeComponent();
            InitializeDashboard();
        }

        private void InitializeDashboard()
        {
            dataService = new DataService();

            devices = new List<DeviceModel>
            {
                new DeviceModel("Front Door Lock", "Security"),
                new DeviceModel("Smoke Sensor", "Safety"),
                new DeviceModel("Thermostat", "Climate"),
                new DeviceModel("Living Room Light", "Lighting"),
                new DeviceModel("Garage Door", "Access"),
                new DeviceModel("Smart Plug", "Power"),
                new DeviceModel("Doorbell", "Security")
            };

            RefreshDeviceList();
        }

        private void RefreshDeviceList()
        {
            listDevices.Items.Clear();

            foreach (var device in devices)
            {
                string status = dataService.GetStatus(device.Name);
                device.Status = status;
                listDevices.Items.Add($"{device.Name} — {status}");
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshDeviceList();
        }
    }
}
