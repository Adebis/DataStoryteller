using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class MainController : MonoBehaviour
{
    public Material lineMat;
    // Specified in the editor
    public GameObject node_prefab;
    public GameObject edge_prefab;
    public GameObject node_text_prefab;
    // Camera movement variables
    public float zoom_speed;
    public float pan_speed;

    private Vector3 drag_origin;

    private DataStoryteller data_storyteller;
    private List<GameObject> all_node_objects;
    // Map nodes to their corresponding node controllers.
    private Dictionary<Node, GameObject> node_map;

    // Each edge line between two nodes will be handled by a game object.
    // The first key is the source node. The second key is the destination node. The value is the game object of the edge.
    private Dictionary<Node, Dictionary<Node, GameObject>> edge_map;

    // Use this for initialization
    void Start()
    {
        print("Starting MainController");
        //lineMat = new Material("Shader \"Lines/Colored Blended\" {" + "SubShader { Pass { " + "    Blend SrcAlpha OneMinusSrcAlpha " + "    ZWrite Off Cull Off Fog { Mode Off } " + "    BindChannels {" + "      Bind \"vertex\", vertex Bind \"color\", color }" + "} } }");
        node_map = new Dictionary<Node, GameObject>();
        all_node_objects = new List<GameObject>();
        edge_map = new Dictionary<Node, Dictionary<Node, GameObject>>();
        //line_game_objects = new Dictionary<Node, GameObject>();

        // Create a new data storyteller.
        data_storyteller = new DataStoryteller();
        InstantiateGraphNodes();
    } //end start

    private void InstantiateGraphNodes()
    {
        Dictionary<string, Dictionary<string, double>> zsec_months = data_storyteller.FilterData("Zsec", "month");
        Dictionary<string, Dictionary<string, double>> zsec_years = data_storyteller.FilterData("Zsec", "year");
        Dictionary<string, Dictionary<string, double>> zsec_sites = data_storyteller.FilterData("Zsec", "site");
        // Create a node for each datapoint
        // 
    }//end method InstatiateGraphs

    // Instatiate a set of connected topic nodes.
    private void InstantiateTopicNodes()
    {
        // For each of the storyteller's nodes, create a node prefab object
        float x_placement = 0;
        float y_placement = 0;
        // Set node's optimal distance according to the number of nodes in the graph.
        int node_count = data_storyteller.nodes.Count;
        foreach (Node node in data_storyteller.nodes)
        {
            // Randomly displace in the x and y directions.
            float x_displacement = Random.Range(-node_count * 1.5f, node_count * 1.5f);
            float y_displacement = Random.Range(-node_count * 1.5f, node_count * 1.5f);
            GameObject new_node_object = Instantiate(node_prefab, new Vector3(x_displacement, y_displacement, 0), Quaternion.identity);
            new_node_object.GetComponent<NodeController>().optimal_neighbor_distance = Mathf.Max(node_count * 1.5f, 25.0f);
            new_node_object.GetComponent<NodeController>().inner_node = node;
            all_node_objects.Add(new_node_object);
            node_map.Add(node, new_node_object);
            // Create a text object, making it a child of the source node.
            GameObject node_text = Instantiate(this.node_text_prefab, new_node_object.transform);
            new_node_object.GetComponent<NodeController>().node_text_object = node_text;
        }//end foreach

        // Go through all the new game objects and give them their neighbors.
        foreach (KeyValuePair<Node, GameObject> node_map_entry in node_map)
        {
            // A map between the nodes that are neighbors of the current source node and the game objects representing the edge between the two.
            Dictionary<Node, GameObject> current_neighbor_map = new Dictionary<Node, GameObject>();
            foreach (Node neighbor in node_map_entry.Key.neighbors)
            {
                GameObject neighbor_object = node_map[neighbor];
                node_map_entry.Value.GetComponent<NodeController>().neighbor_node_objects.Add(neighbor_object);
                // Create a line edge object, making it a child of the source node.
                GameObject line_edge_object = Instantiate(this.edge_prefab, node_map_entry.Value.transform);
                // Add it to the local map.
                current_neighbor_map.Add(neighbor, line_edge_object);
                // Add it to the node's edge map
                node_map_entry.Value.GetComponent<NodeController>().edge_map.Add(neighbor, line_edge_object);
            }//end foreach
            // Add the local map to the global map.
            this.edge_map.Add(node_map_entry.Key, current_neighbor_map);
        }//end foreach

        // Begin the scene with all story nodes selected and locked.
        foreach (Node story_node in data_storyteller.story_nodes)
        {
            node_map[story_node].GetComponent<NodeController>().ToggleSelect();
            node_map[story_node].GetComponent<NodeController>().ToggleLock();
        }//end foreach
    }//end method InstantiateTopicNodes

    // Update is called once per frame
    void Update ()
    {
        // MOUSE CONTROLS
        // Zoom in/out with scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        //transform.Translate(0, scroll * 2.0f, scroll * 2.0f, Space.World);
        Camera.main.orthographicSize = Camera.main.orthographicSize - scroll * zoom_speed;
        //Pan camera with right mouse button
        if (Input.GetMouseButtonDown(1)) //Store the click position wheneve the mouse is clicked
            drag_origin = Input.mousePosition;
        if (Input.GetMouseButton(1)) //While the mouse is down translate the position of the camera
        {
            Vector3 delta = Camera.main.ScreenToWorldPoint(drag_origin) - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Camera.main.transform.Translate(delta.x, delta.y, 0);
            drag_origin = Input.mousePosition;
        } //end if

        // If we are dragging any of the nodes, and the left mouse button comes up, stop here.
        if (Input.GetMouseButtonUp(0))
        {
            foreach (GameObject node_object in all_node_objects)
            {
                NodeController temp_controller = node_object.GetComponent<NodeController>();
                if (temp_controller.mouse_dragged)
                    temp_controller.EndDrag();
                // Also check whether we're locking the node down or not.
                if (temp_controller.maybe_lock)
                {
                    float mouse_up_time = Time.time;
                    // Click in less than the amount of time (in seconds) to toggle lock on a node.
                    if (mouse_up_time - temp_controller.mouse_down_time < 0.15f)
                        temp_controller.ToggleLock();
                }//end if
                // Check if we're selecting the node or not.
                if (temp_controller.maybe_select)
                {
                    float mouse_up_time = Time.time;
                    // Click in less than the amount of time (in seconds) to toggle select on a node.
                    if (mouse_up_time - temp_controller.mouse_down_time < 0.15f)
                        temp_controller.ToggleSelect();
                }//end if
            }//end foreach
        }//end if


        // GRAPH EDGE DRAWING
        List<Edge> parsed_edges = new List<Edge>();
        // Go through all the edges and draw a line between the nodes (if the nodes are set to draw lines).
        foreach (Edge edge in data_storyteller.edges)
        {
            bool skip_edge = false;
            Node current_src = edge.src;
            Node current_dest = edge.dest;
            GameObject edge_object = this.edge_map[edge.src][edge.dest];
            LineRenderer edge_renderer = edge_object.GetComponent<LineRenderer>();
            // Check the parsed edges. If a line has already been drawn between this
            // pair of nodes, do not do so again.
            foreach (Edge parsed_edge in parsed_edges)
            {
                if (current_src == parsed_edge.dest && current_dest == parsed_edge.src)
                {
                    skip_edge = true;
                    break;
                }//end if
            }//end foreach
            // If we are not drawing edges from this node, skip it.
            if (node_map[current_src].GetComponent<NodeController>().draw_edges == false)
                skip_edge = true;
            if (skip_edge)
            {
                // If we are skipping this edge, disable it's line renderer if it's enabled.
                if (edge_renderer.enabled)
                    edge_renderer.enabled = false;
                continue;
            }//end if
            parsed_edges.Add(edge);

            if (edge_renderer.enabled == false)
                edge_renderer.enabled = true;
            this.DrawLine(edge.src, edge.dest);
        }//end foreach
    } //end update

    // Draw a line from one node to another.
    private void DrawLine(Node source, Node destination)
    {
        // Fetch this pair's edge game object and node objects from the global maps.
        GameObject edge_object = this.edge_map[source][destination];
        GameObject source_object = this.node_map[source];
        GameObject destination_object = this.node_map[destination];

        Vector3 src_position = source_object.transform.position;
        Vector3 dest_position = destination_object.transform.position;

        // Set the edge object's line renderer positions to the src and dest positions.
        Vector3[] line_points = { src_position, dest_position };
        LineRenderer edge_renderer = edge_object.GetComponent<LineRenderer>();
        edge_renderer.SetPositions(line_points);

        // Set the color.
        // If either the source or destination node is moused over, the color will be different.
        if (source_object.GetComponent<NodeController>().moused_over || destination_object.GetComponent<NodeController>().moused_over)
        {
            edge_renderer.startColor = Color.white;
            edge_renderer.endColor = Color.white;
        }//end if
        else
        {
            edge_renderer.startColor = Color.black;
            edge_renderer.endColor = Color.black;
        }//end if
    }//end method DrawLine

    void OnPostRender()
    {
        /*Shader shader = Shader.Find("Lines/Colored Blended");
        List<Edge> parsed_edges = new List<Edge>();
        // Go through all the edges and draw a line between the nodes (if the nodes are set to draw lines).
        foreach (Edge edge in data_storyteller.edges)
        {
            bool skip_edge = false;
            Node current_src = edge.src;
            Node current_dest = edge.dest;
            // Check the parsed edges. If a line has already been drawn between this
            // pair of nodes, do not do so again.
            foreach (Edge parsed_edge in parsed_edges)
            {
                if (current_src == parsed_edge.dest && current_dest == parsed_edge.src)
                {
                    skip_edge = true;
                    break;
                }//end if
            }//end foreach
            if (skip_edge)
                continue;

            // Draw a line between the corresponding src and dest node's game objects
            /*GameObject src_object = this.node_map[current_src];
            GameObject dest_object = this.node_map[current_dest];
            Vector3 src_position = src_object.transform.position;
            Vector3 dest_position = dest_object.transform.position;
            // Line drawing code adapted from: https://gamedev.stackexchange.com/questions/96964/how-to-correctly-draw-a-line-in-unity
            GL.Begin(GL.LINES);
            lineMat.SetPass(0);
            
            GL.Color(new Color(255f, 255f, 255f, 0.25f));
            GL.Vertex3(src_position.x, src_position.y, src_position.z);
            GL.Vertex3(dest_position.x, dest_position.y, dest_position.z);
            GL.End();
        }//end foreach*/
    } //end method OnPostRender
    

}
