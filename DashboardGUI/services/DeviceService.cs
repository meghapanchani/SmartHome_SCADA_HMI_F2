using System;

namespace SmartHomeScadaDashboard.Services
{
    public class DataService
    {
        private readonly Random random = new Random();

        // Later: replace with file reads, sockets, or DB - Whoever is responsible for that
        public string GetStatus(string deviceName)
        {
            string[] statuses = { "ON", "OFF", "Active", "Idle", "Triggered", "Locked", "Unlocked" };
            return statuses[random.Next(statuses.Length)];
        }
    }
}
