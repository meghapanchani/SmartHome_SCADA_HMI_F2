using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartHomeSCADA;
using System.IO;
using System.Threading;

namespace Comfort_Access.Tests
{
    [TestClass]
    public class ThermostatTests
    {
        private static readonly object _lock = new object();
        private readonly string statusFile = "thermo_status.txt";
        private readonly string commandFile = "thermo_cmd.txt";

        [TestCleanup]
        public void Cleanup()
        {
            Thread.Sleep(50); // Wait a bit before cleanup
            try
            {
                if (File.Exists(statusFile)) File.Delete(statusFile);
            }
            catch { }
            try
            {
                if (File.Exists(commandFile)) File.Delete(commandFile);
            }
            catch { }
        }

        [TestMethod]
        public void ReadStatus_ShouldReadTemperatureCorrectly()
        {
            lock (_lock)
            {
                Cleanup();
                File.WriteAllLines(statusFile, new[]
                {
                    "temp=22.5",
                    "hum=45.0",
                    "setpoint=21.0",
                    "mode=HEAT"
                });
                var thermostat = new Thermostat();

                thermostat.ReadStatus();

                Assert.AreEqual(22.5, thermostat.Temperature);
            }
        }

        [TestMethod]
        public void ReadStatus_ShouldReadHumidityCorrectly()
        {
            lock (_lock)
            {
                Cleanup();
                File.WriteAllLines(statusFile, new[]
                {
                    "temp=23.0",
                    "hum=50.5",
                    "setpoint=22.0",
                    "mode=COOL"
                });
                var thermostat = new Thermostat();

                thermostat.ReadStatus();

                Assert.AreEqual(50.5, thermostat.Humidity);
            }
        }

        [TestMethod]
        public void ReadStatus_ShouldReadSetPointCorrectly()
        {
            lock (_lock)
            {
                Cleanup();
                File.WriteAllLines(statusFile, new[]
                {
                    "temp=23.0",
                    "hum=50.5",
                    "setpoint=22.0",
                    "mode=COOL"
                });
                var thermostat = new Thermostat();

                thermostat.ReadStatus();

                Assert.AreEqual(22.0, thermostat.SetPoint);
            }
        }

        [TestMethod]
        public void ReadStatus_ShouldReadModeCorrectly()
        {
            lock (_lock)
            {
                Cleanup();
                File.WriteAllLines(statusFile, new[]
                {
                    "temp=23.0",
                    "hum=50.5",
                    "setpoint=22.0",
                    "mode=COOL"
                });
                var thermostat = new Thermostat();

                thermostat.ReadStatus();

                Assert.AreEqual("COOL", thermostat.Mode);
            }
        }

        [TestMethod]
        public void ApplyCommand_ShouldUpdateSetPoint()
        {
            lock (_lock)
            {
                Cleanup();
                File.WriteAllLines(statusFile, new[]
                {
                    "temp=22.0",
                    "hum=45.0",
                    "setpoint=20.0",
                    "mode=HEAT"
                });
                File.WriteAllLines(commandFile, new[]
                {
                    "setpoint=24.0"
                });
                var thermostat = new Thermostat();
                thermostat.ReadStatus();

                thermostat.ApplyCommand();

                Assert.AreEqual(24.0, thermostat.SetPoint);
            }
        }

        [TestMethod]
        public void ApplyCommand_ShouldUpdateMode()
        {
            lock (_lock)
            {
                Cleanup();
                File.WriteAllLines(statusFile, new[]
                {
                    "temp=22.0",
                    "hum=45.0",
                    "setpoint=20.0",
                    "mode=HEAT"
                });
                File.WriteAllLines(commandFile, new[]
                {
                    "mode=COOL"
                });
                var thermostat = new Thermostat();
                thermostat.ReadStatus();

                thermostat.ApplyCommand();

                Assert.AreEqual("COOL", thermostat.Mode);
            }
        }
    }

    [TestClass]
    public class SmartPlugTests
    {
        private static readonly object _lock = new object();
        private readonly string statusFile = "plug_status.txt";
        private readonly string commandFile = "plug_cmd.txt";

        [TestCleanup]
        public void Cleanup()
        {
            Thread.Sleep(50); // Wait a bit before cleanup
            try
            {
                if (File.Exists(statusFile)) File.Delete(statusFile);
            }
            catch { }
            try
            {
                if (File.Exists(commandFile)) File.Delete(commandFile);
            }
            catch { }
        }

        [TestMethod]
        public void ReadStatus_ShouldReturnTrue_ForOnState()
        {
            lock (_lock)
            {
                Cleanup();
                File.WriteAllText(statusFile, "state=ON");
                var plug = new SmartPlug();

                plug.ReadStatus();

                Assert.IsTrue(plug.IsOn);
            }
        }

        [TestMethod]
        public void ReadStatus_ShouldReturnFalse_ForOffState()
        {
            lock (_lock)
            {
                Cleanup();
                File.WriteAllText(statusFile, "state=OFF");
                var plug = new SmartPlug();

                plug.ReadStatus();

                Assert.IsFalse(plug.IsOn);
            }
        }

        [TestMethod]
        public void ApplyCommand_ShouldTurnPlugOn()
        {
            lock (_lock)
            {
                Cleanup();
                File.WriteAllText(statusFile, "state=OFF");
                File.WriteAllText(commandFile, "cmd=ON");
                var plug = new SmartPlug();
                plug.ReadStatus();

                plug.ApplyCommand();

                Assert.IsTrue(plug.IsOn);
            }
        }

        [TestMethod]
        public void ApplyCommand_ShouldTurnPlugOff()
        {
            lock (_lock)
            {
                Cleanup();
                File.WriteAllText(statusFile, "state=ON");
                File.WriteAllText(commandFile, "cmd=OFF");
                var plug = new SmartPlug();
                plug.ReadStatus();

                plug.ApplyCommand();

                Assert.IsFalse(plug.IsOn);
            }
        }

        [TestMethod]
        public void ApplyCommand_ShouldWriteStatusFile()
        {
            lock (_lock)
            {
                Cleanup();
                File.WriteAllText(commandFile, "cmd=ON");
                var plug = new SmartPlug();

                plug.ApplyCommand();

                Assert.IsTrue(File.Exists(statusFile));
            }
        }
    }
}