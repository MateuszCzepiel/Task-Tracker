using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SqlClient;
using System.Data.SQLite;
using System.Security.Principal;
using Newtonsoft.Json;
using System.Security.Policy;

namespace Database_Server
{
    internal class Database
    {
        public static string databaseName = "TaskTrackerDatabase.db";
        public static void InitialiseDatabase()
        {
            if (!System.IO.File.Exists(databaseName))
            {
                SQLiteConnection.CreateFile(databaseName);
            }

            using (SQLiteConnection databaseConnection = new SQLiteConnection("Data Source=" + databaseName))
            {
                databaseConnection.Open();
                var databaseCommand = new SQLiteCommand(databaseConnection);
                databaseCommand.CommandText = "CREATE TABLE IF NOT EXISTS Accounts (Id INTEGER PRIMARY KEY, Username TEXT NOT NULL UNIQUE, Email TEXT NOT NULL, Password TEXT NOT NULL, ProjectID TEXT)";
                databaseCommand.ExecuteNonQuery();
                databaseCommand.CommandText = "CREATE TABLE IF NOT EXISTS Projects (Id INTEGER PRIMARY KEY, ProjectNames TEXT NOT NULL, TasksID)";
                databaseCommand.ExecuteNonQuery();
                databaseCommand.CommandText = "CREATE TABLE IF NOT EXISTS Tasks (Id INTEGER PRIMARY KEY, Task TEXT, Due TEXT, Done TEXT, Priority TEXT)";
                databaseCommand.ExecuteNonQuery();
            }
        }
        public static bool CreateAccount(string username, string email, string password)
        {
            using (SQLiteConnection databaseConnection = new SQLiteConnection("Data Source=" + databaseName))
            {
                databaseConnection.Open();
                using (SQLiteCommand databaseCommand = new SQLiteCommand("SELECT ID FROM Accounts ORDER BY ID DESC LIMIT 1", databaseConnection))
                {
                    try
                    {
                        object lastID = databaseCommand.ExecuteScalar();
                        int nextID = Convert.ToInt32(lastID) + 1;
                        SQLiteCommand addCommand = new SQLiteCommand($"INSERT INTO Accounts (Id, Username, Email, Password) VALUES (@id, @username, @email, @password)", databaseConnection);
                        addCommand.Parameters.AddWithValue("@id", nextID);
                        addCommand.Parameters.AddWithValue("@username", username);
                        addCommand.Parameters.AddWithValue("@email", email);
                        addCommand.Parameters.AddWithValue("@password", Functions.Hash(password));
                        addCommand.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(username);
                        Console.WriteLine(e.Message);
                        return false;
                    }
                }
                return true;
            }
        }
        public static bool FindAccount(string username)
        {
            using (SQLiteConnection databaseConnection = new SQLiteConnection("Data Source=" + databaseName))
            {
                databaseConnection.Open();
                using (SQLiteCommand findAccountCommand = new SQLiteCommand("SELECT * FROM Accounts WHERE Username = @Username", databaseConnection))
                {
                    findAccountCommand.Parameters.AddWithValue("@Username", username);
                    using (SQLiteDataReader reader = findAccountCommand.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            return true;
                        }
                        return false;
                    }
                }
            }
        }
        public static bool VerifyPassword(string username, string password)
        {
            using (SQLiteConnection databaseConnection = new SQLiteConnection("Data Source=" + databaseName))
            {
                databaseConnection.Open();
                using (SQLiteCommand getPasswordCommand = new SQLiteCommand("SELECT Password FROM Accounts WHERE Username = @Username", databaseConnection))
                {
                    getPasswordCommand.Parameters.AddWithValue("@Username", username);
                    using (SQLiteDataReader reader = getPasswordCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string StoredHash = reader["Password"].ToString();
                            if (Functions.Hash(password) == StoredHash)
                            {
                                return true;
                            }
                        }
                        return false;
                    }
                }
            }
        }
        public static bool CreateProject(string username, string projectName)
        {
            int nextID = 0;
            using (SQLiteConnection databaseConnection = new SQLiteConnection("Data Source=" + databaseName))
            {
                databaseConnection.Open();
                using (SQLiteCommand selectIDCommand = new SQLiteCommand("SELECT ID FROM Projects ORDER BY ID DESC LIMIT 1", databaseConnection))
                {
                    try
                    {
                        object lastID = selectIDCommand.ExecuteScalar();
                        nextID = Convert.ToInt32(lastID) + 1;
                        SQLiteCommand insertCommand = new SQLiteCommand($"INSERT INTO Projects (Id, ProjectNames) VALUES (@id, @projectname)", databaseConnection);
                        insertCommand.Parameters.AddWithValue("@id", nextID);
                        insertCommand.Parameters.AddWithValue("@projectname", projectName);
                        insertCommand.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }

                using (SQLiteCommand selectProjectIDCommand = new SQLiteCommand("SELECT ProjectID FROM Accounts WHERE Username = @accountusername", databaseConnection))
                {
                    selectProjectIDCommand.Parameters.AddWithValue("@accountusername", username);
                    using (SQLiteDataReader reader = selectProjectIDCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var receivedProjectID = reader["ProjectID"].ToString();
                            reader.Close();

                            if (receivedProjectID != "")
                            {
                                var deserializedDataList = JsonConvert.DeserializeObject<List<object>>(receivedProjectID);
                                deserializedDataList.Add((object)nextID);
                                var serializedDataList = JsonConvert.SerializeObject(deserializedDataList);
                                using (SQLiteCommand updateProjectID = new SQLiteCommand($"UPDATE Accounts SET ProjectID = @projectid WHERE Username = @accountusername", databaseConnection))
                                {
                                    updateProjectID.Parameters.AddWithValue("@projectid", serializedDataList);
                                    updateProjectID.Parameters.AddWithValue("@accountusername", username);

                                    int rowsAffected = updateProjectID.ExecuteNonQuery();
                                    if (rowsAffected == 1)
                                    {
                                        Console.WriteLine("Data updated successfully.");
                                        return true;
                                    }
                                    else
                                    {
                                        Console.WriteLine("No data was updated.");
                                        return false;
                                    }
                                }
                            }
                            else
                            {
                                List<object> projectID = new List<object>() { nextID };
                                var serializedDataList = JsonConvert.SerializeObject(projectID);
                                using (SQLiteCommand updateProjectID = new SQLiteCommand($"UPDATE Accounts SET ProjectID = @projectid WHERE Username = @accountusername", databaseConnection))
                                {
                                    updateProjectID.Parameters.AddWithValue("@projectid", serializedDataList);
                                    updateProjectID.Parameters.AddWithValue("@accountusername", username);

                                    int rowsAffected = updateProjectID.ExecuteNonQuery();
                                    if (rowsAffected == 1)
                                    {
                                        Console.WriteLine("Data inserted successfully.");
                                        return true;
                                    }
                                    else
                                    {
                                        Console.WriteLine("No data was inserted.");
                                        return false;
                                    } 
                                }
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
        }
        public static bool AddTask(int projectID)
        {
            string task = "Type the task here...";
            string due = "Date";
            string done = "Completed?";
            string priority = "Priority";
            int nextID = 0;
            using (SQLiteConnection databaseConnection = new SQLiteConnection("Data Source=" + databaseName))
            {
                databaseConnection.Open();
                using (SQLiteCommand selectIDCommand = new SQLiteCommand("SELECT ID FROM Tasks ORDER BY ID DESC LIMIT 1", databaseConnection))
                {
                    try
                    {
                        object lastID = selectIDCommand.ExecuteScalar();
                        nextID = Convert.ToInt32(lastID) + 1;
                        using (SQLiteCommand insertTaskCommand = new SQLiteCommand($"INSERT INTO Tasks (Id, Task, Due, Done, Priority) VALUES (@id, @taskstoinsert, @due, @done, @priority)", databaseConnection))
                        {
                            insertTaskCommand.Parameters.AddWithValue("@id", nextID);
                            insertTaskCommand.Parameters.AddWithValue("@taskstoinsert", task);
                            insertTaskCommand.Parameters.AddWithValue("@due", due);
                            insertTaskCommand.Parameters.AddWithValue("@done", done);
                            insertTaskCommand.Parameters.AddWithValue("@priority", priority);
                            insertTaskCommand.ExecuteNonQuery(); 
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }

                using (SQLiteCommand selectTaskIDCommand = new SQLiteCommand("SELECT TasksID FROM Projects WHERE Id = @projectid", databaseConnection))
                {
                    selectTaskIDCommand.Parameters.AddWithValue("@projectid", projectID);
                    using (SQLiteDataReader reader = selectTaskIDCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var receivedTaskID = reader["TasksID"].ToString();

                            if (receivedTaskID != "")
                            {
                                var deserializedDataList = JsonConvert.DeserializeObject<List<object>>(receivedTaskID);
                                deserializedDataList.Add((object)nextID);
                                var serializedDataList = JsonConvert.SerializeObject(deserializedDataList);
                                using (SQLiteCommand updateTaskIDCommand = new SQLiteCommand($"UPDATE Projects SET TasksID = @taskid WHERE Id = @id", databaseConnection))
                                {
                                    updateTaskIDCommand.Parameters.AddWithValue("@taskid", serializedDataList);
                                    updateTaskIDCommand.Parameters.AddWithValue("@id", projectID);

                                    int rowsAffected = updateTaskIDCommand.ExecuteNonQuery();
                                    if (rowsAffected == 1)
                                    {
                                        Console.WriteLine("Data inserted successfully.");
                                        return true;
                                    }
                                    else
                                    {
                                        Console.WriteLine("No data was updated.");
                                        return false;
                                    } 
                                }
                            }
                            else
                            {
                                List<object> taskID = new List<object>() { nextID };
                                var serializedDataList = JsonConvert.SerializeObject(taskID);
                                using (SQLiteCommand updateTaskIDCommand = new SQLiteCommand($"UPDATE Projects SET TasksID = @taskid WHERE Id = @id", databaseConnection))
                                {
                                    updateTaskIDCommand.Parameters.AddWithValue("@taskid", serializedDataList);
                                    updateTaskIDCommand.Parameters.AddWithValue("@id", projectID);

                                    int rowsAffected = updateTaskIDCommand.ExecuteNonQuery();
                                    if (rowsAffected == 1)
                                    {
                                        Console.WriteLine("Data inserted successfully.");
                                        return true;
                                    }
                                    else
                                    {
                                        Console.WriteLine("No data was inserted.");
                                        return false;
                                    } 
                                }
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
        }
        public static bool UpdateTask(string field, string data, int taskID)
        {
            using (SQLiteConnection databaseConnection = new SQLiteConnection("Data Source=" + databaseName))
            {
                databaseConnection.Open();
                using (SQLiteCommand updateTaskCommand = new SQLiteCommand(databaseConnection))
                {
                    try
                    {
                        switch (field)
                        {
                            case "Task":
                                updateTaskCommand.CommandText = $"UPDATE Tasks SET Task = @newtask WHERE Id = @id";
                                break;
                            case "Due":
                                updateTaskCommand.CommandText = $"UPDATE Tasks SET Due = @newtask WHERE Id = @id";
                                break;
                            case "Done":
                                updateTaskCommand.CommandText = $"UPDATE Tasks SET Done = @newtask WHERE Id = @id";
                                break;
                            case "Priority":
                                updateTaskCommand.CommandText = $"UPDATE Tasks SET Priority = @newtask WHERE Id = @id";
                                break;
                        }
                        updateTaskCommand.Parameters.AddWithValue("@newtask", data);
                        updateTaskCommand.Parameters.AddWithValue("@id", taskID);
                        updateTaskCommand.ExecuteNonQuery();
                        return true;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        return false;
                    }
                }
            }
        }
        public static bool DeleteTask(int taskID, int projectID)
        {
            using (SQLiteConnection databaseConnection = new SQLiteConnection("Data Source=" + databaseName))
            {
                databaseConnection.Open();
                using (SQLiteCommand deleteTaskCommand = new SQLiteCommand("DELETE FROM Tasks WHERE Id = @taskid", databaseConnection))
                {
                    deleteTaskCommand.Parameters.AddWithValue("@taskid", taskID);
                    deleteTaskCommand.ExecuteNonQuery();
                }
                using (SQLiteCommand selectTaskIDCommand = new SQLiteCommand("SELECT TasksID FROM Projects WHERE Id = @projectid", databaseConnection))
                {
                    selectTaskIDCommand.Parameters.AddWithValue("@projectid", projectID);
                    using (SQLiteDataReader reader = selectTaskIDCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var taskIDList = reader["TasksID"].ToString();
                            reader.Close();

                            if (taskIDList != "")
                            {
                                var deserializedDataList = JsonConvert.DeserializeObject<List<int>>(taskIDList);
                                List<object> newTaskID = new List<object>();
                                foreach (int task in deserializedDataList)
                                {
                                    if (task != taskID)
                                    {
                                        newTaskID.Add(task);
                                    }
                                }
                                var serializedDataList = JsonConvert.SerializeObject(newTaskID);
                                using (SQLiteCommand updateTaskIDCommand = new SQLiteCommand($"UPDATE Projects SET TasksID = @taskid WHERE Id = @id", databaseConnection))
                                {
                                    updateTaskIDCommand.Parameters.AddWithValue("@taskid", serializedDataList);
                                    updateTaskIDCommand.Parameters.AddWithValue("@id", projectID);

                                    int rowsAffected = updateTaskIDCommand.ExecuteNonQuery();
                                    if (rowsAffected == 1)
                                    {
                                        Console.WriteLine("Deleted Task");
                                        return true;
                                    }
                                    else
                                    {
                                        Console.WriteLine("Didn't Work");
                                        return false;
                                    } 
                                }
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
        }
        public static bool DeleteProject(string username,int projectID)
        {
            using (SQLiteConnection databaseConnection = new SQLiteConnection("Data Source=" + databaseName))
            {
                databaseConnection.Open();
                using (SQLiteCommand selectTaskIDCommand = new SQLiteCommand("SELECT TasksID FROM Projects WHERE Id = @projectid", databaseConnection))
                {
                    selectTaskIDCommand.Parameters.AddWithValue("@projectid", projectID);
                    using (SQLiteDataReader reader = selectTaskIDCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var taskIDList = reader["TasksID"].ToString();
                            reader.Close();

                            if (taskIDList != "")
                            {
                                var deserializedDataList = JsonConvert.DeserializeObject<List<int>>(taskIDList);
                                foreach (int task in deserializedDataList)
                                {
                                    using (SQLiteCommand deleteTasksCommand = new SQLiteCommand("DELETE FROM Tasks WHERE Id = @taskid", databaseConnection))
                                    {
                                        deleteTasksCommand.Parameters.AddWithValue("@taskid", task);
                                        deleteTasksCommand.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                    }
                }
                using (SQLiteCommand selectProjectIDCommand = new SQLiteCommand("SELECT ProjectID FROM Accounts WHERE Username = @username", databaseConnection))
                {
                    selectProjectIDCommand.Parameters.AddWithValue("@username", username);
                    using (SQLiteDataReader reader = selectProjectIDCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var ProjectIDList = reader["ProjectID"].ToString();
                            reader.Close();

                            if (ProjectIDList != "")
                            {
                                var deserializeDataList = JsonConvert.DeserializeObject<List<int>>(ProjectIDList);
                                List<object> newProjectList = new List<object>();
                                foreach (int project in deserializeDataList)
                                {
                                    if (project != projectID)
                                    {
                                        newProjectList.Add(project);
                                    }
                                }
                                var serializedDataList = JsonConvert.SerializeObject(newProjectList);
                                using (SQLiteCommand updateProjectIDCommand = new SQLiteCommand($"UPDATE Accounts SET ProjectID = @projectid WHERE Username = @username", databaseConnection))
                                {
                                    updateProjectIDCommand.Parameters.AddWithValue("@projectid", serializedDataList);
                                    updateProjectIDCommand.Parameters.AddWithValue("@username", username);

                                    int rowsAffected = updateProjectIDCommand.ExecuteNonQuery();

                                    if (rowsAffected == 1)
                                    {
                                        using (SQLiteCommand deleteProjectCommand = new SQLiteCommand("DELETE FROM Projects WHERE Id = @id", databaseConnection))
                                        {
                                            deleteProjectCommand.Parameters.AddWithValue("@id", projectID);
                                            deleteProjectCommand.ExecuteNonQuery();
                                        }
                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    } 
                                }
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
        }
        public static bool DeleteAccount(string username)
        {
            using (SQLiteConnection databaseConnection = new SQLiteConnection("Data Source=" + databaseName))
            {
                databaseConnection.Open();
                using (SQLiteCommand selectProjectIDCommand = new SQLiteCommand("SELECT ProjectID FROM Accounts WHERE Username = @username", databaseConnection))
                {
                    selectProjectIDCommand.Parameters.AddWithValue("@username", username);
                    using (SQLiteDataReader reader = selectProjectIDCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var projectIDList = reader["ProjectID"].ToString();
                            reader.Close();

                            if (projectIDList != "")
                            {
                                var deserializedProjectDataList = JsonConvert.DeserializeObject<List<int>>(projectIDList);
                                foreach (int project in deserializedProjectDataList)
                                {
                                    using (SQLiteCommand selectTaskIDCommand = new SQLiteCommand("SELECT TasksID FROM Projects WHERE Id = @id", databaseConnection))
                                    {
                                        selectTaskIDCommand.Parameters.AddWithValue("@id", project);
                                        using (SQLiteDataReader idReader = selectTaskIDCommand.ExecuteReader())
                                        {
                                            if (idReader.Read())
                                            {
                                                var taskIDList = idReader["TasksID"].ToString();
                                                idReader.Close();

                                                if (taskIDList != "")
                                                {
                                                    var deserializedTaskDataList = JsonConvert.DeserializeObject<List<int>>(taskIDList);
                                                    foreach (int task in deserializedTaskDataList)
                                                    {
                                                        using (SQLiteCommand deleteTaskCommand = new SQLiteCommand("DELETE FROM Tasks WHERE Id = @id", databaseConnection))
                                                        {
                                                            deleteTaskCommand.Parameters.AddWithValue("@id", task);
                                                            deleteTaskCommand.ExecuteNonQuery();
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    using (SQLiteCommand deleteProjectCommand = new SQLiteCommand("DELETE FROM Projects WHERE Id = @id", databaseConnection))
                                    {
                                        deleteProjectCommand.Parameters.AddWithValue("@id", project);
                                        deleteProjectCommand.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                    }
                }
                using (SQLiteCommand deleteAccountCommand = new SQLiteCommand("DELETE FROM Accounts WHERE Username = @username", databaseConnection))
                {
                    deleteAccountCommand.Parameters.AddWithValue("@username", username);
                    deleteAccountCommand.ExecuteNonQuery();
                }
                return true;
            }
        }
        public static List<List<List<List<object>>>> GetAllData(string username)
        {
            List<List<List<List<object>>>> projectAndTaskData = new List<List<List<List<object>>>>();
            using (SQLiteConnection databaseConnection = new SQLiteConnection("Data Source=" + databaseName))
            {
                databaseConnection.Open();
                using (SQLiteCommand selectProjectIDCommand = new SQLiteCommand("SELECT ProjectID FROM Accounts WHERE Username = @Username", databaseConnection))
                {
                    selectProjectIDCommand.Parameters.AddWithValue("@Username", username);
                    using (SQLiteDataReader reader = selectProjectIDCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var projectIDList = reader["ProjectID"].ToString();
                            var deserializedProjectIDList = JsonConvert.DeserializeObject<List<object>>(projectIDList);
                            if (deserializedProjectIDList == null)
                            {
                                return projectAndTaskData;
                            }

                            List<List<List<object>>> projectNameAndIDList = new List<List<List<object>>>();


                            foreach (var projectID in deserializedProjectIDList)
                            {
                                using (SQLiteCommand selectProjectNamesCommand = new SQLiteCommand("SELECT ProjectNames FROM Projects WHERE Id = @id", databaseConnection))
                                {
                                    selectProjectNamesCommand.Parameters.Add("@id", System.Data.DbType.Object).Value = projectID;
                                    using (SQLiteDataReader nameReader = selectProjectNamesCommand.ExecuteReader())
                                    {
                                        if (nameReader.Read())
                                        {
                                            var projectNameList = nameReader["ProjectNames"].ToString();
                                            projectNameAndIDList.Add(new List<List<object>> { new List<object> { projectNameList }, new List<object> { projectID } });
                                        }
                                    } 
                                }
                            }
                            projectAndTaskData.Add(projectNameAndIDList);


                            List<List<List<object>>> taskPerProjectList = new List<List<List<object>>>();
                            foreach (var projectID in deserializedProjectIDList)
                            {
                                using (SQLiteCommand selectTaskIDCommand = new SQLiteCommand("SELECT TasksID FROM Projects WHERE Id = @id", databaseConnection))
                                {
                                    selectTaskIDCommand.Parameters.Add("@id", System.Data.DbType.Object).Value = projectID;
                                    using (SQLiteDataReader idReader = selectTaskIDCommand.ExecuteReader())
                                    {
                                        if (idReader.Read() && idReader["TasksID"] != DBNull.Value)
                                        {
                                            var taskIDList = idReader["TasksID"].ToString();
                                            var deserializedTaskID = JsonConvert.DeserializeObject<List<object>>(taskIDList);

                                            List<List<object>> listOfTasks = new List<List<object>>();

                                            foreach (object taskID in deserializedTaskID)
                                            {
                                                using (SQLiteCommand selectTaskCommand = new SQLiteCommand("SELECT * FROM Tasks WHERE Id = @id", databaseConnection))
                                                {
                                                    selectTaskCommand.Parameters.Add("@id", System.Data.DbType.Object).Value = taskID;
                                                    using (SQLiteDataReader infoReader = selectTaskCommand.ExecuteReader())
                                                    {
                                                        if (infoReader.Read())
                                                        {
                                                            listOfTasks.Add(new List<object> { taskID, infoReader["Task"].ToString(), infoReader["Due"].ToString(), infoReader["Done"].ToString(), infoReader["Priority"].ToString() });
                                                        }
                                                        else
                                                        {
                                                            listOfTasks.Add(new List<object> { });
                                                        }
                                                    } 
                                                }
                                            }
                                            taskPerProjectList.Add(listOfTasks);
                                        }
                                        else
                                        {
                                            taskPerProjectList.Add(new List<List<object>>());
                                        }
                                    } 
                                }




                            }
                            projectAndTaskData.Add(taskPerProjectList);
                        }
                        else
                        {
                            Console.WriteLine("Found Nothing");
                        }
                    } 
                }
            }
            return projectAndTaskData;
        }
    }
}
