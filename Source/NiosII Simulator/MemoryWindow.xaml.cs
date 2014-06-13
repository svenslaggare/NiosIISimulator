using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
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
    /// Interaction logic for MemoryWindow.xaml
    /// </summary>
    public partial class MemoryWindow : Window
    {

        #region Fields
        private VirtualMachine virtualMachine;                                                                      //The VM
        private bool canClose;                                                                                      //Indicates if the window can be closed
        #endregion

        #region Constructors
        /// <summary>
        /// Creates an new memory window
        /// </summary>
        /// <param name="virtualMachine">The virtual machine</param>
        public MemoryWindow(VirtualMachine virtualMachine)
        {
            InitializeComponent();
            this.virtualMachine = virtualMachine;
        }
        #endregion

        #region Properties
        
        #endregion

        #region Methods

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

        #region Memory Methods
        /// <summary>
        /// Loads the value at the given memory address
        /// </summary>
        /// <param name="addressString">The address string</param>
        private void LoadFromAddress(string addressString)
        {
            uint memoryAddress = 0;
            bool hasAddress = false;

            //Check if hex
            if (addressString.StartsWith("0x"))
            {
                if (uint.TryParse(addressString.Substring(2, addressString.Length - 2), NumberStyles.AllowHexSpecifier, null, out memoryAddress))
                {
                    hasAddress = true;
                }
            }
            else
            {
                if (uint.TryParse(addressString, out memoryAddress))
                {
                    hasAddress = true;
                }
            }

            if (hasAddress)
            {
                string dataType = this.ValueTypeBox.SelectionBoxItem.ToString();
                bool isSigned = this.ValueIsSigned.IsChecked.Value;
                
                if (dataType == "Byte")
                {
                    if (isSigned)
                    {
                        this.ValueBox.Text = ((sbyte)this.virtualMachine.ReadByteFromMemory(memoryAddress)).ToString();
                    }
                    else
                    {
                        this.ValueBox.Text = this.virtualMachine.ReadByteFromMemory(memoryAddress).ToString();
                    }
                }
                else
                {
                    if (isSigned)
                    {
                        this.ValueBox.Text = this.virtualMachine.ReadWordFromMemory(memoryAddress).ToString();
                    }
                    else
                    {
                        this.ValueBox.Text = ((uint)this.virtualMachine.ReadWordFromMemory(memoryAddress)).ToString();
                    }
                }
            }
            else
            {
                MessageBox.Show("Invalid address.");
            }
        }

        /// <summary>
        /// Saves the value at the given memory address
        /// </summary>
        /// <param name="addressString">The address string</param>
        private void SaveAtAddress(string addressString)
        {
            uint memoryAddress = 0;
            bool hasAddress = false;

            //Check if hex
            if (addressString.StartsWith("0x"))
            {
                if (uint.TryParse(addressString.Substring(2, addressString.Length - 2), NumberStyles.AllowHexSpecifier, null, out memoryAddress))
                {
                    hasAddress = true;
                }
            }
            else
            {
                if (uint.TryParse(addressString, out memoryAddress))
                {
                    hasAddress = true;
                }
            }

            if (hasAddress)
            {
                string dataType = this.ValueTypeBox.SelectionBoxItem.ToString();
                bool isSigned = this.ValueIsSigned.IsChecked.Value;
                string valueStr = this.ValueBox.Text;
                //NumberStyles numStyle = NumberStyles.an

                if (dataType == "Byte")
                {
                    if (isSigned)
                    {
                        sbyte value = 0;

                        if (sbyte.TryParse(valueStr, NumberStyles.AllowLeadingSign, null, out value))
                        {
                            this.virtualMachine.WriteByteToMemory(memoryAddress, (byte)value);
                        }
                        else
                        {
                            MessageBox.Show("Invalid value.");
                        }
                    }
                    else
                    {
                        byte value = 0;

                        if (byte.TryParse(valueStr, out value))
                        {
                            this.virtualMachine.WriteByteToMemory(memoryAddress, value);
                        }
                        else
                        {
                            MessageBox.Show("Invalid value.");
                        }
                    }
                }
                else
                {
                    if (isSigned)
                    {
                        int value = 0;

                        if (int.TryParse(valueStr, NumberStyles.AllowLeadingSign, null, out value))
                        {
                            this.virtualMachine.WriteWordToMemory(memoryAddress, value);
                        }
                        else
                        {
                            MessageBox.Show("Invalid value.");
                        }
                    }
                    else
                    {
                        uint value = 0;

                        if (uint.TryParse(valueStr, out value))
                        {
                            this.virtualMachine.WriteWordToMemory(memoryAddress, (int)value);
                        }
                        else
                        {
                            MessageBox.Show("Invalid value.");
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Invalid address.");
            }
        }
        #endregion

        #region Events
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!this.canClose)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            this.LoadFromAddress(this.MemoryAddressBox.Text);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            this.SaveAtAddress(this.MemoryAddressBox.Text);
        }
        #endregion

        #endregion

    }
}
