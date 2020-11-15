using System;

namespace IntcodeComputer
{
    internal static class GravityAssist
    {
        public static Tuple<int, int> AdjustFor(string program, int target)
        {
            var processor = new Processor();

            for (var noun = 0; noun <= 99; noun++)
            {
                for(var verb = 0; verb <= 99; verb++)
                {
                    processor.Load(program);
                    processor.Poke(1, noun);
                    processor.Poke(2, verb);
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
