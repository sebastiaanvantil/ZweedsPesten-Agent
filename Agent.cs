namespace ZweedsPesten_Agent;

public class Agent {
    private readonly TILDE_RT _qFunction;
    private readonly List<Dictionary<string, object>> _examples;
    private readonly int _agentId;
    
    public Agent(TILDE_RT qFunction) {
        _qFunction = qFunction;
        _examples = new List<Dictionary<string, object>>();
        
        var rnd = new Random();
        _agentId = rnd.Next(1, 3);
    }

    public void Train(int maxEpisodes) {
        for (int episode = 0; episode < maxEpisodes; episode++) {
            Console.WriteLine("Starting Episode: " + episode);
            var newExamples = RunEpisode();
            var examplesToRemove = new List<Dictionary<string, object>>();
            foreach (var newExample in newExamples) {
                _examples.Add(newExample);
                examplesToRemove.AddRange(_examples.Where(example => EqualDicts(newExample, example)));
            }
            foreach (var example in examplesToRemove) {
                _examples.Remove(example);
            }
            UpdateQFunction(_examples);
        }
        _qFunction.PrintTree(_qFunction.Root, 100);
    }

    private List<Dictionary<string, object>> RunEpisode() {
        var game = new Game(3, _agentId, _qFunction);
        game.Run();
        var newExamples = game.GetExamplesFromHistory();
        return newExamples;
    }

    private void UpdateQFunction(List<Dictionary<string, object>> examples) {
        _qFunction.Train(examples);
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