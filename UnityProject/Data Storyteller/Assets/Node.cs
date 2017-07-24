using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Represents a single node in the graph.
public class Node
{
    public List<Node> neighbors;
    public string name;
    public Dictionary<string, string> node_data;
    public bool is_story_node;

    public Node()
    {
        name = "";
        neighbors = new List<Node>();
        node_data = new Dictionary<string, string>();
        is_story_node = false;
    }//end constructor Node
    public Node(string name_in)
    {
        name = name_in;
        neighbors = new List<Node>();
        node_data = new Dictionary<string, string>();
        is_story_node = false;
    }//end constructor Node

    public void AddNeighbor(Node neighbor_in)
    {
        // Don't accept duplicate neighbors.
        if (!neighbors.Contains(neighbor_in))
            neighbors.Add(neighbor_in);
    }//end method AddNeighbor
}
