using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class NodeController : MonoBehaviour
{
    public float diameter;

    public Node inner_node;
    public List<GameObject> neighbor_node_objects;
    // A map of the game objects acting as this node's edges, keyed to the destination node.
    public Dictionary<Node, GameObject> edge_map;

    // This node's text
    public GameObject node_text_object;

    // Whether or not we should draw the edges coming from or to this node.
    public bool draw_edges;
    // Whether or not other neighbors should obey spring forces for this node.
    public bool obey_spring_forces;

    // Whether or not this node is currently moused over.
    public bool moused_over;
    public bool mouse_dragged;
    public bool mouse_exit_flag;
    public bool maybe_select;
    public bool maybe_lock;
    public float mouse_down_time;
    public bool locked_down;
    public bool selected;

    // The sprite renderer for this node.
    public SpriteRenderer sprite_renderer;

    // A bunch of useful color defenitions
    public Color current_color;
    public Color idle_color;
    public Color active_color;
    private Color light_blue;
    private Color medium_blue;
    private Color dark_blue;
    private Color light_green;
    private Color faded_white;

    // Physics-related variables
    // What the optimal length between this node and one of its neighbors is.
    public float optimal_neighbor_distance;
    // What is the spring constant
    public float spring_constant;
    // This object's 2d rigidbody
    private Rigidbody2D node_rigidbody;
    // Whether or not this node is a node in a line/spatial graph
    public bool data_node;

    // Use this for initialization
    void Awake()
    {
        // The length and width of this node is 500 units.
        // This is based on the node's sprite, which is 522x522 pixels with a 1-to-1 pixel to unit ratio.
        data_node = false;
        sprite_renderer = gameObject.GetComponent<SpriteRenderer>();
        diameter = (float)sprite_renderer.bounds.size.x;
        inner_node = new Node();
        neighbor_node_objects = new List<GameObject>();
        edge_map = new Dictionary<Node, GameObject>();

        draw_edges = false;
        obey_spring_forces = false;

        node_rigidbody = gameObject.GetComponent<Rigidbody2D>();
        // Starting color of the node.
        this.DefineColors();
        current_color = Color.white;

        idle_color = Color.white;
        active_color = light_blue;

        ChangeColor(idle_color);

        moused_over = false;
        mouse_dragged = false;
        mouse_exit_flag = false;

        locked_down = false;
        maybe_lock = false;
        maybe_select = false;
        selected = false;
        mouse_down_time = 0f;

        // Initialize physics variables
        optimal_neighbor_distance = 25.0f;
        spring_constant = 1.0f;
    }

    private void DefineColors()
    {
        faded_white = new Color(1.0f, 1.0f, 1.0f, 0.5f);
        light_blue = new Color(0.25f, 0.25f, 1, 1);
        medium_blue = new Color(0.05f, 0.1f, 1f, 1);
        dark_blue = new Color(0, 0, 0.75f, 1);
        light_green = new Color(0.25f, 1, 0.25f, 1);
    }//end method DefineColors

    void Start()
    {

    } //end Start

    public void SetInnerNode(Node node_in)
    {
        inner_node = node_in;
    }//end method SetInnerNode

    // Update is called once per frame.
    // Use for graphics.
    void Update()
    {
        if (!data_node)
        {
            DisplayText();
            // If the node is locked down, don't put any forces on it (but the damping force).
            if (!locked_down)
            {
                // Check each of this node's neighbors.
                foreach (GameObject neighboring_node in this.neighbor_node_objects)
                {
                    // Skip this neighbor if both this node and this neighbor are telling each other not to obey spring forces.
                    if (!obey_spring_forces && !neighboring_node.GetComponent<NodeController>().obey_spring_forces)
                        continue;
                    Vector3 node_position = gameObject.transform.position;
                    Vector3 neighbor_position = neighboring_node.transform.position;
                    float distance = Vector3.Distance(node_position, neighbor_position);
                    float spring_displacement = this.optimal_neighbor_distance - distance;
                    // If the displacement isn't over some threshold, don't apply a restoring force.
                    if (Mathf.Abs(spring_displacement) < 0.1f)
                        continue;
                    // Apply a force pushing (or pulling) this node if it is not at the optimal distance.
                    // The direction of the force.
                    Vector2 restoring_force = new Vector2(neighbor_position.x - node_position.x, neighbor_position.y - node_position.y);
                    // Apply the magnitude of the spring force to its direction.
                    restoring_force *= -spring_displacement * spring_constant;

                    //print("Restoring force: " + restoring_force.ToString());

                    node_rigidbody.AddForce(restoring_force);
                }//end foreach
                // Damp this node's movements
                Vector2 damping_force = node_rigidbody.velocity;
                float damping_factor = 5.0f;
                damping_force *= -1 * damping_factor;
                node_rigidbody.AddForce(damping_force);
            }//end if
            else
            {
                // Remove this node's velocity.
                node_rigidbody.velocity = new Vector2(0, 0);
            }//end else
        }//end if

    } //end method Update

    // Set this node's text and display it.
    private void DisplayText()
    {
        string node_text_string = "";
        node_text_string = inner_node.name;
        if (inner_node.is_story_node)
        {
            node_text_string += ": " + inner_node.node_data["date"] + ", " + inner_node.node_data["secchi_depth"] + " zdepth.";
        }//end if
        this.node_text_object.GetComponent<TextMesh>().text = node_text_string;
    }//end method DisplayText

    // Color the node its idle color
    private void ColorIdle()
    {
        // If we are locked down and selected, then our idle color is dark blue.
        if (locked_down && selected)
            idle_color = dark_blue;
        // If we are just locked down, then our idle color is medium blue.
        else if (locked_down)
            idle_color = medium_blue;
        // If we are just selected, then our idle color is light blue.
        else if (selected)
            idle_color = light_blue;
        // If neither of these are true, our idle color is white.
        else
            idle_color = Color.white;
        ChangeColor(idle_color);
    }//end method ColorIdle
    // Color the node its active color
    private void ColorActive()
    {
        active_color = light_green;
        ChangeColor(active_color);
    }//end method ColorActive

    // Color the node its active color

    // Change just the hue, not the opacity.
    private void ChangeHue(Color new_color)
    {
        Color hue_change = new Color(new_color.r, new_color.g, new_color.b, this.current_color.a);
        this.current_color = hue_change;
        sprite_renderer.material.color = current_color;
    }//end method ChangeColor
    // Change both the hue and the opacity.
    private void ChangeColor(Color new_color)
    {
        this.current_color = new_color;
        sprite_renderer.material.color = current_color;
    }//end method ChangeColor

    // Stop mouse-dragging this node.
    public void EndDrag()
    {
        mouse_dragged = false;
        // Change the color
        //ColorIdle();
        moused_over = false;
    }//end function EndDrag

    // Lock the position of this node wherever it is.
    public void LockNode()
    {
        locked_down = true;
        node_rigidbody.isKinematic = true;
        ColorIdle();
    }//end method LockDown
    public void UnlockNode()
    {
        locked_down = false;
        node_rigidbody.isKinematic = false;
        ColorIdle();
    }//end method UnlockNode
    public void ToggleLock()
    {
        if (locked_down)
        {
            UnlockNode();
        }//end if
        else
        {
            LockNode();
        }//end else
        maybe_lock = false;
    }//end method LockDown

    public void ToggleSelect()
    {
        if (selected)
        {
            DeselectNode();
        }//end if
        else
        {
            SelectNode();
        }//end else
        maybe_select = false;
    }//end method ToggleSelect
    // If the user clicks on the node, we consider that node selected.
    public void SelectNode()
    {
        selected = true;
        // Draw the node's outgoing edges.
        draw_edges = true;
        obey_spring_forces = true;
    }//end method SelectNode
    public void DeselectNode()
    {
        selected = false;
        // Don't draw the node's outgoing edges.
        draw_edges = false;
        obey_spring_forces = false;
    }//end method DeselectNode

    private void OnMouseEnter()
    {
        // Change the color
        ColorActive();
        moused_over = true;
    }//end method OnMouseOver
    private void OnMouseExit()
    {
        if (!mouse_dragged)
        {
            // Change the color
            ColorIdle();
        }//end if
        moused_over = false;
    }//end method OnMouseExit
    // Lets users click and drag nodes around.
    private void OnMouseDrag()
    {
        if (!data_node)
        {
            Vector3 mouse_world_position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,
                                                                                                Input.mousePosition.y,
                                                                                                0));
            // Don't change the z position of this node.
            mouse_world_position.z = gameObject.transform.position.z;
            gameObject.transform.position = mouse_world_position;
        }//end if
    }/// end method OnMouseDrag
    private void OnMouseDown()
    {
        // Change the color
        ColorActive();
        mouse_dragged = true;
        // We may be locking this node
        // If the user is pressing ctrl at the same time.
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            maybe_lock = true;
        }//end if
        else
        {
            maybe_select = true;
        }//end else
        // Note when the mouse was pressed down on this node.
        mouse_down_time = Time.time;
    }//end method OnMouseDown
    private void OnMouseUp()
    {

    }//end method OnMouseUp
}
