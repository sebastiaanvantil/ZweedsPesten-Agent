namespace ZweedsPesten_Agent;

public class TILDE_RT_Node {
    public string? Test;
    public TILDE_RT_Node? LeftChild;
    public TILDE_RT_Node? RightChild;
    public double Qvalue;

    public TILDE_RT_Node(string? test = null, double? qvalue = null) {
        Test = test;
        Qvalue = qvalue ?? 0;
    }
    
    public bool IsLeafNode() => Qvalue == 0;
}