using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NiosII_Simulator.Core;

namespace NiosII_Simulator.Test
{
    /// <summary>
    /// Tests the Vm
    /// </summary>
    [TestClass]
    public class TestVM
    {
        private VirtualMachine virtualMachine;

        [TestInitialize]
        public void Initialize()
        {
            this.virtualMachine = new VirtualMachine();
        }

        [TestMethod]
        public void TestInstructions()
        {
            //Test the I-format
            this.virtualMachine.SetRegisterValue(Registers.R8, 17);
            IFormatInstruction addiInst = new IFormatInstruction(
                OperationCodes.Addi.Code(),
                Registers.R8.Number(),
                Registers.R9.Number(),
                278);

            this.virtualMachine.ExecuteInstruction(addiInst.Encode());
            Assert.AreEqual(278 + 17, this.virtualMachine.GetRegisterValue(Registers.R9));

            this.virtualMachine.SetRegisterValue(Registers.R8, 17);
            addiInst = new IFormatInstruction(
                OperationCodes.Addi.Code(),
                Registers.R8.Number(),
                Registers.R9.Number(),
                -234);

            this.virtualMachine.ExecuteInstruction(addiInst.Encode());
            Assert.AreEqual(-234 + 17, this.virtualMachine.GetRegisterValue(Registers.R9));

            //Test the R-format
            this.virtualMachine.SetRegisterValue(Registers.R8, 17);
            this.virtualMachine.SetRegisterValue(Registers.R9, 4717);
            RFormatInstruction addInst = new RFormatInstruction(
                OperationCodes.Add.Code(),
                OperationXCodes.Add,
                Registers.R8.Number(),
                Registers.R9.Number(),
                Registers.R10.Number());

            this.virtualMachine.ExecuteInstruction(addInst.Encode());
            Assert.AreEqual(4717 + 17, this.virtualMachine.GetRegisterValue(Registers.R10));

            //Tests memory instructions
            this.virtualMachine.SetRegisterValue(Registers.R8, 1024);
            this.virtualMachine.SetRegisterValue(Registers.R9, 4711);
            IFormatInstruction storeInst = new IFormatInstruction(
                OperationCodes.Stw.Code(),
                Registers.R8.Number(),
                Registers.R9.Number(),
                0);

            this.virtualMachine.ExecuteInstruction(storeInst.Encode());
            int value = this.virtualMachine.ReadWordFromMemory(1024);
            Assert.AreEqual(4711, value);

            IFormatInstruction loadInst = new IFormatInstruction(
                OperationCodes.Ldw.Code(),
                Registers.R8.Number(),
                Registers.R10.Number(),
                0);

            this.virtualMachine.ExecuteInstruction(loadInst.Encode());
            Assert.AreEqual(4711, this.virtualMachine.GetRegisterValue(Registers.R10));

            //Tests branch instructions
            IFormatInstruction branchInst = new IFormatInstruction(
                OperationCodes.Br.Code(),
                0,
                0,
                4 * 4);

            this.virtualMachine.ExecuteInstruction(branchInst.Encode());
            Assert.AreEqual((uint)(4 * 4), this.virtualMachine.ProgramCounter);
        }

        [TestMethod]
        public void TestProgram()
        {
            this.virtualMachine.SetRegisterValue(Registers.R9, 0);

            var instructions = new Instruction[]
            {
                new IFormatInstruction(OperationCodes.Addi.Code(), Registers.R0.Number(), Registers.R8.Number(), 50).AsInstruction(),
                new IFormatInstruction(OperationCodes.Addi.Code(), Registers.R8.Number(), Registers.R8.Number(), -1).AsInstruction(),
                new IFormatInstruction(OperationCodes.Addi.Code(), Registers.R9.Number(), Registers.R9.Number(), 5).AsInstruction(),
                new IFormatInstruction(OperationCodes.Bne.Code(), Registers.R0.Number(), Registers.R8.Number(), -12).AsInstruction()
            };

            var testProgram = Program.NewProgram(instructions, new Dictionary<string, uint>(), null);
            this.virtualMachine.Run(testProgram);
            Assert.AreEqual(50 * 5, this.virtualMachine.GetRegisterValue(Registers.R9));
        }
    }
}
