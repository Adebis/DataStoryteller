from DataStoryteller import DataStoryteller
from node import node
# Add location of NodeBox to sys.path before importing
#MODULE = '/users/tom/python/nodebox'
#import sys
#if MODULE not in sys.path: sys.path.append(MODULE)
#import nodebox
#from nodebox.graphics import *
#import networkx as nx
#import json
#from networkx.readwrite import json_graph
#import http_server
import pygame
import sys

def run():
    storyteller = DataStoryteller()
    #storyteller.TellStory()
    #main_graph = nx.Graph()
    #main_graph.add_edge(1, 2)
    #main_graph.add_edge(2, 3)
    #main_graph.add_edge(1, 3)
    #for node_id in main_graph:
    #    main_graph.node[node_id]['name'] = node_id

    # Create a json object of the graph. 
    #graph_dump = json_graph.node_link_data(main_graph)
    #json.dump(graph_dump, open('force/force.json', 'w'))

    #http_server.load_url('force/force.html')

    #print('\nGo to http://localhost:8000/force/force.html to see the graph\n')

    pygame.init()
    # Create a new screen, a surface object that represents actual displayed graphics.
    screen = pygame.display.set_mode((500, 500), 0, 32)
    pygame.display.set_caption("Data Storyteller")

    # A bunch of colors
    background_color = (0, 0, 0)
    BLACK = (0, 0, 0)
    WHITE = (255, 255, 255)
    BLUE = (0, 0, 255)

    test_node = node()

    # Drawing and input handling loop
    while (True):
        for event in pygame.event.get():
            if event.type == pygame.QUIT: sys.exit()
        # Wipe the screen with the background color.
        screen.fill(BLACK)
        # Draw a node!
        test_node.draw(screen)

        # Actually draw the updated screen onto the display.
        pygame.display.flip()

def main():
    while (True):
        input_string = input("Input command: ")
        # storyteller = DataStoryteller()

        if (input_string == "help"):
            print ("Commands: ")
            print ("quit")
            print ("    Exits the program.")
            print ("start")
            print ("    Starts the storyteller.")
        elif (input_string == "quit"):
            print ("Exiting program...")
            break
        elif (input_string == "start"):
            print("start")
            run()
        elif (input_string == "tell story"):
            print ("Characters: ")
            for key in storyteller.yearly_citations_by_algorithm.keys():
                print ("    " + key)
            input_string = input("Input character name: ")
            storyteller.TellCharacterStory(input_string)

if __name__ == "__main__":
    main()