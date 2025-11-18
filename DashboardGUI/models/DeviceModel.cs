namespace SmartHomeScadaDashboard.Models
{
    public class DeviceModel
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public string Category { get; set; }

        public DeviceModel(string name, string category)
        {
            Name = name;
            Category = category;
            Status = "Unknown";
        }
    }
}
