using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SmartHomeSCADA.SecurityModule;   // DoorLock class

namespace SmartHomeSCADA.Tests
{
    [TestClass]
    public class DoorLockTests
    {
        // These filenames MUST match DoorLock.cs
        private readonly string statusFile = "lock_status.txt";
        private readonly string commandFile = "lock_cmd.txt";
        private readonly string logFile = "lock_log.txt";
        private readonly string alertFile = "lock_alert.txt";

        [TestInitialize]
        public void Setup()
        {
            // Clean files before each test
            if (File.Exists(statusFile)) File.Delete(statusFile);
            if (File.Exists(commandFile)) File.Delete(commandFile);
            if (File.Exists(logFile)) File.Delete(logFile);
            if (File.Exists(alertFile)) File.Delete(alertFile);

            // Just to be safe, start fresh
            File.WriteAllText(statusFile, "LOCKED");
            File.WriteAllText(commandFile, "");
            File.WriteAllText(alertFile, "NONE");
        }

        // 1) When door is UNLOCKED and we send LOCK, it should become LOCKED
        [TestMethod]
        public void Lock_WhenUnlocked_SetsStatusToLocked()
        {
            // Arrange
            File.WriteAllText(statusFile, "UNLOCKED");   // starting state
            DoorLock doorLock = new DoorLock();

            // Act
            doorLock.Lock();   // write LOCK into lock_cmd.txt
            doorLock.Poll();   // process the command

            // Assert
            string finalStatus = File.ReadAllText(statusFile).Trim().ToUpper();
            Assert.AreEqual("LOCKED", finalStatus, "Status should be LOCKED after Lock() + Poll().");
        }

        // 2) When door is LOCKED and we send UNLOCK, it should become UNLOCKED
        [TestMethod]
        public void Unlock_WhenLocked_SetsStatusToUnlocked()
        {
            // Arrange
            File.WriteAllText(statusFile, "LOCKED");
            DoorLock doorLock = new DoorLock();

            // Act
            doorLock.Unlock();   // write UNLOCK into lock_cmd.txt
            doorLock.Poll();     // process

            // Assert
            string finalStatus = File.ReadAllText(statusFile).Trim().ToUpper();
            Assert.AreEqual("UNLOCKED", finalStatus, "Status should be UNLOCKED after Unlock() + Poll().");
        }

        // 3) Toggle should flip LOCKED -> UNLOCKED and UNLOCKED -> LOCKED
        [TestMethod]
        public void Toggle_ChangesStateBetweenLockedAndUnlocked()
        {
            // Arrange: start locked
            File.WriteAllText(statusFile, "LOCKED");
            DoorLock doorLock = new DoorLock();

            // Act 1: toggle once -> expect UNLOCKED
            doorLock.Toggle();
            doorLock.Poll();
            string afterFirstToggle = File.ReadAllText(statusFile).Trim().ToUpper();
            Assert.AreEqual("UNLOCKED", afterFirstToggle, "First toggle should set status to UNLOCKED.");

            // Act 2: toggle again -> expect LOCKED
            doorLock.Toggle();
            doorLock.Poll();
            string afterSecondToggle = File.ReadAllText(statusFile).Trim().ToUpper();
            Assert.AreEqual("LOCKED", afterSecondToggle, "Second toggle should set status back to LOCKED.");
        }

        // 4) Forced entry should put door into ALERT state and set alert file
        [TestMethod]
        public void ReportForcedEntry_SetsAlertAndAlertFile()
        {
            // Arrange
            File.WriteAllText(statusFile, "LOCKED");   // assume door was locked
            File.WriteAllText(alertFile, "NONE");
            DoorLock doorLock = new DoorLock();

            // Act
            doorLock.ReportForcedEntry();

            // Assert
            string finalStatus = File.ReadAllText(statusFile).Trim().ToUpper();
            string finalAlert = File.ReadAllText(alertFile).Trim().ToUpper();

            Assert.AreEqual("ALERT", finalStatus, "Status should be ALERT after forced entry.");
            Assert.AreEqual("FORCED_ENTRY", finalAlert, "Alert file should say FORCED_ENTRY.");
        }

