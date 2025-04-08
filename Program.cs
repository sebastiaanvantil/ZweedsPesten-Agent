namespace ZweedsPesten_Agent
{
    public class Program
    {
        public static void Main(string[] args) {
            const int maxDepth = 10;
            const int maxEpisodes = 400;
            var qFunction = new TILDE_RT(maxDepth, 2);
            var zweedsPestenAgent = new Agent(qFunction);
            zweedsPestenAgent.Train(maxEpisodes);
            TILDE_RT.PrintTree(qFunction.Root);
            }
    }
}
