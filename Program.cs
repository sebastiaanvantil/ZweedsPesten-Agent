namespace ZweedsPesten_Agent
{
    public class Program
    {
        public static void Main(string[] args) {
            
            var qFunction = new TILDE_RT(100, 2);
            var zweedsPestenAgent = new Agent(qFunction);
            zweedsPestenAgent.Train(10);
            qFunction.PrintTree(qFunction.Root, 100);
            }
    }
}
