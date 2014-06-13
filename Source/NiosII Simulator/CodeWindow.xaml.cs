using System;
using System.Collections.Generic;
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
using NiosII_Simulator.Core.Assembler;

namespace NiosII_Simulator
{
    /// <summary>
    /// Interaction logic for CodeWindow.xaml
    /// </summary>
    public partial class CodeWindow : Window
    {

        #region Fields
        private VirtualMachine virtualMachine;                                                                      //The VM
        private bool canClose;                                                                                      //Indicates if the window can be closed
        #endregion

        #region Constructors
        /// <summary>
        /// Creates an new code window
        /// </summary>
        /// <param name="virtualMachine">The virtual machine</param>
        public CodeWindow(VirtualMachine virtualMachine)
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

        #region Events
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!this.canClose)
            {
                e.Cancel = true;

                try
                {
                    string codeText = this.CodeBox.Text;
                    Program program = NiosAssembler.New().Assemble(codeText);
                    this.virtualMachine.LoadProgram(program);
                    this.Hide();
                }
                catch(AssemblerException exception)
                {
                    MessageBox.Show(exception.Message, "Assembler error");
                }
            }
        }
        #endregion

        #endregion

    }
}
