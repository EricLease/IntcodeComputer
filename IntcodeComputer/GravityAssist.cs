using System;

namespace IntcodeComputer
{
    internal static class GravityAssist
    {
        private const int MinNoun = 0;
        private const int MinVerb = 0;
        private const int MaxNoun = 99;
        private const int MaxVerb = 99;
        private const int NounInputAddress = 1;
        private const int VerbInputAddress = 2;

        internal static Tuple<int, int> AdjustFor(string program, int target)
        {
            var processor = new Processor();

            for (var noun = MinNoun; noun <= MaxNoun; noun++)
            {
                for(var verb = MinVerb; verb <= MaxVerb; verb++)
                {
                    processor.Load(program);
                    processor.Poke(NounInputAddress, noun);
                    processor.Poke(VerbInputAddress, verb);
                    processor.Run();

                    if (processor.ProgramOutput == target)
                    {
                        return new Tuple<int, int>(noun, verb);
                    }
                }
            }

            return null;
        }
    }
}
