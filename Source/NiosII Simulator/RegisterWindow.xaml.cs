﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using NiosII_Simulator.Core;

namespace NiosII_Simulator
{
    /// <summary>
    /// Represents an register for the register view
    /// </summary>
    public class RegisterItem
    {
        /// <summary>
        /// The name of the register
        /// </summary>
        public string Register { get; set; }

        /// <summary>
        /// The value of the register
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Returns the register for the current register item
        /// </summary>
        public Registers GetRegister()
        {
            return (Registers)Enum.Parse(typeof(Registers), this.Register);
        }
    }

    /// <summary>
    /// Interaction logic for RegisterWindow.xaml
    /// </summary>
    public partial class RegisterWindow : Window
    {

        #region Fields
        private VirtualMachine virtualMachine;                                                                      //The VM
        private List<RegisterItem> registers;                                                                       //The registers
        private Timer registerTimer;                                                                                //The update register timer
        private bool canClose;                                                                                      //Indicates if the window can be closed
        #endregion

        #region Constructors
        /// <summary>
        /// Creates an new register window
        /// </summary>
        /// <param name="virtualMachine">The virtual machine</param>
        public RegisterWindow(VirtualMachine virtualMachine)
        {
            InitializeComponent();
            this.virtualMachine = virtualMachine;
            this.InitializeRegisterListBox();

            this.registerTimer = new Timer(state =>
            {
                this.UpdateRegisterView();
            }, null, 0, 100);
        }
        #endregion

        #region Properties
        
        #endregion

        #region Methods

        #region GUI Methods
        /// <summary>
        /// Intializes the register list box
        /// </summary>
        private void InitializeRegisterListBox()
        {
            this.registers = new List<RegisterItem>();

            for (int i = 1; i <= 31; i++)
            {
                RegisterItem regItem = new RegisterItem()
                {
                    Register = "R" + i,
                    Value = 0
                };

                this.registers.Add(regItem);
                //this.RegistersView.Items[this.RegistersView.Items.Count - 1];
            }

            this.RegistersView.ItemsSource = this.registers;
            this.RegistersView.AutoGenerateColumns = true;
            this.RegistersView.CanUserAddRows = false;
            this.RegistersView.CanUserDeleteRows = false;
        }

        /// <summary>
        /// Updates the register view
        /// </summary>
        private void UpdateRegisterView()
        {
            bool updated = false;

            for (int i = 0; i < 31; i++)
            {
                RegisterItem regItem = this.registers[i];
                int getValue = this.virtualMachine.GetRegisterValue(regItem.GetRegister());

                if (regItem.Value != getValue)
                {
                    regItem.Value = getValue;
                    updated = true;
                }
            }

            //Update the register view
            if (updated)
            {
                this.RegistersView.Dispatcher.Invoke(() =>
                {
                    this.RegistersView.Items.Refresh();
                });
            }
        }
        #endregion

        #region Window Methods
        /// <summary>
        /// Closes the window
        /// </summary>
        public void CloseWindow()
        {
            this.canClose = true;
            this.Close();
        }
        #endregion

        #region Events
        private void RegistersView_AutoGeneratedColumns(object sender, EventArgs e)
        {
            this.RegistersView.Columns[0].IsReadOnly = true;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.registerTimer.Dispose();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!this.canClose)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void RegistersView_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            int value = 0;

            if (int.TryParse(((TextBox)e.EditingElement).Text, out value))
            {
                Registers register = (Registers)Enum.Parse(typeof(Registers), ((RegisterItem)e.Row.Item).Register);
                this.virtualMachine.SetRegisterValue(register, value);
            }
            else
            {
                MessageBox.Show("Invalid value.");
            }
        }
        #endregion

        #endregion


    }
}
