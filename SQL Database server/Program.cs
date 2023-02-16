using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Database_Server
{
    public class DeleteAccount : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            Console.WriteLine("recieved " + e.Data);
            var receivedData = e.Data;
            var deserializedDataList = JsonConvert.DeserializeObject<List<object>>(receivedData);
            bool accountDeleted = Database.DeleteAccount(deserializedDataList[0].ToString());
            if (accountDeleted)
            {
                Send("Done");
                Console.WriteLine($"Account Deleted - Sent DONE to account '{deserializedDataList[0].ToString()}'");
            }
            else
            {
                Send("Failed");
                Console.WriteLine($"Account Deleted - Sent FAILED to account '{deserializedDataList[0].ToString()}'");
            }
        }
    }
    public class DeleteProject : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            Console.WriteLine("recieved " + e.Data);
            var receivedData = e.Data;
            var deserializedDataList = JsonConvert.DeserializeObject<List<object>>(receivedData);
            bool projectDeleted = Database.DeleteProject(deserializedDataList[0].ToString(), int.Parse(deserializedDataList[1].ToString()));
            if (projectDeleted)
            {
                Send("Done");
                Console.WriteLine($"Project Deleted - Sent DONE to account '{deserializedDataList[0].ToString()}'");
            }
            else
            {
                Send("Failed");
                Console.WriteLine($"Project Deleted - Sent FAILED to account '{deserializedDataList[0].ToString()}'");
            }
        }
    }
    public class DeleteTask : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            Console.WriteLine("recieved " + e.Data);
            var receivedData = e.Data; // assuming e.Data contains the received serialized JSON string
            var deserializedDataList = JsonConvert.DeserializeObject<List<object>>(receivedData);
            bool taskDeleted = Database.DeleteTask(int.Parse(deserializedDataList[0].ToString()), int.Parse(deserializedDataList[1].ToString()));
            if (taskDeleted)
            {
                Send("Done");
                Console.WriteLine("Task Deleted - Sent DONE");
            }
            else
            {
                Send("Failed");
                Console.WriteLine("Task Deleted - Sent FAILED");
            }
        }
    }
    public class AddTask : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            Console.WriteLine("recieved " + e.Data);
            var receivedData = e.Data; // assuming e.Data contains the received serialized JSON string
            var deserializedDataList = JsonConvert.DeserializeObject<List<object>>(receivedData);
            bool taskAdded = Database.AddTask(int.Parse(deserializedDataList[0].ToString()));
            if (taskAdded)
            {
                Send("Done");
                Console.WriteLine($"Task Added - Sent DONE to account '{deserializedDataList[0].ToString()}'");
            }
            else
            {
                Send("Failed");
                Console.WriteLine($"Task Added - Sent FAILED to account '{deserializedDataList[0].ToString()}'");
            }
        }
    }
    public class UpdateTask : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            Console.WriteLine("recieved " + e.Data);
            var receivedData = e.Data;
            var deserializedDataList = JsonConvert.DeserializeObject<List<object>>(receivedData);
            bool taskUpdated = Database.UpdateTask(deserializedDataList[0].ToString(), deserializedDataList[1].ToString(), int.Parse(deserializedDataList[2].ToString()));
            if (taskUpdated)
            {
                Send("Done");
                Console.WriteLine("Task Updated - Sent DONE");
            }
            else
            {
                Send("Failed");
                Console.WriteLine($"Task Updated - Sent FAILED");
            }
        }
    }
    public class MakeNewProject : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            Console.WriteLine("recieved " + e.Data);
            var receivedData = e.Data; // assuming e.Data contains the received serialized JSON string
            var deserializedDataList = JsonConvert.DeserializeObject<List<string>>(receivedData);
            bool projectCreated = Database.CreateProject(deserializedDataList[0], deserializedDataList[1]);
            if (projectCreated)
            {
                Send("Done");
                Console.WriteLine($"Project Created - Sent DONE to account '{deserializedDataList[0].ToString()}'");
            }
            else
            {
                Send("Failed");
                Console.WriteLine($"Project Created - Sent FAILED to account '{deserializedDataList[0].ToString()}'");
            }
        }
    }
    public class GetData : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            Console.WriteLine("recieved " + e.Data);
            var receivedData = e.Data;
            var deserializedDataList = JsonConvert.DeserializeObject<List<object>>(receivedData);
            var accountDataList = Database.GetAllData(deserializedDataList[0].ToString());
            var serializedDataList = JsonConvert.SerializeObject(accountDataList);
            Console.WriteLine($"Get Account Data - Sent All Data to account '{deserializedDataList[0].ToString()}'");
            Send(serializedDataList);
        }
    }
    public class SignUp : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            Console.WriteLine("recieved " + e.Data);
            var receivedData = e.Data;
            var deserializedDataList = JsonConvert.DeserializeObject<List<object>>(receivedData);
            bool accountCreated = Database.CreateAccount(deserializedDataList[0].ToString(), deserializedDataList[1].ToString(), deserializedDataList[2].ToString());
            var dataList = new List<object> {accountCreated, deserializedDataList[0].ToString()};
            var serializedDataList = JsonConvert.SerializeObject(dataList);
            Console.WriteLine($"Account Created = {accountCreated.ToString()} with Username '{deserializedDataList[0].ToString()}'");
            Send(serializedDataList);
        }
    }
    public class SignIn : WebSocketBehavior
    {
        protected override void OnMessage(MessageEventArgs e)
        {
            Console.WriteLine("recieved " + e.Data);
            var receivedData = e.Data;
            var deserializedDataList = JsonConvert.DeserializeObject<List<object>>(receivedData);
            bool signedIn = false;
            bool accountFound = Database.FindAccount(deserializedDataList[0].ToString());
            bool passwordVerified = Database.VerifyPassword(deserializedDataList[0].ToString(), deserializedDataList[1].ToString());
            if (accountFound && passwordVerified)
            {
                signedIn = true;
            }
            var dataList = new List<object> { signedIn, deserializedDataList[0].ToString() };
            var serializedDataList = JsonConvert.SerializeObject(dataList);
            Send(serializedDataList);
            Console.WriteLine($"Signed in = {signedIn.ToString()} with Username {deserializedDataList[0].ToString()} ");
        }
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            WebSocketServer wss = new WebSocketServer("ws://192.168.0.20:7687");
            wss.AddWebSocketService<SignUp>("/Sign up");
            wss.AddWebSocketService<SignIn>("/Sign in");
            wss.AddWebSocketService<GetData>("/Get data");
            wss.AddWebSocketService<MakeNewProject>("/New project");
            wss.AddWebSocketService<UpdateTask>("/Update task");
            wss.AddWebSocketService<AddTask>("/Add task");
            wss.AddWebSocketService<DeleteTask>("/Delete task");
            wss.AddWebSocketService<DeleteProject>("/Delete project");
            wss.AddWebSocketService<DeleteAccount>("/Delete account");
            wss.Start();
            Console.WriteLine("Server started on ws://192.168.0.20:7687");
            
            Database.InitialiseDatabase();
            Console.WriteLine("Database Ready");

            Console.ReadKey();
        }
    }
}
