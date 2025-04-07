using System.Diagnostics;

namespace ZweedsPesten_Agent;

public class Agent {
    private readonly TILDE_RT _qFunction;
    private List<Dictionary<string, object>> _examples;
    private readonly int _agentId;
    private const int MaxIterations = 2000;
    
    public Agent(TILDE_RT qFunction) {
        _qFunction = qFunction;
        _examples = new List<Dictionary<string, object>>();
        
        var rnd = new Random();
        _agentId = rnd.Next(1, 3);
    }

    public void Train(int maxEpisodes) {
        for (int episode = 0; episode < maxEpisodes; episode++) {
            var newExamples = RunEpisode(episode);
            var examplesToRemove = new List<Dictionary<string, object>>();
            foreach (var newExample in newExamples) {
                examplesToRemove.AddRange(_examples.Where(example => EqualDicts(newExample, example)));
                _examples.Add(newExample);
            }
            foreach (var example in examplesToRemove) {
                _examples.Remove(example);
            }
            _examples.RemoveAll(example => (double)example["q_value"] == 0);
            _qFunction.Train(_examples);
        }
        _qFunction.PrintTree(_qFunction.Root, 100);
    }

    private List<Dictionary<string, object>> RunEpisode(int episode) {
        Console.WriteLine("Starting Episode: " + episode);
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var game = new Game(3, _agentId, _qFunction);
        (int iterations, int invalidGameCount) = game.Run();
        var newExamples = new List<Dictionary<string, object>>();
        if (iterations < MaxIterations) {
            newExamples = game.GetExamplesFromHistory();
        }
        stopwatch.Stop();
        Console.WriteLine("Finished Episode " + episode + 
                          "\nInvalid Game Count: " + invalidGameCount + 
                          "\nRuntime: " + stopwatch.Elapsed.TotalSeconds + " seconds" +
                          "\n");
        return newExamples;
    }

    private static bool EqualDicts(Dictionary<string, object> a, Dictionary<string, object> b) {
        const string excludedKey = "q_value";
        
        var keysA = a.Keys.Where(k => k != excludedKey).ToList();
        var keysB = b.Keys.Where(k => k != excludedKey).ToList();

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