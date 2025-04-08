namespace ZweedsPesten_Agent;

public class TILDE_RT_Node {
    public string? Test;
    public TILDE_RT_Node? LeftChild;
    public TILDE_RT_Node? RightChild;
    public List<TILDE_RT_Node> ActionChildren = [];
    public double Qvalue;
    public bool IsLeaf;

    public TILDE_RT_Node(bool isIsLeaf, string? test = null, double? qvalue = null) {
        Test = test;
        Qvalue = qvalue ?? 0;
        IsLeaf = isIsLeaf;
    }
    
    public bool IsLeafNode() => LeftChild == null && RightChild == null;
}