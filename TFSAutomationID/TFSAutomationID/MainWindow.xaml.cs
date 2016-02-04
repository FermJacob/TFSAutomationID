//-----------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Fermium.io">
//  Copyright 2016 Jacob Ferm, All rights Reserved
// </copyright>
// <summary>Main window for the TFS Automation ID application</summary>
//-----------------------------------------------------
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.TestManagement.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Text.RegularExpressions;
using System.Windows;

namespace TFSAutomationID
{
    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// TFS project collection variable
        /// </summary>
        private TfsTeamProjectCollection tfs;

        /// <summary>
        /// TeamProject interface for test management items
        /// </summary>
        private ITestManagementTeamProject testManagementTeamProject;

        /// <summary>
        /// String to store the team project name
        /// </summary>
        private string teamProject;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Method to connect to the TFS project
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            TeamProjectPicker tpp = new TeamProjectPicker(TeamProjectPickerMode.SingleProject, false);
            tpp.ShowDialog();

            this.tfs = tpp.SelectedTeamProjectCollection;
            this.teamProject = tpp.SelectedProjects[0].Name;
            TFSProject.Text = this.teamProject;
            WorkItem.IsEnabled = true;
            ExecuteButton.IsEnabled = true;
            ITestManagementService test_service = (ITestManagementService)this.tfs.GetService(typeof(ITestManagementService));
            this.testManagementTeamProject = test_service.GetTeamProject(tpp.SelectedProjects[0].Name);
        }

        /// <summary>
        /// Method to perform work when the button is clicked
        /// </summary>
        /// <param name="sender">Sender event</param>
        /// <param name="e">Event arguments</param>
        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(WorkItem.Text) || !Regex.IsMatch(WorkItem.Text, @"^\d+$") || Convert.ToInt32(WorkItem.Text) < 0)
            {
                MessageBox.Show("Please enter a valid work item");
            }
            else
            {
                ITestCase testCase = null;
                try
                {
                    testCase = this.testManagementTeamProject.TestCases.Find(Convert.ToInt32(WorkItem.Text));
                }
                catch (DeniedOrNotExistException)
                {
                    MessageBox.Show("Work item does not exist\nPlease enter a new number", "Notice", MessageBoxButton.OK);
                    return;
                }
                catch (Exception)
                {
                    return;
                }

                if (testCase.Implementation == null)
                {
                    MessageBox.Show("Work item is not linked to an automated test case", "Notice", MessageBoxButton.OK);
                    return;
                }

                string guidID = ((ITmiTestImplementation)testCase.Implementation).TestId.ToString();
                GUIDTextBox.Text = guidID;
            }
        }
    }
}
