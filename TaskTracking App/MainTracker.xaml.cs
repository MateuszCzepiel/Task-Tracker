using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Security.RightsManagement;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WebSocketSharp;

namespace TaskTracking_App
{
    public partial class MainTracker : Page
    {
        public string username;
        public int currentProjectOpen = 0;
        public List<List<List<object>>> userProjects = new List<List<List<object>>>();
        public List<List<List<object>>> userTasks = new List<List<List<object>>>();
        public List<Button> projectButtons = new List<Button>();
        public bool newTask = false;
        public bool appJustStarted = true;
        public MainTracker(string data)
        {
            InitializeComponent();

            username = data;
            UsernameDisplay.Content = username;
            AddTaskButton.IsEnabled = false;
            GetAccountData();
        }
        public void GetAccountData()
        {
            WebSocket webSocketConnection = new WebSocket("ws://192.168.0.20:7687/Get data");
            try
            {
                webSocketConnection.Connect();
                if (webSocketConnection.IsAlive)
                {
                    var dataList = new List<object> { username };
                    var serializedDataList = JsonConvert.SerializeObject(dataList);
                    webSocketConnection.Send(serializedDataList);
                    webSocketConnection.OnMessage += DisplayProjects;
                }
                else
                {
                    Errors.Content = "WebSocket state: " + webSocketConnection.ReadyState;
                }
            }
            catch (Exception ex)
            {
                Errors.Content = "An error occurred while connecting to the WebSocket: " + ex.Message;
            }
        }
        public void LogOut(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new StartUp());
        }
        public void AddNewTask(object sender, RoutedEventArgs e)
        {
            string currentProjectID = userProjects[currentProjectOpen][1][0].ToString();
            WebSocket webSocketConnection = new WebSocket("ws://192.168.0.20:7687/Add task");
            webSocketConnection.Connect();
            if (webSocketConnection.IsAlive)
            {
                var dataList = new List<object> { currentProjectID };
                var serializedDataList = JsonConvert.SerializeObject(dataList);
                webSocketConnection.Send(serializedDataList);
                webSocketConnection.OnMessage += ConfirmNewTaskAdded;
            }
            else
            {
                Errors.Content = "WebSocket state: " + webSocketConnection.ReadyState;
            }
        }
        public void MakeNewProject(object sender, RoutedEventArgs e)
        {
            string newProjectName = NewProjectName.Text;
            int numberOfSpaces = 0;
            foreach (char character in newProjectName)
            {
                if (character == ' ')
                {
                    numberOfSpaces++; 
                }
            }
            if (numberOfSpaces == newProjectName.Length || newProjectName == "")
            {
                CreateProject.Content = "Invalid Project Name";
                CreateProject.Foreground = Brushes.Red;
                return;
            }

            CreateProject.Content = "Create Project:";
            CreateProject.Foreground = Brushes.White;

            WebSocket webSocketConnection = new WebSocket("ws://192.168.0.20:7687/New project");
            try
            {
                webSocketConnection.Connect();
                if (webSocketConnection.IsAlive)
                {
                    var dataList = new List<object> { username, newProjectName };
                    var serializedDataList = JsonConvert.SerializeObject(dataList);
                    webSocketConnection.Send(serializedDataList);
                    webSocketConnection.OnMessage += ConfirmProjectAdded;
                }
                else
                {
                    Errors.Content = "WebSocket state: " + webSocketConnection.ReadyState;
                }
            }
            catch (Exception ex)
            {
                Errors.Content = "An error occurred while connecting to the WebSocket: " + ex.Message;
            }
        }
        public void SaveTask(object sender, RoutedEventArgs e)
        {
            TextBox changedTextBox = (TextBox)sender;
            int currentTaskID = int.Parse(changedTextBox.Name.ToString().Remove(0,2));
            string changedColumnName = changedTextBox.Name.ToString().Substring(0,changedTextBox.Name.ToString().Length-currentTaskID.ToString().Length);
            string dataToChange = changedTextBox.Text;
            switch (changedColumnName)
            {
                case "Ta":
                    changedColumnName = "Task";
                    break;
                case "Du":
                    changedColumnName = "Due";
                    break;
                case "Do":
                    changedColumnName = "Done";
                    break;
                case "Pr":
                    changedColumnName = "Priority";
                    break;
            }
            WebSocket webSocketConnection = new WebSocket("ws://192.168.0.20:7687/Update task");
            try
            {
                webSocketConnection.Connect();
                if (webSocketConnection.IsAlive)
                {
                    var dataList = new List<object> { changedColumnName, dataToChange, currentTaskID };
                    var serializedDataList = JsonConvert.SerializeObject(dataList);
                    webSocketConnection.Send(serializedDataList);
                    webSocketConnection.OnMessage += ConfirmTaskUpdated;
                }
                else
                {
                    Errors.Content = "WebSocket state: " + webSocketConnection.ReadyState;
                }
            }
            catch (Exception ex)
            {
                Errors.Content = "An error occurred while connecting to the WebSocket: " + ex.Message;
            }

        }
        public void ProjectButtonClicked(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;
            foreach (Button button in ProjectsPanel.Children)
            {
                button.Background = new SolidColorBrush(Color.FromRgb(40, 40, 43));
                button.Foreground = Brushes.White;
            }
            clickedButton.Background = new SolidColorBrush(Color.FromRgb(216, 116, 165));
            clickedButton.Foreground = Brushes.Black;
            currentProjectOpen = int.Parse(clickedButton.Name.Remove(0, 6));
            ProjectDisplay.Content = clickedButton.Content;
            DeleteProjectButton.IsEnabled = true;
            AddTaskButton.IsEnabled = true;
            DisplayTasks(currentProjectOpen);
        }
        public void DisplayTasks(int project)
        {
            if (project == 0 && userTasks.Count == 0)
            {
                return;
            }
            TasksPanel.Children.Clear();
            List<List<object>> projectTasks = userTasks[project];
            if (projectTasks.Count == 0)
            {
                ProjectDisplay.Content = userProjects[currentProjectOpen][0][0];
                Label noTasks = new Label();
                noTasks.Content = $"There are no tasks in {userProjects[currentProjectOpen][0][0]}. Add one.";
                noTasks.FontSize = 18;
                noTasks.Foreground = Brushes.White;
                TasksPanel.Children.Add(noTasks);
                DeleteProjectButton.IsEnabled = true;
                AddTaskButton.IsEnabled = true;
            }
            foreach (List<object> task in projectTasks)
            {
                Border whiteBorder = new Border();
                whiteBorder.BorderBrush = Brushes.White;
                whiteBorder.BorderThickness = new Thickness(2);
                whiteBorder.CornerRadius = new CornerRadius(10);
                whiteBorder.Margin = new Thickness(0, 0, 0, 10);

                Grid columnGrid = new Grid();
                columnGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(40) });
                columnGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(640) });
                columnGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(30) });
                
                Grid rowGrid = new Grid();
                rowGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(20) });
                rowGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(20) });

                Grid infoGrid = new Grid();
                infoGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(40) });
                infoGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(170) });
                infoGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(80) });
                infoGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(120) });
                infoGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(110) });
                infoGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(110) });

                TextBox taskTextBox = new TextBox();
                taskTextBox.Width = 630;
                taskTextBox.Text = task[1].ToString();
                taskTextBox.TextChanged += SaveTask;
                taskTextBox.Name = "Ta" + task[0].ToString();
                taskTextBox.Background = Brushes.Transparent;
                taskTextBox.Foreground = Brushes.White;
                taskTextBox.BorderThickness = new Thickness(0);
                taskTextBox.FontSize = 14;
                taskTextBox.CaretBrush = Brushes.White;
                taskTextBox.HorizontalAlignment = HorizontalAlignment.Right;

                TextBox dueTextBox = new TextBox();
                dueTextBox.Width = 160;
                dueTextBox.Text = task[2].ToString();
                dueTextBox.TextChanged += SaveTask;
                dueTextBox.Name = "Du" + task[0].ToString();
                dueTextBox.Background = Brushes.Transparent;
                dueTextBox.Foreground = Brushes.White;
                dueTextBox.BorderThickness = new Thickness(0);
                dueTextBox.FontSize = 14;
                dueTextBox.CaretBrush = Brushes.White;
                dueTextBox.HorizontalAlignment = HorizontalAlignment.Right;

                TextBox doneTextBox = new TextBox();
                doneTextBox.Width = 110;
                doneTextBox.Text = task[3].ToString();
                doneTextBox.TextChanged += SaveTask;
                doneTextBox.Name = "Do" + task[0].ToString();
                doneTextBox.Background = Brushes.Transparent;
                doneTextBox.Foreground = Brushes.White;
                doneTextBox.BorderThickness = new Thickness(0);
                doneTextBox.FontSize = 14;
                doneTextBox.CaretBrush = Brushes.White;
                doneTextBox.HorizontalAlignment = HorizontalAlignment.Right;

                TextBox priorityTextBox = new TextBox();
                priorityTextBox.Width = 110;
                priorityTextBox.Text = task[4].ToString();
                priorityTextBox.TextChanged += SaveTask;
                priorityTextBox.Name = "Pr" + task[0].ToString();
                priorityTextBox.Background = Brushes.Transparent;
                priorityTextBox.Foreground = Brushes.White;
                priorityTextBox.BorderThickness = new Thickness(0);
                priorityTextBox.FontSize = 14;
                priorityTextBox.CaretBrush = Brushes.White;
                priorityTextBox.HorizontalAlignment = HorizontalAlignment.Right;

                Button deleteTaskButton = new Button();
                deleteTaskButton.Click += DeleteTaskButtonClicked;
                deleteTaskButton.Background = Brushes.Transparent;
                deleteTaskButton.BorderThickness = new Thickness(0);
                deleteTaskButton.Height= 20;
                deleteTaskButton.Width= 20;
                deleteTaskButton.Name = "b" + task[0].ToString();
                Image image = new Image();
                image.Source = new BitmapImage(new Uri("\\Images\\Delete.png", UriKind.RelativeOrAbsolute));
                deleteTaskButton.Content = image;
                deleteTaskButton.Style = (Style)FindResource("RoundedButtonStyle");
                deleteTaskButton.Cursor = Cursors.Hand;

                TextBlock dueLabel = new TextBlock();
                dueLabel.Text = "Due:";
                dueLabel.FontSize= 14;
                dueLabel.Height = 20;
                dueLabel.Width = 30;
                dueLabel.HorizontalAlignment= HorizontalAlignment.Right;
                dueLabel.Foreground = Brushes.White;

                TextBlock doneLabel = new TextBlock();
                doneLabel.Text = "Done:";
                doneLabel.FontSize = 14;
                doneLabel.Height = 20;
                doneLabel.Width = 40;
                doneLabel.HorizontalAlignment = HorizontalAlignment.Right;
                doneLabel.Foreground = Brushes.White;

                TextBlock priorityLabel = new TextBlock();
                priorityLabel.Text = "Priority:";
                priorityLabel.FontSize = 14;
                priorityLabel.Height = 20;
                priorityLabel.Width = 70;
                priorityLabel.HorizontalAlignment = HorizontalAlignment.Right;
                priorityLabel.Foreground = Brushes.White;


                Grid.SetColumn(dueLabel, 0);
                Grid.SetColumn(doneLabel, 2);
                Grid.SetColumn(priorityLabel, 4);
                infoGrid.Children.Add(dueLabel);
                infoGrid.Children.Add(doneLabel);
                infoGrid.Children.Add(priorityLabel);

                Grid.SetColumn(dueTextBox, 1);
                Grid.SetColumn(doneTextBox, 3);
                Grid.SetColumn(priorityTextBox, 5);
                infoGrid.Children.Add(dueTextBox);
                infoGrid.Children.Add(doneTextBox);
                infoGrid.Children.Add(priorityTextBox);

                Grid.SetRow(deleteTaskButton, 0);
                Grid.SetColumn(deleteTaskButton, 1);
                columnGrid.Children.Add(deleteTaskButton);

                Grid.SetRow(taskTextBox, 0);
                rowGrid.Children.Add(taskTextBox);

                Grid.SetRow(infoGrid, 1);
                rowGrid.Children.Add(infoGrid);

                Grid.SetRow(rowGrid, 0);
                Grid.SetColumn(rowGrid, 0);
                columnGrid.Children.Add(rowGrid);

                whiteBorder.Child = columnGrid;
                
                TasksPanel.Children.Add(whiteBorder);
            }
        }
        private void DisplayProjects(object sender, MessageEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                ProjectsPanel.Children.Clear();
                var receivedData = e.Data;
                var deserializedDataList = JsonConvert.DeserializeObject<List<List<List<List<object>>>>>(receivedData);

                if (deserializedDataList.Count() == 0)
                {
                    Label noProjectsLabel = new Label();
                    noProjectsLabel.Content = $"There are no Projects. Create One.";
                    noProjectsLabel.FontSize = 18;
                    noProjectsLabel.Foreground = Brushes.White;
                    TasksPanel.Children.Add(noProjectsLabel);
                    ProjectDisplay.Content = "No Project Selected";
                    DeleteProjectButton.IsEnabled = false;
                    return;
                }
                userProjects = deserializedDataList[0];
                userTasks = deserializedDataList[1];
                if (newTask)
                {
                    DisplayTasks(currentProjectOpen);
                }
                newTask= false;
                int buttonIndex = 0;
                foreach (List<List<object>> project in userProjects)
                {
                    Button currentButton = new Button();
                    currentButton.Name = "button" + buttonIndex.ToString();
                    currentButton.Content = project[0][0].ToString();
                    currentButton.Click += ProjectButtonClicked;
                    currentButton.Cursor = Cursors.Hand;
                    currentButton.HorizontalAlignment = HorizontalAlignment.Center;
                    currentButton.Height = 25;
                    currentButton.Width = 155;
                    currentButton.BorderBrush = Brushes.Black;
                    currentButton.BorderThickness = new Thickness(2);
                    currentButton.FontFamily = new FontFamily("Arial");
                    currentButton.FontSize = 14;
                    currentButton.Style = (Style)FindResource("RoundedButtonStyle");
                    currentButton.Margin = new Thickness(0, 10, 0, 0);

                    if (buttonIndex == currentProjectOpen && appJustStarted == false)
                    {
                        currentButton.Background = new SolidColorBrush(Color.FromRgb(216, 116, 165));
                        currentButton.Foreground = Brushes.Black;
                    }
                    else
                    {
                        currentButton.Background = new SolidColorBrush(Color.FromRgb(40, 40, 43));
                        currentButton.Foreground = Brushes.White;
                    }
                    ProjectsPanel.Children.Add(currentButton);

                    buttonIndex++;
                }
                if (appJustStarted)
                {
                    TasksPanel.Children.Clear();
                    Label noProjects = new Label();
                    noProjects.Content = $"Select a Project.";
                    noProjects.FontSize = 18;
                    noProjects.Foreground = Brushes.White;
                    TasksPanel.Children.Add(noProjects);
                    ProjectDisplay.Content = "No Project Selected";
                    DeleteProjectButton.IsEnabled = false;
                    appJustStarted = false;
                }

            });
        }
        public void DeleteTaskButtonClicked(object sender, RoutedEventArgs e)
        {
            Button buttonTriggered = (Button)sender;
            int currentTaskID = int.Parse(buttonTriggered.Name.Remove(0,1));
            int currentProjectID = int.Parse(userProjects[currentProjectOpen][1][0].ToString());
            WebSocket webSocketConnection = new WebSocket("ws://192.168.0.20:7687/Delete task");
            try
            {
                webSocketConnection.Connect();
                if (webSocketConnection.IsAlive)
                {
                    var dataList = new List<object> { currentTaskID,currentProjectID };
                    var serializedDataList = JsonConvert.SerializeObject(dataList);
                    webSocketConnection.Send(serializedDataList);
                    webSocketConnection.OnMessage += ConfirmTaskDeleted;
                }
                else
                {
                    Errors.Content = "WebSocket state: " + webSocketConnection.ReadyState;
                }
            }
            catch (Exception ex)
            {
                Errors.Content = "An error occurred while connecting to the WebSocket: " + ex.Message;
            }
        }
        public void DeleteProjectButtonClicked(object sender, RoutedEventArgs e)
        {
            if (ProjectsPanel.Children.Count > 1)
            {
                Button currentProject = (Button)ProjectsPanel.Children[1];
                ProjectDisplay.Content = currentProject.Content; 
            }
            int currentProjectID = int.Parse(userProjects[currentProjectOpen][1][0].ToString());
            WebSocket webSocketConnection = new WebSocket("ws://192.168.0.20:7687/Delete project");
            try
            {
                webSocketConnection.Connect();
                if (webSocketConnection.IsAlive)
                {
                    var dataList = new List<object> { username, currentProjectID };
                    var serializedDataList = JsonConvert.SerializeObject(dataList);
                    webSocketConnection.Send(serializedDataList);
                    webSocketConnection.OnMessage += ConfirmProjectDeleted;
                }
                else
                {
                    Errors.Content = "WebSocket state: " + webSocketConnection.ReadyState;
                }
            }
            catch (Exception ex)
            {
                Errors.Content = "An error occurred while connecting to the WebSocket: " + ex.Message;
            }
        }
        public void DeleteAccountButtonClicked(object sender, RoutedEventArgs e)
        {
            WebSocket webSocketConnection = new WebSocket("ws://192.168.0.20:7687/Delete account");
            try
            {
                webSocketConnection.Connect();
                if (webSocketConnection.IsAlive)
                {
                    var dataList = new List<object> { username };
                    var serializedDataList = JsonConvert.SerializeObject(dataList);
                    webSocketConnection.Send(serializedDataList);
                    webSocketConnection.OnMessage += ConfirmAccountDeleted;
                }
                else
                {
                    Errors.Content = "WebSocket state: " + webSocketConnection.ReadyState;
                }
            }
            catch (Exception ex)
            {
                Errors.Content = "An error occurred while connecting to the WebSocket: " + ex.Message;
            }
        }
        private void ConfirmNewTaskAdded(object sender, MessageEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (e.Data == "Done")
                {
                    ProjectDisplay.Content = userProjects[currentProjectOpen][0][0];
                    newTask = true;
                    GetAccountData();
                }
            });
        }
        private void ConfirmTaskUpdated(object sender, MessageEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (e.Data == "Done")
                {
                    GetAccountData();
                }
            });
        }
        private void ConfirmTaskDeleted(object sender, MessageEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (e.Data == "Done")
                {
                    newTask = true;
                    GetAccountData();
                }
            });
        }
        private void ConfirmProjectAdded(object sender, MessageEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (e.Data == "Done")
                {
                    NewProjectName.Text = "";
                    if (userProjects.Count != 0)
                    {
                        ProjectDisplay.Content = userProjects[currentProjectOpen][0][0];
                        DeleteProjectButton.IsEnabled = true;
                        AddTaskButton.IsEnabled = true; 
                    }
                    newTask = true;
                    GetAccountData();
                }
            });
        }
        private void ConfirmProjectDeleted(object sender, MessageEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (e.Data == "Done")
                {
                    currentProjectOpen = 0;
                    newTask = true;
                    if (ProjectsPanel.Children.Count == 1)
                    {
                        TasksPanel.Children.Clear();
                        ProjectDisplay.Content = "No Project Selected";
                        Label noTasks = new Label();
                        noTasks.Content = "No Project Selected";
                        noTasks.FontSize = 18;
                        noTasks.Foreground = Brushes.White;
                        TasksPanel.Children.Add(noTasks);
                        DeleteProjectButton.IsEnabled = false;
                        AddTaskButton.IsEnabled = false;
                    }
                    GetAccountData();
                }
            });
        }
        private void ConfirmAccountDeleted(object sender, MessageEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                this.NavigationService.Navigate(new StartUp());
            });
        }
    }
}