        // 5) Clearing alert should reset alert file to NONE and lock the door
        [TestMethod]
        public void ClearAlert_ResetsAlertAndLocksDoor()
        {
            // Arrange: simulate that forced entry has already happened
            File.WriteAllText(statusFile, "ALERT");
            File.WriteAllText(alertFile, "FORCED_ENTRY");
            DoorLock doorLock = new DoorLock();

            // Act
            doorLock.ClearAlert();

            // Assert
            string finalStatus = File.ReadAllText(statusFile).Trim().ToUpper();
            string finalAlert = File.ReadAllText(alertFile).Trim().ToUpper();

            Assert.AreEqual("LOCKED", finalStatus, "After clearing alert, door should be LOCKED for safety.");
            Assert.AreEqual("NONE", finalAlert, "Alert file should be NONE after ClearAlert().");
        }

        // 6) While in ALERT state, normal commands should be ignored
        [TestMethod]
        public void Poll_IgnoresCommandsWhenInAlertState()
        {
            // Arrange: door is in ALERT, with FORCED_ENTRY
            File.WriteAllText(statusFile, "ALERT");
            File.WriteAllText(alertFile, "FORCED_ENTRY");
            File.WriteAllText(commandFile, "UNLOCK");   // someone tries to unlock
            DoorLock doorLock = new DoorLock();

            // Act
            doorLock.Poll();   // should ignore UNLOCK because of ALERT state

            // Assert
            string finalStatus = File.ReadAllText(statusFile).Trim().ToUpper();
            Assert.AreEqual("ALERT", finalStatus, "Status must remain ALERT even if UNLOCK command is present.");
        }

        // 7) If door is already LOCKED and we send LOCK again, it should stay LOCKED
        [TestMethod]
        public void Lock_WhenAlreadyLocked_KeepsLockedState()
        {
            // Arrange
            File.WriteAllText(statusFile, "LOCKED");
            DoorLock doorLock = new DoorLock();

            // Act
            doorLock.Lock();
            doorLock.Poll();

            // Assert
            string finalStatus = File.ReadAllText(statusFile).Trim().ToUpper();
            Assert.AreEqual("LOCKED", finalStatus, "Status should remain LOCKED if it was already locked.");
        }

        // 8) If door is already UNLOCKED and we send UNLOCK again, it should stay UNLOCKED
        [TestMethod]
        public void Unlock_WhenAlreadyUnlocked_KeepsUnlockedState()
        {
            // Arrange
            File.WriteAllText(statusFile, "UNLOCKED");
            DoorLock doorLock = new DoorLock();

            // Act
            doorLock.Unlock();
            doorLock.Poll();

            // Assert
            string finalStatus = File.ReadAllText(statusFile).Trim().ToUpper();
            Assert.AreEqual("UNLOCKED", finalStatus, "Status should remain UNLOCKED if it was already unlocked.");
        }

        // 9) Invalid command in lock_cmd.txt should be ignored (no state change)
        [TestMethod]
        public void Poll_IgnoresInvalidCommand()
        {
            // Arrange
            File.WriteAllText(statusFile, "LOCKED");
            File.WriteAllText(commandFile, "ABC123");   // invalid command
            DoorLock doorLock = new DoorLock();

            // Act
            doorLock.Poll();

            // Assert
            string finalStatus = File.ReadAllText(statusFile).Trim().ToUpper();
            string finalCmd = File.ReadAllText(commandFile).Trim();  // we didn't clear invalid command in code
            Assert.AreEqual("LOCKED", finalStatus, "Invalid command must not change the lock status.");
            // (We don't assert on finalCmd strictly, because our implementation does not handle invalid clearing.)
        }

    }
}
