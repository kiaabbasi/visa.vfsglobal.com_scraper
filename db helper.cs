 using System;
using System.Data;
using System.Data.SQLite;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace visa.vfsglobal.com
{
    internal class dbCommand
    {
        static public string conctionStringDataBase1 = $"data source={Application.StartupPath}\\Database\\userdb.db";
        //static public string conctionStringDataBase1 = @"Flags=Default;Base Schema Name=DataBase;data source=C:\Users\HRT\source\repos\WindowsFormsApp1\bot2\Database\database.db";

        
        static public void resetdb()
        {
            MessageBox.Show("دیتابیس مجدد راه اندازی میشود");
            runQuery("DROP TABLE IF EXISTS [banks];\r\nCREATE TABLE [banks] (\r\n  [Id] bigint NOT NULL\r\n, [name] text NOT NULL\r\n, [branchName] text NOT NULL\r\n, [onUserId] text NOT NULL\r\n, CONSTRAINT [sqlite_autoindex_banks_1] PRIMARY KEY ([Id])\r\n);", conctionStringDataBase1);
            runQuery("DROP TABLE IF EXISTS [setting];\r\nCREATE TABLE [setting] (\r\n  [Id] bigint NOT NULL\r\n, [playSoundPage1] bigint DEFAULT (0) NOT NULL\r\n, [playSoundPage2] bigint DEFAULT (0) NOT NULL\r\n, [playSoundPage3] bigint DEFAULT (0) NOT NULL\r\n, [autoSmsFiller] bigint DEFAULT (0) NOT NULL\r\n, [tryNoResursCount] bigint DEFAULT (0) NOT NULL\r\n, [refreshCaptchaPerMinnet] bigint DEFAULT (0) NOT NULL\r\n, [refreshFromPerMinnet] bigint DEFAULT (0) NOT NULL\r\n, [checkBankPerSeccend] bigint DEFAULT (0) NOT NULL\r\n, [autoStartNextUser] bigint DEFAULT (0) NOT NULL\r\n, [autoBooking] bigint DEFAULT (0) NULL\r\n, [delayForTryInPage1] bigint NULL\r\n, [autoCaptchaSlover] bigint DEFAULT (1) NOT NULL\r\n, [backToPage1] bigint DEFAULT (0) NULL\r\n, CONSTRAINT [sqlite_autoindex_setting_1] PRIMARY KEY ([Id])\r\n);\r\n", conctionStringDataBase1);
            runQuery("DROP TABLE IF EXISTS [users];\r\nCREATE TABLE [users] (\r\n  [Id] bigint NOT NULL\r\n, [codeMelly] text NOT NULL\r\n, [codeMellyPhoneNumberOwner] text NULL\r\n, [province] text NULL\r\n, [city] text NULL\r\n, [bornDay] text NULL\r\n, [bornMon] text NULL\r\n, [bornYer] text NULL\r\n, [name] text NULL\r\n, [familyName] text NULL\r\n, [fatherName] text NULL\r\n, [staticPhoneNumber] text NULL\r\n, [phoneNumber] text NULL\r\n, [codeMellyHamsar] text NULL\r\n, [postalCode] text NULL\r\n, [address] text NULL\r\n, [sacrificeState] bigint NULL\r\n, [armyState] bigint NULL\r\n, [citizenshipStatus] bigint NULL\r\n, [status] text DEFAULT (5) NULL\r\n, [maryDay] text NULL\r\n, [maryMon] text NULL\r\n, [maryYer] text NULL\r\n, [rayCode] text NULL\r\n, [gender] text NULL\r\n, [email] text NULL\r\n, [banksState] bigint DEFAULT (0) NULL\r\n, [smsForwarderConnection] text NULL\r\n, [smsCount] bigint DEFAULT (0) NULL\r\n, CONSTRAINT [sqlite_autoindex_users_1] PRIMARY KEY ([Id])\r\n);\r\n", conctionStringDataBase1);
            runQuery("INSERT INTO  [setting] ([Id],[playSoundPage1],[playSoundPage2],[playSoundPage3],[autoSmsFiller],[tryNoResursCount],[refreshCaptchaPerMinnet],[refreshFromPerMinnet],[checkBankPerSeccend],[autoStartNextUser],[autoBooking],[delayForTryInPage1],[autoCaptchaSlover],[backToPage1]) VALUES (\r\n1,0,0,0,0,10,0,650,5,0,0,3,1,0);\r\n", conctionStringDataBase1);
        }
        static public int getLastIdFromUser()
        {
            DataTable dt = runQuery("Select * from users", conctionStringDataBase1);
            dt.DefaultView.Sort = "Id ASC";
            DataView sortedView = dt.DefaultView;
            int x = 0;

            foreach (DataRowView rowView in sortedView)
            {  
                x++;
            }
            if (x > 0)
            {
                
                int a = Convert.ToInt32(sortedView[x - 1][0]);
                return a+1;
            }
            return 0;
        }

        static public int getLastIdFromBanks()
        {
            DataTable dt = runQuery("Select * from banks", conctionStringDataBase1);
            dt.DefaultView.Sort = "Id ASC";
            DataView sortedView = dt.DefaultView;
            int x = 0;

            foreach (DataRowView rowView in sortedView)
            {
                x++;
            }
            if (x > 0)
            {

                int a = Convert.ToInt32(sortedView[x - 1][0]);
                return a + 1;
            }
            return 0;
        }
        private static string ExtractColumnNameFromErrorMessage(string errorMessage)
        {
            var match = Regex.Match(errorMessage, @"no such column: (\w+)");
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        private static string ExtractTableNameFromQuery(string query)
        {
            // Extract table name from UPDATE queries
            var updateMatch = Regex.Match(query, @"UPDATE\s+(\w+)", RegexOptions.IgnoreCase);
            if (updateMatch.Success)
            {
                return updateMatch.Groups[1].Value;
            }

            // Extract table name from SELECT queries
            var selectMatch = Regex.Match(query, @"FROM\s+(\w+)", RegexOptions.IgnoreCase);
            return selectMatch.Success ? selectMatch.Groups[1].Value : string.Empty;
        }
        private static void AddMissingColumn(string connectionString, string tableName, string columnName)
        {
            string alterTableQuery = $"ALTER TABLE {tableName} ADD COLUMN {columnName} INTEGER DEFAULT 0";

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (var command = new SQLiteCommand(alterTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
        public static string ExtractTableFromEroor(string eroor)
        {
            string pattern = @"table\s+(\w+)";
            Match match = Regex.Match(eroor, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return null;
        }


        public static string ExtractColumnNameFromErrorMessage2(string errorMessage)
        {
            string pattern = @"column named (\w+)";
            Match match = Regex.Match(errorMessage, pattern);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return null;
        }


    static public DataTable runQuery(string query, string cs, bool showErrors = true)
        {
            const int maxRetries = 5;
            const int retryDelayMilliseconds = 1000; // Adjust the delay as needed

            for (int retryCount = 0; retryCount < maxRetries; retryCount++)
            {
                try
                {
                    using (var connection = new SQLiteConnection(cs))
                    {
                        connection.Open();

                        using (var command = new SQLiteCommand(query, connection))
                        {
                            using (var adapter = new SQLiteDataAdapter(command))
                            {
                                DataTable dataTable = new DataTable();
                                adapter.Fill(dataTable);
                                return dataTable;
                            }
                        }
                    }
                }
                catch (SQLiteException ex)
                {
                    if (ex.ErrorCode == (int)SQLiteErrorCode.Locked || ex.ErrorCode == (int)SQLiteErrorCode.Busy)
                    {
                        // Database is locked or busy, retry after a delay
                        Thread.Sleep(retryDelayMilliseconds);
                    }
                    else if (ex.ErrorCode == (int)SQLiteErrorCode.Error && (ex.Message.Contains("no such column:") ||ex.Message.Contains("has no column named") || ex.Message.Contains("does not belong to table")))
                    {
                        // Extract the column name from the error message
                        string columnName = ExtractColumnNameFromErrorMessage(ex.Message);

                        // Extract the table name from the query
                        string tableName = ExtractTableNameFromQuery(query);

                        // Add the missing column to the table
                        if (!string.IsNullOrEmpty(columnName) && !string.IsNullOrEmpty(tableName))
                        {
                            AddMissingColumn(cs, tableName, columnName);
                        }
                        else
                        {
                             columnName = ExtractColumnNameFromErrorMessage2(ex.Message);

                         
                             tableName = ExtractTableFromEroor(ex.Message);
                            if (!string.IsNullOrEmpty(columnName) && !string.IsNullOrEmpty(tableName))
                            {
                                AddMissingColumn(cs, tableName, columnName);
                            }
                        }
                        // After adding the column, retry the query
                        Thread.Sleep(retryDelayMilliseconds); // Small delay before retrying
                        return runQuery(query, cs, showErrors);
                    }
                    else
                    {
                        if (showErrors)
                            MessageBox.Show(ex.Message);
                        resetdb();
                        return new DataTable(); // Return an empty DataTable in case of error
                    }
                }
                catch (Exception e)
                {
                    if (showErrors)
                        MessageBox.Show(e.Message);
                    return new DataTable(); // Return an empty DataTable in case of error
                }
            }
            return new DataTable(); // Return an empty DataTable if all retries fail
        }


        static public bool isExistTable(string name, string cs)
        {
            bool x = false;
            DataTable dt = runQuery("SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName", cs);
            foreach (DataRow row in dt.Rows)
            {
                if (row["name"].ToString() == name)
                    x = true;
            }

            return x;
        }

        static public bool isExistColumn(string columnName, string tableName, string cs)
        {
            bool x = false;
            DataTable dt = runQuery($"PRAGMA table_info({tableName})", cs);
            foreach (DataRow row in dt.Rows)
            {
                if (row["name"].ToString() == columnName)
                    x = true;
            }
            return x;
        }

        static public bool isExistValue(string id, string tableName, string cs)
        {
            DataTable dt = runQuery($"SELECT * FROM {tableName} WHERE [itemID] = '{id}'", cs, false);
            return dt.Rows.Count > 0;
        }
    }
}
