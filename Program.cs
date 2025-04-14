namespace ZweedsPesten_Agent
{
    public class Program
    {
        public static void Main(string[] args) {
            const int maxDepth = 5;
            const int maxEpisodes = 1400;
            const int minSamplesSplit = 5;
            var qFunction = new TILDE_RT(maxDepth, minSamplesSplit);
            var zweedsPestenAgent = new Agent(qFunction);
            zweedsPestenAgent.Train(maxEpisodes);
        }
    }
}
