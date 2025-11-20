using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SmartHomeSCADA.SecurityModule;

namespace SmartHomeSCADA.Tests
{
    [TestClass]
    public class DoorbellTests
    {
        // These match the file names used in Doorbell.cs
        private readonly string statusFile = "doorbell_status.txt";
        private readonly string commandFile = "doorbell_cmd.txt";
        private readonly string logFile = "doorbell_log.txt";

        [TestInitialize]
        public void Setup()
        {
            // Clean files before each test
            if (File.Exists(statusFile)) File.Delete(statusFile);
            if (File.Exists(commandFile)) File.Delete(commandFile);
            if (File.Exists(logFile)) File.Delete(logFile);

            // Create fresh default files
            File.WriteAllText(statusFile, "NORMAL");
            File.WriteAllText(commandFile, "");
        }

        // 1) When status = RING and user sends ACK, it should reset to NORMAL and clear command
        [TestMethod]
        public void Acknowledge_WhenRinging_ResetsStatusToNormal()
        {
            // Arrange
            Doorbell bell = new Doorbell();
            File.WriteAllText(statusFile, "RING");   // someone pressed the bell

            // Act
            bell.Acknowledge();      // write ACK into command file
            bell.Poll();             // let doorbell process command

            // Assert
            string finalStatus = File.ReadAllText(statusFile).Trim().ToUpper();
            string finalCmd = File.ReadAllText(commandFile).Trim();

            Assert.AreEqual("NORMAL", finalStatus, "Status should be NORMAL after ACK.");
            Assert.AreEqual("", finalCmd, "Command file should be empty after ACK.");
        }

        // 2) When status = RING and user sends MUTE, it should set status to MUTED immediately
        [TestMethod]
        public void Mute_WhenRinging_SetsStatusToMuted()
        {
            // Arrange
            Doorbell bell = new Doorbell();
            File.WriteAllText(statusFile, "RING");

            // Act
            bell.Mute();   // user presses MUTE
            bell.Poll();   // process the command

            // Assert
            string statusAfterMute = File.ReadAllText(statusFile).Trim().ToUpper();
            Assert.AreEqual("MUTED", statusAfterMute, "Status should be MUTED after MUTE.");
        }

        // 3) When status = RING and no command for a short time, it should still be RING (no MISSED yet)
        [TestMethod]
        public void Poll_BeforeTimeout_KeepsRinging()
        {
            // Arrange
            Doorbell bell = new Doorbell();
            File.WriteAllText(statusFile, "RING");
            File.WriteAllText(commandFile, "");  // no ACK, no MUTE

            // Act: call Poll a few times quickly (less than 10 seconds total)
            for (int i = 0; i < 3; i++)
            {
                bell.Poll();
                Thread.Sleep(200);    // small delay so time moves
            }

            // Assert
            string status = File.ReadAllText(statusFile).Trim().ToUpper();
            Assert.AreEqual("RING", status, "Before 10 seconds, status should still be RING.");
        }
    }
}