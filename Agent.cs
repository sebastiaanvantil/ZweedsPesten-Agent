namespace ZweedsPesten_Agent;

public class Agent {
    private TILDE_RT QFunction;
    private List<Dictionary<string, object>> examples;
    private int AgentId;
    
    public Agent(TILDE_RT qFunction) {
        QFunction = qFunction;
        examples = new List<Dictionary<string, object>>();
        
        var rnd = new Random();
        AgentId = rnd.Next(1, 4);
    }

    public void Train(int maxEpisodes) {
        for (int episode = 0; episode < maxEpisodes; episode++) {
            var newExamples = RunEpisode();
            foreach (var example in newExamples) {
                
            }
            UpdateQFunction(examples);
        }
    }

    public List<Dictionary<string, object>> RunEpisode() {
        var game = new Game(3, AgentId, QFunction);
        game.Run();
        var newExamples = game.GetExamplesFromHistory();
        return newExamples;
    }

    public void UpdateQFunction(List<Dictionary<string, object>> examples) {
        QFunction.Train(examples);
    }

    public bool EqualDicts(Dictionary<string, object> a, Dictionary<string, object> b) {
        const string excludedKey = "q_value";
        
        var keysA = a.Keys.Where(k => k != excludedKey).ToList();
        var keysB = b.Keys.Where(k => k != excludedKey).ToList();

        if (keysA.Count != keysB.Count) {
            return false;
        }

        foreach (string key in keysA) {
            if (!b.ContainsKey(key)) return false;
            if (!Equals(a[key], b[key])) return false;
        }

        return true;
    }
}