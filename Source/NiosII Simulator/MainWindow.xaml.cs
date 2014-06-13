using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NiosII_Simulator.Core;
using NiosII_Simulator.Core.Assembler;
using System.Threading;

namespace NiosII_Simulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region Fields
        private VirtualMachine virtualMachine;                                                                      //The VM
		private CancellationTokenSource runCancelToken;																//The run cancel token
        private CodeWindow codeWindow;                                                                              //The code window
        private RegisterWindow registerWindow;                                                                      //The register window
        private MemoryWindow memoryWindow;                                                                          //The memory window
        #endregion

        #region Constructors
        public MainWindow()
        {
            InitializeComponent();
            this.virtualMachine = new VirtualMachine();

            this.codeWindow = new CodeWindow(this.virtualMachine);
            this.registerWindow = new RegisterWindow(this.virtualMachine);
            this.memoryWindow = new MemoryWindow(this.virtualMachine);
        }
        #endregion

        #region Properties
        
        #endregion

        #region Methods

        #region GUI Methods
        /// <summary>
        /// Updates the CPU info
        /// </summary>
        private void UpdateCPUInfo()
        {
            this.IRLabel.Content = "IR: " + this.virtualMachine.InstructionRegister.Data;
            this.PCLabel.Content = "PC: " + this.virtualMachine.ProgramCounter;
        }
        #endregion

        #region Events

        #region GUI
        private async void RunButton_Click(object sender, RoutedEventArgs e)
        {
			//Parse the run speed
			int runSpeed = -1;
			int.TryParse(this.RunSpeed.Text, out runSpeed);

            if (runSpeed != -1)
			{
				//Cancel any running program
				if (this.runCancelToken != null)
				{
					this.runCancelToken.Cancel();
				}

				this.runCancelToken = new CancellationTokenSource();

				this.virtualMachine.RestartProgram();
				double sleepTime = 1 / (double)runSpeed * 1000;

				this.RunButton.IsEnabled = false;

				await Task.Run(() =>
				{
					while (this.virtualMachine.ProgramCounter < this.virtualMachine.ProgramEnd)
					{
						this.virtualMachine.Fetch();
						this.virtualMachine.Execute();

						this.Dispatcher.Invoke(() =>
						{
							this.UpdateCPUInfo();
						});

						if (this.runCancelToken.IsCancellationRequested)
						{
							break;
						}

						Thread.Sleep(TimeSpan.FromMilliseconds(sleepTime));
					}

				}, this.runCancelToken.Token);

				this.RunButton.IsEnabled = true;
			}
			else
			{
				this.RunButton.IsEnabled = false;
				this.virtualMachine.Run();
				this.UpdateCPUInfo();
				this.RunButton.IsEnabled = true;
			}
        }

        private void StepButton_Click(object sender, RoutedEventArgs e)
        {
            this.virtualMachine.Fetch();
            this.virtualMachine.Execute();

            if (this.virtualMachine.ProgramCounter > this.virtualMachine.ProgramEnd)
            {
                this.virtualMachine.RestartProgram();
            }

            this.UpdateCPUInfo();
        }

		private void StopButton_Click(object sender, RoutedEventArgs e)
		{
			if (this.runCancelToken != null)
			{
				this.runCancelToken.Cancel();
			}
		}
        #endregion

        #region File Menu
        private void File_LoadProgramFromFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog loadProgramFileDialog = new OpenFileDialog();

            if (loadProgramFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string codeText = File.ReadAllText(loadProgramFileDialog.FileName);

                try
                {
					Program program = NiosAssembler.New().Assemble(codeText);
                    this.virtualMachine.LoadProgram(program);
                }
                catch (AssemblerException exception)
                {
                    System.Windows.MessageBox.Show(exception.Message, "Assembler error");
                }
            }
        }

        private void File_LoadProgramFromFileToCodeWindow_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog loadProgramFileDialog = new OpenFileDialog();

            if (loadProgramFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string codeText = File.ReadAllText(loadProgramFileDialog.FileName);
                this.codeWindow.CodeBox.Text = codeText;
                this.codeWindow.Show();
            }
        }
        #endregion

        #region Windows Menu
        private void Windows_Code_Click(object sender, RoutedEventArgs e)
        {
            this.codeWindow.Show();
        }

        private void Windows_Registers_Click(object sender, RoutedEventArgs e)
        {
            this.registerWindow.Show();
        }

        private void Windows_Memory_Click(object sender, RoutedEventArgs e)
        {
            this.memoryWindow.Show();
        }
        #endregion

        #region Window
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.registerWindow.CloseWindow();
            this.memoryWindow.CloseWindow();
            this.codeWindow.CloseWindow();
        }
        #endregion

        #endregion

        #endregion

    }
}
