using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.IO;

namespace FBMS_CumulusFlights.Controllers.Admin
{
    [Authorize(Roles = "SuperAdmin")]
    public class DatabaseController : Controller
    {
        private readonly string _connectionString = "Server=YOUR_SERVER;Database=YOUR_DB;User Id=YOUR_USER;Password=YOUR_PASSWORD;";

        public IActionResult BackupDatabase()
        {
            try
            {
                string backupDirectory = @"C:\DB_Backups\";

                // Ensure backup directory exists
                if (!Directory.Exists(backupDirectory))
                {
                    Directory.CreateDirectory(backupDirectory);
                }

                string backupFileName = $"Backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
                string backupPath = Path.Combine(backupDirectory, backupFileName);

                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = $"BACKUP DATABASE db_FBMS_CumulusFlights TO DISK = '{backupPath}'";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                TempData["Success"] = $"Database backup completed! File saved to: {backupPath}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Backup failed: " + ex.Message;
            }

            return RedirectToAction("Dashboard", "Admin");
        }


        public IActionResult RestoreDatabase(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
                {
                    TempData["Error"] = "Invalid backup file path!";
                    return RedirectToAction("Dashboard", "Admin");
                }

                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = $@"
                ALTER DATABASE db_FBMS_CumulusFlights SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                RESTORE DATABASE db_FBMS_CumulusFlights FROM DISK = '{filePath}' WITH REPLACE;
                ALTER DATABASE db_FBMS_CumulusFlights SET MULTI_USER;";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                TempData["Success"] = "Database restored successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Restore failed: " + ex.Message;
            }

            return RedirectToAction("Dashboard", "Admin");
        }

    }
}
