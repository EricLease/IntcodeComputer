using System;
using System.Collections.Generic;
using System.Linq;

namespace IntcodeComputer
{
    internal class Processor
    {
        internal ProcessorState State { get; private set; }
        internal string Message { get; private set; }
        internal int? ProgramOutput => (_memory?.ContainsKey(0) ?? false) ? _memory[0] : null;
        
        private Dictionary<uint, int?> _memory;
        private uint _ip; // instruction pointer
        private uint _max; // current max memory address

        private int? CurrentInstruction =>  _memory.ContainsKey(_ip) ? _memory[_ip] : null;

        internal Processor(string program = null) => Load(program);

        internal bool Load(string program)
        {
            State = ProcessorState.Fresh;
            Message = "";
            _memory = null;
            _ip = 0;
            _max = 0;
            
            if (string.IsNullOrWhiteSpace(program)) return false;

            _memory = program
                .Split(',')
                .Select((v, k) => new
                {
                    k,
                    v = int.TryParse(v, out int n) ? (int?)n : null
                })
                .ToDictionary(kvp => (uint)kvp.k, kvp => kvp.v);
            State = ProcessorState.Loaded;
            _max = (uint)_memory.Keys.Count - 1;

            return true;
        }

        internal void Poke(uint addr, int value)
        {
            if (State != ProcessorState.Loaded)
            {
                throw new ApplicationException("Ensure `Load()` returns `true` before calling `Poke()`");
            }

            if (!_memory.ContainsKey(addr))
            {
                FillMemory(addr);
            }

            _memory[addr] = value;
        }

        internal string Run()
        {
            if (State != ProcessorState.Loaded)
            {
                throw new ApplicationException("Ensure `Load()` returns `true` before calling `Run()`");
            }

            State = ProcessorState.Running;

            while (State == ProcessorState.Running) ExecuteInstruction();

            return string.Join(",", 
                _memory.Select(kvp => kvp.Value.HasValue 
                    ? kvp.Value.Value.ToString() : "null"));
        }

        private void ExecuteInstruction()
        {
            switch (CurrentInstruction ?? -1)
            {
                case InstructionSet.Add:
                case InstructionSet.Multiply:
                    BinaryOperation();                    
                    return;

                case InstructionSet.Halt:
                    State = ProcessorState.Halted;
                    Message += $"Program completed successfully at {_ip}\n";
                    return;

                default:
                    State = ProcessorState.Error;
                    Message += $"ERROR: Invalid instruction ({(CurrentInstruction?.ToString() ?? "null")}) at {_ip}\n";
                    return;

            }
        }

        private void BinaryOperation()
        {
            var p1 = GetParameter(1);
            var p2 = GetParameter(2);
            var dest = GetDestinationAddress();

            if (State == ProcessorState.Error) return;

            FillMemory(dest.Value);
            _memory[dest.Value] = CurrentInstruction == InstructionSet.Add
                ? (p1 + p2) 
                : (p1 * p2);
            _ip += 4;
        }

        private int? GetParameter(uint offset)
        {
            var addr = _ip + offset;
            var opAddr = _memory.ContainsKey(addr) ? _memory[addr] : null;
            var param =  opAddr.HasValue && opAddr.Value > -1 && _memory.ContainsKey((uint)opAddr.Value)
                ? _memory[(uint)opAddr.Value]
                : null;

            if (!param.HasValue)
            {
                State = ProcessorState.Error;
                Message += $"ERROR: Parameter {offset} invalid for instruction at {_ip} ({CurrentInstruction})\n";
            }

            return param;
        }

        private uint? GetDestinationAddress(uint offset = 3)
        {
            var addr = _ip + offset;
            var dest = _memory.ContainsKey(addr) && 
                _memory[addr].HasValue && 
                _memory[addr].Value > -1 
                ? (uint?)_memory[addr] 
                : null;

            if (!dest.HasValue)
            {
                State = ProcessorState.Error;
                Message += $"ERROR: Invalid destination address for instruction at {_ip} ({CurrentInstruction})\n";
            }

            return dest;
        }

        private void FillMemory(uint test)
        {
            if (test <= _max) return;

            for (var i = _max + 1; i <= test; i++)
                _memory.Add(i, null);

            _max = test;
        }
    }
}
