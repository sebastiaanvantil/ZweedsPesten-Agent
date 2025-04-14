using System.Diagnostics;

namespace ZweedsPesten_Agent;

public class Agent {
    private readonly TILDE_RT _qFunction;
    private List<Dictionary<string, object>> _examples;
    private readonly int _agentId;
    private const int MaxIterations = 300;
    
    public Agent(TILDE_RT qFunction) {
        _qFunction = qFunction;
        _examples = [];
        
        var rnd = new Random();
        _agentId = rnd.Next(1, 3);
    }

    public void Train(int maxEpisodes) {
        double timeElapsed = 0;
        for (int episode = 0; episode < maxEpisodes; episode++) {
            Console.WriteLine("Starting Episode: " + episode);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var newExamples = RunEpisode(episode);
            AddNewExamplesToOldExamples(newExamples);
            _qFunction.Train(_examples);
            stopwatch.Stop();
            timeElapsed += stopwatch.Elapsed.TotalSeconds;
            double meanEpisodeTime = timeElapsed / (episode + 1);
            double projectedFinishTime = ((maxEpisodes - episode) * meanEpisodeTime) / 3600;
            Console.WriteLine("Finished Episode " + episode + 
                              "\nRuntime: " + stopwatch.Elapsed.TotalSeconds.ToString("F2") + " seconds" +
                              "\nMean Runtime: " + meanEpisodeTime.ToString("F2") + " seconds" +
                              "\nProjected Finishing Time: " + projectedFinishTime.ToString("F2") + " hours" +
                              "\n");
        }
        Console.WriteLine("Training has finished");
        Console.WriteLine("Number of training examples: " + _examples.Count);
        double avgQ = _examples.Sum(example => (double)example["q_value"]) / _examples.Count;
        Console.WriteLine("Average q_value: " + avgQ);
        _qFunction.PrintTree();
    }

    private void AddNewExamplesToOldExamples(List<Dictionary<string, object>> newExamples) {
        foreach (var newExample in newExamples) {
            var equalExample = _examples.Find(example => EqualDicts(newExample, example));
            if (equalExample != null) {
                int count = (int)equalExample["count"];
                double newQValue = (count * (double)newExample["q_value"] + (double)newExample["q_value"]) / (count + 1);
                equalExample["count"] = count + 1;
                equalExample["q_value"] = newQValue;
            }
            else {
                _examples.Add(newExample);
            }
        }
    }

    private List<Dictionary<string, object>> RunEpisode(int episode) {
        
        var game = new Game(3, _agentId, _qFunction);
        (int iterations, int invalidGameCount) = game.Run();
        var newExamples = new List<Dictionary<string, object>>();
        if (iterations < MaxIterations) {
            newExamples = game.GetExamplesFromHistory();
        }
        return newExamples;
    }

    private static bool EqualDicts(Dictionary<string, object> a, Dictionary<string, object> b) {
        var excludedKeys = new HashSet<string> { "q_value", "count" };

        var keysA = a.Keys.Where(k => !excludedKeys.Contains(k)).ToList();
        var keysB = b.Keys.Where(k => !excludedKeys.Contains(k)).ToList();

        if (keysA.Count != keysB.Count) {
            return false;
        }

        foreach (string key in keysA) {
            if (!b.TryGetValue(key, out object? value)) return false;
            if (!Equals(a[key], value)) return false;
        }

        return true;
    }

}